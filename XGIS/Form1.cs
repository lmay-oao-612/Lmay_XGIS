
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;

namespace XGIS
{
    public partial class Form1 : Form
    {
        XDocument document = new XDocument();
        XView view;
        Bitmap backwindow;
        XExploreActions currentMouseAction = XExploreActions.noaction;
        XMouseTool currentMouseTool = XMouseTool.none;

        Point MouseDownLocation, MouseMovingLocation;// 鼠标操作相关的变量
        List<XVertex> tempDrawingVertices = new List<XVertex>(); // 绘制过程中的临时点
        string currentDrawingSubType = ""; // 多边形子类型：Triangle, Rectangle, Circle
        XVertex firstVertex = null; // 测距工具的第一个点
        int randomLayerCount = 0; // 随机图层的计数器
        XFeature highlightedFeature = null; // 当前被高亮显示的要素
        XVectorLayer currentDrawingLayer = null;// 当前正在绘制的图层
        public Form1()
        {
            InitializeComponent(); // 保持控件创建
            view = new XView(new XExtent(new XVertex(0, 0), new XVertex(100, 100)), ClientRectangle);
            this.MouseWheel += Form1_MouseWheel;
            this.MouseEnter += (s, e) => this.Focus();
            this.SizeChanged += Form1_SizeChanged;
            InitialTV();
            // 双缓冲设置，减少闪烁
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer |
                  ControlStyles.AllPaintingInWmPaint |
                  ControlStyles.UserPaint, true);
            this.UpdateStyles();
        }

        /// <summary>
        /// 初始化图层树状列表（tvLayer）
        /// </summary>
        private void InitialTV()
        {
            tvLayer.Nodes.Clear();
            foreach (var layer in document.Layers)
            {
                TreeNode node = tvLayer.Nodes.Add(layer.Name);
                node.Checked = layer.Visible;
            }
        }

        /// <summary>
        /// 清除当前选择的要素（如果有），并刷新地图。
        /// </summary>
        private void ClearSelection()
        {
            if (highlightedFeature != null)
            {
                highlightedFeature = null;
            }
            document.ClearSelection();
            UpdateMap();
        }

        /// <summary>
        /// 交换图层位置的通用方法
        /// </summary>
        private void MoveLayer(int oldIndex, int newIndex)
        {

            document.AdjustLayerOrder(document.Layers[oldIndex], newIndex - oldIndex);

            RefreshLayerListUI(newIndex);

            UpdateMap();
        }

        /// <summary>
        /// 刷新图层列表的 UI
        /// </summary>
        /// <param name="targetIndex"></param>
        private void RefreshLayerListUI(int targetIndex)
        {
            tvLayer.Nodes.Clear();
            foreach (var ly in document.Layers)
            {
                TreeNode node = tvLayer.Nodes.Add(ly.Name);
                node.Checked = ly.Visible;
            }
            // 选中移动后的节点
            if (targetIndex >= 0 && targetIndex < tvLayer.Nodes.Count)
                tvLayer.SelectedNode = tvLayer.Nodes[targetIndex];
        }

