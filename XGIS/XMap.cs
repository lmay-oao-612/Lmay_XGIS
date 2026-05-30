using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using static XGIS.Form1;

namespace XGIS
{
    public partial class XMap : UserControl
    {
        public XDocument Document = new XDocument();
        public XView View { get; private set; }

        private Bitmap backwindow;
        public XExploreActions CurrentMouseAction { get; set; } = XExploreActions.noaction;
        public XMouseTool CurrentMouseTool { get; set; } = XMouseTool.none;

        private Point MouseDownLocation, MouseMovingLocation;
        private List<XVertex> tempDrawingVertices = new List<XVertex>();
        public string CurrentDrawingSubType { get; set; } = "";
        private XVertex firstVertex = null;
        private int measureCount = 0;

        public XVectorLayer CurrentDrawingLayer { get; set; }
        public XFeature HighlightedFeature { get; set; }


        // 定义事件，通知外部（Form2）状态变化或需要更新 UI
        public event Action<string> StatusTextChanged;
        public event Action LayerAdded;
        public event Action<DataTable, Point> FeatureIdentified;
        public event Action ClearIdentifyUI;

        public XMap()
        {
            InitializeComponent();
            InitializeView();

            this.MouseWheel += Form1_MouseWheel;
            this.MouseEnter += (s, e) => this.Focus();
            DoubleBuffered = true; // 启用双缓冲，减少闪烁
        }

        /// <summary>
        /// 初始化视图
        /// </summary>
        public void InitializeView()
        {
            if (View == null)
            {
                View = new XView(new XExtent(new XVertex(0, 0), new XVertex(100, 100)), ClientRectangle);
            }
        }

        /// <summary>
        /// 清楚选择状态
        /// </summary>
        private void ClearSelection()
        {
            if (HighlightedFeature != null)
            {
                HighlightedFeature = null;
            }
            Document.ClearSelection();
            UpdateMap();
        }

        /// <summary>
        /// 更新地图显示
        /// </summary>
        private void UpdateMap()
        {
            if (ClientRectangle.Width * ClientRectangle.Height == 0) return;
            if (View == null) return; // 防御性检查
            View.UpdateMapWindow(ClientRectangle);

            if (backwindow != null) backwindow.Dispose();
            backwindow = new Bitmap(ClientRectangle.Width, ClientRectangle.Height);

            using (Graphics g = Graphics.FromImage(backwindow))
            {
                g.Clear(Color.White);
                Document.DrawLayers(g, View);
            }
            Invalidate();
        }

        /// <summary>
        /// 缩放到全图范围
        /// </summary>
        public void ZoomToFullExtent()
        {
            if (Document != null && Document.Layers.Count > 0 && Document.Extent != null)
            {
                View.Update(Document.Extent, ClientRectangle);
                UpdateMap();
            }
        }

