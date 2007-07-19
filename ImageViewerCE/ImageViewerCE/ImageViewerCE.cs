using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;


namespace ImageViewerCE {
    public partial class ImageViewerCEForm : Form {
        volatile bool workingThreadRunning;
        volatile bool killWorkingThread;

        Size previewAreaSize;
        Rectangle previewAreaOnThumbnailsImageRectangle;
        Rectangle previewAreaOnScreenRectangle;

        Size singleThumbnailImageSize;
        Rectangle singleThumbnailImageRectangle;
        Size singleThumbnailWithSpacingSize;
        Rectangle targetOnThumbnailsImageRectangle;

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

        int lastValidX;
        int firstVisibleY;
        int lastVisibleY;
        
        public ImageViewerCEForm() {
            InitializeComponent();

            workingThreadRunning = false;
            killWorkingThread = false;

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

            // Vorsicht: instabiler Synchronizations HACK !!! (lock Statement reicht hier
            // nicht und ich hab keine Ahnung wie die "höherwertigen" Konstrukte bei C#
            // heißen)
            killWorkingThread = true;
            Thread.Sleep(0);
            if (workingThreadRunning) {                
                for(int i=0; i<1000; i++) {
                    Thread.Sleep(0);
                    if (!workingThreadRunning)
                        break;
                }
            }
            killWorkingThread = false;

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
            targetOnThumbnailsImageRectangle = new Rectangle(thumbnailSpacing, thumbnailSpacing, singleThumbnailImageSize.Width, singleThumbnailImageSize.Height);

            lastValidX = thumbnailsImage.Width - singleThumbnailWithSpacingSize.Width;
            firstVisibleY = previewAreaOnThumbnailsImageRectangle.Y - singleThumbnailImageSize.Height + 1;
            lastVisibleY = previewAreaOnThumbnailsImageRectangle.Y + previewAreaOnThumbnailsImageRectangle.Height - 1;

            WaitCallback w = new WaitCallback(fillThumbnailsImageFromFolder_Callback);
            ThreadPool.QueueUserWorkItem(w, imageFilenames);
        }

        private void fillThumbnailsImageFromFolder_Callback(object val) {
            workingThreadRunning = true;

            List<string> imageFilenames = (List<string>)val;
            foreach (string imageFilename in imageFilenames) {
                if (killWorkingThread) {
                    workingThreadRunning = false;
                    return;
                }                

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

                if (killWorkingThread) {
                    workingThreadRunning = false;
                    return;
                }     
                this.Invoke(new WaitCallback(DrawOnThumbnailsImage), singleThumbnailImage);
                
                //thumbnailsImageG.DrawImage(singleThumbnailImage, targetOnThumbnailsImageRectangle, singleThumbnailImageRectangle, GraphicsUnit.Pixel);
                singleThumbnailImage.Dispose();

                if (targetOnThumbnailsImageRectangle.Y > firstVisibleY
                        && targetOnThumbnailsImageRectangle.Y < lastVisibleY)
                    this.Invoke(new EventHandler((Draw_Event)));

                targetOnThumbnailsImageRectangle.X += singleThumbnailWithSpacingSize.Width;
                if (targetOnThumbnailsImageRectangle.X > lastValidX) {
                    targetOnThumbnailsImageRectangle.X = thumbnailSpacing;
                    targetOnThumbnailsImageRectangle.Y += singleThumbnailWithSpacingSize.Height;
                }
                
            }
            if (killWorkingThread) {
                workingThreadRunning = false;
                return;
            }   
            this.Invoke(new EventHandler((Draw_Event)));

            workingThreadRunning = false;

        }

        public void DrawOnThumbnailsImage(object val) {
            Bitmap singleThumbnailImage = (Bitmap) val;
            thumbnailsImageG.DrawImage(singleThumbnailImage, targetOnThumbnailsImageRectangle, singleThumbnailImageRectangle, GraphicsUnit.Pixel);
        }

        protected override void OnPaint(PaintEventArgs e) {
            Refresh();
        }

        public override void Refresh() {
            Draw();
        }

        public void Draw_Event(object sender, EventArgs e) {
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
            treeView.Visible = !treeView.Visible;
        }
      
    }
}