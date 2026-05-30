﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace XGIS
{
    #region 文件类
    /// <summary>
    /// 表示一个字段的信息，包括字段名、数据类型和长度等。
    /// </summary>

    public class XMyFile
    {
        static List<Type> AllTypes = new List<Type>{
            typeof(bool),
            typeof(byte),
            typeof(char),
            typeof(decimal),
            typeof(double),
            typeof(float),
            typeof(int),
            typeof(long),
            typeof(sbyte),
            typeof(short),
            typeof(string),
            typeof(uint),
            typeof(ulong),
            typeof(ushort)
        };

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        struct MyFileHeader
        {
            public double MinX, MinY, MaxX, MaxY;
            public int FeatureCount, ShapeType, FieldCount;
        }

        static void WriteFileHeader(XVectorLayer layer, BinaryWriter bw)
        {
            MyFileHeader mfh = new MyFileHeader();
            mfh.MinX = layer.Extent.getMinX();
            mfh.MinY = layer.Extent.getMinY();
            mfh.MaxX = layer.Extent.getMaxX();
            mfh.MaxY = layer.Extent.getMaxY();
            mfh.FeatureCount = layer.FeatureCount();
            mfh.ShapeType = (int)(layer.ShapeType);
            mfh.FieldCount = layer.Fields.Count;
            bw.Write(XTools.FromStructToBytes(mfh));
        }
        static List<XField> ReadFields(BinaryReader br, int FieldCount)
        {
            List<XField> fields = new List<XField>();
            for (int fieldindex = 0; fieldindex < FieldCount; fieldindex++)
            {
                Type fieldtype = AllTypes[br.ReadInt32()];
                string fieldname = XTools.ReadString(br);
                fields.Add(new XField(fieldtype, fieldname));
            }
            return fields;
        }

        static void WriteFields(List<XField> fields, BinaryWriter bw)
        {
            for (int fieldindex = 0; fieldindex < fields.Count; fieldindex++)
            {
                XField field = fields[fieldindex];
                bw.Write(AllTypes.IndexOf(field.datatype));
                XTools.WriteString(field.name, bw);
            }
        }

        static List<XVertex> ReadMultipleVertexes(BinaryReader br)
        {
            List<XVertex> vs = new List<XVertex>();
            int vcount = br.ReadInt32();
            for (int vc = 0; vc < vcount; vc++)
                vs.Add(new XVertex(br));
            return vs;
        }

        static void WriteMultipleVertexes(List<XVertex> vs, BinaryWriter bw)
        {
            bw.Write(vs.Count);
            for (int vc = 0; vc < vs.Count; vc++)
                vs[vc].Write(bw);
        }

        static void ReadFeatures(XVectorLayer layer, BinaryReader br, int FeatureCount)
        {
            for (int featureindex = 0; featureindex < FeatureCount; featureindex++)
            {
                List<XVertex> vs = ReadMultipleVertexes(br);
                XAttribute attribute = new XAttribute(layer.Fields, br);
                XSpatial spatial = null;
                if (layer.ShapeType == SHAPETYPE.Point)
                    spatial = new XPoint(vs[0]);
                else if (layer.ShapeType == SHAPETYPE.Line)
                    spatial = new XLine(vs);
                else if (layer.ShapeType == SHAPETYPE.Polygon)
                    spatial = new XPolygon(vs);
                XFeature feature = new XFeature(spatial, attribute);
                layer.AddFeature(feature);
            }
        }

        static void WriteFeatures(XVectorLayer layer, BinaryWriter bw)
        {
            for (int featureindex = 0; featureindex < layer.FeatureCount(); featureindex++)
            {
                XFeature feature = layer.GetFeature(featureindex);
                WriteMultipleVertexes(feature.spatial.vertexes, bw);
                feature.attribute.Write(bw);
            }
        }

        public static XVectorLayer ReadFile(string filename)
        {
            using (FileStream fsr = new FileStream(filename, FileMode.Open))
            using (BinaryReader br = new BinaryReader(fsr))
            {
                MyFileHeader mfh = (MyFileHeader)(XTools.FromBytes2Struct(br, typeof(MyFileHeader)));
                SHAPETYPE ShapeType = (SHAPETYPE)Enum.Parse(typeof(SHAPETYPE), mfh.ShapeType.ToString());
                string layername = XTools.ReadString(br);
                XVectorLayer layer = new XVectorLayer(layername, ShapeType);
                layer.Fields = ReadFields(br, mfh.FieldCount);
                layer.Extent = new XExtent(mfh.MinX, mfh.MaxX, mfh.MinY, mfh.MaxY);
                ReadFeatures(layer, br, mfh.FeatureCount);
                return layer;
            }
        }

        public static void WriteFile(XVectorLayer layer, string filename)
        {
            using (FileStream fsr = new FileStream(filename, FileMode.Create))
            using (BinaryWriter bw = new BinaryWriter(fsr))
            {
                WriteFileHeader(layer, bw);
                XTools.WriteString(layer.Name, bw);
                WriteFields(layer.Fields, bw);
                WriteFeatures(layer, bw);
            }
        }

    }

    public class XField
    {
        public Type datatype;
        public string name;
        public int DBFFieldLength;

        public XField(Type _dt, string _name)
        {
            datatype = _dt;
            name = _name;
        }

        public XField(BinaryReader br)
        {
            XShapefile.DBFField dbfField = (XShapefile.DBFField)XTools.FromBytes2Struct(br, typeof(XShapefile.DBFField));
            this.DBFFieldLength = dbfField.LengthOfField;

            byte[] bs = new byte[] {
            dbfField.b1, dbfField.b2, dbfField.b3, dbfField.b4, dbfField.b5,
            dbfField.b6, dbfField.b7, dbfField.b8, dbfField.b9, dbfField.b10, dbfField.b11
        };
            this.name = XTools.BytesToString(bs).Trim();
            switch ((char)dbfField.FieldType)
            {
                case 'N':
                    if (dbfField.NumberOfDecimalPlaces == 0)
                        datatype = Type.GetType("System.Int32");
                    else
                        datatype = Type.GetType("System.Double");
                    break;
                case 'F':
                    datatype = Type.GetType("System.Double");
                    break;
                case 'D': 
                    datatype = typeof(DateTime);
                    break;
                case 'L': 
                    datatype = typeof(bool);
                    break;
                default:
                    datatype = Type.GetType("System.String");
                    break;
            }
        }

        public object DBFValueToObject(BinaryReader br)
        {
            byte[] temp = br.ReadBytes(DBFFieldLength);
            string sv = XTools.BytesToString(temp).Trim();
            if (datatype == Type.GetType("System.String"))
                return sv;
            else if (datatype == Type.GetType("System.Double"))
                return double.Parse(sv);
            else if (datatype == Type.GetType("System.Int32"))
                return int.Parse(sv);
            else if (datatype == typeof(DateTime))
            {
                if (DateTime.TryParseExact(sv, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out DateTime dt))
                    return dt;
                return DateTime.MinValue; // 或者返回 null
            }
            else if (datatype == typeof(bool))
            {
                string s = sv.ToUpperInvariant();
                if (s == "T" || s == "Y" || s == "1" || s == "TRUE") return true;
                return false;
            }
            return sv;
        }

    }

    /// <summary>
    /// Shapefile 文件类，包含读取 SHP 和 DBF 文件的逻辑
    /// </summary>
    public class XShapefile
    {
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        struct DBFHeader
        {
            public byte FileType, Year, Month, Day;
            public int RecordCount;
            public short HeaderLength, RecordLength;
            public long Unused1, Unused2;
            public int Unused3;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct DBFField
        {
            public byte b1, b2, b3, b4, b5, b6, b7, b8, b9, b10, b11;
            public byte FieldType;
            public int DisplacementInRecord;
            public byte LengthOfField;
            public byte NumberOfDecimalPlaces;
            public long Unused1;
            public int Unused2;
            public short Unused3;
        }


        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        struct ShapefileHeader
        {
            public int Unused1, Unused2, Unused3, Unused4;
            public int Unused5, Unused6, Unused7, Unused8;
            public int ShapeType;
            public double Xmin;
            public double Ymin;
            public double Xmax;
            public double Ymax;
            public double Unused9, Unused10, Unused11, Unused12;
        };

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        struct RecordHeader
        {
            public int RecordNumber;
            public int RecordLength;
            public int ShapeType;
        };

        // SHP 文件中 ShapeType 的整数值与枚举的对应关系
        static Dictionary<int, SHAPETYPE> Int2Shapetype = new Dictionary<int, SHAPETYPE>
        {
            {1, SHAPETYPE.Point }, {3, SHAPETYPE.Line }, {5, SHAPETYPE.Polygon }
        };

        static List<XField> ReadDBFFields(string dbffilename)
        {
            using (FileStream fsr = new FileStream(dbffilename, FileMode.Open))
            using (BinaryReader br = new BinaryReader(fsr))
            {
                DBFHeader dh = (DBFHeader)XTools.FromBytes2Struct(br, typeof(DBFHeader));
                int FieldCount = (dh.HeaderLength - 33) / 32;
                List<XField> fields = new List<XField>();
                for (int i = 0; i < FieldCount; i++)
                    fields.Add(new XField(br));
                return fields;
            }
        }

        static List<XAttribute> ReadDBFValues(string dbffilename, List<XField> fields)
        {
            using (FileStream fsr = new FileStream(dbffilename, FileMode.Open))
            using (BinaryReader br = new BinaryReader(fsr))
            {
                DBFHeader dh = (DBFHeader)XTools.FromBytes2Struct(br, typeof(DBFHeader));
                int FieldCount = (dh.HeaderLength - 33) / 32;
                br.ReadBytes(32 * FieldCount + 1); //跳过字段区及结束标志字节
                List<XAttribute> attributes = new List<XAttribute>();
                for (int i = 0; i < dh.RecordCount; i++)
                {
                    XAttribute attribute = new XAttribute();
                    char tempchar = (char)br.ReadByte();  //每个记录的开始都有一个起始字节
                    for (int j = 0; j < FieldCount; j++)
                        attribute.AddValue(fields[j].DBFValueToObject(br));
                    attributes.Add(attribute);
                }
                return attributes;
            }
        }

        static ShapefileHeader ReadFileHeader(BinaryReader br)
        {
            return (ShapefileHeader)XTools.FromBytes2Struct(br, typeof(ShapefileHeader));
        }

        static RecordHeader ReadRecordHeader(BinaryReader br)
        {
            return (RecordHeader)XTools.FromBytes2Struct(br, typeof(RecordHeader));
        }

        public static XVectorLayer ReadShapefile(string shpfilename)
        {
            // 读取 SHP 文件头部信息，获取 ShapeType 和范围信息32
            using (FileStream fsr = new FileStream(shpfilename, FileMode.Open))
            using (BinaryReader br = new BinaryReader(fsr))
            {
                ShapefileHeader sfh = ReadFileHeader(br);
                SHAPETYPE ShapeType = Int2Shapetype[sfh.ShapeType];
                string layerName = System.IO.Path.GetFileNameWithoutExtension(shpfilename);
                // 创建一个新的图层对象，并设置其范围信息
                XVectorLayer layer = new XVectorLayer(layerName, ShapeType);
                layer.Extent = new XExtent(sfh.Xmax, sfh.Xmin, sfh.Ymax, sfh.Ymin);

                // 读取 DBF 文件中的字段信息和属性值
                string dbffilename = shpfilename.ToLower().Replace(".shp", ".dbf");
                layer.Fields = ReadDBFFields(dbffilename);
                List<XAttribute> attributes = ReadDBFValues(dbffilename, layer.Fields);
                int index = 0;
                while (br.PeekChar() != -1)
                {
                    RecordHeader rh = ReadRecordHeader(br);
                    int ByteLength = XTools.ReverseInt(rh.RecordLength) * 2 - 4;
                    byte[] RecordContent = br.ReadBytes(ByteLength);
                    if (ShapeType == SHAPETYPE.Point)
                    {
                        XPoint onepoint = ReadPoint(RecordContent);
                        // 确保 index 不越界
                        XAttribute attr = (index < attributes.Count) ? attributes[index] : new XAttribute();
                        XFeature feature = new XFeature(onepoint, attr);
                        layer.AddFeature(feature);
                    }
                    else if (ShapeType == SHAPETYPE.Line)
                    {
                        List<XLine> lines = ReadLines(RecordContent);
                        for (int i = 0; i < lines.Count; i++)
                        {
                            XFeature onefeature = new XFeature(lines[i], new XAttribute(attributes[index]));
                            layer.AddFeature(onefeature);
                        }
                    }
                    else if (ShapeType == SHAPETYPE.Polygon)
                    {
                        List<XPolygon> polygons = ReadPolygons(RecordContent);
                        for (int i = 0; i < polygons.Count; i++)
                        {
                            XFeature onefeature = new XFeature(polygons[i], new XAttribute(attributes[index]));
                            layer.AddFeature(onefeature);
                        }
                    }
                    index++;

                }
                return layer;
            }
        }

        static XPoint ReadPoint(byte[] RecordContent)
        {
            double x = BitConverter.ToDouble(RecordContent, 0);
            double y = BitConverter.ToDouble(RecordContent, 8);
            return new XPoint(new XVertex(x, y));
        }


        static List<XLine> ReadLines(byte[] RecordContent)
        {
            int N = BitConverter.ToInt32(RecordContent, 32);
            int M = BitConverter.ToInt32(RecordContent, 36);
            int[] parts = new int[N + 1];

            for (int i = 0; i < N; i++)
            {
                parts[i] = BitConverter.ToInt32(RecordContent, 40 + i * 4);
            }
            parts[N] = M;
            List<XLine> lines = new List<XLine>();
            for (int i = 0; i < N; i++)
            {
                List<XVertex> vertexes = new List<XVertex>();
                for (int j = parts[i]; j < parts[i + 1]; j++)
                {
                    double x = BitConverter.ToDouble(RecordContent, 40 + N * 4 + j * 16);
                    double y = BitConverter.ToDouble(RecordContent, 40 + N * 4 + j * 16 + 8);
                    vertexes.Add(new XVertex(x, y));
                }
                lines.Add(new XLine(vertexes));
            }
            return lines;
        }

        static List<XPolygon> ReadPolygons(byte[] RecordContent)
        {
            int N = BitConverter.ToInt32(RecordContent, 32);
            int M = BitConverter.ToInt32(RecordContent, 36);
            int[] parts = new int[N + 1];
            for (int i = 0; i < N; i++)
            {
                parts[i] = BitConverter.ToInt32(RecordContent, 40 + i * 4);
            }
            parts[N] = M;
            List<XPolygon> polygons = new List<XPolygon>();
            for (int i = 0; i < N; i++)
            {
                List<XVertex> vertexes = new List<XVertex>();
                for (int j = parts[i]; j < parts[i + 1]; j++)
                {
                    double x = BitConverter.ToDouble(RecordContent, 40 + N * 4 + j * 16);
                    double y = BitConverter.ToDouble(RecordContent, 40 + N * 4 + j * 16 + 8);
                    vertexes.Add(new XVertex(x, y));
                }
                polygons.Add(new XPolygon(vertexes));
            }
            return polygons;
        }

        public static int GetFeatureCount(string shpfilename)
        {
            string shxPath = Path.ChangeExtension(shpfilename, ".shx");
            if (!File.Exists(shxPath)) return -1; // 如果索引文件不存在

            using (FileStream fs = new FileStream(shxPath, FileMode.Open))
            using (BinaryReader br = new BinaryReader(fs))
            {
                // SHX 头部第 24 字节开始是 File Length (以 16-bit word 为单位)
                br.BaseStream.Seek(24, SeekOrigin.Begin);
                int fileLengthInWords = XTools.ReverseInt(br.ReadInt32());
                int fileLengthInBytes = fileLengthInWords * 2;

                // SHX 头部固定 100 字节，每个记录项（Offset+Length）固定 8 字节
                int count = (fileLengthInBytes - 100) / 8;
                return count;
            }
        }
    }

    
    #endregion

    #region 视图与投影逻辑
    public enum SHAPETYPE { Point, Line, Polygon, unknown };
    public enum XExploreActions { zoomin, zoomout, moveup, movedown, moveleft, moveright, zoominbybox, identifybybox, pan, noaction };
    public enum XMouseTool { identify, measure, none , draw }


    public class XView
    {
        public XExtent CurrentMapExtent;
        public Rectangle MapWindowSize;
        double MapMinX, MapMinY, ScaleX, ScaleY;
        int WinH;

        public XView(XExtent _extent, Rectangle _rectangle) { Update(_extent, _rectangle); }

        public void Update(XExtent _extent, Rectangle _rectangle)
        {
            MapWindowSize = _rectangle;
            WinH = MapWindowSize.Height;
            ScaleX = ScaleY = Math.Max(_extent.getWidth() / MapWindowSize.Width, _extent.getHeight() / MapWindowSize.Height);

            double MapW = ScaleX * MapWindowSize.Width;
            double MapH = ScaleY * MapWindowSize.Height;
            XVertex center = _extent.getCenter();
            MapMinX = center.x - MapW / 2;
            MapMinY = center.y - MapH / 2;
            CurrentMapExtent = new XExtent(MapMinX, MapMinX + MapW, MapMinY, MapMinY + MapH);
        }

        public Point ToScreenPoint(XVertex v) => new Point((int)((v.x - MapMinX) / ScaleX), (int)(WinH - (v.y - MapMinY) / ScaleY));
        public List<Point> ToScreenPoints(List<XVertex> vs)
        {
            List<Point> points = new List<Point>();
            foreach (var v in vs)
                points.Add(ToScreenPoint(v));
            return points;
        }
        public XVertex ToMapVertex(Point p) => new XVertex(ScaleX * p.X + MapMinX, ScaleY * (WinH - p.Y) + MapMinY);

        public void OffsetCenter(XVertex vFrom, XVertex vTo)
        {
            Point pFrom = ToScreenPoint(vFrom);
            Point pTo = ToScreenPoint(vTo);
            Point newCenterP = new Point(MapWindowSize.Width / 2 - pTo.X + pFrom.X, MapWindowSize.Height / 2 - pTo.Y + pFrom.Y);
            CurrentMapExtent.SetCenter(ToMapVertex(newCenterP));
            Update(CurrentMapExtent, MapWindowSize);
        }

        public double ToScreenDistance(double mapDist, XVertex v)
            => Math.Abs(ToScreenPoint(v).X - ToScreenPoint(new XVertex(v.x - mapDist, v.y)).X);

        public void ChangeView(XExploreActions action)
        {
            CurrentMapExtent.ChangeExtent(action);
            Update(CurrentMapExtent, MapWindowSize);
        }
        public void ZoomAtPoint(Point mouseLocation, double factor)
        {
            //记录缩放前鼠标指向的地图坐标
            XVertex mapPointBefore = ToMapVertex(mouseLocation);

            //执行缩放：改变 Scale
            ScaleX *= factor;
            ScaleY *= factor;

            // 计算新的地图范围起始点，使 mapPointBefore 依然对应 mouseLocation
            MapMinX = mapPointBefore.x - mouseLocation.X * ScaleX;
            MapMinY = mapPointBefore.y - (WinH - mouseLocation.Y) * ScaleY;

            //更新当前范围
            double MapW = ScaleX * MapWindowSize.Width;
            double MapH = ScaleY * MapWindowSize.Height;
            CurrentMapExtent = new XExtent(MapMinX, MapMinX + MapW, MapMinY, MapMinY + MapH);
        }
        public void UpdateMapWindow(Rectangle rect)
        {
            MapWindowSize = rect;
            Update(CurrentMapExtent, MapWindowSize);
        }
        public double ToMapDistance(int pixelCount)
        {
            Point p1 = new Point(0, 0);
            Point p2 = new Point(0, pixelCount);
            XVertex v1 = ToMapVertex(p1);
            XVertex v2 = ToMapVertex(p2);
            return v1.Distance(v2);
        }
    }
    #endregion

    #region 空间数据结构
    public class XExtent
    {
        public XVertex bottomleft, upright;
        public XExtent(double x1, double x2, double y1, double y2)
        {
            bottomleft = new XVertex(Math.Min(x1, x2), Math.Min(y1, y2));
            upright = new XVertex(Math.Max(x1, x2), Math.Max(y1, y2));
        }
        public XExtent(XVertex v1, XVertex v2) : this(v1.x, v2.x, v1.y, v2.y) { }
        public XExtent(XExtent e) : this(e.bottomleft.x, e.upright.x, e.bottomleft.y, e.upright.y) { }

        public double getMinX() => bottomleft.x;
        public double getMaxX() => upright.x;
        public double getMinY() => bottomleft.y;
        public double getMaxY() => upright.y;
        public double getWidth() => upright.x - bottomleft.x;
        public double getHeight() => upright.y - bottomleft.y;
        public XVertex getCenter() => new XVertex((upright.x + bottomleft.x) / 2, (upright.y + bottomleft.y) / 2);

        public void Merge(XExtent e)
        {
            bottomleft.x = Math.Min(bottomleft.x, e.bottomleft.x);
            bottomleft.y = Math.Min(bottomleft.y, e.bottomleft.y);
            upright.x = Math.Max(upright.x, e.upright.x);
            upright.y = Math.Max(upright.y, e.upright.y);
        }

        public bool IntersectOrNot(XExtent e) => !(upright.x < e.bottomleft.x || bottomleft.x > e.upright.x || upright.y < e.bottomleft.y || bottomleft.y > e.upright.y);

        public void SetCenter(XVertex c)
        {
            double w = getWidth(), h = getHeight();
            bottomleft = new XVertex(c.x - w / 2, c.y - h / 2);
            upright = new XVertex(c.x + w / 2, c.y + h / 2);
        }

        public void ChangeExtent(XExploreActions action)
        {
            double factor = (action == XExploreActions.zoomin) ? 0.5 : 2.0;
            if (action == XExploreActions.zoomin || action == XExploreActions.zoomout)
            {
                XVertex c = getCenter();
                double nw = getWidth() * factor, nh = getHeight() * factor;
                bottomleft = new XVertex(c.x - nw / 2, c.y - nh / 2);
                upright = new XVertex(c.x + nw / 2, c.y + nh / 2);
            }
        }
    }

    public class XFeature
    {
        public XSpatial spatial;
        public XAttribute attribute;
        public bool IsSelected = false;
        public XFeature(XSpatial s, XAttribute a) { spatial = s; attribute = a; }
        public XExtent GetExtent() => spatial.extent;
        public void draw(Graphics g, XView v, bool label, int idx)
        {
            spatial.IsSelected = this.IsSelected;
            spatial.draw(g, v);
            if (label) attribute.draw(g, v, spatial.centroid, idx);
        }
        public object getAttribute(int i) => attribute.GetValue(i);
        public string getAttributeAsString(int i) => attribute.GetValue(i)?.ToString() ?? "";
        public double Distance(XVertex v) => spatial.Distance(v);
    }

    public abstract class XSpatial
    {
        public XVertex centroid;
        public XExtent extent;
        public List<XVertex> vertexes;
        public bool IsSelected = false;

        public XSpatial(List<XVertex> vs)
        {
            vertexes = vs;
            double sx = 0, sy = 0, xmin = double.MaxValue, ymin = double.MaxValue, xmax = double.MinValue, ymax = double.MinValue;
            foreach (var v in vs)
            {
                sx += v.x; sy += v.y;
                xmin = Math.Min(xmin, v.x); ymin = Math.Min(ymin, v.y);
                xmax = Math.Max(xmax, v.x); ymax = Math.Max(ymax, v.y);
            }
            centroid = new XVertex(sx / vs.Count, sy / vs.Count);
            extent = new XExtent(xmin, xmax, ymin, ymax);
        }
        public abstract void draw(Graphics g, XView v);
        public virtual double Distance(XVertex v) => centroid.Distance(v);
    }

    public class XVertex
    {
        public double x, y;
        public XVertex(double _x, double _y) { x = _x; y = _y; }
        public XVertex(XVertex v) { x = v.x; y = v.y; }
        public XVertex(BinaryReader br) { x = br.ReadDouble(); y = br.ReadDouble(); }
        public void Write(BinaryWriter bw) { bw.Write(x); bw.Write(y); }
        public double Distance(XVertex v) => Math.Sqrt((x - v.x) * (x - v.x) + (y - v.y) * (y - v.y));
        public bool IsSame(XVertex vertex)
        {
            return x == vertex.x && y == vertex.y;
        }
    }

    public class XPoint : XSpatial
    {
        public XPoint(XVertex v) : base(new List<XVertex> { v }) { }
        public override void draw(Graphics g, XView v)
        {
            Point sp = v.ToScreenPoint(centroid);
            if (IsSelected)
                g.FillEllipse(Brushes.Yellow, sp.X - 5, sp.Y - 5, 10, 10);
            g.FillEllipse(Brushes.Red, sp.X - 3, sp.Y - 3, 6, 6);
        }
    }

    class XLine : XSpatial
    {
        public bool ShowNodes = false;
        public double Length => XTools.CalculateLength(vertexes);
        public XLine(List<XVertex> _vertexes) : base(_vertexes) { }
        public override double Distance(XVertex vertex)
        {
            double distance = Double.MaxValue;
            for (int i = 0; i < vertexes.Count - 1; i++)
            {
                distance = Math.Min(XTools.DistanceBetweenPointAndSegment(
                    vertexes[i], vertexes[i + 1], vertex),
                    distance);
            }
            return distance;
        }

        public override void draw(Graphics graphics, XView view)
        {
            if (vertexes.Count < 2) return;
            Point[] points = new Point[vertexes.Count];
            for (int i = 0; i < vertexes.Count; i++)
            {
                points[i] = view.ToScreenPoint(vertexes[i]);
            }

            if (IsSelected)
            {
                using (Pen highlightPen = new Pen(Color.Yellow, 5))
                    graphics.DrawLines(highlightPen, points);
            }
            using (Pen linePen = new Pen(Color.Blue, 2))
            {
                graphics.DrawLines(linePen, points);
            }
            if (ShowNodes)
            {
                using (Pen markPen = new Pen(Color.Red, 2))
                {
                    foreach (Point p in points)
                    {
                        graphics.DrawLine(markPen, p.X - 8, p.Y, p.X + 8, p.Y);
                        graphics.DrawLine(markPen, p.X, p.Y - 8, p.X, p.Y + 8);
                    }
                }

            }
        }
    }


    public class XPolygon : XSpatial
    {
        public double Area;
        public XPolygon(List<XVertex> _vertexes) : base(_vertexes)
        {
            Area = XTools.CalculateArea(_vertexes);
        }

        public bool Contains(XVertex vertex, out bool inside)
        {
            int count = 0;
            inside = true;
            for (int i = 0; i < vertexes.Count; i++)
            {
                //满足情况3
                if (vertexes[i].IsSame(vertex))
                {
                    inside = false;
                    return true;
                }
                //由序号为i及next的两个节点构成一条线段，一般情况下next为i+1，
                //而针对最后一条线段，i为vertexes.Count-1，next为0
                int next = (i + 1) % vertexes.Count;
                //确定线段的坐标极值
                double minX = Math.Min(vertexes[i].x, vertexes[next].x);
                double minY = Math.Min(vertexes[i].y, vertexes[next].y);
                double maxX = Math.Max(vertexes[i].x, vertexes[next].x);
                double maxY = Math.Max(vertexes[i].y, vertexes[next].y);
                //如果线段是平行于射线的。
                if (minY == maxY)
                {
                    //满足情况2
                    if (minY == vertex.y && vertex.x >= minX && vertex.x <= maxX)
                    {
                        inside = false;
                        return true;
                    }
                    //满足情况1或者射线与线段平行无交点
                    else continue;
                }
                //点在线段坐标极值之外，不可能有交点
                if (vertex.x > maxX || vertex.y > maxY || vertex.y < minY) continue;
                //计算交点横坐标，纵坐标无需计算，就是vertex.y
                double X0 = vertexes[i].x + (vertex.y - vertexes[i].y) *
                    (vertexes[next].x - vertexes[i].x) / (vertexes[next].y - vertexes[i].y);
                //交点在射线反方向，按无交点计算
                if (X0 < vertex.x) continue;
                //交点即为vertex，且在线段上
                if (X0 == vertex.x)
                {
                    inside = false;
                    return true;
                }
                //射线穿过线段下端点，不记数
                if (vertex.y == minY) continue;
                //其他情况下，交点数加一
                count++;
            }
            return count % 2 != 0;
        }


        public override double Distance(XVertex vertex)
        {
            bool inside;
            if (Contains(vertex, out inside))
            {
                if (inside) return -1;
                else return 0;
            }
            else
            {
                List<XVertex> vs = new List<XVertex>();
                vs.AddRange(vertexes);
                vs.Add(vertexes[0]);
                XLine line = new XLine(vs);
                return line.Distance(vertex);
            }
        }

        public override void draw(Graphics g, XView view)
        {
            Point[] points = view.ToScreenPoints(vertexes).ToArray();
            Color fillColor = IsSelected ? Color.FromArgb(50, Color.Yellow) : Color.FromArgb(50, Color.Orange);
            using (SolidBrush brush = new SolidBrush(fillColor))
                g.FillPolygon(brush, points);

            using (Pen borderPen = IsSelected ? new Pen(Color.Red, 3) : new Pen(Color.Black, 1))
            {
                g.DrawPolygon(borderPen, points);
            }
        }
    }

    public class XAttribute
    {
        List<object> values = new List<object>();
        public XAttribute() { }
        public XAttribute(XAttribute a) 
        { 
            foreach (var v in a.values)
                values.Add(v);
        }

        public XAttribute(List<XField> fs, BinaryReader br)
        {
            for (int i = 0; i < fs.Count; i++)
            {
                Type type = fs[i].datatype;
                if (type.ToString() == "System.Boolean")
                    AddValue(br.ReadBoolean());
                else if (type.ToString() == "System.Byte")
                    AddValue(br.ReadByte());
                else if (type.ToString() == "System.Char")
                    AddValue(br.ReadChar());
                else if (type.ToString() == "System.Decimal")
                    AddValue(br.ReadDecimal());
                else if (type.ToString() == "System.Double")
                    AddValue(br.ReadDouble());
                else if (type.ToString() == "System.Single")
                    AddValue(br.ReadSingle());
                else if (type.ToString() == "System.Int32")
                    AddValue(br.ReadInt32());
                else if (type.ToString() == "System.Int64")
                    AddValue(br.ReadInt64());
                else if (type.ToString() == "System.UInt16")
                    AddValue(br.ReadUInt16());
                else if (type.ToString() == "System.UInt32")
                    AddValue(br.ReadUInt32());
                else if (type.ToString() == "System.UInt64")
                    AddValue(br.ReadUInt64());
                else if (type.ToString() == "System.SByte")
                    AddValue(br.ReadSByte());
                else if (type.ToString() == "System.Int16")
                    AddValue(br.ReadInt16());
                else if (type.ToString() == "System.String")
                    AddValue(XTools.ReadString(br));
            }
        }

        public void Write(BinaryWriter bw)
        {
            for (int i = 0; i < values.Count; i++)
            {
                Type type = GetValue(i).GetType();
                if (type.ToString() == "System.Boolean")
                    bw.Write((bool)GetValue(i));
                else if (type.ToString() == "System.Byte")
                    bw.Write((byte)GetValue(i));
                else if (type.ToString() == "System.Char")
                    bw.Write((char)GetValue(i));
                else if (type.ToString() == "System.Decimal")
                    bw.Write((decimal)GetValue(i));
                else if (type.ToString() == "System.Double")
                    bw.Write((double)GetValue(i));
                else if (type.ToString() == "System.Single")
                    bw.Write((float)GetValue(i));
                else if (type.ToString() == "System.Int32")
                    bw.Write((int)GetValue(i));
                else if (type.ToString() == "System.Int64")
                    bw.Write((long)GetValue(i));
                else if (type.ToString() == "System.UInt16")
                    bw.Write((ushort)GetValue(i));
                else if (type.ToString() == "System.UInt32")
                    bw.Write((uint)GetValue(i));
                else if (type.ToString() == "System.UInt64")
                    bw.Write((ulong)GetValue(i));
                else if (type.ToString() == "System.SByte")
                    bw.Write((sbyte)GetValue(i));
                else if (type.ToString() == "System.Int16")
                    bw.Write((short)GetValue(i));
                else if (type.ToString() == "System.String")
                    XTools.WriteString((string)GetValue(i), bw);
            }
        }

        public void AddValue(object o) => values.Add(o);
        public object GetValue(int i) => values.Count > i ? values[i] : "No Attr";
        public void draw(Graphics g, XView v, XVertex loc, int i)
        {
            Point sp = v.ToScreenPoint(loc);
            g.DrawString(GetValue(i).ToString(), new Font("宋体", 9), Brushes.Green, sp.X, sp.Y);
        }
    }
    #endregion

  

    public class XThematic
    {
        //线实体显示样式
        public Pen LinePen = new Pen(Color.Black, 1);
        //面实体显示样式
        public Pen PolygonPen = new Pen(Color.Blue, 1);
        public SolidBrush PolygonBrush = new SolidBrush(Color.Yellow);
        //点实体显示样式
        public Pen PointPen = new Pen(Color.Red, 1);
        public SolidBrush PointBrush = new SolidBrush(Color.White);
        public int PointRadius = 5;

        public XThematic()
        {

        }

        public XThematic(Pen _LinePen,
            Pen _PolygonPen, SolidBrush _PolygonBrush,
            Pen _PointPen, SolidBrush _PointBrush, int _PointRadius)
        {
            LinePen = _LinePen;
            PolygonPen = _PolygonPen;
            PolygonBrush = _PolygonBrush;
            PointPen = _PointPen;
            PointBrush = _PointBrush;
            PointRadius = _PointRadius;
        }
    }
}
