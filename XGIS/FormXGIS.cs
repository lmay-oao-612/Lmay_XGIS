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
    public partial class FormXGIS : Form
    {
        public FormXGIS()
        {
            InitializeComponent();
            labelSelectionCount = new System.Windows.Forms.ToolStripStatusLabel();
            labelSelectionCount.Text = "当前选中了0个空间对象";
            this.statusStrip1.Items.Add(labelSelectionCount); // 加进状态栏
            MapPanel.LocationChanged += WhenLocationChanged;
            MapPanel.SelectionChanged += WhenSelectionChanged;
        }

        private void WhenLocationChanged(XVertex location)
        {
            labelCoordinates.Text = location.x + ", " + location.y;
        }
        private void WhenSelectionChanged(int count)
        {
            // 假设你在状态栏(StatusStrip)中添加了一个用于显示的 Label，名为 labelSelectionCount
            if (labelSelectionCount != null)
            {
                labelSelectionCount.Text = $"当前选中了{count}个空间对象";
            }
        }
    }
}