        /// <summary>
        /// 处理绘制事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPaint(object sender, PaintEventArgs e)
        {
            if (backwindow == null) return;

            // 只有在平移时需要特殊处理位移，其他情况都画在 0,0
            if (CurrentMouseAction == XExploreActions.pan)
            {
                e.Graphics.DrawImage(backwindow,
                    MouseMovingLocation.X - MouseDownLocation.X,
                    MouseMovingLocation.Y - MouseDownLocation.Y);
            }
            else
            {
                e.Graphics.DrawImage(backwindow, 0, 0);
            }

            //绘制测距预览线
            if (CurrentMouseTool == XMouseTool.measure && firstVertex != null)
            {
                Point pStart = View.ToScreenPoint(firstVertex);
                Point pEnd = MouseMovingLocation;
                using (Pen rubberPen = new Pen(Color.Blue, 1.5f))
                {
                    rubberPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                    e.Graphics.DrawLine(rubberPen, pStart, pEnd);
                }

                // 建议：实时显示一下当前移动的距离，体验更好
                double curDist = firstVertex.Distance(View.ToMapVertex(pEnd));
                e.Graphics.DrawString($"{curDist:F2}", this.Font, Brushes.Blue, pEnd.X + 10, pEnd.Y + 10);
            }

            // 绘制绘制工具的橡皮筋预览
            if (CurrentMouseTool == XMouseTool.draw && tempDrawingVertices.Count > 0 && CurrentDrawingLayer != null)
            {
                using (Pen drawPen = new Pen(Color.Gray, 1.0f) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash })
                {
                    XVertex currentV = View.ToMapVertex(MouseMovingLocation);
                    Point currentP = MouseMovingLocation;

                    if (CurrentDrawingLayer.ShapeType == SHAPETYPE.Line)
                    {
                        Point startP = View.ToScreenPoint(tempDrawingVertices.Last());
                        e.Graphics.DrawLine(drawPen, startP, currentP);
                    }
                    else if (CurrentDrawingLayer.ShapeType == SHAPETYPE.Polygon)
                    {
                        if (CurrentDrawingSubType == "Triangle")
                        {
                            List<Point> pts = View.ToScreenPoints(tempDrawingVertices);
                            if (pts.Count == 1)
                            {
                                e.Graphics.DrawLine(drawPen, pts[0], currentP);
                            }
                            else if (pts.Count == 2)
                            {
                                e.Graphics.DrawPolygon(drawPen, new Point[] { pts[0], pts[1], currentP });
                            }
                        }
                        else if (CurrentDrawingSubType == "Rectangle")
                        {
                            var rectVertices = XTools.CreateRectangle(tempDrawingVertices[0], currentV);
                            var pts = View.ToScreenPoints(rectVertices);
                            e.Graphics.DrawPolygon(drawPen, pts.ToArray());
                        }
                        else if (CurrentDrawingSubType == "Circle")
                        {
                            var circleVertices = XTools.CreateCircle(tempDrawingVertices[0], currentV);
                            var pts = View.ToScreenPoints(circleVertices);
                            if (pts.Count > 0)
                                e.Graphics.DrawPolygon(drawPen, pts.ToArray());
                        }
                    }
                }
            }

            if (CurrentMouseAction == XExploreActions.zoominbybox || CurrentMouseAction == XExploreActions.identifybybox)
            {
                int x = Math.Min(MouseDownLocation.X, MouseMovingLocation.X);
                int y = Math.Min(MouseDownLocation.Y, MouseMovingLocation.Y);
                int width = Math.Abs(MouseDownLocation.X - MouseMovingLocation.X);
                int height = Math.Abs(MouseDownLocation.Y - MouseMovingLocation.Y);
                using (Pen boxPen = new Pen(Color.Red, 2))
                {
                    e.Graphics.DrawRectangle(boxPen, x, y, width, height);
                }
            }
        }

        private void Form_SizeChanged(object sender, EventArgs e)
        {
            UpdateMap();
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            MouseDownLocation = e.Location;
            MouseMovingLocation = e.Location;


            if (CurrentMouseTool == XMouseTool.identify && Control.ModifierKeys == Keys.Control)
            {
                CurrentMouseAction = XExploreActions.identifybybox;
            }
            else if (Control.ModifierKeys == Keys.Shift)
            {
                CurrentMouseAction = XExploreActions.zoominbybox;
            }
            else
            {
                CurrentMouseAction = XExploreActions.pan;
            }
            StatusTextChanged?.Invoke($"Action: {CurrentMouseAction}");
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            MouseMovingLocation = e.Location;
            if ((CurrentMouseTool == XMouseTool.measure && firstVertex != null) ||
                (CurrentMouseTool == XMouseTool.draw && tempDrawingVertices.Count > 0) ||
                CurrentMouseAction != XExploreActions.noaction)
            {
                Invalidate();
            }
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            XVertex mouseMapV = View.ToMapVertex(e.Location);

            if (CurrentMouseAction == XExploreActions.identifybybox)
            {
                IdentifyLogic(mouseMapV, e.Location);
            }
            else if (CurrentMouseAction == XExploreActions.pan)
            {
                // 只有当鼠标真的移动了才平移，否则视为普通点击
                if (MouseDownLocation != e.Location)
                {
                    XVertex v1 = View.ToMapVertex(MouseDownLocation);
                    View.OffsetCenter(v1, mouseMapV);
                }
                else if (CurrentMouseTool == XMouseTool.identify)
                {
                    // 如果没动鼠标，且是查看工具，则执行单点选择
                    IdentifyLogic(mouseMapV, e.Location);
                }
            }
            // 3. 拉框放大
            else if (CurrentMouseAction == XExploreActions.zoominbybox)
            {
                XVertex v1 = View.ToMapVertex(MouseDownLocation);
                View.Update(new XExtent(v1, mouseMapV), ClientRectangle);
            }

            CurrentMouseAction = XExploreActions.noaction;
            UpdateMap();
            StatusTextChanged?.Invoke($"Action: {CurrentMouseAction}");
        }

