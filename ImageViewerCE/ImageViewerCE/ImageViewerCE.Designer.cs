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
            this.settingsButton = new System.Windows.Forms.ToolBarButton();
            this.fullscreenToolBar = new System.Windows.Forms.ToolBar();
            this.thumbnailsButton = new System.Windows.Forms.ToolBarButton();
            this.seperator1 = new System.Windows.Forms.ToolBarButton();
            this.rotateButton = new System.Windows.Forms.ToolBarButton();
            this.seperator2 = new System.Windows.Forms.ToolBarButton();
            this.backwardButton = new System.Windows.Forms.ToolBarButton();
            this.forwardButton = new System.Windows.Forms.ToolBarButton();
            this.seperator3 = new System.Windows.Forms.ToolBarButton();
            this.zoomInButton = new System.Windows.Forms.ToolBarButton();
            this.zoomOutButton = new System.Windows.Forms.ToolBarButton();
            this.normalZoomButton = new System.Windows.Forms.ToolBarButton();
            this.rubberBandButton = new System.Windows.Forms.ToolBarButton();
            this.grayScaleButton = new System.Windows.Forms.ToolBarButton();
            this.settingsPanel = new System.Windows.Forms.Panel();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.labelThumbnailsPerLineCount = new System.Windows.Forms.Label();
            this.thumbnailsPerLinetrackBar = new System.Windows.Forms.TrackBar();
            this.labelThumbnailsPerLine = new System.Windows.Forms.Label();
            this.labelSettings = new System.Windows.Forms.Label();
            this.settingsPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // treeView
            // 
            this.treeView.ImageIndex = 0;
            this.treeView.ImageList = this.menuAndBrowserIcons;
            this.treeView.Indent = 35;
            this.treeView.Location = new System.Drawing.Point(6, 328);
            this.treeView.Name = "treeView";
            this.treeView.SelectedImageIndex = 0;
            this.treeView.Size = new System.Drawing.Size(468, 200);
            this.treeView.TabIndex = 0;
            this.treeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView_AfterSelect);
            // 
            // menuAndBrowserIcons
            // 
            this.menuAndBrowserIcons.ImageSize = new System.Drawing.Size(32, 32);
            this.menuAndBrowserIcons.Images.Clear();
            this.menuAndBrowserIcons.Images.Add(((System.Drawing.Image)(resources.GetObject("resource"))));
            this.menuAndBrowserIcons.Images.Add(((System.Drawing.Image)(resources.GetObject("resource1"))));
            this.menuAndBrowserIcons.Images.Add(((System.Drawing.Image)(resources.GetObject("resource2"))));
            this.menuAndBrowserIcons.Images.Add(((System.Drawing.Image)(resources.GetObject("resource3"))));
            this.menuAndBrowserIcons.Images.Add(((System.Drawing.Image)(resources.GetObject("resource4"))));
            this.menuAndBrowserIcons.Images.Add(((System.Drawing.Image)(resources.GetObject("resource5"))));
            this.menuAndBrowserIcons.Images.Add(((System.Drawing.Image)(resources.GetObject("resource6"))));
            this.menuAndBrowserIcons.Images.Add(((System.Drawing.Image)(resources.GetObject("resource7"))));
            this.menuAndBrowserIcons.Images.Add(((System.Drawing.Image)(resources.GetObject("resource8"))));
            this.menuAndBrowserIcons.Images.Add(((System.Drawing.Image)(resources.GetObject("resource9"))));
            this.menuAndBrowserIcons.Images.Add(((System.Drawing.Image)(resources.GetObject("resource10"))));
            this.menuAndBrowserIcons.Images.Add(((System.Drawing.Image)(resources.GetObject("resource11"))));
            // 
            // thumbnailsToolBar
            // 
            this.thumbnailsToolBar.Buttons.Add(this.browserButton);
            this.thumbnailsToolBar.Buttons.Add(this.settingsButton);
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
            // settingsButton
            // 
            this.settingsButton.ImageIndex = 4;
            // 
            // fullscreenToolBar
            // 
            this.fullscreenToolBar.Buttons.Add(this.thumbnailsButton);
            this.fullscreenToolBar.Buttons.Add(this.seperator1);
            this.fullscreenToolBar.Buttons.Add(this.seperator2);
            this.fullscreenToolBar.Buttons.Add(this.backwardButton);
            this.fullscreenToolBar.Buttons.Add(this.forwardButton);
            this.fullscreenToolBar.Buttons.Add(this.seperator3);
            this.fullscreenToolBar.Buttons.Add(this.zoomInButton);
            this.fullscreenToolBar.Buttons.Add(this.zoomOutButton);
            this.fullscreenToolBar.Buttons.Add(this.normalZoomButton);
            this.fullscreenToolBar.Buttons.Add(this.rubberBandButton);
            this.fullscreenToolBar.ImageList = this.menuAndBrowserIcons;
            this.fullscreenToolBar.Name = "fullscreenToolBar";
            this.fullscreenToolBar.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.fullscreenToolBar_ButtonClick);
            // 
            // thumbnailsButton
            // 
            this.thumbnailsButton.ImageIndex = 2;
            // 
            // seperator1
            // 
            this.seperator1.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
            // 
            // rotateButton
            // 
            this.rotateButton.ImageIndex = 3;
            this.rotateButton.Style = System.Windows.Forms.ToolBarButtonStyle.ToggleButton;
            // 
            // seperator2
            // 
            this.seperator2.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
            // 
            // backwardButton
            // 
            this.backwardButton.ImageIndex = 6;
            // 
            // forwardButton
            // 
            this.forwardButton.ImageIndex = 5;
            // 
            // seperator3
            // 
            this.seperator3.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
            // 
            // zoomInButton
            // 
            this.zoomInButton.ImageIndex = 8;
            // 
            // zoomOutButton
            // 
            this.zoomOutButton.ImageIndex = 9;
            // 
            // normalZoomButton
            // 
            this.normalZoomButton.ImageIndex = 10;
            // 
            // rubberBandButton
            // 
            this.rubberBandButton.ImageIndex = 11;
            this.rubberBandButton.Style = System.Windows.Forms.ToolBarButtonStyle.ToggleButton;
            // 
            // grayScaleButton
            // 
            this.grayScaleButton.ImageIndex = 7;
            this.grayScaleButton.Style = System.Windows.Forms.ToolBarButtonStyle.ToggleButton;
            // 
            // settingsPanel
            // 
            this.settingsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.settingsPanel.BackColor = System.Drawing.SystemColors.ControlLight;
            this.settingsPanel.Controls.Add(this.buttonCancel);
            this.settingsPanel.Controls.Add(this.buttonOK);
            this.settingsPanel.Controls.Add(this.labelThumbnailsPerLineCount);
            this.settingsPanel.Controls.Add(this.thumbnailsPerLinetrackBar);
            this.settingsPanel.Controls.Add(this.labelThumbnailsPerLine);
            this.settingsPanel.Controls.Add(this.labelSettings);
            this.settingsPanel.Location = new System.Drawing.Point(6, 6);
            this.settingsPanel.Name = "settingsPanel";
            this.settingsPanel.Size = new System.Drawing.Size(464, 310);
            this.settingsPanel.Visible = false;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(157, 254);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(144, 40);
            this.buttonCancel.TabIndex = 5;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.Location = new System.Drawing.Point(307, 254);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(144, 40);
            this.buttonOK.TabIndex = 4;
            this.buttonOK.Text = "OK";
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // labelThumbnailsPerLineCount
            // 
            this.labelThumbnailsPerLineCount.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.labelThumbnailsPerLineCount.Font = new System.Drawing.Font("Tahoma", 11F, System.Drawing.FontStyle.Regular);
            this.labelThumbnailsPerLineCount.Location = new System.Drawing.Point(237, 117);
            this.labelThumbnailsPerLineCount.Name = "labelThumbnailsPerLineCount";
            this.labelThumbnailsPerLineCount.Size = new System.Drawing.Size(214, 36);
            this.labelThumbnailsPerLineCount.Text = "4";
            this.labelThumbnailsPerLineCount.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // thumbnailsPerLinetrackBar
            // 
            this.thumbnailsPerLinetrackBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.thumbnailsPerLinetrackBar.LargeChange = 1;
            this.thumbnailsPerLinetrackBar.Location = new System.Drawing.Point(4, 156);
            this.thumbnailsPerLinetrackBar.Minimum = 1;
            this.thumbnailsPerLinetrackBar.Name = "thumbnailsPerLinetrackBar";
            this.thumbnailsPerLinetrackBar.Size = new System.Drawing.Size(457, 45);
            this.thumbnailsPerLinetrackBar.TabIndex = 2;
            this.thumbnailsPerLinetrackBar.Value = 4;
            this.thumbnailsPerLinetrackBar.ValueChanged += new System.EventHandler(this.thumbnailsPerLinetrackBar_ValueChanged);
            // 
            // labelThumbnailsPerLine
            // 
            this.labelThumbnailsPerLine.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.labelThumbnailsPerLine.Location = new System.Drawing.Point(4, 120);
            this.labelThumbnailsPerLine.Name = "labelThumbnailsPerLine";
            this.labelThumbnailsPerLine.Size = new System.Drawing.Size(352, 33);
            this.labelThumbnailsPerLine.Text = "Thumbnails per line:";
            // 
            // labelSettings
            // 
            this.labelSettings.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.labelSettings.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular);
            this.labelSettings.Location = new System.Drawing.Point(4, 4);
            this.labelSettings.Name = "labelSettings";
            this.labelSettings.Size = new System.Drawing.Size(457, 44);
            this.labelSettings.Text = "Settings";
            this.labelSettings.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // ImageViewerCEForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(192F, 192F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(480, 536);
            this.Controls.Add(this.settingsPanel);
            this.Controls.Add(this.thumbnailsToolBar);
            this.Controls.Add(this.treeView);
            this.KeyPreview = true;
            this.Location = new System.Drawing.Point(0, 52);
            this.Name = "ImageViewerCEForm";
            this.Text = "ImageViewerCE";
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ImageViewerCEForm_MouseUp);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ImageViewerCEForm_MouseMove);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ImageViewerCEForm_KeyDown);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ImageViewerCEForm_MouseDown);
            this.settingsPanel.ResumeLayout(false);
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
        private System.Windows.Forms.ToolBarButton grayScaleButton;
        private System.Windows.Forms.ToolBarButton settingsButton;
        private System.Windows.Forms.Panel settingsPanel;
        private System.Windows.Forms.Label labelSettings;
        private System.Windows.Forms.TrackBar thumbnailsPerLinetrackBar;
        private System.Windows.Forms.Label labelThumbnailsPerLine;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Label labelThumbnailsPerLineCount;
        private System.Windows.Forms.ToolBarButton seperator1;
        private System.Windows.Forms.ToolBarButton seperator2;
        private System.Windows.Forms.ToolBarButton seperator3;
        private System.Windows.Forms.ToolBarButton backwardButton;
        private System.Windows.Forms.ToolBarButton forwardButton;
        private System.Windows.Forms.ToolBarButton zoomInButton;
        private System.Windows.Forms.ToolBarButton zoomOutButton;
        private System.Windows.Forms.ToolBarButton normalZoomButton;
        private System.Windows.Forms.ToolBarButton rubberBandButton;

    }
}

