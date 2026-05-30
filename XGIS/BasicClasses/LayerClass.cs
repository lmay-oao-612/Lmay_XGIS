using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XGIS
{
    public class XDocument
    {
        public List<XLayer> Layers = new List<XLayer>();
        private readonly HashSet<string> _layerNames = new HashSet<string>();
        public XExtent Extent = null;

        public void AddLayer(XLayer layer)
        {
            string name = layer.Name;
            int count = 0;
            while (!UniqueName(name))
            {
                count++;
                name = layer.Name + "_" + count;
            }
            layer.Name = name;
            Layers.Add(layer);
            _layerNames.Add(name);
            if (layer.Extent != null)
            {
                if (Extent == null) Extent = new XExtent(layer.Extent);
                else Extent.Merge(layer.Extent);
            }
        }


        private bool UniqueName(string name)
        {
            return !_layerNames.Contains(name);
        }

        public void DeleteLayer(XLayer layer)
        {
            if (!Layers.Contains(layer)) return;
            _layerNames.Remove(layer.Name);
            Layers.Remove(layer);
            if (Layers.Count == 0) return;
            else
            {
                // 修复：重新计算总范围时需要过滤掉范围为空的图层
                Extent = null;
                foreach (XLayer _layer in Layers)
                {
                    if (_layer.Extent == null) continue;
                    if (Extent == null) Extent = new XExtent(_layer.Extent);
                    else Extent.Merge(_layer.Extent);
                }
            }
        }


        public XLayer FindLayer(string layerName)
        {
            foreach (XLayer layer in Layers)
                if (layer.Name == layerName) return layer;
            return null;
        }

        public bool ChangeLayerName(XLayer layer, string layerName)
        {
            if (layer.Name == layerName) return true;
            if (!UniqueName(layerName)) return false;

            _layerNames.Remove(layer.Name);
            layer.Name = layerName;
            _layerNames.Add(layerName);
            return true;
        }

        public bool AdjustLayerOrder(XLayer layer, int step)
        {
            int index = Layers.IndexOf(layer);
            if (index + step < 0 || index + step >= Layers.Count) return false;
            XLayer _layer = Layers[index + step];
            Layers[index + step] = layer;
            Layers[index] = _layer;
            return true;
        }

        public void DrawLayers(Graphics g, XView view)
        {
            foreach (XLayer layer in Layers)
            {
                if (layer.Visible) layer.draw(g, view);
            }
        }

        public void ClearSelection()
        {
            foreach (XLayer layer in Layers)
            {
                if (layer is XVectorLayer)
                {
                    XVectorLayer vlayer = (XVectorLayer)layer;
                    vlayer.SelectedFeatures.Clear();
                }
            }
        }
        public void SelectByVertex(XVertex vertex, double tolerance, bool modify)
        {
            foreach (XLayer layer in Layers)
            {
                if (layer is XVectorLayer)
                {
                    XVectorLayer vlayer = (XVectorLayer)layer;
                    if (vlayer.Selectable)
                        vlayer.SelectByVertex(vertex, tolerance, modify);
                }
            }
        }

        public void SelectByExtent(XExtent extent, bool modify)
        {
            foreach (XLayer layer in Layers)
            {
                if (layer is XVectorLayer)
                {
                    XVectorLayer vlayer = (XVectorLayer)layer;
                    if (vlayer.Selectable)
                        vlayer.SelectByExtent(extent, modify);
                }
            }
        }

        public void Write(string filename)
        {
            using (FileStream fsr = new FileStream(filename, FileMode.Create))
            using (BinaryWriter bw = new BinaryWriter(fsr))
            {
                foreach (XLayer layer in Layers)
                {
                    if (layer is XVectorLayer)
                        XTools.WriteString("XVectorLayer", bw);
                    else if (layer is XRasterLayer)
                        XTools.WriteString("XRasterLayer", bw);
                    layer.WriteToDoc(bw);
                }
            }
        }

        public void Read(string filename)
        {
            Layers.Clear();
            _layerNames.Clear();
            Extent = null; // 读取新文档前重置范围
            using (FileStream fsr = new FileStream(filename, FileMode.Open))
            using (BinaryReader br = new BinaryReader(fsr))
            {
                while (br.PeekChar() != -1)
                {
                    XLayer newLayer;
                    string layerType = XTools.ReadString(br);
                    if (layerType == "XVectorLayer")
                        newLayer = XVectorLayer.ReadFromDoc(br);
                    else if (layerType == "XRasterLayer")
                        newLayer = XRasterLayer.ReadFromDoc(br);
                    else
                        newLayer = null;
                    if (newLayer == null) continue;
                    AddLayer(newLayer);
                }
            }
        }


    }

    public abstract class XLayer
    {
        public string Name;
        public XExtent Extent;
        public string Path = "";
        public bool Visible = true;
        public abstract void draw(Graphics g, XView v);
        public abstract void WriteToDoc(BinaryWriter bw);
    }

    /// <summary>
    /// 矢量图层类
    /// </summary>
    public class XVectorLayer : XLayer
    {
        public SHAPETYPE ShapeType;

        public List<XFeature> Features = new List<XFeature>();
        public List<XFeature> SelectedFeatures = new List<XFeature>();
        public List<XField> Fields = new List<XField>();

        public bool LabelOrNot = false;
        public int LabelIndex = 0;
        public bool Selectable = true;
        public XThematic UnselectedThematic, SelectedThematic;

        public XVectorLayer(string _name, SHAPETYPE _shapetype)
        {
            Name = _name;
            ShapeType = _shapetype;
            UnselectedThematic = new XThematic();
            SelectedThematic = new XThematic(new Pen(Color.Red, 1),
                new Pen(Color.Red, 1), new SolidBrush(Color.Pink),
                new Pen(Color.Red, 1), new SolidBrush(Color.Pink), 5);
        }

        public void AddFeature(XFeature f)
        {
            Features.Add(f);
            //更新范围
            if (Features.Count == 1) Extent = new XExtent(f.spatial.extent);
            else Extent.Merge(f.spatial.extent);
        }

        public void SelectByAttribute(XSelect.OPERATOR op, int fieldIndex, object key, bool modify)
        {
            List<XFeature> features = XSelect.SelectFeaturesByAttribute(Features, op, fieldIndex, key);
            ModifySelection(features, modify);
        }

        public void SelectByVertex(XVertex vertex, double tolerance, bool modify)
        {
            List<XFeature> features = XSelect.ToFeatures(
                XSelect.SelectFeaturesByVertex(vertex, Features, tolerance));
            ModifySelection(features, modify);
        }

        public void SelectByExtent(XExtent extent, bool modify)
        {
            List<XFeature> features = XSelect.ToFeatures(
                XSelect.SelectFeaturesByExtent(extent, Features));
            ModifySelection(features, modify);
        }
        
        public void ModifySelection(List<XFeature> features, bool Modify)
        {
            if (!Modify)
            {
                SelectedFeatures = features;
            }
            else
            {
                bool IncludeAll = true;
                foreach (XFeature feature in features)
                {
                    if (!SelectedFeatures.Contains(feature))
                    {
                        //情景2：添加入选择集
                        IncludeAll = false;
                        SelectedFeatures.Add(feature);
                    }
                }
                if (IncludeAll)
                {
                    //情景1：从选择集中移出
                    foreach (XFeature feature in features)
                    {
                        SelectedFeatures.Remove(feature);
                    }
                }
            }
        }

        public void UpdateExtent()
        {
            if (Features.Count == 0)
                Extent = null;
            else
            {
                Extent = new XExtent(Features[0].spatial.extent);
                for (int i = 1; i < Features.Count; i++)
                    Extent.Merge(Features[i].spatial.extent);
            }
        }


        public void RemoveFeature(int index)
        {
            Features.RemoveAt(index);
            UpdateExtent();
        }

        public int FeatureCount()
        {
            return Features.Count;
        }

        public XFeature GetFeature(int index)
        {
            return Features[index];
        }

        public void Clear()
        {
            Features.Clear();
            Extent = null;
        }

        public override void draw(Graphics g, XView v)
        {
            if (Extent == null || !Extent.IntersectOrNot(v.CurrentMapExtent) || !Visible) return;
            foreach (var f in Features)
            {
                if (f.spatial.extent.IntersectOrNot(v.CurrentMapExtent))
                    f.draw(g, v, LabelOrNot, LabelIndex);
            }
        }

        public override void WriteToDoc(BinaryWriter bw)
        {
            XTools.WriteString(Path, bw);
            XTools.WriteString(Name, bw);
            bw.Write(Visible);
            bw.Write(Selectable);
            bw.Write(LabelOrNot);
            bw.Write(LabelIndex);
        }

        public static XVectorLayer ReadFromDoc(BinaryReader br)
        {
            string path = XTools.ReadString(br);
            string name = XTools.ReadString(br);
            bool visible = br.ReadBoolean();
            bool selectable = br.ReadBoolean();
            bool labelOrNot = br.ReadBoolean();
            int labelIndex = br.ReadInt32();
            XVectorLayer newLayer = (XVectorLayer)XTools.OpenLayer(path);
            if (newLayer == null) return null;
            newLayer.Name = name;
            newLayer.Visible = visible;
            newLayer.Selectable = selectable;
            newLayer.LabelOrNot = labelOrNot;
            newLayer.LabelIndex = labelIndex;
            return newLayer;
        }
    }

    /// <summary>
    /// 栅格图层类
    /// </summary>
    public class XRasterLayer : XLayer
    {
        public Bitmap rasterimage;
        public XRasterLayer(string filename)
        {
            using (StreamReader objReader = new StreamReader(filename))
            {
                //图层名称
                Name = System.IO.Path.GetFileNameWithoutExtension(filename);
                //获得图片文件路径，其与描述文件相同
                FileInfo fi = new FileInfo(filename);
                //打开图片文件
                rasterimage = new Bitmap(fi.DirectoryName + "\\" + objReader.ReadLine());
                //图片范围
                double x1 = double.Parse(objReader.ReadLine());
                double y1 = double.Parse(objReader.ReadLine());
                double x2 = double.Parse(objReader.ReadLine());
                double y2 = double.Parse(objReader.ReadLine());
                Extent = new XExtent(new XVertex(x1, y1), new XVertex(x2, y2));
            }
        }

        public override void draw(Graphics graphics, XView view)
        {
            //根据当前地图可视范围确定图片的显示范围
            XExtent extent = view.CurrentMapExtent;
            int x = (int)((extent.getMinX() - Extent.getMinX()) / Extent.getWidth() * rasterimage.Width);
            int y = (int)((Extent.getMaxY() - extent.getMaxY()) / Extent.getHeight() * rasterimage.Height);
            int width = (int)(extent.getWidth() / Extent.getWidth() * rasterimage.Width);
            int height = (int)(extent.getHeight() / Extent.getHeight() * rasterimage.Height);
            Rectangle sourceRect = new Rectangle(new Point(x, y), new Size(width, height));
            //图片应该出现的当前窗口范围
            Rectangle destRect = view.MapWindowSize;
            graphics.DrawImage(rasterimage, destRect, sourceRect, GraphicsUnit.Pixel);
        }

        public override void WriteToDoc(BinaryWriter bw)
        {
            XTools.WriteString(Path, bw);
            XTools.WriteString(Name, bw);
            bw.Write(Visible);
        }

        public static XRasterLayer ReadFromDoc(BinaryReader br)
        {
            string path = XTools.ReadString(br);
            string name = XTools.ReadString(br);
            bool visible = br.ReadBoolean();
            XRasterLayer newLayer = (XRasterLayer)XTools.OpenLayer(path);
            if (newLayer == null) return null;
            newLayer.Name = name;
            newLayer.Visible = visible;
            return newLayer;
        }
    }

    public class XSelect
    {
        public enum OPERATOR
        {
            Equal, LessThan, MoreThan,
            LessEqual, MoreEqual, Has, NotEqual
        }

        public class SelectResult
        {
            public XFeature feature;
            public double criterion;//排序依据
            public SelectResult(XFeature _feature, double _criterion)
            {
                feature = _feature;
                criterion = _criterion;
            }
        }
        
        public static List<XFeature> ToFeatures(List<SelectResult> selection)
        {
            List<XFeature> features = new List<XFeature>();
            foreach (SelectResult sr in selection)
                features.Add(sr.feature);
            return features;
        }

        /// <summary>
        /// 通过点选位置选择要素，返回与点选位置距离最近且在容差范围内的要素
        /// </summary>
        /// <param name="vertex"></param>
        /// <param name="layers"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public static List<SelectResult> SelectFeaturesByVertex(XVertex vertex, List<XFeature> features, double tolerance)
        {
            List<SelectResult> selection = new List<SelectResult>();
            XExtent extent = new XExtent(vertex.x - tolerance, vertex.x + tolerance,
                vertex.y - tolerance, vertex.y + tolerance);
            foreach (XFeature feature in features)
            {
                if (!extent.IntersectOrNot(feature.spatial.extent)) continue;
                double distance = feature.spatial.Distance(vertex);
                if (distance <= tolerance)
                    selection.Add(new SelectResult(feature, distance));
            }
            selection.Sort((x, y) => x.criterion.CompareTo(y.criterion));
            return selection;
        }

        /// <summary>
        /// 通过框选范围选择要素，返回所有与框选范围相交的要素列表
        /// </summary>
        /// <param name="boxExtent"></param>
        /// <param name="layers"></param>
        /// <returns></returns>
        public static List<SelectResult> SelectFeaturesByExtent(XExtent extent, List<XFeature> features)
        {
            List<SelectResult> selection = new List<SelectResult>();
            foreach (XFeature feature in features)
            {
                if (extent.IntersectOrNot(feature.spatial.extent))
                    selection.Add(new SelectResult(feature, 0));
            }
            return selection;
        }

        public static List<XFeature> SelectFeaturesByAttribute(List<XFeature> features, OPERATOR op, int fieldIndex, object key)
        {
            List<XFeature> fs = new List<XFeature>();
            foreach (XFeature f in features)
            {
                object value = f.getAttribute(fieldIndex);
                if (CompareValue(value, op, key))
                    fs.Add(f);
            }
            return fs;
        }

        public static bool CompareValue(object value, OPERATOR op, object key)
        {

            if (op == OPERATOR.Equal)
                return value.ToString() == key.ToString();
            else if (op == OPERATOR.NotEqual)
                return value.ToString() != key.ToString();
            if (value is bool) return false;
            switch (op)
            {
                case OPERATOR.Has:
                    if (value is string)
                        return value.ToString().IndexOf(key.ToString()) >= 0;
                    else
                        return false;
                case OPERATOR.LessEqual:
                    if (value is string)
                        return ((string)value).CompareTo((string)key) <= 0;
                    else if (value is char)
                        return ((char)value).CompareTo((char)key) <= 0;
                    else
                        return Convert.ToDouble(value).CompareTo(Convert.ToDouble(key)) <= 0;
                case OPERATOR.LessThan:
                    if (value is string)
                        return ((string)value).CompareTo((string)key) < 0;
                    else if (value is char)
                        return ((char)value).CompareTo((char)key) < 0;
                    else
                        return Convert.ToDouble(value).CompareTo(Convert.ToDouble(key)) < 0;
                case OPERATOR.MoreEqual:
                    if (value is string)
                        return ((string)value).CompareTo((string)key) >= 0;
                    else if (value is char)
                        return ((char)value).CompareTo((char)key) >= 0;
                    else
                        return Convert.ToDouble(value).CompareTo(Convert.ToDouble(key)) >= 0;
                case OPERATOR.MoreThan:
                    if (value is string)
                        return ((string)value).CompareTo((string)key) > 0;
                    else if (value is char)
                        return ((char)value).CompareTo((char)key) > 0;
                    else
                        return Convert.ToDouble(value).CompareTo(Convert.ToDouble(key)) > 0;
            }
            return false;
        }
    }









}
