
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
        int randomLayerCount = 0;
        public Form1()
        {
            InitializeComponent(); 
            if (xMap1 != null) {
                xMap1.StatusTextChanged += (status) => statusStrip1.Items[0].Text = status;
                xMap1.LayerAdded += InitialTV;
            }
            InitialTV();
            this.MouseEnter += (s, e) => this.Focus();
        }

        /// <summary>
        /// 初始化图层树状列表（tvLayer）
        /// </summary>
        private void InitialTV()
        {
            tvLayer.Nodes.Clear();
            foreach (var layer in xMap1.Document.Layers)
            {
                TreeNode node = tvLayer.Nodes.Add(layer.Name);
                node.Checked = layer.Visible;
            }
        }

        

        /// <summary>
        /// 交换图层位置的通用方法
        /// </summary>
        private void MoveLayer(int oldIndex, int newIndex)
        {

            xMap1.Document.AdjustLayerOrder(xMap1.Document.Layers[oldIndex], newIndex - oldIndex);

            RefreshLayerListUI(newIndex);

            xMap1.UpdateMap();
        }

        /// <summary>
        /// 刷新图层列表的 UI
        /// </summary>
        /// <param name="targetIndex"></param>
        private void RefreshLayerListUI(int targetIndex)
        {
            tvLayer.Nodes.Clear();
            foreach (var ly in xMap1.Document.Layers)
            {
                TreeNode node = tvLayer.Nodes.Add(ly.Name);
                node.Checked = ly.Visible;
            }
            // 选中移动后的节点
            if (targetIndex >= 0 && targetIndex < tvLayer.Nodes.Count)
                tvLayer.SelectedNode = tvLayer.Nodes[targetIndex];
        }

        
        private void bCreateRandomObjects_Click(object sender, EventArgs e)
        {
            Random rand = new Random();
            randomLayerCount++; // 计数器自增

            //创建一个新的点图层，命名为 random + 序号
            string newLayerName = "random" + randomLayerCount;
            XVectorLayer newLayer = new XVectorLayer(newLayerName, SHAPETYPE.Point);

            //获取当前视图范围
            XExtent currentExtent = xMap1.View.CurrentMapExtent;
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
            xMap1.Document.AddLayer(newLayer);

            //同步更新树状列表
            TreeNode node = tvLayer.Nodes.Add(newLayer.Name);
            node.Checked = true;
            tvLayer.SelectedNode = node;

            //重新绘图
            xMap1.UpdateMap();
            MessageBox.Show("已在当前视图范围内生成100个随机点！");
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
            xMap1.UpdateMap();
        }
        */

        

        private void bFullExtent_Click(object sender, EventArgs e)
        {
            ZoomToFullExtent();
            xMap1.UpdateMap();
        }

        private void tvLayer_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (e.Action == TreeViewAction.Unknown) return; // 过滤代码触发的检查

            int index = e.Node.Index;
            if (index >= 0 && index < xMap1.Document.Layers.Count)
            {
                xMap1.Document.Layers[index].Visible = e.Node.Checked;
                xMap1.UpdateMap();
            }
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
                    xMap1.Document.AddLayer(newLayer);
                    
                    TreeNode node = tvLayer.Nodes.Add(newLayer.Name);
                    node.Checked = true;

                    MessageBox.Show($"图层: {newLayer.Name}\n要素数量: {(count != -1 ? count.ToString() : "未知")}");
                }

                // 缩放到所有图层的共同最大范围
                ZoomToFullExtent();
                xMap1.UpdateMap();
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
                    xMap1.Document.AddLayer(newLayer);
                    TreeNode node = tvLayer.Nodes.Add(newLayer.Name);
                    node.Checked = true;
                    tvLayer.SelectedNode = node;

                    ZoomToFullExtent();
                    xMap1.UpdateMap();
                }
            }
        }

        public void ZoomTo(XVertex center, XExtent extent, XFeature feature)
        {
            xMap1.ZoomTo(center, extent, feature);
        }

        private void ZoomToFullExtent()
        {
            xMap1.ZoomToFullExtent();
        }

        


        private void bIdentify_Click(object sender, EventArgs e)
        {
            xMap1.CurrentMouseTool = XMouseTool.identify;
            xMap1.Cursor = Cursors.Help;
            MessageBox.Show("已切换至：要素查看模式");
        }

        private void bMeasure_Click(object sender, EventArgs e)
        {
            xMap1.CurrentMouseTool = XMouseTool.measure;
            xMap1.firstVertex = null;
            xMap1.Cursor = Cursors.Cross;
            MessageBox.Show("已切换至：距离测量模式\n请在地图上点击两点。");
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

            var result = MessageBox.Show($"确定要移除图层 [{xMap1.Document.Layers[index].Name}] 吗？", "提示", MessageBoxButtons.YesNo);

            if (result == DialogResult.Yes)
            {

                xMap1.Document.DeleteLayer(xMap1.Document.Layers[index]);
                tvLayer.Nodes.RemoveAt(index);

                xMap1.UpdateMap(); // 重新绘图
            }
        }

        private void TshowAttribute_Click(object sender, EventArgs e)
        {
            if (tvLayer.SelectedNode == null) return;
            int index = tvLayer.SelectedNode.Index;

            FormAttribute formAttribute = new FormAttribute((XVectorLayer)xMap1.Document.Layers[index], this);
            formAttribute.Show();
        }

        

        private void TmoveUp_Click(object sender, EventArgs e)
        {
            if (tvLayer.SelectedNode == null) return;
            int index = tvLayer.SelectedNode.Index;
            // 如果没有选中，或者已经在最顶部（索引为0），则无法上移
            if (index <= 0 || index >= xMap1.Document.Layers.Count) return;

            MoveLayer(index, index - 1);
        }

        private void TmoveDown_Click(object sender, EventArgs e)
        {
            if (tvLayer.SelectedNode == null) return;
            int index = tvLayer.SelectedNode.Index;
            // 如果没有选中，或者已经在最底部，则无法下移
            if (index < 0 || index >= xMap1.Document.Layers.Count - 1) return;

            MoveLayer(index, index + 1);
        }

        private void 清除所有状态ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            xMap1.ClearSelection();
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
            xMap1.DrawPoint();
        }

        private void TDrawLine_Click(object sender, EventArgs e)
        {
            xMap1.DrawLine();
        }

        private void TDrawPolygon_Click(object sender, EventArgs e)
        {
            xMap1.DrawPolygon();
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
            xMap1.Document = new XDocument(); // 重置当前文档进行测试
            Stopwatch swHash = Stopwatch.StartNew();
            for (int i = 0; i < testCount; i++)
            {
                // AddLayer 内部使用的是 HashSet.Contains (O(1))
                // 面对重名冲突，while 循环依然运行 i 次，但内部查找变为 O(1)，
                // 因此总复杂度降为 O(N^2)
                XVectorLayer testLayer = new XVectorLayer(baseName, SHAPETYPE.Point);
                xMap1.Document.AddLayer(testLayer);
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
            xMap1.UpdateMap();
        }

        private void mNewXDoc_Click(object sender, EventArgs e)
        {
            xMap1.Document = new XDocument();
            InitialTV(); 
            xMap1.UpdateMap(); 
        }

        private void mOpenXDoc_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "地图文档|*.xdoc";
            if (dialog.ShowDialog() != DialogResult.OK) return;
            xMap1.Document.Read(dialog.FileName);
            InitialTV(); // 核心修复：读取后必须刷新图层树
            ZoomToFullExtent();
            xMap1.UpdateMap(); // 核心修复：读取后必须重绘地图
        }

        private void mSaveXDoc_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "地图文档|*.xdoc";
            if (dialog.ShowDialog() != DialogResult.OK) return;
            xMap1.Document.Write(dialog.FileName);
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
