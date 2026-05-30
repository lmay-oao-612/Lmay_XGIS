using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace XGIS
{
    public class XTools
    {
        public static string ReadString(BinaryReader br)
        {
            int length = br.ReadInt32();
            byte[] sbytes = br.ReadBytes(length);
            return Encoding.GetEncoding("gb2312").GetString(sbytes);
        }

        public static void WriteString(string s, BinaryWriter bw)
        {
            byte[] sbytes = Encoding.GetEncoding("gb2312").GetBytes(s);
            bw.Write(sbytes.Length);
            bw.Write(sbytes);
        }

        public static string BytesToString(byte[] byteArray)
        {
            int count = byteArray.Length;
            for (int i = 0; i < byteArray.Length; i++)
            {
                if (byteArray[i] == 0)
                {
                    count = i;
                    break;
                }
            }
            return Encoding.GetEncoding("gb2312").GetString(byteArray, 0, count);
        }

        public static Object FromBytes2Struct(BinaryReader br, Type type)
        {
            byte[] buff = br.ReadBytes(Marshal.SizeOf(type));
            GCHandle handle = GCHandle.Alloc(buff, GCHandleType.Pinned);
            Object result = Marshal.PtrToStructure(handle.AddrOfPinnedObject(), type);
            handle.Free();
            return result;
        }

        public static byte[] FromStructToBytes(object struc)
        {
            byte[] bytes = new byte[Marshal.SizeOf(struc.GetType())];
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            Marshal.StructureToPtr(struc, handle.AddrOfPinnedObject(), false);
            handle.Free();
            return bytes;
        }


        public static int ReverseInt(int value)
        {
            byte[] barray = BitConverter.GetBytes(value);
            Array.Reverse(barray);
            return BitConverter.ToInt32(barray, 0);
        }

        public static double CalculateLength(List<XVertex> _vertexes)
        {
            double length = 0;
            for (int i = 0; i < _vertexes.Count - 1; i++)
            {
                length += _vertexes[i].Distance(_vertexes[i + 1]);
            }
            return length;
        }

        public static double CalculateArea(List<XVertex> _vertexes)
        {
            double area = 0;
            for (int i = 0; i < _vertexes.Count - 1; i++)
            {
                area += VectorProduct(_vertexes[i], _vertexes[i + 1]);
            }
            area += VectorProduct(_vertexes[_vertexes.Count - 1], _vertexes[0]);
            return area / 2;
        }

        public static double VectorProduct(XVertex v1, XVertex v2)
        {
            return v1.x * v2.y - v1.y * v2.x;
        }

        public static double DistanceBetweenPointAndSegment(XVertex A, XVertex B, XVertex C)
        {
            double dot1 = DotProduct(A, B, C);
            if (dot1 > 0) return B.Distance(C);
            double dot2 = DotProduct(B, A, C);
            if (dot2 > 0) return A.Distance(C);
            double dist = CrossProduct(A, B, C) / A.Distance(B);
            return Math.Abs(dist);
        }

        static double DotProduct(XVertex A, XVertex B, XVertex C)
        {
            XVertex AB = new XVertex(B.x - A.x, B.y - A.y);
            XVertex BC = new XVertex(C.x - B.x, C.y - B.y);
            return AB.x * BC.x + AB.y * BC.y;
        }

        static double CrossProduct(XVertex A, XVertex B, XVertex C)
        {
            XVertex AB = new XVertex(B.x - A.x, B.y - A.y);
            XVertex AC = new XVertex(C.x - A.x, C.y - A.y);
            return VectorProduct(AB, AC);
        }

        /// <summary>
        /// 根据两个对角顶点生成矩形的四个顶点
        /// </summary>
        public static List<XVertex> CreateRectangle(XVertex v1, XVertex v2)
        {
            List<XVertex> vs = new List<XVertex>();
            // 顺时针或逆时针构建四个角点
            vs.Add(new XVertex(v1.x, v1.y));
            vs.Add(new XVertex(v2.x, v1.y));
            vs.Add(new XVertex(v2.x, v2.y));
            vs.Add(new XVertex(v1.x, v2.y));
            return vs;
        }

        /// <summary>
        /// 根据中心点和边缘点生成圆形的近似多边形顶点（每10度一个点）
        /// </summary>
        public static List<XVertex> CreateCircle(XVertex center, XVertex edge)
        {
            List<XVertex> vs = new List<XVertex>();
            double radius = center.Distance(edge);
            if (radius <= 0) return vs;

            // 每隔10度采样一个点，共36个点来模拟圆形
            for (int i = 0; i < 360; i += 10)
            {
                double radians = i * Math.PI / 180.0;
                double x = center.x + radius * Math.Cos(radians);
                double y = center.y + radius * Math.Sin(radians);
                vs.Add(new XVertex(x, y));
            }
            return vs;
        }

        public static XLayer OpenLayer(string layerPath)
        {
            XLayer layer;
            try
            {
                if (layerPath.ToLower().Contains(".shp"))
                    layer = XShapefile.ReadShapefile(layerPath);
                else if (layerPath.ToLower().Contains(".gis"))
                    layer = XMyFile.ReadFile(layerPath);
                else if (layerPath.ToLower().Contains(".rst"))
                    layer = new XRasterLayer(layerPath);
                else
                    return null;
            }
            catch
            {
                return null;
            }
            layer.Path = layerPath;
            return layer;
        }
    }
}
