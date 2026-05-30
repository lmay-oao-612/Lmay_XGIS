using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace XGIS
{
    public partial class XPanel : UserControl
    {
        XDocument document = new XDocument();
        XView view = null;
        Bitmap backwindow;
        Point MouseDownLocation, MouseMovingLocation;
        XExploreActions currentMouseAction = XExploreActions.noaction;

        public delegate void DelegateLocationChanged(XVertex location);
        public event DelegateLocationChanged LocationChanged;
        public delegate void DelegateSelectionChanged(int selectedCount);
        public event DelegateSelectionChanged SelectionChanged;

        public XVertex CurrentLocation = new XVertex(0, 0);

        XLayer CurrentLayer = null;

        public XPanel()
        {
            // 在构造函数中初始化后再绑定
            InitializeComponent();
            view = new XView(new XExtent(new XVertex(0, 0), new XVertex(100, 100)), ClientRectangle);
            this.SizeChanged += XPanel_SizeChanged;
            LocationChanged = WhenLocationChanged;
        }

        private void WhenLocationChanged(XVertex location)
        {
        }

        private void XPanel_Paint(object sender, PaintEventArgs e)
        {
            if (backwindow == null) return;
            if (currentMouseAction == XExploreActions.pan)
            {
                e.Graphics.DrawImage(backwindow,
                    MouseMovingLocation.X - MouseDownLocation.X,
                    MouseMovingLocation.Y - MouseDownLocation.Y);
            }
            else if (currentMouseAction == XExploreActions.zoominbybox ||
                currentMouseAction == XExploreActions.identifybybox)
            {
                e.Graphics.DrawImage(backwindow, 0, 0);
                int x = Math.Min(MouseDownLocation.X, MouseMovingLocation.X);
                int y = Math.Min(MouseDownLocation.Y, MouseMovingLocation.Y);
                int width = Math.Abs(MouseDownLocation.X - MouseMovingLocation.X);
                int height = Math.Abs(MouseDownLocation.Y - MouseMovingLocation.Y);
                e.Graphics.DrawRectangle(new Pen(new SolidBrush(Color.Red), 2), x, y, width, height);
            }
            else
            {
                e.Graphics.DrawImage(backwindow, 0, 0);
            }
        }

        public void UpdateMap()
        {
            if (ClientRectangle.Width * ClientRectangle.Height == 0) return;
            if (view == null) return; // 防御性检查
            //更新view，以确保其地图窗口尺寸是正确的
            view.UpdateMapWindow(ClientRectangle);
            //如果背景窗口不为空，则先清除
            if (backwindow != null) backwindow.Dispose();
            //根据最新的地图窗口尺寸建立背景窗口
            backwindow = new Bitmap(ClientRectangle.Width, ClientRectangle.Height);
            //在背景窗口上绘图
            Graphics g = Graphics.FromImage(backwindow);
            //清空窗口
            g.FillRectangle(new SolidBrush(Color.White), ClientRectangle);
            //绘制空间对象
            document.DrawLayers(g, view);
            //回收绘图工具
            g.Dispose();
            //重绘前景窗口
            Invalidate();
        }

        private void FullExtent()
        {
            view.Update(document.Extent, ClientRectangle);
            UpdateMap();
        }

        private void XPanel_MouseMove(object sender, MouseEventArgs e)
        {
            MouseMovingLocation = e.Location;
            CurrentLocation = view.ToMapVertex(MouseMovingLocation);
            LocationChanged(CurrentLocation);
            if (currentMouseAction == XExploreActions.zoominbybox ||
                currentMouseAction == XExploreActions.pan ||
                currentMouseAction == XExploreActions.identifybybox)
            {
                Invalidate();
            }
        }

        private void XPanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            MouseDownLocation = e.Location;
            if (Control.ModifierKeys == Keys.Shift)
                currentMouseAction = XExploreActions.zoominbybox;
            else if (Control.ModifierKeys == Keys.Alt ||
                Control.ModifierKeys == (Keys.Alt | Keys.Control))
                currentMouseAction = XExploreActions.identifybybox;
            else
                currentMouseAction = XExploreActions.pan;
        }

        

        private void XPanel_MouseWheel(object sender, MouseEventArgs e)
        {
            XExploreActions action = XExploreActions.noaction;
            if (e.Delta > 0)
            {
                action = XExploreActions.zoomin;
            }
            else
            {
                action = XExploreActions.zoomout;
            }
            view.ChangeView(action);
            UpdateMap();
        }

        private void XPanel_SizeChanged(object sender, EventArgs e)
        {
            UpdateMap();
        }

        public void NewDoc()
        {
            document = new XDocument();
            UpdateMap();
        }

        public void OpenDoc()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "地图文档|*.xdoc";
            if (dialog.ShowDialog() != DialogResult.OK) return;
            document.Read(dialog.FileName);
            FullExtent();
        }

        public void SaveDoc()
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "地图文档|*.xdoc";
            if (dialog.ShowDialog() != DialogResult.OK) return;
            document.Write(dialog.FileName);
            MessageBox.Show("地图文档已写入" + dialog.FileName);
        }

        public void AddLayer()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "图层文件|*.shp;*.gis;*.rst";
            if (dialog.ShowDialog() != DialogResult.OK) return;
            XLayer layer = XTools.OpenLayer(dialog.FileName);
            if (layer == null)
            {
                MessageBox.Show("图层读取错误！");
            }
            else
            {
                document.AddLayer(layer);
                UpdateMap();
            }
        }

        private void mNewDoc_Click(object sender, EventArgs e)
        {
            NewDoc();
        }

        private void mOpenDoc_Click(object sender, EventArgs e)
        {
            OpenDoc();
        }

        private void mSaveDoc_Click(object sender, EventArgs e)
        {
            SaveDoc();
        }

        private void mAddLayer_Click(object sender, EventArgs e)
        {
            AddLayer();
        }

        private void mFullExtent_Click(object sender, EventArgs e)
        {
            FullExtent();
        }

        private void XPanel_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                MenuDoc.Show(this.PointToScreen(e.Location));
        }

        private void WhenLayerClicked(object sender, EventArgs e)
        {
            int pos = sender.ToString().IndexOf("|");
            string layerName = sender.ToString().Substring(pos + 1);
            CurrentLayer = document.FindLayer(layerName);

            mSaveLayer.Enabled = mShowAttribute.Enabled =
                mAttributeQuery.Enabled = mSelectable.Enabled =
                mLabel.Enabled = mFieldIndex.Enabled = CurrentLayer is XVectorLayer;

            tbLayerName.Text = CurrentLayer.Name;
            mVisible.Checked = CurrentLayer.Visible;

            if (CurrentLayer is XVectorLayer)
            {
                XVectorLayer vlayer = (XVectorLayer)CurrentLayer;
                mSelectable.Checked = vlayer.Selectable;
                mLabel.Checked = vlayer.LabelOrNot;
            }

            MenuLayer.Show(MenuDoc.Left, MenuDoc.Top);
        }

        private void mRenameLayer_Click(object sender, EventArgs e)
        {
            if (tbLayerName.Text=="")
            {
                MessageBox.Show("请给出图层名称！");
            }
            if (CurrentLayer.Name==tbLayerName.Text)
            {
                MessageBox.Show("新名称与现有名称一致，无需修改！");
            }
            else if (document.ChangeLayerName(CurrentLayer, tbLayerName.Text))
            {
                MessageBox.Show("图层名已成功修改！");
            }
            else
            {
                MessageBox.Show("图层名无法修改！");
            }
        }

        private void mDeleteLayer_Click(object sender, EventArgs e)
        {
            document.DeleteLayer(CurrentLayer);
            UpdateMap();
        }

        private void mSaveLayer_Click(object sender, EventArgs e)
        {
            XVectorLayer vlayer = (XVectorLayer)CurrentLayer;
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "地图图层文件|*.gis";
            if (dialog.ShowDialog() != DialogResult.OK) return;
            XMyFile.WriteFile(vlayer, dialog.FileName);
            MessageBox.Show("图层已保存至" + dialog.FileName);
        }

        private void mMoveUp_Click(object sender, EventArgs e)
        {
            if (!document.AdjustLayerOrder(CurrentLayer, 1))
            {
                MessageBox.Show("无法调整！");
            }
            else
            {
                UpdateMap();
            }
        }

        private void mMoveDown_Click(object sender, EventArgs e)
        {
            if (!document.AdjustLayerOrder(CurrentLayer, -1))
            {
                MessageBox.Show("无法调整！");
            }
            else
            {
                UpdateMap();
            }
        }


        private void AfterSelect()
        {
            UpdateMap();
        }

        private void mVisible_Click(object sender, EventArgs e)
        {
            CurrentLayer.Visible = !CurrentLayer.Visible;
            mVisible.Checked = CurrentLayer.Visible;
            UpdateMap();
        }

        private void mSelectable_Click(object sender, EventArgs e)
        {
            XVectorLayer vlayer = (XVectorLayer)CurrentLayer;
            vlayer.Selectable = !vlayer.Selectable;
            mSelectable.Checked = vlayer.Selectable;
            UpdateMap();
        }

        private void mLabel_Click(object sender, EventArgs e)
        {
            XVectorLayer vlayer = (XVectorLayer)CurrentLayer;
            vlayer.LabelOrNot = !vlayer.LabelOrNot;
            mLabel.Checked = vlayer.LabelOrNot;
            UpdateMap();
        }

        private void WhenFieldClicked(object sender, EventArgs e)
        {
            string fieldName = sender.ToString();
            XVectorLayer vlayer = (XVectorLayer)CurrentLayer;
            for (int i = 0; i < vlayer.Fields.Count; i++)
            {
                if (vlayer.Fields[i].name==fieldName)
                {
                    vlayer.LabelIndex = i;
                    UpdateMap();
                    return;
                }
            }
        }

        private void MenuDoc_Opening(object sender, CancelEventArgs e)
        {
            mLayerList.DropDownItems.Clear();
            foreach (XLayer layer in document.Layers)
            {
                ToolStripMenuItem item = new ToolStripMenuItem();
                if (layer is XVectorLayer)
                    item.Text = "矢量图层" + "|" + layer.Name;
                else if (layer is XRasterLayer)
                    item.Text = "栅格图层" + "|" + layer.Name;
                else
                    item.Text = "未知图层" + "|" + layer.Name;
                item.Click += WhenLayerClicked;
                mLayerList.DropDownItems.Insert(0, item);
            }
        }

        private void MenuLayer_Opening(object sender, CancelEventArgs e)
        {
            mFieldIndex.DropDownItems.Clear();
            XVectorLayer vlayer = (XVectorLayer)CurrentLayer;
            for (int i = 0; i < vlayer.Fields.Count; i++)
            {
                ToolStripMenuItem item = new ToolStripMenuItem();
                item.Text = vlayer.Fields[i].name;
                item.Checked = vlayer.LabelIndex == i;
                item.Click += WhenFieldClicked;
                mFieldIndex.DropDownItems.Add(item);
            }
        }
        private void mZoomToLayer_Click(object sender, EventArgs e)
        {
            if (CurrentLayer != null && CurrentLayer.Extent != null)
            {
                // 使用当前图层的Extent和当前的ClientRectangle更新视图
                view.Update(CurrentLayer.Extent, ClientRectangle);
                UpdateMap();
            }
        }
        private void UpdateSelectionCount()
        {
            if (SelectionChanged != null)
            {
                int count = 0;
                foreach (XLayer layer in document.Layers)
                {
                    if (layer is XVectorLayer)
                    {
                        XVectorLayer vlayer = (XVectorLayer)layer;
                        count += vlayer.SelectedFeatures.Count;
                    }
                }
                // 触发事件，传递统计结果
                SelectionChanged(count);
            }
        }
        private void XPanel_MouseUp(object sender, MouseEventArgs e)
        {
            XVertex v1 = view.ToMapVertex(MouseDownLocation);
            if (MouseDownLocation == e.Location)
            {
                if (currentMouseAction == XExploreActions.identifybybox)
                {
                    document.SelectByVertex(v1, view.ToMapDistance(5), Control.ModifierKeys == (Keys.Alt | Keys.Control));
                    UpdateMap();
                    UpdateSelectionCount(); // 单击选择后更新统计
                }
                currentMouseAction = XExploreActions.noaction;
                return;
            }

            XVertex v2 = view.ToMapVertex(e.Location);

            if (currentMouseAction == XExploreActions.zoominbybox)
            {
                XExtent extent = new XExtent(v1, v2);
                view.Update(extent, ClientRectangle);
            }
            else if (currentMouseAction == XExploreActions.pan)
            {
                view.OffsetCenter(v1, v2);
            }
            else if (currentMouseAction == XExploreActions.identifybybox)
            {
                document.SelectByExtent(new XExtent(v1, v2), Control.ModifierKeys == (Keys.Alt | Keys.Control));
                UpdateSelectionCount(); // 框选后更新统计
            }
            UpdateMap();
            currentMouseAction = XExploreActions.noaction;
        }
    }


}
