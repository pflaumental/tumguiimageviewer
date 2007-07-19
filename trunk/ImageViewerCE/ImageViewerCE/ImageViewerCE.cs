using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;


namespace ImageViewerCE {
    public partial class ImageViewerCEForm : Form {
        Size previewAreaSize;
        Rectangle previewAreaOnThumbnailsImageRectangle;
        Rectangle previewAreaOnScreenRectangle;

        Size singleThumbnailImageSize;
        Rectangle singleThumbnailImageRectangle;
        Size singleThumbnailWithSpacingSize;

        int thumbnailSpacing;

        int thumbnailsPerLine;

        int thumbnailsLineCount;
        
        Graphics screenG;

        Bitmap thumbnailsImage;
        Graphics thumbnailsImageG;
        
        string currentDirectory;
        string currentDirectoryIdString;
        string tempDirectory;

        Brush backgroundBrush;
        Color backgroundColor;

        public string standardDirectory = "\\";
        
        public ImageViewerCEForm() {
            InitializeComponent();

            previewAreaSize = new Size(ClientSize.Width, ClientSize.Height);
            previewAreaOnThumbnailsImageRectangle = new Rectangle(0, 0, previewAreaSize.Width, previewAreaSize.Height);
            previewAreaOnScreenRectangle = new Rectangle(0, 0, previewAreaSize.Width, previewAreaSize.Height);

            backgroundColor = Color.Black;
            backgroundBrush = new SolidBrush(backgroundColor);
            screenG = this.CreateGraphics();

            thumbnailsPerLine = 4;
            thumbnailSpacing = 1;

            int singleThumbnailWith = (ClientSize.Width - (thumbnailsPerLine + 1) * thumbnailSpacing)
                    / thumbnailsPerLine;
            singleThumbnailImageSize = new Size(singleThumbnailWith, singleThumbnailWith);
            singleThumbnailWithSpacingSize = new Size(singleThumbnailImageSize.Width + thumbnailSpacing, singleThumbnailImageSize.Height + thumbnailSpacing);
            singleThumbnailImageRectangle = new Rectangle(0, 0, singleThumbnailImageSize.Width, singleThumbnailImageSize.Height);
            currentDirectory = "\\storage card";
            currentDirectoryIdString = currentDirectory.Replace('\\', '_') + "__";
            string tempPath = System.IO.Path.GetTempPath();
            tempDirectory = tempPath + "thumbnails";
            if (!System.IO.Directory.Exists(tempDirectory))
                System.IO.Directory.CreateDirectory(tempDirectory);

            screenG.Clear(backgroundColor);
            DrawTreeView(standardDirectory);
        }

        private void ChangeDirectory(string newDirectory) {
            currentDirectory = newDirectory;
            currentDirectoryIdString = currentDirectory.Replace('\\', '_') + "__";   
        }

        private void DrawTreeView(string directory) {
            treeView.Nodes.Add(directory);
            treeView.Nodes[0].Tag = directory;
            DrawTreeView(directory, treeView.Nodes[0].Nodes);
        }

        private void DrawTreeView(string directory, TreeNodeCollection nodes) {           
            string[] dirs = Directory.GetDirectories(directory);
            for(int i=0; i < dirs.Length; i++) {
                nodes.Add(Path.GetFileName(dirs[i]));
                nodes[i].Tag = dirs[i];
                nodes[i].ImageIndex = GetImagesCount(dirs[i]) == 0 ? 0 : 1;
                DrawTreeView(dirs[i], nodes[i].Nodes);
            }     
        }

        private int GetImagesCount(string directory) {
            int count = 0;
            count = Directory.GetFiles(directory, "*.jpg").Length;
            count += Directory.GetFiles(directory, "*.bmp").Length;
            count += Directory.GetFiles(directory, "*.png").Length;
            return count;   
        }


        private void treeView_AfterSelect(object sender, TreeViewEventArgs e) {
            ChangeDirectory(treeView.SelectedNode.Tag.ToString());
            thumbnailsImageFromCurrentFolder();
        }


