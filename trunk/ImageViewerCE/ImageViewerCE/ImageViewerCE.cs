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
        Size targetSize;
        Rectangle sourceRectangle;
        Rectangle destinationRectangle;
        Size thumbnailSize;
        Size thumbnailDrawStepSize;
        int thumbnailsPerLine;
        int thumbnailsLineCount;
        Graphics screenG;
        Rectangle thumbnailRectangle;
        Graphics thumbnailsG;
        Bitmap thumbnailsImage;
        string currentDirectory;
        string currentDirectoryIdString;
        string tempDirectory;
        Brush backgroundBrush;

        public string standardDirectory = "\\";

        public int previewPicturePerRow = 4;
        public int previewPicturesPerColumn = 3;

        public int borderWidth = 2;


        private int imageWidth;
        private int imageHeight;

        
        public ImageViewerCEForm() {
            InitializeComponent();

            targetSize = new Size(ClientSize.Width, ClientSize.Height - treeView.Height);
            sourceRectangle = new Rectangle(0, 0, targetSize.Width, targetSize.Height);
            destinationRectangle = new Rectangle(0, 0, targetSize.Width, targetSize.Height);

            backgroundBrush = new SolidBrush(Color.Black);
            screenG = this.CreateGraphics();

            thumbnailsPerLine = 4;
            int thumbnailWith = (ClientSize.Width - thumbnailsPerLine - 1) / thumbnailsPerLine;
            thumbnailSize = new Size(thumbnailWith, thumbnailWith);
            thumbnailDrawStepSize = new Size(thumbnailSize.Width + 1, thumbnailSize.Height + 1);
            thumbnailRectangle = new Rectangle(0, 0, thumbnailSize.Width, thumbnailSize.Height);
            currentDirectory = "\\storage card";
            currentDirectoryIdString = currentDirectory.Replace('\\', '_') + "__";
            string tempPath = System.IO.Path.GetTempPath();
            tempDirectory = tempPath + "thumbnails";
            if (!System.IO.Directory.Exists(tempDirectory))
                System.IO.Directory.CreateDirectory(tempDirectory);
            thumbnailsImageFromCurrentFolder();

            //imageWidth = (previewPanel.ClientSize.Width - ((previewPicturePerRow + 1) * borderWidth)) / previewPicturePerRow;
            //imageHeight = (previewPanel.ClientSize.Height - ((previewPicturesPerColumn + 1) * borderWidth)) / previewPicturesPerColumn;


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


            /*
            previewPanel.Controls.Clear();
            
            
            List<string> imageStrings = new List<string>();
            imageStrings.AddRange(Directory.GetFiles(treeView.SelectedNode.Tag.ToString(), "*.jpg"));
            imageStrings.AddRange(Directory.GetFiles(treeView.SelectedNode.Tag.ToString(), "*.bmp"));
            imageStrings.AddRange(Directory.GetFiles(treeView.SelectedNode.Tag.ToString(), "*.png"));

            int imageCount = imageStrings.Count;
            PictureBox[] pictureBoxes = new PictureBox[imageCount];
            
            for (int i = 0; i < imageCount; i++) {
                pictureBoxes[i] = new PictureBox();
           
                pictureBoxes[i].SizeMode = PictureBoxSizeMode.StretchImage;
                pictureBoxes[i].Size = new Size(imageWidth, imageHeight);
                pictureBoxes[i].Location = new Point( ((i % previewPicturePerRow) + 1) * borderWidth + (i % previewPicturePerRow) * imageWidth,
                                                      (((i/4)+1) * borderWidth + (i/4) * imageHeight));
                pictureBoxes[i].Image = new Bitmap(imageStrings[i]);
                previewPanel.Controls.Add(pictureBoxes[i]);
            }
            Update();
            */
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
            if (imageFilenames.Count <= 0)
                return;
            thumbnailsLineCount = (int)Math.Ceiling((double)imageFilenames.Count / thumbnailsPerLine);
            thumbnailsImage = new Bitmap(ClientSize.Width, thumbnailDrawStepSize.Height * thumbnailsLineCount + 1);
            thumbnailsG = Graphics.FromImage(thumbnailsImage);
            thumbnailsG.FillRectangle(backgroundBrush, new Rectangle(0, 0, thumbnailsImage.Width, thumbnailsImage.Height));
            Rectangle targetRectangle = new Rectangle(1, 1, thumbnailSize.Width, thumbnailSize.Height);
            int stopX = thumbnailsImage.Width - thumbnailDrawStepSize.Width;
            foreach (string imageFilename in imageFilenames) {
                Bitmap thumbnailImage = null;
                Bitmap loadedImage = null;
                string thumbnailTempPath = tempDirectory
                        + "\\"
                        + currentDirectoryIdString
                        + System.IO.Path.GetFileName(imageFilename);
                if (false /*System.IO.File.Exists(thumbnailTempPath)*/) {
                    loadedImage = new Bitmap(thumbnailTempPath);
                    if (loadedImage.Width < thumbnailSize.Width
                            || loadedImage.Height < thumbnailSize.Height) {
                        loadedImage.Dispose();
                        loadedImage = new Bitmap(imageFilename);
                    }
                }
                else {
                    loadedImage = new Bitmap(imageFilename);
                }
                if (false /*loadedImage.Width == thumbnailSize.Width
                        && loadedImage.Height == thumbnailSize.Height*/) {
                    thumbnailImage = (Bitmap)loadedImage.Clone();
                    loadedImage.Dispose();
                }
                else {
                    thumbnailImage = new Bitmap(thumbnailSize.Width, thumbnailSize.Height);
                    Rectangle loadedImageRectangle = new Rectangle(0, 0, loadedImage.Width, loadedImage.Height);
                    Graphics thumbnailImageG = Graphics.FromImage(thumbnailImage);
                    thumbnailImageG.DrawImage(loadedImage, thumbnailRectangle, loadedImageRectangle, GraphicsUnit.Pixel);
                    //loadedImage.Dispose();

                    //thumbnailImage.Save(thumbnailTempPath, System.Drawing.Imaging.ImageFormat.Bmp);
                }

                thumbnailsG.DrawImage(thumbnailImage, targetRectangle, thumbnailRectangle, GraphicsUnit.Pixel);
                thumbnailImage.Dispose();
                targetRectangle.X += thumbnailDrawStepSize.Width;
                if (targetRectangle.X > stopX) {
                    targetRectangle.X = 1;
                    targetRectangle.Y += thumbnailDrawStepSize.Height;
                }
                Refresh();
            }
        }

        protected override void OnPaint(PaintEventArgs e) {
            Refresh();
        }

        public override void Refresh() {
            Draw();
        }

        public void Draw() {
            if (thumbnailsImage == null)
                return;
            screenG.DrawImage(thumbnailsImage, destinationRectangle, sourceRectangle, GraphicsUnit.Pixel);
        }

        private void menuBrowser_Click(object sender, EventArgs e) {
            if (treeView.Visible) {
                targetSize.Height = ClientSize.Height;
            }
            else {
                targetSize.Height = ClientSize.Height - treeView.Height;
            }

            treeView.Visible = !treeView.Visible;
        }

        

        private void ImageViewerCEForm_Paint(object sender, PaintEventArgs e) {
            if (thumbnailsImage == null)
                return;
            screenG.DrawImage(thumbnailsImage, destinationRectangle, sourceRectangle, GraphicsUnit.Pixel);
        }

      
    }
}