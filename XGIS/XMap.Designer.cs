namespace XGIS
{
    partial class XMap
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.pnlInfo = new System.Windows.Forms.Panel();
            this.dgvAttributes = new System.Windows.Forms.DataGridView();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.加入数据ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.上移ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.下移ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.显示属性数据ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.矢量数据ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.栅格数据ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pnlInfo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvAttributes)).BeginInit();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlInfo
            // 
            this.pnlInfo.Controls.Add(this.dgvAttributes);
            this.pnlInfo.Location = new System.Drawing.Point(18, 32);
            this.pnlInfo.Name = "pnlInfo";
            this.pnlInfo.Size = new System.Drawing.Size(336, 209);
            this.pnlInfo.TabIndex = 0;
            // 
            // dgvAttributes
            // 
            this.dgvAttributes.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvAttributes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvAttributes.Location = new System.Drawing.Point(0, 0);
            this.dgvAttributes.Name = "dgvAttributes";
            this.dgvAttributes.RowHeadersWidth = 51;
            this.dgvAttributes.RowTemplate.Height = 27;
            this.dgvAttributes.Size = new System.Drawing.Size(336, 209);
            this.dgvAttributes.TabIndex = 0;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.加入数据ToolStripMenuItem,
            this.上移ToolStripMenuItem,
            this.下移ToolStripMenuItem,
            this.显示属性数据ToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(211, 128);
            // 
            // 加入数据ToolStripMenuItem
            // 
            this.加入数据ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.矢量数据ToolStripMenuItem,
            this.栅格数据ToolStripMenuItem});
            this.加入数据ToolStripMenuItem.Name = "加入数据ToolStripMenuItem";
            this.加入数据ToolStripMenuItem.Size = new System.Drawing.Size(210, 24);
            this.加入数据ToolStripMenuItem.Text = "加入数据";
            // 
            // 上移ToolStripMenuItem
            // 
            this.上移ToolStripMenuItem.Name = "上移ToolStripMenuItem";
            this.上移ToolStripMenuItem.Size = new System.Drawing.Size(210, 24);
            this.上移ToolStripMenuItem.Text = "上移";
            // 
            // 下移ToolStripMenuItem
            // 
            this.下移ToolStripMenuItem.Name = "下移ToolStripMenuItem";
            this.下移ToolStripMenuItem.Size = new System.Drawing.Size(210, 24);
            this.下移ToolStripMenuItem.Text = "下移";
            // 
            // 显示属性数据ToolStripMenuItem
            // 
            this.显示属性数据ToolStripMenuItem.Name = "显示属性数据ToolStripMenuItem";
            this.显示属性数据ToolStripMenuItem.Size = new System.Drawing.Size(210, 24);
            this.显示属性数据ToolStripMenuItem.Text = "显示属性数据";
            // 
            // 矢量数据ToolStripMenuItem
            // 
            this.矢量数据ToolStripMenuItem.Name = "矢量数据ToolStripMenuItem";
            this.矢量数据ToolStripMenuItem.Size = new System.Drawing.Size(224, 26);
            this.矢量数据ToolStripMenuItem.Text = "矢量数据";
            this.矢量数据ToolStripMenuItem.Click += new System.EventHandler(this.矢量数据ToolStripMenuItem_Click);
            // 
            // 栅格数据ToolStripMenuItem
            // 
            this.栅格数据ToolStripMenuItem.Name = "栅格数据ToolStripMenuItem";
            this.栅格数据ToolStripMenuItem.Size = new System.Drawing.Size(224, 26);
            this.栅格数据ToolStripMenuItem.Text = "栅格数据";
            // 
            // XMap
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pnlInfo);
            this.Name = "XMap";
            this.Size = new System.Drawing.Size(1108, 627);
            this.SizeChanged += new System.EventHandler(this.Form_SizeChanged);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.OnPaint);
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseClick);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseUp);
            this.pnlInfo.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvAttributes)).EndInit();
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlInfo;
        private System.Windows.Forms.DataGridView dgvAttributes;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem 加入数据ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 矢量数据ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 栅格数据ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 上移ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 下移ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 显示属性数据ToolStripMenuItem;
    }
}