        /// <summary>
        /// 更新状态栏文本，显示当前的鼠标工具和动作状态
        /// </summary>
        private void UpdateStatusLabel()
        {
            string modeText = "浏览";
            switch (currentMouseTool)
            {
                case XMouseTool.identify:
                    modeText = "属性查看";
                    break;
                case XMouseTool.measure:
                    modeText = "距离测量";
                    break;
                case XMouseTool.none:
                    modeText = "浏览";
                    break;
                case XMouseTool.draw:
                    modeText = "绘制";
                    break;

            }

            // 如果有临时动作（如平移或拉框），也可以显示
            if (currentMouseAction == XExploreActions.pan) modeText += " (平移中...)";
            else if (currentMouseAction == XExploreActions.zoominbybox) modeText += " (拉框放大中...)";

            SShowMode.Text = $"{modeText}";
        }
        private void bCreateRandomObjects_Click(object sender, EventArgs e)
        {
            Random rand = new Random();
            randomLayerCount++; // 计数器自增

            //创建一个新的点图层，命名为 random + 序号
            string newLayerName = "random" + randomLayerCount;
            XVectorLayer newLayer = new XVectorLayer(newLayerName, SHAPETYPE.Point);

            //获取当前视图范围
            XExtent currentExtent = view.CurrentMapExtent;
            double minX = currentExtent.bottomleft.x;
            double maxX = currentExtent.upright.x;
            double minY = currentExtent.bottomleft.y;
            double maxY = currentExtent.upright.y;

            //生成 100 个随机点并加入新图层
            for (int i = 0; i < 100; i++)
            {
                double x = minX + rand.NextDouble() * (maxX - minX);
                double y = minY + rand.NextDouble() * (maxY - minY);

                XPoint point = new XPoint(new XVertex(x, y));
                XAttribute attribute = new XAttribute();
                attribute.AddValue("ID: " + i);

                XFeature feature = new XFeature(point, attribute);
                newLayer.AddFeature(feature);
            }

            //将新图层添加到总图层列表中
            document.AddLayer(newLayer);

            //同步更新树状列表
            TreeNode node = tvLayer.Nodes.Add(newLayer.Name);
            node.Checked = true;
            tvLayer.SelectedNode = node;

            //重新绘图
            UpdateMap();
            MessageBox.Show("已在当前视图范围内生成100个随机点！");
        }

        private void UpdateMap()
        {
            if (ClientRectangle.Width * ClientRectangle.Height == 0) return;
            if (view == null) return; // 防御性检查
            view.UpdateMapWindow(ClientRectangle);

            if (backwindow != null) backwindow.Dispose();
            backwindow = new Bitmap(ClientRectangle.Width, ClientRectangle.Height);

            using (Graphics g = Graphics.FromImage(backwindow))
            {
                g.Clear(Color.White);
                document.DrawLayers(g, view);
            }
            Invalidate();
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            if (backwindow == null) return;

            // 只有在平移时需要特殊处理位移，其他情况都画在 0,0
            if (currentMouseAction == XExploreActions.pan)
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
            if (currentMouseTool == XMouseTool.measure && firstVertex != null)
            {
                Point pStart = view.ToScreenPoint(firstVertex);
                Point pEnd = MouseMovingLocation;
                using (Pen rubberPen = new Pen(Color.Blue, 1.5f))
                {
                    rubberPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                    e.Graphics.DrawLine(rubberPen, pStart, pEnd);
                }

                // 建议：实时显示一下当前移动的距离，体验更好
                double curDist = firstVertex.Distance(view.ToMapVertex(pEnd));
                e.Graphics.DrawString($"{curDist:F2}", this.Font, Brushes.Blue, pEnd.X + 10, pEnd.Y + 10);
            }

            // 绘制绘制工具的橡皮筋预览
            if (currentMouseTool == XMouseTool.draw && tempDrawingVertices.Count > 0 && currentDrawingLayer != null)
            {
                using (Pen drawPen = new Pen(Color.Gray, 1.0f) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash })
                {
                    XVertex currentV = view.ToMapVertex(MouseMovingLocation);
                    Point currentP = MouseMovingLocation;

                    if (currentDrawingLayer.ShapeType == SHAPETYPE.Line)
                    {
                        Point startP = view.ToScreenPoint(tempDrawingVertices.Last());
                        e.Graphics.DrawLine(drawPen, startP, currentP);
                    }
                    else if (currentDrawingLayer.ShapeType == SHAPETYPE.Polygon)
                    {
                        if (currentDrawingSubType == "Triangle")
                        {
                            List<Point> pts = view.ToScreenPoints(tempDrawingVertices);
                            if (pts.Count == 1)
                            {
                                e.Graphics.DrawLine(drawPen, pts[0], currentP);
                            }
                            else if (pts.Count == 2)
                            {
                                e.Graphics.DrawPolygon(drawPen, new Point[] { pts[0], pts[1], currentP });
                            }
                        }
                        else if (currentDrawingSubType == "Rectangle")
                        {
                            var rectVertices = XTools.CreateRectangle(tempDrawingVertices[0], currentV);
                            var pts = view.ToScreenPoints(rectVertices);
                            e.Graphics.DrawPolygon(drawPen, pts.ToArray());
                        }
                        else if (currentDrawingSubType == "Circle")
                        {
                            var circleVertices = XTools.CreateCircle(tempDrawingVertices[0], currentV);
                            var pts = view.ToScreenPoints(circleVertices);
                            if (pts.Count > 0)
                                e.Graphics.DrawPolygon(drawPen, pts.ToArray());
                        }
                    }
                }
            }