        public void ZoomTo(XVertex center, XExtent extent, XFeature feature)
        {
            ClearSelection();

            this.HighlightedFeature = feature;
            feature.IsSelected = true;

            double w = extent.getWidth();
            double h = extent.getHeight();


            // 如果是点要素，w 和 h 会是 0。我们需要给定一个合理的观察范围
            if (w == 0) w = 0.05;
            if (h == 0) h = 0.05;

            XExtent bufferedExtent = new XExtent(
                new XVertex(center.x - w * 1.5, center.y - h * 1.5),
                new XVertex(center.x + w * 1.5, center.y + h * 1.5)
            );


            View.Update(bufferedExtent, ClientRectangle);
            UpdateMap();
        }


        private void Form1_MouseWheel(object sender, MouseEventArgs e)
        {
            double factor = (e.Delta > 0) ? 0.5 : 2.0; // 0.5代表放大（范围缩小），2.0代表缩小

            if (Control.ModifierKeys == Keys.Shift)
            {
                // 以鼠标为中心缩放
                View.ZoomAtPoint(e.Location, factor);
            }
            else
            {
                // 中心缩放
                View.ChangeView(e.Delta > 0 ? XExploreActions.zoomin : XExploreActions.zoomout);
            }
            UpdateMap();
        }

        private void Form1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                // 如果在测量中途右键，清空第一点，不生成图层
                if (CurrentMouseTool == XMouseTool.measure && firstVertex != null)
                {
                    firstVertex = null;
                    Invalidate(); // 清除橡皮筋预览线
                    return;
                }

