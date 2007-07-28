namespace ImageViewerCE {
    partial class ImageViewerCEForm {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ImageViewerCEForm));
            this.treeView = new System.Windows.Forms.TreeView();
            this.menuAndBrowserIcons = new System.Windows.Forms.ImageList();
            this.thumbnailsToolBar = new System.Windows.Forms.ToolBar();
            this.browserButton = new System.Windows.Forms.ToolBarButton();
            this.fullscreenToolBar = new System.Windows.Forms.ToolBar();
            this.thumbnailsButton = new System.Windows.Forms.ToolBarButton();
            this.rotateButton = new System.Windows.Forms.ToolBarButton();
            this.SuspendLayout();
            // 
            // treeView
            // 
            this.treeView.ImageIndex = 0;
            this.treeView.ImageList = this.menuAndBrowserIcons;
            this.treeView.Location = new System.Drawing.Point(3, 164);
            this.treeView.Name = "treeView";
            this.treeView.SelectedImageIndex = 0;
            this.treeView.Size = new System.Drawing.Size(234, 100);
            this.treeView.TabIndex = 0;
            this.treeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView_AfterSelect);
            this.menuAndBrowserIcons.Images.Clear();
            this.menuAndBrowserIcons.Images.Add(((System.Drawing.Image)(resources.GetObject("resource"))));
            this.menuAndBrowserIcons.Images.Add(((System.Drawing.Image)(resources.GetObject("resource1"))));
            this.menuAndBrowserIcons.Images.Add(((System.Drawing.Image)(resources.GetObject("resource2"))));
            this.menuAndBrowserIcons.Images.Add(((System.Drawing.Image)(resources.GetObject("resource3"))));
            // 
            // thumbnailsToolBar
            // 
            this.thumbnailsToolBar.Buttons.Add(this.browserButton);
            this.thumbnailsToolBar.ImageList = this.menuAndBrowserIcons;
            this.thumbnailsToolBar.Name = "thumbnailsToolBar";
            this.thumbnailsToolBar.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.thumbnailsToolBar_ButtonClick);
            // 
            // browserButton
            // 
            this.browserButton.ImageIndex = 0;
            this.browserButton.Pushed = true;
            this.browserButton.ToolTipText = "Show Browser";
            // 
            // fullscreenToolBar
            // 
            this.fullscreenToolBar.Buttons.Add(this.thumbnailsButton);
            this.fullscreenToolBar.Buttons.Add(this.rotateButton);
            this.fullscreenToolBar.ImageList = this.menuAndBrowserIcons;
            this.fullscreenToolBar.Name = "fullscreenToolBar";
            this.fullscreenToolBar.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.fullscreenToolBar_ButtonClick);
            // 
            // thumbnailsButton
            // 
            this.thumbnailsButton.ImageIndex = 2;
            // 
            // rotateButton
            // 
            this.rotateButton.ImageIndex = 3;
            this.rotateButton.Style = System.Windows.Forms.ToolBarButtonStyle.ToggleButton;
            // 
            // ImageViewerCEForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(240, 268);
            this.Controls.Add(this.thumbnailsToolBar);
            this.Controls.Add(this.treeView);
            this.KeyPreview = true;
            this.Name = "ImageViewerCEForm";
            this.Text = "ImageViewerCE";
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ImageViewerCEForm_MouseUp);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ImageViewerCEForm_MouseMove);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ImageViewerCEForm_KeyDown);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ImageViewerCEForm_MouseDown);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TreeView treeView;
        private System.Windows.Forms.ImageList menuAndBrowserIcons;
        private System.Windows.Forms.ToolBar thumbnailsToolBar;
        private System.Windows.Forms.ToolBarButton browserButton;
        private System.Windows.Forms.ToolBar fullscreenToolBar;
        private System.Windows.Forms.ToolBarButton thumbnailsButton;
        private System.Windows.Forms.ToolBarButton rotateButton;

    }
}

