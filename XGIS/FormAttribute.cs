using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace XGIS
{
    public partial class FormAttribute : Form
    {
        private XVectorLayer _layer;
        private Form1 _mainForm;

        public FormAttribute(XVectorLayer layer, Form1 mainForm)
        {
            InitializeComponent();
            this._layer = layer;
            this._mainForm = mainForm; // 存起来
            LoadLayerData(layer);

            // 设置 DataGridView 选中整行
            dgvValues.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvValues.MultiSelect = false;

        }

        private void LoadLayerData(XVectorLayer layer)
        {
            DataTable dt = new DataTable();

            // 构建表头
            foreach (var field in layer.Fields)
            {
                dt.Columns.Add(field.name, typeof(string));
            }
            dt.Columns.Add("Calc", typeof(double));

            foreach (var feature in layer.Features)
            {
                DataRow row = dt.NewRow();

                // 填充属性
                for (int i = 0; i < layer.Fields.Count; i++)
                {
                    row[i] = feature.getAttribute(i)?.ToString() ?? "";
                }

                // 计算几何属性
                if (feature.spatial is XPolygon poly)
                {
                    row["Calc"] = poly.Area;
                }
                else if (feature.spatial is XLine line)
                {
                    row["Calc"] = line.Length;
                }
                else if (feature.spatial is XPoint)
                {
                    row["Calc"] = 0; // 点没有长度和面积，填0
                }

                // 核心修正：无论什么几何类型，都要把这一行加入 DataTable
                dt.Rows.Add(row);
            }

            // 性能优化：循环结束后再绑定数据源
            dgvValues.DataSource = dt;
        }

        private void dgvValues_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvValues.SelectedRows.Count > 0)
            {
                // 获取当前选中的行索引
                int index = dgvValues.SelectedRows[0].Index;
                // 边界检查，防止索引越界
                if (index >= 0 && index < _layer.Features.Count)
                {
                    ZoomToFeature(index);
                }
            }
        }

        private void ZoomToFeature(int index)
        {
            XFeature feature = _layer.Features[index];

            // 获取要素的外接矩形
            XExtent featureExtent = feature.GetExtent();
            // 获取中心点
            XVertex center = featureExtent.getCenter();
            _mainForm.ZoomTo(center, featureExtent, feature);

        }
    }
}