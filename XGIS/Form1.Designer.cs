namespace XGIS
{
    partial class Form1
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

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("节点0");
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this.tSBfile = new System.Windows.Forms.ToolStripDropDownButton();
            this.mNewXDoc = new System.Windows.Forms.ToolStripMenuItem();
            this.mAdd = new System.Windows.Forms.ToolStripMenuItem();
            this.bReadShaprfile = new System.Windows.Forms.ToolStripMenuItem();
            this.bReadRaster = new System.Windows.Forms.ToolStripMenuItem();
            this.mOpenXDoc = new System.Windows.Forms.ToolStripMenuItem();
            this.mSaveXDoc = new System.Windows.Forms.ToolStripMenuItem();
            this.tSBtool = new System.Windows.Forms.ToolStripDropDownButton();
            this.全图显示ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.生成随机点对象ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.测距ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.查看ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.TCLear = new System.Windows.Forms.ToolStripMenuItem();
            this.tDraw = new System.Windows.Forms.ToolStripDropDownButton();
            this.TDrawPoint = new System.Windows.Forms.ToolStripMenuItem();
            this.TDrawLine = new System.Windows.Forms.ToolStripMenuItem();
            this.TDrawPolygon = new System.Windows.Forms.ToolStripMenuItem();
            this.tTest = new System.Windows.Forms.ToolStripDropDownButton();
            this.tVerifyAreaAlgorithm = new System.Windows.Forms.ToolStripMenuItem();
            this.tRunPerformance = new System.Windows.Forms.ToolStripMenuItem();
            this.layerMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.TshowAttribute = new System.Windows.Forms.ToolStripMenuItem();
            this.TremoveLayer = new System.Windows.Forms.ToolStripMenuItem();
            this.TmoveUp = new System.Windows.Forms.ToolStripMenuItem();
            this.TmoveDown = new System.Windows.Forms.ToolStripMenuItem();
            this.cmSelectable = new System.Windows.Forms.ToolStripMenuItem();
            this.tSelect = new System.Windows.Forms.ToolStripMenuItem();
            this.TUnselect = new System.Windows.Forms.ToolStripMenuItem();
            this.重命名ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pnlInfo = new System.Windows.Forms.Panel();
            this.dgvAttributes = new System.Windows.Forms.DataGridView();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.SCurrentMode = new System.Windows.Forms.ToolStripStatusLabel();
            this.SShowMode = new System.Windows.Forms.ToolStripStatusLabel();
            this.tvLayer = new System.Windows.Forms.TreeView();
            this.toolStrip.SuspendLayout();
            this.layerMenu.SuspendLayout();
            this.pnlInfo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvAttributes)).BeginInit();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip
            // 
            this.toolStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tSBfile,
            this.tSBtool,
            this.tDraw,
            this.tTest});
            this.toolStrip.Location = new System.Drawing.Point(0, 0);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.Size = new System.Drawing.Size(1059, 27);
            this.toolStrip.TabIndex = 12;
            this.toolStrip.Text = "toolStrip";
            // 
            // tSBfile
            // 
            this.tSBfile.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tSBfile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mNewXDoc,
            this.mAdd,
            this.mOpenXDoc,
            this.mSaveXDoc});
            this.tSBfile.Image = ((System.Drawing.Image)(resources.GetObject("tSBfile.Image")));
            this.tSBfile.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tSBfile.Name = "tSBfile";
            this.tSBfile.Size = new System.Drawing.Size(53, 24);
            this.tSBfile.Text = "文件";
            // 
            // mNewXDoc
            // 
            this.mNewXDoc.Name = "mNewXDoc";
            this.mNewXDoc.Size = new System.Drawing.Size(182, 26);
            this.mNewXDoc.Text = "新建地图文档";
            this.mNewXDoc.Click += new System.EventHandler(this.mNewXDoc_Click);
            // 
            // mAdd
            // 
            this.mAdd.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.bReadShaprfile,
            this.bReadRaster});
            this.mAdd.Name = "mAdd";
            this.mAdd.Size = new System.Drawing.Size(182, 26);
            this.mAdd.Text = "导入数据";
            // 
            // bReadShaprfile
            // 
            this.bReadShaprfile.Name = "bReadShaprfile";
            this.bReadShaprfile.Size = new System.Drawing.Size(189, 26);
            this.bReadShaprfile.Text = "Shapefile数据";
            this.bReadShaprfile.Click += new System.EventHandler(this.bReadShp_Click);
            // 
            // bReadRaster
            // 
            this.bReadRaster.Name = "bReadRaster";
            this.bReadRaster.Size = new System.Drawing.Size(189, 26);
            this.bReadRaster.Text = "Raster数据";
            this.bReadRaster.Click += new System.EventHandler(this.bReadRaster_Click);
            // 
            // mOpenXDoc
            // 
            this.mOpenXDoc.Name = "mOpenXDoc";
            this.mOpenXDoc.Size = new System.Drawing.Size(182, 26);
            this.mOpenXDoc.Text = "打开地图文档";
            this.mOpenXDoc.Click += new System.EventHandler(this.mOpenXDoc_Click);
            // 
            // mSaveXDoc
            // 
            this.mSaveXDoc.Name = "mSaveXDoc";
            this.mSaveXDoc.Size = new System.Drawing.Size(182, 26);
            this.mSaveXDoc.Text = "保存地图文档";
            this.mSaveXDoc.Click += new System.EventHandler(this.mSaveXDoc_Click);
            // 
            // tSBtool
            // 
            this.tSBtool.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tSBtool.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.全图显示ToolStripMenuItem,
            this.生成随机点对象ToolStripMenuItem,
            this.测距ToolStripMenuItem,
            this.查看ToolStripMenuItem,
            this.TCLear});
            this.tSBtool.Image = ((System.Drawing.Image)(resources.GetObject("tSBtool.Image")));
            this.tSBtool.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tSBtool.Name = "tSBtool";
            this.tSBtool.Size = new System.Drawing.Size(53, 24);
            this.tSBtool.Text = "工具";
            // 
            // 全图显示ToolStripMenuItem
            // 
            this.全图显示ToolStripMenuItem.Name = "全图显示ToolStripMenuItem";
            this.全图显示ToolStripMenuItem.Size = new System.Drawing.Size(197, 26);
            this.全图显示ToolStripMenuItem.Text = "全图显示";
            this.全图显示ToolStripMenuItem.Click += new System.EventHandler(this.bFullExtent_Click);
            // 
            // 生成随机点对象ToolStripMenuItem
            // 
            this.生成随机点对象ToolStripMenuItem.Name = "生成随机点对象ToolStripMenuItem";
            this.生成随机点对象ToolStripMenuItem.Size = new System.Drawing.Size(197, 26);
            this.生成随机点对象ToolStripMenuItem.Text = "生成随机点对象";
            this.生成随机点对象ToolStripMenuItem.Click += new System.EventHandler(this.bCreateRandomObjects_Click);
            // 
            // 测距ToolStripMenuItem
            // 
            this.测距ToolStripMenuItem.Name = "测距ToolStripMenuItem";
            this.测距ToolStripMenuItem.Size = new System.Drawing.Size(197, 26);
            this.测距ToolStripMenuItem.Text = "测距";
            this.测距ToolStripMenuItem.Click += new System.EventHandler(this.bMeasure_Click);
            // 
            // 查看ToolStripMenuItem
            // 
            this.查看ToolStripMenuItem.Name = "查看ToolStripMenuItem";
            this.查看ToolStripMenuItem.Size = new System.Drawing.Size(197, 26);
            this.查看ToolStripMenuItem.Text = "查看";
            this.查看ToolStripMenuItem.Click += new System.EventHandler(this.bIdentify_Click);
            // 
            // TCLear
            // 
            this.TCLear.Name = "TCLear";
            this.TCLear.Size = new System.Drawing.Size(197, 26);
            this.TCLear.Text = "清除所有状态";
            this.TCLear.Click += new System.EventHandler(this.清除所有状态ToolStripMenuItem_Click);
            // 
            // tDraw
            // 
            this.tDraw.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tDraw.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.TDrawPoint,
            this.TDrawLine,
            this.TDrawPolygon});
            this.tDraw.Image = ((System.Drawing.Image)(resources.GetObject("tDraw.Image")));
            this.tDraw.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tDraw.Name = "tDraw";
            this.tDraw.Size = new System.Drawing.Size(53, 24);
            this.tDraw.Text = "绘制";
            // 
            // TDrawPoint
            // 
            this.TDrawPoint.Name = "TDrawPoint";
            this.TDrawPoint.Size = new System.Drawing.Size(137, 26);
            this.TDrawPoint.Text = "点要素";
            this.TDrawPoint.Click += new System.EventHandler(this.TDrawPoint_Click);
            // 
            // TDrawLine
            // 
            this.TDrawLine.Name = "TDrawLine";
            this.TDrawLine.Size = new System.Drawing.Size(137, 26);
            this.TDrawLine.Text = "线要素";
            this.TDrawLine.Click += new System.EventHandler(this.TDrawLine_Click);
            // 
            // TDrawPolygon
            // 
            this.TDrawPolygon.Name = "TDrawPolygon";
            this.TDrawPolygon.Size = new System.Drawing.Size(137, 26);
            this.TDrawPolygon.Text = "面要素";
            this.TDrawPolygon.Click += new System.EventHandler(this.TDrawPolygon_Click);
            // 
            // tTest
            // 
            this.tTest.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tTest.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tVerifyAreaAlgorithm,
            this.tRunPerformance});
            this.tTest.Image = ((System.Drawing.Image)(resources.GetObject("tTest.Image")));
            this.tTest.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tTest.Name = "tTest";
            this.tTest.Size = new System.Drawing.Size(53, 24);
            this.tTest.Text = "测试";
            // 
            // tVerifyAreaAlgorithm
            // 
            this.tVerifyAreaAlgorithm.Name = "tVerifyAreaAlgorithm";
            this.tVerifyAreaAlgorithm.Size = new System.Drawing.Size(182, 26);
            this.tVerifyAreaAlgorithm.Text = "面积计算验证";
            this.tVerifyAreaAlgorithm.Click += new System.EventHandler(this.VerifyAreaAlgorithm_Click);
            // 
            // tRunPerformance
            // 
            this.tRunPerformance.Name = "tRunPerformance";
            this.tRunPerformance.Size = new System.Drawing.Size(182, 26);
            this.tRunPerformance.Text = "性能测试";
            this.tRunPerformance.Click += new System.EventHandler(this.tRunPerformance_Click);
            // 
            // layerMenu
            // 
            this.layerMenu.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.layerMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.TshowAttribute,
            this.TremoveLayer,
            this.TmoveUp,
            this.TmoveDown,
            this.cmSelectable,
            this.重命名ToolStripMenuItem});
            this.layerMenu.Name = "layerMenu";
            this.layerMenu.Size = new System.Drawing.Size(169, 148);
            // 
            // TshowAttribute
            // 
            this.TshowAttribute.Name = "TshowAttribute";
            this.TshowAttribute.Size = new System.Drawing.Size(168, 24);
            this.TshowAttribute.Text = "打开属性表";
            this.TshowAttribute.Click += new System.EventHandler(this.TshowAttribute_Click);
            // 
            // TremoveLayer
            // 
            this.TremoveLayer.Name = "TremoveLayer";
            this.TremoveLayer.Size = new System.Drawing.Size(168, 24);
            this.TremoveLayer.Text = "移除图层";
            this.TremoveLayer.Click += new System.EventHandler(this.TremoveLayer_Click);
            // 
            // TmoveUp
            // 
            this.TmoveUp.Name = "TmoveUp";
            this.TmoveUp.Size = new System.Drawing.Size(168, 24);
            this.TmoveUp.Text = "图层上移";
            this.TmoveUp.Click += new System.EventHandler(this.TmoveUp_Click);
            // 
            // TmoveDown
            // 
            this.TmoveDown.Name = "TmoveDown";
            this.TmoveDown.Size = new System.Drawing.Size(168, 24);
            this.TmoveDown.Text = "图层下移";
            this.TmoveDown.Click += new System.EventHandler(this.TmoveDown_Click);
            // 
            // cmSelectable
            // 
            this.cmSelectable.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tSelect,
            this.TUnselect});
            this.cmSelectable.Name = "cmSelectable";
            this.cmSelectable.Size = new System.Drawing.Size(168, 24);
            this.cmSelectable.Text = "设置图层可选";
            // 
            // tSelect
            // 
            this.tSelect.Name = "tSelect";
            this.tSelect.Size = new System.Drawing.Size(137, 26);
            this.tSelect.Text = "可选";
            // 
            // TUnselect
            // 
            this.TUnselect.Name = "TUnselect";
            this.TUnselect.Size = new System.Drawing.Size(137, 26);
            this.TUnselect.Text = "不可选";
            // 
            // 重命名ToolStripMenuItem
            // 
            this.重命名ToolStripMenuItem.Name = "重命名ToolStripMenuItem";
            this.重命名ToolStripMenuItem.Size = new System.Drawing.Size(168, 24);
            this.重命名ToolStripMenuItem.Text = "重命名";
            // 
            // pnlInfo
            // 
            this.pnlInfo.BackColor = System.Drawing.SystemColors.Info;
            this.pnlInfo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlInfo.Controls.Add(this.dgvAttributes);
            this.pnlInfo.Location = new System.Drawing.Point(654, 387);
            this.pnlInfo.Name = "pnlInfo";
            this.pnlInfo.Size = new System.Drawing.Size(401, 152);
            this.pnlInfo.TabIndex = 13;
            this.pnlInfo.Visible = false;
            // 
            // dgvAttributes
            // 
            this.dgvAttributes.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvAttributes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvAttributes.Location = new System.Drawing.Point(0, 0);
            this.dgvAttributes.Name = "dgvAttributes";
            this.dgvAttributes.RowHeadersWidth = 51;
            this.dgvAttributes.RowTemplate.Height = 27;
            this.dgvAttributes.Size = new System.Drawing.Size(399, 150);
            this.dgvAttributes.TabIndex = 0;
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.SCurrentMode,
            this.SShowMode});
            this.statusStrip1.Location = new System.Drawing.Point(0, 544);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1059, 26);
            this.statusStrip1.TabIndex = 14;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // SCurrentMode
            // 
            this.SCurrentMode.Name = "SCurrentMode";
            this.SCurrentMode.Size = new System.Drawing.Size(84, 20);
            this.SCurrentMode.Text = "当前模式：";
            // 
            // SShowMode
            // 
            this.SShowMode.Name = "SShowMode";
            this.SShowMode.Size = new System.Drawing.Size(39, 20);
            this.SShowMode.Text = "浏览";
            // 
            // tvLayer
            // 
            this.tvLayer.CheckBoxes = true;
            this.tvLayer.Dock = System.Windows.Forms.DockStyle.Left;
            this.tvLayer.Location = new System.Drawing.Point(0, 27);
            this.tvLayer.Name = "tvLayer";
            treeNode1.Checked = true;
            treeNode1.Name = "节点0";
            treeNode1.Text = "节点0";
            this.tvLayer.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode1});
            this.tvLayer.Size = new System.Drawing.Size(178, 517);
            this.tvLayer.TabIndex = 15;
            this.tvLayer.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.tvLayer_AfterCheck);
            this.tvLayer.MouseDown += new System.Windows.Forms.MouseEventHandler(this.tvLayer_MouseDown);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1059, 570);
            this.Controls.Add(this.tvLayer);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.pnlInfo);
            this.Controls.Add(this.toolStrip);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "Form1";
            this.Text = "Form1";
            this.SizeChanged += new System.EventHandler(this.Form1_SizeChanged);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.Form1_Paint);
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseClick);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseUp);
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            this.layerMenu.ResumeLayout(false);
            this.pnlInfo.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvAttributes)).EndInit();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripDropDownButton tSBfile;
        private System.Windows.Forms.ToolStripMenuItem mAdd;
        private System.Windows.Forms.ToolStripMenuItem bReadShaprfile;
        private System.Windows.Forms.ToolStripDropDownButton tSBtool;
        private System.Windows.Forms.ToolStripMenuItem 全图显示ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 生成随机点对象ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 测距ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 查看ToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip layerMenu;
        private System.Windows.Forms.ToolStripMenuItem TshowAttribute;
        private System.Windows.Forms.ToolStripMenuItem TremoveLayer;
        private System.Windows.Forms.Panel pnlInfo;
        private System.Windows.Forms.DataGridView dgvAttributes;
        private System.Windows.Forms.ToolStripMenuItem TmoveUp;
        private System.Windows.Forms.ToolStripMenuItem TmoveDown;
        private System.Windows.Forms.ToolStripMenuItem TCLear;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel SCurrentMode;
        private System.Windows.Forms.ToolStripStatusLabel SShowMode;
        private System.Windows.Forms.ToolStripDropDownButton tDraw;
        private System.Windows.Forms.ToolStripMenuItem TDrawPoint;
        private System.Windows.Forms.ToolStripMenuItem TDrawLine;
        private System.Windows.Forms.ToolStripMenuItem TDrawPolygon;
        private System.Windows.Forms.ToolStripMenuItem mNewXDoc;
        private System.Windows.Forms.ToolStripMenuItem mOpenXDoc;
        private System.Windows.Forms.ToolStripMenuItem mSaveXDoc;
        private System.Windows.Forms.TreeView tvLayer;
        private System.Windows.Forms.ToolStripDropDownButton tTest;
        private System.Windows.Forms.ToolStripMenuItem tVerifyAreaAlgorithm;
        private System.Windows.Forms.ToolStripMenuItem tRunPerformance;
        private System.Windows.Forms.ToolStripMenuItem cmSelectable;
        private System.Windows.Forms.ToolStripMenuItem tSelect;
        private System.Windows.Forms.ToolStripMenuItem TUnselect;
        private System.Windows.Forms.ToolStripMenuItem 重命名ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem bReadRaster;
    }
}