        private void thumbnailsImageFromCurrentFolder() {
            List<string> imageFilenames = new List<string>();
            imageFilenames.AddRange(System.IO.Directory.GetFiles(currentDirectory, "*.jpg"));
            imageFilenames.AddRange(System.IO.Directory.GetFiles(currentDirectory, "*.jpeg"));
            imageFilenames.AddRange(System.IO.Directory.GetFiles(currentDirectory, "*.tiff"));
            imageFilenames.AddRange(System.IO.Directory.GetFiles(currentDirectory, "*.tif"));
            imageFilenames.AddRange(System.IO.Directory.GetFiles(currentDirectory, "*.png"));
            imageFilenames.AddRange(System.IO.Directory.GetFiles(currentDirectory, "*.gif"));
            imageFilenames.AddRange(System.IO.Directory.GetFiles(currentDirectory, "*.bmp"));
            
            if (imageFilenames.Count <= 0) {
                thumbnailsImage = null;
                Draw();
                return;
            }

            thumbnailsLineCount = (int)Math.Ceiling((double)imageFilenames.Count / thumbnailsPerLine);
            thumbnailsImage = new Bitmap(ClientSize.Width, singleThumbnailWithSpacingSize.Height * thumbnailsLineCount + 1);
            thumbnailsImageG = Graphics.FromImage(thumbnailsImage);
            thumbnailsImageG.FillRectangle(backgroundBrush, new Rectangle(0, 0, thumbnailsImage.Width, thumbnailsImage.Height));
            Rectangle targetOnThumbnailsImageRectangle = new Rectangle(thumbnailSpacing, thumbnailSpacing, singleThumbnailImageSize.Width, singleThumbnailImageSize.Height);

            int lastValidX = thumbnailsImage.Width - singleThumbnailWithSpacingSize.Width;
            int firstVisibleY = previewAreaOnThumbnailsImageRectangle.Y - singleThumbnailImageSize.Height + 1;
            int lastVisibleY = previewAreaOnThumbnailsImageRectangle.Y + previewAreaOnThumbnailsImageRectangle.Height - 1;
            foreach (string imageFilename in imageFilenames) {
                Bitmap singleThumbnailImage = null;
                Bitmap loadedImage = null;
                string thumbnailTempPath = tempDirectory
                        + "\\"
                        + currentDirectoryIdString
                        + System.IO.Path.GetFileName(imageFilename);
                if (System.IO.File.Exists(thumbnailTempPath)) {
                    loadedImage = new Bitmap(thumbnailTempPath);
                    if (loadedImage.Width < singleThumbnailImageSize.Width
                            || loadedImage.Height < singleThumbnailImageSize.Height) {
                        loadedImage.Dispose();
                        loadedImage = new Bitmap(imageFilename);
                    }
                }
                else {
                    loadedImage = new Bitmap(imageFilename);
                }
                if (loadedImage.Width == singleThumbnailImageSize.Width
                        && loadedImage.Height == singleThumbnailImageSize.Height) {
                    singleThumbnailImage = (Bitmap)loadedImage.Clone();
                    loadedImage.Dispose();
                }
                else {
                    singleThumbnailImage = new Bitmap(singleThumbnailImageSize.Width, singleThumbnailImageSize.Height);
                    Rectangle loadedImageRectangle = new Rectangle(0, 0, loadedImage.Width, loadedImage.Height);
                    Graphics singleThumbnailImageG = Graphics.FromImage(singleThumbnailImage);
                    singleThumbnailImageG.DrawImage(loadedImage, singleThumbnailImageRectangle, loadedImageRectangle, GraphicsUnit.Pixel);
                    loadedImage.Dispose();

                    singleThumbnailImage.Save(thumbnailTempPath, System.Drawing.Imaging.ImageFormat.Bmp);
                }

                thumbnailsImageG.DrawImage(singleThumbnailImage, targetOnThumbnailsImageRectangle, singleThumbnailImageRectangle, GraphicsUnit.Pixel);
                singleThumbnailImage.Dispose();

                if (targetOnThumbnailsImageRectangle.Y > firstVisibleY 
                        && targetOnThumbnailsImageRectangle.Y < lastVisibleY)
                    Draw();

                targetOnThumbnailsImageRectangle.X += singleThumbnailWithSpacingSize.Width;
                if (targetOnThumbnailsImageRectangle.X > lastValidX) {
                    targetOnThumbnailsImageRectangle.X = thumbnailSpacing;
                    targetOnThumbnailsImageRectangle.Y += singleThumbnailWithSpacingSize.Height;
                }
                
            }
            Draw();
        }

        protected override void OnPaint(PaintEventArgs e) {
            Refresh();
        }

        public override void Refresh() {
            Draw();
        }

        public void Draw() {
            if (thumbnailsImage == null) {
                screenG.Clear(backgroundColor);
                this.Update();
                return;
            }
            screenG.Clear(backgroundColor);
            screenG.DrawImage(thumbnailsImage, previewAreaOnScreenRectangle, previewAreaOnThumbnailsImageRectangle, GraphicsUnit.Pixel);
            this.Update();
        }

        private void menuBrowser_Click(object sender, EventArgs e) {
            //if (treeView.Visible) {
            //    previewAreaSize.Height = ClientSize.Height;
            //}
            //else {
            //    previewAreaSize.Height = ClientSize.Height - treeView.Height;
            //}

            treeView.Visible = !treeView.Visible;
        }
      
    }
}