            if (currentMouseAction == XExploreActions.zoominbybox || currentMouseAction == XExploreActions.identifybybox)
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

        /*
        private void MapExploreButtonClick(object sender, EventArgs e)
        {
            XExploreActions action = XExploreActions.zoomin;
            if ((Button)sender == bZoomIn) action = XExploreActions.zoomin;
            else if ((Button)sender == bZoomOut) action = XExploreActions.zoomout;
            else if ((Button)sender == bMoveUp) action = XExploreActions.moveup;
            else if ((Button)sender == bMoveDown) action = XExploreActions.movedown;
            else if ((Button)sender == bMoveLeft) action = XExploreActions.moveleft;
            else if ((Button)sender == bMoveRight) action = XExploreActions.moveright;
            view.ChangeView(action);
            UpdateMap();
        }
        */

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            UpdateMap();
        }

        private void bFullExtent_Click(object sender, EventArgs e)
        {
            ZoomToFullExtent();
            UpdateMap();
        }

        private void tvLayer_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (e.Action == TreeViewAction.Unknown) return; // 过滤代码触发的检查

            int index = e.Node.Index;
            if (index >= 0 && index < document.Layers.Count)
            {
                document.Layers[index].Visible = e.Node.Checked;
                UpdateMap();
            }
        }

        /// <summary>
        /// 按下鼠标某个按钮时触发的事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            MouseDownLocation = e.Location;
            MouseMovingLocation = e.Location; 


            if (currentMouseTool == XMouseTool.identify && Control.ModifierKeys == Keys.Control)
            {
                currentMouseAction = XExploreActions.identifybybox;
            }
            else if (Control.ModifierKeys == Keys.Shift)
            {
                currentMouseAction = XExploreActions.zoominbybox;
            }
            else
            {
                currentMouseAction = XExploreActions.pan;
            }
            UpdateStatusLabel();
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            MouseMovingLocation = e.Location;
            if ((currentMouseTool == XMouseTool.measure && firstVertex != null) ||
                (currentMouseTool == XMouseTool.draw && tempDrawingVertices.Count > 0) ||
                currentMouseAction != XExploreActions.noaction)
            {
                Invalidate();
            }
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            XVertex mouseMapV = view.ToMapVertex(e.Location);

            if (currentMouseAction == XExploreActions.identifybybox)
            {
                IdentifyLogic(mouseMapV, e.Location);
            }
            else if (currentMouseAction == XExploreActions.pan)
            {
                // 只有当鼠标真的移动了才平移，否则视为普通点击
                if (MouseDownLocation != e.Location)
                {
                    XVertex v1 = view.ToMapVertex(MouseDownLocation);
                    view.OffsetCenter(v1, mouseMapV);
                }
                else if (currentMouseTool == XMouseTool.identify)
                {
                    // 如果没动鼠标，且是查看工具，则执行单点选择
                    IdentifyLogic(mouseMapV, e.Location);
                }
            }
            // 3. 拉框放大
            else if (currentMouseAction == XExploreActions.zoominbybox)
            {
                XVertex v1 = view.ToMapVertex(MouseDownLocation);
                view.Update(new XExtent(v1, mouseMapV), ClientRectangle);
            }

            currentMouseAction = XExploreActions.noaction;
            UpdateMap();
            UpdateStatusLabel();
        }

        private void bReadShp_Click(object sender, EventArgs e)
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
                    document.AddLayer(newLayer);
                    
                    TreeNode node = tvLayer.Nodes.Add(newLayer.Name);
                    node.Checked = true;

                    MessageBox.Show($"图层: {newLayer.Name}\n要素数量: {(count != -1 ? count.ToString() : "未知")}");
                }

                // 缩放到所有图层的共同最大范围
                ZoomToFullExtent();
                UpdateMap();
            }
        }
        
        private void bReadRaster_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "栅格描述文件|*.rst";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                XLayer newLayer = XTools.OpenLayer(dialog.FileName);
                if (newLayer != null)
                {
                    document.AddLayer(newLayer);
                    TreeNode node = tvLayer.Nodes.Add(newLayer.Name);
                    node.Checked = true;
                    tvLayer.SelectedNode = node;

                    ZoomToFullExtent();
                    UpdateMap();
                }
            }
        }

        public void ZoomTo(XVertex center, XExtent extent, XFeature feature)
        {
            ClearSelection();

            this.highlightedFeature = feature;
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


            view.Update(bufferedExtent, ClientRectangle);
            UpdateMap();
        }

        private void ZoomToFullExtent()
        {
            if (document.Layers.Count > 0 && document.Extent != null) 
                view.Update(document.Extent, ClientRectangle);
        }

        private void Form1_MouseWheel(object sender, MouseEventArgs e)
        {
            double factor = (e.Delta > 0) ? 0.5 : 2.0; // 0.5代表放大（范围缩小），2.0代表缩小

            if (Control.ModifierKeys == Keys.Shift)
            {
                // 以鼠标为中心缩放
                view.ZoomAtPoint(e.Location, factor);
            }
            else
            {
                // 中心缩放
                view.ChangeView(e.Delta > 0 ? XExploreActions.zoomin : XExploreActions.zoomout);
            }
            UpdateMap();
        }


        private void bIdentify_Click(object sender, EventArgs e)
        {
            currentMouseTool = XMouseTool.identify;
            this.Cursor = Cursors.Help; // 切换光标形状
            MessageBox.Show("已切换至：要素查看模式");
            UpdateStatusLabel();
        }

        private void bMeasure_Click(object sender, EventArgs e)
        {
            currentMouseTool = XMouseTool.measure;
            firstVertex = null; // 重置测距起点
            this.Cursor = Cursors.Cross; // 切换为十字光标
            MessageBox.Show("已切换至：距离测量模式\n请在地图上点击两点。");
            UpdateStatusLabel();
        }

        private void Form1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                // 如果在测量中途右键，清空第一点，不生成图层
                if (currentMouseTool == XMouseTool.measure && firstVertex != null)
                {
                    firstVertex = null;
                    Invalidate(); // 清除橡皮筋预览线
                    return;
                }

                // 退出工具模式
                currentMouseTool = XMouseTool.none;
                this.Cursor = Cursors.Default;
                Invalidate();
                UpdateStatusLabel();
            }

            // 左键点击：执行原有逻辑
            if (e.Button == MouseButtons.Left)
            {
                XVertex mouseMapV = view.ToMapVertex(e.Location);
                switch (currentMouseTool)
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

        private int measureCount = 0; // 成员变量，用于给测量图层编号

        // 测量逻辑
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

                //将新图层加入全局集合
                document.AddLayer(newMeasureLayer);

                //同步更新树状列表
                TreeNode node = tvLayer.Nodes.Add(newMeasureLayer.Name);
                node.Checked = true;
                tvLayer.SelectedNode = node;

                //重置状态并刷新地图
                firstVertex = null;
                UpdateMap();
            }
        }

        private void tvLayer_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                TreeNode node = tvLayer.GetNodeAt(e.Location);
                if (node != null)
                {
                    tvLayer.SelectedNode = node;
                    layerMenu.Show(tvLayer, e.Location);
                }
            }
        }

        private void TremoveLayer_Click(object sender, EventArgs e)
        {
            if (tvLayer.SelectedNode == null) return;
            int index = tvLayer.SelectedNode.Index;

            var result = MessageBox.Show($"确定要移除图层 [{document.Layers[index].Name}] 吗？", "提示", MessageBoxButtons.YesNo);

            if (result == DialogResult.Yes)
            {

                document.DeleteLayer(document.Layers[index]);
                tvLayer.Nodes.RemoveAt(index);

                UpdateMap(); // 重新绘图
            }
        }

        private void TshowAttribute_Click(object sender, EventArgs e)
        {
            if (tvLayer.SelectedNode == null) return;
            int index = tvLayer.SelectedNode.Index;

            FormAttribute formAttribute = new FormAttribute((XVectorLayer)document.Layers[index], this);
            formAttribute.Show();
        }

        // 查看要素逻辑
        private void IdentifyLogic(XVertex mouseMapV, Point mouseScreenP)
        {
            ClearSelection(); // 清理旧选择

            List<XFeature> allfeatures = new List<XFeature>();
            List<XFeature> selectedFeatures = new List<XFeature>();

            foreach(var ly in document.Layers)
            {
                if (ly.Visible && ly is XVectorLayer vLayer) allfeatures.AddRange(vLayer.Features);
            }

            if (currentMouseAction == XExploreActions.identifybybox)
            {
                XVertex v1 = view.ToMapVertex(MouseDownLocation);
                XExtent boxExtent = new XExtent(v1, mouseMapV);
                var selectResults = XSelect.SelectFeaturesByExtent(boxExtent, allfeatures);
                selectedFeatures = XSelect.ToFeatures(selectResults);
            }
            else
            {
                // 单点点选
                var hitResults = XSelect.SelectFeaturesByVertex(mouseMapV, allfeatures, view.ToMapDistance(5));
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
                foreach (var ly in document.Layers)
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

        private void TmoveUp_Click(object sender, EventArgs e)
        {
            if (tvLayer.SelectedNode == null) return;
            int index = tvLayer.SelectedNode.Index;
            // 如果没有选中，或者已经在最顶部（索引为0），则无法上移
            if (index <= 0 || index >= document.Layers.Count) return;

            MoveLayer(index, index - 1);
        }

        private void TmoveDown_Click(object sender, EventArgs e)
        {
            if (tvLayer.SelectedNode == null) return;
            int index = tvLayer.SelectedNode.Index;
            // 如果没有选中，或者已经在最底部，则无法下移
            if (index < 0 || index >= document.Layers.Count - 1) return;

            MoveLayer(index, index + 1);
        }

        private void 清除所有状态ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ClearSelection();
            dgvAttributes.DataSource = null;
            pnlInfo.Visible = false;
        }

        public void VerifyAreaAlgorithm_Click(object sender, EventArgs e)
        {
            try
            {
                StringBuilder sb = new StringBuilder();

                // 1. 正方形测试
                List<XVertex> square = new List<XVertex>
                {
                    new XVertex(0, 0), new XVertex(0, 1), new XVertex(1, 1), new XVertex(1, 0)
                };
                double areaSquare = Math.Abs(XTools.CalculateArea(square));
                sb.AppendLine($"正方形: 期望 1.0, 实际 {areaSquare:F5}");

                // 2. 三角形测试
                List<XVertex> triangle = new List<XVertex>
                {
                    new XVertex(0, 0), new XVertex(4, 0), new XVertex(0, 3)
                };
                double areaTriangle = Math.Abs(XTools.CalculateArea(triangle));
                sb.AppendLine($"三角形: 期望 6.0, 实际 {areaTriangle:F5}");

                // 3. 圆形测试
                List<XVertex> circleApprox = new List<XVertex>();
                for (int i = 0; i < 360; i++)
                {
                    double angle = i * Math.PI / 180.0;
                    circleApprox.Add(new XVertex(Math.Cos(angle), Math.Sin(angle)));
                }
                double areaCircle = Math.Abs(XTools.CalculateArea(circleApprox));
                sb.AppendLine($"圆形 (PI): 期望 {Math.PI:F6}, 实际 {areaCircle:F6}");

                // 弹出结果
                MessageBox.Show(sb.ToString(), "算法验证");
            }
            catch (Exception ex)
            {
                MessageBox.Show("计算过程出错：" + ex.Message);
            }
        }

        private void TDrawPoint_Click(object sender, EventArgs e)
        {
            DrawPoint();
        }

        private void TDrawLine_Click(object sender, EventArgs e)
        {
            DrawLine();
        }

        private void TDrawPolygon_Click(object sender, EventArgs e)
        {
            DrawPolygon();
        }

        // --- 核心绘制方法生成 ---

        public void DrawPoint()
        {
            if (!PrepareDrawingLayer(SHAPETYPE.Point, "点")) return;
            currentMouseTool = XMouseTool.draw;
            this.Cursor = Cursors.Cross;
            UpdateStatusLabel();
        }

        public void DrawLine()
        {
            if (!PrepareDrawingLayer(SHAPETYPE.Line, "线")) return;
            tempDrawingVertices.Clear();
            currentMouseTool = XMouseTool.draw;
            this.Cursor = Cursors.Cross;
            UpdateStatusLabel();
        }

        public void DrawPolygon()
        {
            string choice = Prompt.ShowDialog("请选择类型: 1-三角形, 2-矩形, 3-圆形", "选择多边形形状");
            if (choice == "1") currentDrawingSubType = "Triangle";
            else if (choice == "2") currentDrawingSubType = "Rectangle";
            else if (choice == "3") currentDrawingSubType = "Circle";
            else return;

            if (!PrepareDrawingLayer(SHAPETYPE.Polygon, "面")) return;
            tempDrawingVertices.Clear();
            currentMouseTool = XMouseTool.draw;
            this.Cursor = Cursors.Cross;
            UpdateStatusLabel();
        }

        // 绘制内部逻辑
        private void DrawLogic(XVertex v)
        {
            if (currentDrawingLayer == null) return;
            tempDrawingVertices.Add(v);

            bool isFinished = false;
            XSpatial geometry = null;

            if (currentDrawingLayer.ShapeType == SHAPETYPE.Point)
            {
                geometry = new XPoint(v);
                isFinished = true;
            }
            else if (currentDrawingLayer.ShapeType == SHAPETYPE.Line)
            {
                if (tempDrawingVertices.Count == 2)
                {
                    geometry = new XLine(new List<XVertex>(tempDrawingVertices));
                    isFinished = true;
                }
            }
            else if (currentDrawingLayer.ShapeType == SHAPETYPE.Polygon)
            {
                if (currentDrawingSubType == "Triangle" && tempDrawingVertices.Count == 3)
                {
                    geometry = new XPolygon(new List<XVertex>(tempDrawingVertices));
                    isFinished = true;
                }
                else if (currentDrawingSubType == "Rectangle" && tempDrawingVertices.Count == 2)
                {
                    geometry = new XPolygon(XTools.CreateRectangle(tempDrawingVertices[0], tempDrawingVertices[1]));
                    isFinished = true;
                }
                else if (currentDrawingSubType == "Circle" && tempDrawingVertices.Count == 2)
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
                currentDrawingLayer.AddFeature(feature);
                tempDrawingVertices.Clear();
                UpdateMap();
            }
        }

        private bool PrepareDrawingLayer(SHAPETYPE type, string typeName)
        {
            // 1. 弹出对话框获取名称
            string layerName = Prompt.ShowDialog($"请输入新{typeName}图层名称:", "新建绘图图层");
            if (string.IsNullOrWhiteSpace(layerName)) return false;

            if (document.Layers.Any(ly => ly.Name == layerName)) 
            {
                MessageBox.Show("图层名称已存在，请重新输入。");
                return false;
            }
            currentDrawingLayer = new XVectorLayer(layerName, type);
            document.AddLayer(currentDrawingLayer);
            
            TreeNode node = tvLayer.Nodes.Add(currentDrawingLayer.Name);
            node.Checked = true;
            tvLayer.SelectedNode = node;
            return true;
        }

        private void RunPerformanceTest()
        {
            int testCount = 1000;
            string baseName = "PerformanceTestLayer";

            // -------测试旧的 List 遍历逻辑 (模拟 O(N) 查找)------------
            List<string> listNames = new List<string>();
            Stopwatch swList = Stopwatch.StartNew();
            for (int i = 0; i < testCount; i++)
            {
                string name = baseName;
                int count = 0;
                // 模拟原先的 UniqueName 逻辑：遍历 List 检查是否存在
                // 由于每次添加都是重名，这里产生的冲突会导致 while 循环运行 i 次，
                // 每次 Any() 又是 O(i) 的遍历，因此总复杂度是 O(N^3)
                while (listNames.Any(n => n == name))
                {
                    count++;
                    name = baseName + "_" + count;
                }
                listNames.Add(name);
            }
            swList.Stop();

            // --------测试当前的 HashSet 优化逻辑-------------
            document = new XDocument(); // 重置当前文档进行测试
            Stopwatch swHash = Stopwatch.StartNew();
            for (int i = 0; i < testCount; i++)
            {
                // AddLayer 内部使用的是 HashSet.Contains (O(1))
                // 面对重名冲突，while 循环依然运行 i 次，但内部查找变为 O(1)，
                // 因此总复杂度降为 O(N^2)
                XVectorLayer testLayer = new XVectorLayer(baseName, SHAPETYPE.Point);
                document.AddLayer(testLayer);
            }
            swHash.Stop();

            // 3. 计算并展示对比结果
            double listMs = swList.Elapsed.TotalMilliseconds;
            double hashMs = swHash.Elapsed.TotalMilliseconds;
            double speedup = hashMs > 0 ? listMs / hashMs : (double)swList.ElapsedTicks / Math.Max(1, swHash.ElapsedTicks);

            string report = $"测试规模: {testCount} 次连续重名冲突添加\n" +
                            $"------------------------------------\n" +
                            $"List 遍历 (旧逻辑模拟): {listMs:F2} ms\n" +
                            $"HashSet 查找 (当前优化): {hashMs:F2} ms\n" +
                            $"------------------------------------\n" +
                            $"性能提升约: {speedup:F1} 倍";

            MessageBox.Show(report, "图层命名查重性能对比");

            InitialTV(); // 更新 UI 验证结果
            UpdateMap();
        }

        private void mNewXDoc_Click(object sender, EventArgs e)
        {
            document = new XDocument();
            InitialTV(); 
            UpdateMap(); 
        }

        private void mOpenXDoc_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "地图文档|*.xdoc";
            if (dialog.ShowDialog() != DialogResult.OK) return;
            document.Read(dialog.FileName);
            InitialTV(); // 核心修复：读取后必须刷新图层树
            ZoomToFullExtent();
            UpdateMap(); // 核心修复：读取后必须重绘地图
        }

        private void mSaveXDoc_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "地图文档|*.xdoc";
            if (dialog.ShowDialog() != DialogResult.OK) return;
            document.Write(dialog.FileName);
            MessageBox.Show("地图文档已写入" + dialog.FileName);
        }


        private void tRunPerformance_Click(object sender, EventArgs e)
        {
            RunPerformanceTest();
        }

        public static class Prompt
        {
            public static string ShowDialog(string text, string caption)
            {
                Form prompt = new Form()
                {
                    Width = 300,
                    Height = 150,
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    Text = caption,
                    StartPosition = FormStartPosition.CenterScreen
                };
                System.Windows.Forms.Label textLabel = new System.Windows.Forms.Label() { Left = 20, Top = 20, Text = text, Width = 250 };
                TextBox textBox = new TextBox() { Left = 20, Top = 50, Width = 240 };
                Button confirmation = new Button() { Text = "确定", Left = 160, Width = 100, Top = 80, DialogResult = DialogResult.OK };
                confirmation.Click += (sender, e) => { prompt.Close(); };
                prompt.Controls.Add(textBox);
                prompt.Controls.Add(confirmation);
                prompt.Controls.Add(textLabel);
                prompt.AcceptButton = confirmation;

                return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : "";
            }
        }


    }
}
