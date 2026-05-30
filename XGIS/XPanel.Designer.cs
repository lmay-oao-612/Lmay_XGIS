
namespace XGIS
{
    partial class XPanel
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
            this.MenuLayer = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tbLayerName = new System.Windows.Forms.ToolStripTextBox();
            this.mRenameLayer = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator12 = new System.Windows.Forms.ToolStripSeparator();
            this.mDeleteLayer = new System.Windows.Forms.ToolStripMenuItem();
            this.mSaveLayer = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.mMoveUp = new System.Windows.Forms.ToolStripMenuItem();
            this.mMoveDown = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.mShowAttribute = new System.Windows.Forms.ToolStripMenuItem();
            this.mAttributeQuery = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.mVisible = new System.Windows.Forms.ToolStripMenuItem();
            this.mSelectable = new System.Windows.Forms.ToolStripMenuItem();
            this.mLabel = new System.Windows.Forms.ToolStripMenuItem();
            this.mFieldIndex = new System.Windows.Forms.ToolStripMenuItem();
            this.mNewDoc = new System.Windows.Forms.ToolStripMenuItem();
            this.mOpenDoc = new System.Windows.Forms.ToolStripMenuItem();
            this.mSaveDoc = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.mFullExtent = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.mLayerList = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuDoc = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.mAddLayer = new System.Windows.Forms.ToolStripMenuItem();
            this.tZoomToLayer = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuLayer.SuspendLayout();
            this.MenuDoc.SuspendLayout();
            this.SuspendLayout();
            // 
            // MenuLayer
            // 
            this.MenuLayer.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.MenuLayer.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tbLayerName,
            this.mRenameLayer,
            this.toolStripSeparator12,
            this.mDeleteLayer,
            this.mSaveLayer,
            this.toolStripSeparator5,
            this.mMoveUp,
            this.mMoveDown,
            this.toolStripSeparator6,
            this.mShowAttribute,
            this.mAttributeQuery,
            this.toolStripSeparator7,
            this.mVisible,
            this.mSelectable,
            this.mLabel,
            this.mFieldIndex,
            this.tZoomToLayer});
            this.MenuLayer.Name = "MenuLayer";
            this.MenuLayer.Size = new System.Drawing.Size(211, 397);
            this.MenuLayer.Opening += new System.ComponentModel.CancelEventHandler(this.MenuLayer_Opening);
            // 
            // tbLayerName
            // 
            this.tbLayerName.BackColor = System.Drawing.Color.White;
            this.tbLayerName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tbLayerName.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
            this.tbLayerName.ForeColor = System.Drawing.Color.Black;
            this.tbLayerName.Name = "tbLayerName";
            this.tbLayerName.Size = new System.Drawing.Size(100, 27);
            this.tbLayerName.Text = "图层名称";
            // 
            // mRenameLayer
            // 
            this.mRenameLayer.Name = "mRenameLayer";
            this.mRenameLayer.Size = new System.Drawing.Size(210, 26);
            this.mRenameLayer.Text = "重命名";
            this.mRenameLayer.Click += new System.EventHandler(this.mRenameLayer_Click);
            // 
            // toolStripSeparator12
            // 
            this.toolStripSeparator12.Name = "toolStripSeparator12";
            this.toolStripSeparator12.Size = new System.Drawing.Size(207, 6);
            // 
            // mDeleteLayer
            // 
            this.mDeleteLayer.Name = "mDeleteLayer";
            this.mDeleteLayer.Size = new System.Drawing.Size(210, 26);
            this.mDeleteLayer.Text = "删除图层";
            this.mDeleteLayer.Click += new System.EventHandler(this.mDeleteLayer_Click);
            // 
            // mSaveLayer
            // 
            this.mSaveLayer.Name = "mSaveLayer";
            this.mSaveLayer.Size = new System.Drawing.Size(210, 26);
            this.mSaveLayer.Text = "保存图层";
            this.mSaveLayer.Click += new System.EventHandler(this.mSaveLayer_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(207, 6);
            // 
            // mMoveUp
            // 
            this.mMoveUp.Name = "mMoveUp";
            this.mMoveUp.Size = new System.Drawing.Size(210, 26);
            this.mMoveUp.Text = "上移";
            this.mMoveUp.Click += new System.EventHandler(this.mMoveUp_Click);
            // 
            // mMoveDown
            // 
            this.mMoveDown.Name = "mMoveDown";
            this.mMoveDown.Size = new System.Drawing.Size(210, 26);
            this.mMoveDown.Text = "下移";
            this.mMoveDown.Click += new System.EventHandler(this.mMoveDown_Click);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(207, 6);
            // 
            // mShowAttribute
            // 
            this.mShowAttribute.Name = "mShowAttribute";
            this.mShowAttribute.Size = new System.Drawing.Size(210, 26);
            this.mShowAttribute.Text = "显示属性数据";

            // 
            // mAttributeQuery
            // 
            this.mAttributeQuery.Name = "mAttributeQuery";
            this.mAttributeQuery.Size = new System.Drawing.Size(210, 26);
            this.mAttributeQuery.Text = "属性查询";

            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(207, 6);
            // 
            // mVisible
            // 
            this.mVisible.Checked = true;
            this.mVisible.CheckState = System.Windows.Forms.CheckState.Checked;
            this.mVisible.Name = "mVisible";
            this.mVisible.Size = new System.Drawing.Size(210, 26);
            this.mVisible.Text = "可视";
            this.mVisible.Click += new System.EventHandler(this.mVisible_Click);
            // 
            // mSelectable
            // 
            this.mSelectable.Checked = true;
            this.mSelectable.CheckState = System.Windows.Forms.CheckState.Checked;
            this.mSelectable.Name = "mSelectable";
            this.mSelectable.Size = new System.Drawing.Size(210, 26);
            this.mSelectable.Text = "可选";
            this.mSelectable.Click += new System.EventHandler(this.mSelectable_Click);
            // 
            // mLabel
            // 
            this.mLabel.Checked = true;
            this.mLabel.CheckState = System.Windows.Forms.CheckState.Checked;
            this.mLabel.Name = "mLabel";
            this.mLabel.Size = new System.Drawing.Size(210, 26);
            this.mLabel.Text = "可标注";
            this.mLabel.Click += new System.EventHandler(this.mLabel_Click);
            // 
            // mFieldIndex
            // 
            this.mFieldIndex.Name = "mFieldIndex";
            this.mFieldIndex.Size = new System.Drawing.Size(210, 26);
            this.mFieldIndex.Text = "标注字段选择";
            // 
            // mNewDoc
            // 
            this.mNewDoc.Name = "mNewDoc";
            this.mNewDoc.Size = new System.Drawing.Size(168, 24);
            this.mNewDoc.Text = "新建地图文档";
            this.mNewDoc.Click += new System.EventHandler(this.mNewDoc_Click);
            // 
            // mOpenDoc
            // 
            this.mOpenDoc.Name = "mOpenDoc";
            this.mOpenDoc.Size = new System.Drawing.Size(168, 24);
            this.mOpenDoc.Text = "打开地图文档";
            this.mOpenDoc.Click += new System.EventHandler(this.mOpenDoc_Click);
            // 
            // mSaveDoc
            // 
            this.mSaveDoc.Name = "mSaveDoc";
            this.mSaveDoc.Size = new System.Drawing.Size(168, 24);
            this.mSaveDoc.Text = "保存地图文档";
            this.mSaveDoc.Click += new System.EventHandler(this.mSaveDoc_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(165, 6);
            // 
            // mFullExtent
            // 
            this.mFullExtent.Name = "mFullExtent";
            this.mFullExtent.Size = new System.Drawing.Size(168, 24);
            this.mFullExtent.Text = "全图显示";
            this.mFullExtent.Click += new System.EventHandler(this.mFullExtent_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(165, 6);
            // 
            // mLayerList
            // 
            this.mLayerList.Name = "mLayerList";
            this.mLayerList.Size = new System.Drawing.Size(168, 24);
            this.mLayerList.Text = "图层列表";
            // 
            // MenuDoc
            // 
            this.MenuDoc.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.MenuDoc.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mNewDoc,
            this.mOpenDoc,
            this.mSaveDoc,
            this.toolStripSeparator3,
            this.mAddLayer,
            this.toolStripSeparator1,
            this.mFullExtent,
            this.toolStripSeparator2,
            this.mLayerList});
            this.MenuDoc.Name = "MenuDoc";
            this.MenuDoc.Size = new System.Drawing.Size(169, 166);
            this.MenuDoc.Opening += new System.ComponentModel.CancelEventHandler(this.MenuDoc_Opening);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(165, 6);
            // 
            // mAddLayer
            // 
            this.mAddLayer.Name = "mAddLayer";
            this.mAddLayer.Size = new System.Drawing.Size(168, 24);
            this.mAddLayer.Text = "添加图层";
            this.mAddLayer.Click += new System.EventHandler(this.mAddLayer_Click);
            // 
            // tZoomToLayer
            // 
            this.tZoomToLayer.Name = "tZoomToLayer";
            this.tZoomToLayer.Size = new System.Drawing.Size(210, 26);
            this.tZoomToLayer.Text = "缩放至图层";
            this.tZoomToLayer.Click += new System.EventHandler(this.mZoomToLayer_Click);
            // 
            // XPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "XPanel";
            this.Size = new System.Drawing.Size(868, 644);
            this.SizeChanged += new System.EventHandler(this.XPanel_SizeChanged);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.XPanel_Paint);
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.XPanel_MouseClick);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.XPanel_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.XPanel_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.XPanel_MouseUp);
            this.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.XPanel_MouseWheel);
            this.MenuLayer.ResumeLayout(false);
            this.MenuLayer.PerformLayout();
            this.MenuDoc.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.ContextMenuStrip MenuLayer;
        private System.Windows.Forms.ToolStripMenuItem mDeleteLayer;
        private System.Windows.Forms.ToolStripMenuItem mSaveLayer;
        private System.Windows.Forms.ToolStripMenuItem mRenameLayer;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem mMoveUp;
        private System.Windows.Forms.ToolStripMenuItem mMoveDown;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripMenuItem mShowAttribute;
        private System.Windows.Forms.ToolStripMenuItem mNewDoc;
        private System.Windows.Forms.ToolStripMenuItem mOpenDoc;
        private System.Windows.Forms.ToolStripMenuItem mSaveDoc;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem mFullExtent;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem mLayerList;
        private System.Windows.Forms.ContextMenuStrip MenuDoc;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem mAddLayer;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ToolStripMenuItem mVisible;
        private System.Windows.Forms.ToolStripMenuItem mSelectable;
        private System.Windows.Forms.ToolStripMenuItem mLabel;
        private System.Windows.Forms.ToolStripMenuItem mFieldIndex;
        private System.Windows.Forms.ToolStripTextBox tbLayerName;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator12;
        private System.Windows.Forms.ToolStripMenuItem mAttributeQuery;
        private System.Windows.Forms.ToolStripMenuItem tZoomToLayer;
    }
}