                // 退出工具模式
                CurrentMouseTool = XMouseTool.none;
                this.Cursor = Cursors.Default;
                Invalidate();
                StatusTextChanged?.Invoke($"Action: {CurrentMouseAction}");
            }

            // 左键点击：执行原有逻辑
            if (e.Button == MouseButtons.Left)
            {
                XVertex mouseMapV = View.ToMapVertex(e.Location);
                switch (CurrentMouseTool)
                {
                    case XMouseTool.measure:
                        MeasureLogic(mouseMapV);
                        break;
                    case XMouseTool.draw:
                        DrawLogic(mouseMapV);
                        break;
                }
            }
        }


        // --------------测量逻辑---------------
        private void MeasureLogic(XVertex currentVertex)
        {
            if (firstVertex == null)
            {
                //记录起点
                firstVertex = currentVertex;
            }
            else
            {
                //计算并创建新图层
                measureCount++;
                double length = firstVertex.Distance(currentVertex);

                //创建新图层
                string newLayerName = $"测量_{measureCount} ({length:F2})";
                XVectorLayer newMeasureLayer = new XVectorLayer(newLayerName, SHAPETYPE.Line);

                //构造线要素
                XLine newLine = new XLine(new List<XVertex> { firstVertex, currentVertex });
                newLine.ShowNodes = true;
                XAttribute attr = new XAttribute();
                attr.AddValue(newLayerName);

                XFeature feature = new XFeature(newLine, attr);
                newMeasureLayer.AddFeature(feature);

                Document.AddLayer(newMeasureLayer);
                LayerAdded?.Invoke();

                //重置状态并刷新地图
                firstVertex = null;
                UpdateMap();
            }
        }
        // ---------------查看要素逻辑------------------
        private void IdentifyLogic(XVertex mouseMapV, Point mouseScreenP)
        {
            ClearSelection(); // 清理旧选择

            List<XFeature> allfeatures = new List<XFeature>();
            List<XFeature> selectedFeatures = new List<XFeature>();

            foreach (var ly in Document.Layers)
            {
                if (ly.Visible && ly is XVectorLayer vLayer) allfeatures.AddRange(vLayer.Features);
            }

            if (CurrentMouseAction == XExploreActions.identifybybox)
            {
                XVertex v1 = View.ToMapVertex(MouseDownLocation);
                XExtent boxExtent = new XExtent(v1, mouseMapV);
                var selectResults = XSelect.SelectFeaturesByExtent(boxExtent, allfeatures);
                selectedFeatures = XSelect.ToFeatures(selectResults);
            }
            else
            {
                // 单点点选
                var hitResults = XSelect.SelectFeaturesByVertex(mouseMapV, allfeatures, View.ToMapDistance(5));
                if (hitResults != null && hitResults.Count > 0)
                {
                    var hit = hitResults[0].feature;
                    if (hit != null) selectedFeatures.Add(hit);
                }
            }

            // 渲染结果
            if (selectedFeatures.Count > 0)
            {
                foreach (var f in selectedFeatures) f.IsSelected = true;


                // 绑定第一个要素的所有属性
                XFeature first = selectedFeatures[0];
                XVectorLayer ownerLayer = null;
                foreach (var ly in Document.Layers)
                {
                    if (ly is XVectorLayer vLayer && vLayer.Features.Contains(first))
                    {
                        ownerLayer = vLayer;
                        break;
                    }
                }
                DataTable dt = new DataTable();
                dt.Columns.Add("属性名");
                dt.Columns.Add("值");

                for (int i = 0; i < ownerLayer.Fields.Count; i++)
                {
                    string fieldName = ownerLayer.Fields[i].name;
                    object val = first.attribute.GetValue(i);

                    if (val.ToString() == "No Attr") continue;
                    dt.Rows.Add(fieldName, val.ToString());
                }

                dgvAttributes.DataSource = dt;

                // 3. 计算浮窗位置，防止超出屏幕右侧或下方
                int posX = mouseScreenP.X + 15;
                int posY = mouseScreenP.Y + 15;
                if (posX + pnlInfo.Width > this.ClientSize.Width) posX = mouseScreenP.X - pnlInfo.Width - 15;
                if (posY + pnlInfo.Height > this.ClientSize.Height) posY = mouseScreenP.Y - pnlInfo.Height - 15;

                pnlInfo.Location = new Point(posX, posY);
                pnlInfo.Visible = true;
                pnlInfo.BringToFront();
            }
            else
            {
                pnlInfo.Visible = false;
            }

            UpdateMap();
        }
        // --- 核心绘制方法生成 ---

        public void DrawPoint()
        {
            if (!PrepareDrawingLayer(SHAPETYPE.Point, "点")) return;
            CurrentMouseTool = XMouseTool.draw;
            this.Cursor = Cursors.Cross;
            StatusTextChanged?.Invoke($"Action: {CurrentMouseAction}");
        }

        public void DrawLine()
        {
            if (!PrepareDrawingLayer(SHAPETYPE.Line, "线")) return;
            tempDrawingVertices.Clear();
            CurrentMouseTool = XMouseTool.draw;
            this.Cursor = Cursors.Cross;
            StatusTextChanged?.Invoke($"Action: {CurrentMouseAction}");
        }

        public void DrawPolygon()
        {
            string choice = Prompt.ShowDialog("请选择类型: 1-三角形, 2-矩形, 3-圆形", "选择多边形形状");
            if (choice == "1") CurrentDrawingSubType = "Triangle";
            else if (choice == "2") CurrentDrawingSubType = "Rectangle";
            else if (choice == "3") CurrentDrawingSubType = "Circle";
            else return;

            if (!PrepareDrawingLayer(SHAPETYPE.Polygon, "面")) return;
            tempDrawingVertices.Clear();
            CurrentMouseTool = XMouseTool.draw;
            this.Cursor = Cursors.Cross;
            StatusTextChanged?.Invoke($"Action: {CurrentMouseAction}");
        }

        // 绘制内部逻辑
        private void DrawLogic(XVertex v)
        {
            if (CurrentDrawingLayer == null) return;
            tempDrawingVertices.Add(v);

            bool isFinished = false;
            XSpatial geometry = null;

            if (CurrentDrawingLayer.ShapeType == SHAPETYPE.Point)
            {
                geometry = new XPoint(v);
                isFinished = true;
            }
            else if (CurrentDrawingLayer.ShapeType == SHAPETYPE.Line)
            {
                if (tempDrawingVertices.Count == 2)
                {
                    geometry = new XLine(new List<XVertex>(tempDrawingVertices));
                    isFinished = true;
                }
            }
            else if (CurrentDrawingLayer.ShapeType == SHAPETYPE.Polygon)
            {
                if (CurrentDrawingSubType == "Triangle" && tempDrawingVertices.Count == 3)
                {
                    geometry = new XPolygon(new List<XVertex>(tempDrawingVertices));
                    isFinished = true;
                }
                else if (CurrentDrawingSubType == "Rectangle" && tempDrawingVertices.Count == 2)
                {
                    geometry = new XPolygon(XTools.CreateRectangle(tempDrawingVertices[0], tempDrawingVertices[1]));
                    isFinished = true;
                }
                else if (CurrentDrawingSubType == "Circle" && tempDrawingVertices.Count == 2)
                {
                    geometry = new XPolygon(XTools.CreateCircle(tempDrawingVertices[0], tempDrawingVertices[1]));
                    isFinished = true;
                }
            }
            // 如果完成了一个要素的绘制，就创建要素并加入图层
            if (isFinished && geometry != null)
            {
                XFeature feature = new XFeature(geometry, new XAttribute());
                feature.attribute.AddValue("新建要素");
                CurrentDrawingLayer.AddFeature(feature);
                tempDrawingVertices.Clear();
                UpdateMap();
            }
        }

        private void 矢量数据ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Shapefile|*.shp";
            dialog.Multiselect = true; // 允许一次选多个文件

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                foreach (string file in dialog.FileNames)
                {
                    int count = XShapefile.GetFeatureCount(file);
                    XVectorLayer newLayer = XShapefile.ReadShapefile(file);
                    Document.AddLayer(newLayer);
                    LayerAdded?.Invoke();

                    MessageBox.Show($"图层: {newLayer.Name}\n要素数量: {(count != -1 ? count.ToString() : "未知")}");
                }

                // 缩放到所有图层的共同最大范围
                ZoomToFullExtent();
                UpdateMap();
            }
        }

        private bool PrepareDrawingLayer(SHAPETYPE type, string typeName)
        {
            string layerName = Prompt.ShowDialog($"请输入新{typeName}图层名称:", "新建绘图图层");
            if (string.IsNullOrWhiteSpace(layerName)) return false;

            if (Document.Layers.Any(ly => ly.Name == layerName))
            {
                MessageBox.Show("图层名称已存在，请重新输入。");
                return false;
            }
            CurrentDrawingLayer = new XVectorLayer(layerName, type);
            Document.AddLayer(CurrentDrawingLayer);
            LayerAdded?.Invoke();
            return true;
        }
    }
}
