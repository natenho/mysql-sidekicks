namespace MySqlSideKicks.Win
{
    partial class SessionForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SessionForm));
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.filterOption = new System.Windows.Forms.GroupBox();
            this.filterByDefinition = new System.Windows.Forms.RadioButton();
            this.filterByName = new System.Windows.Forms.RadioButton();
            this.searchBox = new System.Windows.Forms.TextBox();
            this.objectExplorerListBox = new System.Windows.Forms.ListBox();
            this.editor = new ScintillaNET.Scintilla();
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this.navigateBackwardToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.navigateForwardToolStripButton = new System.Windows.Forms.ToolStripButton();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.filterOption.SuspendLayout();
            this.toolStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer
            // 
            this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer.Location = new System.Drawing.Point(0, 25);
            this.splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.filterOption);
            this.splitContainer.Panel1.Controls.Add(this.searchBox);
            this.splitContainer.Panel1.Controls.Add(this.objectExplorerListBox);
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.editor);
            this.splitContainer.Size = new System.Drawing.Size(1021, 661);
            this.splitContainer.SplitterDistance = 340;
            this.splitContainer.TabIndex = 3;
            this.splitContainer.TabStop = false;
            // 
            // filterOption
            // 
            this.filterOption.Controls.Add(this.filterByDefinition);
            this.filterOption.Controls.Add(this.filterByName);
            this.filterOption.Location = new System.Drawing.Point(3, 26);
            this.filterOption.Name = "filterOption";
            this.filterOption.Size = new System.Drawing.Size(149, 52);
            this.filterOption.TabIndex = 3;
            this.filterOption.TabStop = false;
            this.filterOption.Text = "Filter By";
            // 
            // filterByDefinition
            // 
            this.filterByDefinition.AutoSize = true;
            this.filterByDefinition.Location = new System.Drawing.Point(69, 20);
            this.filterByDefinition.Name = "filterByDefinition";
            this.filterByDefinition.Size = new System.Drawing.Size(69, 17);
            this.filterByDefinition.TabIndex = 0;
            this.filterByDefinition.Text = "Definition";
            this.filterByDefinition.UseVisualStyleBackColor = true;
            this.filterByDefinition.CheckedChanged += new System.EventHandler(this.control_SearchPerformed);
            // 
            // filterByName
            // 
            this.filterByName.AutoSize = true;
            this.filterByName.Checked = true;
            this.filterByName.Location = new System.Drawing.Point(10, 20);
            this.filterByName.Name = "filterByName";
            this.filterByName.Size = new System.Drawing.Size(53, 17);
            this.filterByName.TabIndex = 0;
            this.filterByName.TabStop = true;
            this.filterByName.Text = "Name";
            this.filterByName.UseVisualStyleBackColor = true;
            this.filterByName.CheckedChanged += new System.EventHandler(this.control_SearchPerformed);
            // 
            // searchBox
            // 
            this.searchBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.searchBox.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.searchBox.Location = new System.Drawing.Point(0, 0);
            this.searchBox.Name = "searchBox";
            this.searchBox.Size = new System.Drawing.Size(340, 20);
            this.searchBox.TabIndex = 2;
            this.searchBox.TextChanged += new System.EventHandler(this.control_SearchPerformed);
            this.searchBox.Enter += new System.EventHandler(this.control_SearchPerformed);
            // 
            // objectExplorerListBox
            // 
            this.objectExplorerListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.objectExplorerListBox.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.objectExplorerListBox.FormattingEnabled = true;
            this.objectExplorerListBox.Location = new System.Drawing.Point(0, 84);
            this.objectExplorerListBox.Name = "objectExplorerListBox";
            this.objectExplorerListBox.Size = new System.Drawing.Size(340, 576);
            this.objectExplorerListBox.TabIndex = 1;
            this.objectExplorerListBox.SelectedIndexChanged += new System.EventHandler(this.objectExplorerListBox_SelectedIndexChanged);
            this.objectExplorerListBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.objectExplorerListBox_KeyPress);
            // 
            // editor
            // 
            this.editor.CaretLineBackColor = System.Drawing.Color.Ivory;
            this.editor.CaretLineVisible = true;
            this.editor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.editor.IndentationGuides = ScintillaNET.IndentView.Real;
            this.editor.Lexer = ScintillaNET.Lexer.Sql;
            this.editor.Location = new System.Drawing.Point(0, 0);
            this.editor.Name = "editor";
            this.editor.Size = new System.Drawing.Size(677, 661);
            this.editor.TabIndex = 3;
            this.editor.UseTabs = true;
            this.editor.HotspotClick += new System.EventHandler<ScintillaNET.HotspotClickEventArgs>(this.editor_HotspotClick);
            this.editor.UpdateUI += new System.EventHandler<ScintillaNET.UpdateUIEventArgs>(this.editor_UpdateUI);
            this.editor.KeyDown += new System.Windows.Forms.KeyEventHandler(this.editor_KeyDown);
            this.editor.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.editor_KeyPress);
            this.editor.MouseMove += new System.Windows.Forms.MouseEventHandler(this.editor_MouseMove);
            // 
            // toolStrip
            // 
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.navigateBackwardToolStripButton,
            this.navigateForwardToolStripButton});
            this.toolStrip.Location = new System.Drawing.Point(0, 0);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.Size = new System.Drawing.Size(1021, 25);
            this.toolStrip.TabIndex = 5;
            this.toolStrip.Text = "toolStrip1";
            // 
            // navigateBackwardToolStripButton
            // 
            this.navigateBackwardToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.navigateBackwardToolStripButton.Enabled = false;
            this.navigateBackwardToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("navigateBackwardToolStripButton.Image")));
            this.navigateBackwardToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.navigateBackwardToolStripButton.Name = "navigateBackwardToolStripButton";
            this.navigateBackwardToolStripButton.Size = new System.Drawing.Size(23, 22);
            this.navigateBackwardToolStripButton.Text = "Navigate Backward";
            this.navigateBackwardToolStripButton.Click += new System.EventHandler(this.navigateBackwardSplitButton_Click);
            // 
            // navigateForwardToolStripButton
            // 
            this.navigateForwardToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.navigateForwardToolStripButton.Enabled = false;
            this.navigateForwardToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("navigateForwardToolStripButton.Image")));
            this.navigateForwardToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.navigateForwardToolStripButton.Name = "navigateForwardToolStripButton";
            this.navigateForwardToolStripButton.Size = new System.Drawing.Size(23, 22);
            this.navigateForwardToolStripButton.Text = "Navigate Forward";
            this.navigateForwardToolStripButton.Click += new System.EventHandler(this.navigateForwardToolStripButton_Click);
            // 
            // SessionForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1021, 686);
            this.Controls.Add(this.splitContainer);
            this.Controls.Add(this.toolStrip);
            this.KeyPreview = true;
            this.Name = "SessionForm";
            this.Text = "MySqlSideKicks";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MainForm_KeyDown);
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel1.PerformLayout();
            this.splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            this.filterOption.ResumeLayout(false);
            this.filterOption.PerformLayout();
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.ListBox objectExplorerListBox;
        private ScintillaNET.Scintilla editor;
        private System.Windows.Forms.TextBox searchBox;
        private System.Windows.Forms.GroupBox filterOption;
        private System.Windows.Forms.RadioButton filterByDefinition;
        private System.Windows.Forms.RadioButton filterByName;
        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripButton navigateForwardToolStripButton;
        private System.Windows.Forms.ToolStripButton navigateBackwardToolStripButton;
    }
}

