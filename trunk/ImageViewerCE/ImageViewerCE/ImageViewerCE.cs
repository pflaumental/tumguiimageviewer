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

        List<string> imageFilenames;

        Size previewAreaSize;
        Rectangle previewAreaOnThumbnailsImageRectangle;
        Rectangle previewAreaOnScreenRectangle;

        Size singleThumbnailImageSize;
        Rectangle singleThumbnailImageRectangle;
        Size singleThumbnailWithSpacingSize;
        Rectangle targetOnThumbnailsImageRectangle;

        int thumbnailSpacing;
        int stopperHeight;

        int thumbnailsPerLine;

        int thumbnailsLineCount;
        
        Graphics screenG;

        Bitmap thumbnailsImage;
        Graphics thumbnailsImageG;
        
        string currentDirectory;
        string currentDirectoryIdString;
        string tempDirectory;

        Brush backgroundBrush;
        SolidBrush stopperBrush;        
        Color backgroundColor;
        Color foregroundColor;
        Color[] stopperColors;
        int stopperColorsCnt;

        public string standardDirectory = "\\";

        int lastValidX;
        int firstVisibleY;
        int lastVisibleY;

        // Scrolling/Clicking Stuff:
        int mouseDragStartY;
        int lastMouseX;
        int lastMouseY;
        int mouseWayLength;
        int oldMouseWayLength;
        int oldPreviewAreaOnThumbnailsImageY;
        bool isMouseDown;
        bool scrollStyleIsGoogle;
        readonly int middleAreaStartY;
        readonly int middleAreaEndY;
        
        public ImageViewerCEForm() {
            InitializeComponent();           

            workingThreadRunning = false;
            killWorkingThread = false;

            imageFilenames = null;

            backgroundColor = Color.Black;
            foregroundColor = Color.White;
            backgroundBrush = new SolidBrush(backgroundColor);
            stopperBrush = new SolidBrush(foregroundColor);

            stopperHeight = 50;
            stopperColorsCnt = 50;
            stopperColors = new Color[stopperColorsCnt];
            for (int i = 0; i < stopperColorsCnt; i++) {
                stopperColors[i] = Color.FromArgb((byte)(foregroundColor.R * (((float)stopperColorsCnt - i) / stopperColorsCnt)
                        + backgroundColor.R * ((float)i / stopperColorsCnt)),
                        (byte)(foregroundColor.G * (((float)stopperColorsCnt - i) / stopperColorsCnt)
                        + backgroundColor.G * ((float)i / stopperColorsCnt)),
                        (byte)(foregroundColor.B * (((float)stopperColorsCnt - i) / stopperColorsCnt)
                        + backgroundColor.B * ((float)i / stopperColorsCnt)));
            }

            previewAreaSize = new Size(ClientSize.Width, ClientSize.Height);
            previewAreaOnThumbnailsImageRectangle = new Rectangle(0, stopperHeight, previewAreaSize.Width, previewAreaSize.Height);
            previewAreaOnScreenRectangle = new Rectangle(0, 0, previewAreaSize.Width, previewAreaSize.Height);

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

            // Scrolling/Clicking Stuff
            middleAreaStartY = (int)(0.15f * previewAreaSize.Height);
            middleAreaEndY = (int)(0.85f * previewAreaSize.Height);
            scrollStyleIsGoogle = true;
            isMouseDown = false;
            mouseDragStartY = -1;
            lastMouseX = -1;
            lastMouseY = -1;
            oldPreviewAreaOnThumbnailsImageY = 0;
            mouseWayLength = 0;
            oldMouseWayLength = 0;

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
            imageFilenames = new List<string>();
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

            imageFilenames.Sort();

            thumbnailsLineCount = (int)Math.Ceiling((double)imageFilenames.Count / thumbnailsPerLine);
            thumbnailsImage = new Bitmap(ClientSize.Width, singleThumbnailWithSpacingSize.Height * thumbnailsLineCount + 2 * stopperHeight);
            thumbnailsImageG = Graphics.FromImage(thumbnailsImage);
            drawBackground();            
            targetOnThumbnailsImageRectangle = new Rectangle(thumbnailSpacing, thumbnailSpacing + stopperHeight, singleThumbnailImageSize.Width, singleThumbnailImageSize.Height);

            lastValidX = thumbnailsImage.Width - singleThumbnailWithSpacingSize.Width;
            firstVisibleY = previewAreaOnThumbnailsImageRectangle.Y - singleThumbnailImageSize.Height + 1;
            lastVisibleY = previewAreaOnThumbnailsImageRectangle.Y + previewAreaOnThumbnailsImageRectangle.Height - 1;

            WaitCallback w = new WaitCallback(fillThumbnailsImageFromFolder_Callback);
            ThreadPool.QueueUserWorkItem(w, imageFilenames);
        }

        private void drawBackground() {
            thumbnailsImageG.FillRectangle(backgroundBrush, new Rectangle(0, stopperHeight, thumbnailsImage.Width, thumbnailsImage.Height - stopperHeight));
            int stopperLineHeight = stopperHeight / stopperColorsCnt;
            Rectangle upperStopperLineRect = new Rectangle(0, 0, thumbnailsImage.Width, stopperLineHeight);
            Rectangle lowerStopperLineRect = new Rectangle(0, thumbnailsImage.Height - stopperLineHeight, thumbnailsImage.Width, stopperLineHeight);
            for (int i = 0; i < stopperColorsCnt; i++) {
                stopperBrush.Color = stopperColors[i];
                thumbnailsImageG.FillRectangle(stopperBrush, upperStopperLineRect);
                thumbnailsImageG.FillRectangle(stopperBrush, lowerStopperLineRect);
                upperStopperLineRect.Y += stopperLineHeight;
                lowerStopperLineRect.Y -= stopperLineHeight;
            }
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

        private void DrawOnThumbnailsImage(object val) {
            Bitmap singleThumbnailImage = (Bitmap) val;
            thumbnailsImageG.DrawImage(singleThumbnailImage, targetOnThumbnailsImageRectangle, singleThumbnailImageRectangle, GraphicsUnit.Pixel);
        }

        private void ThumbnailClicked(int imageIndex) {
            // TODO
            MessageBox.Show(imageFilenames[imageIndex] + " was clicked.");
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
       
        private void toolBar_ButtonClick(object sender, ToolBarButtonClickEventArgs e) {
            if (e.Button == browserButton) {
                treeView.Visible = !treeView.Visible;
                e.Button.Pushed = !e.Button.Pushed;
            }
        }

        private void ImageViewerCEForm_MouseDown(object sender, MouseEventArgs e) {
            isMouseDown = true;
            mouseDragStartY = e.Y;
            oldPreviewAreaOnThumbnailsImageY = previewAreaOnThumbnailsImageRectangle.Y;
            mouseWayLength = 0;
            oldMouseWayLength = 0;

            lastMouseX = e.X;
            lastMouseY = e.Y;
        }

        private void ImageViewerCEForm_MouseUp(object sender, MouseEventArgs e) {
            int mouseX = lastMouseX;
            int mouseY = lastMouseY;

            lastMouseX = -1;
            lastMouseY = -1;

            if (mouseWayLength > 3)
                return; // no klick

            // klick

            mouseDragStartY = -1;
            isMouseDown = false;

            int thumbnailsImageMouseY = mouseY + oldPreviewAreaOnThumbnailsImageY;
            int selectedImageColumnIndex = mouseX / singleThumbnailWithSpacingSize.Width;
            if(selectedImageColumnIndex >= thumbnailsPerLine)
                return;
            int selectedImageLineIndex = (thumbnailsImageMouseY - stopperHeight) / singleThumbnailWithSpacingSize.Height;
            if (selectedImageLineIndex < 0 || selectedImageLineIndex >= thumbnailsLineCount)
                return;
            int selectedImageIndex = selectedImageLineIndex * thumbnailsPerLine + selectedImageColumnIndex;
            if (selectedImageIndex >= imageFilenames.Count)
                return;

            ThumbnailClicked(selectedImageIndex);

        }

        private void ImageViewerCEForm_MouseMove(object sender, MouseEventArgs e) {
            // Do nothing if mouse wasn't pressed down before
            if (!isMouseDown)
                return;

            // Calculate mouseWayLength
            mouseWayLength += Math.Abs(e.X - lastMouseX);
            mouseWayLength += Math.Abs(e.Y - lastMouseY);

            // Move previewAreaOnThumbnailsImageRectangle
            int newY;
            if (e.Y < middleAreaStartY) { // Upper area
                // Calculate new previewAreaOnThumbnailsImageY
                int speed = mouseWayLength - oldMouseWayLength;
                if (scrollStyleIsGoogle)
                    newY = previewAreaOnThumbnailsImageRectangle.Y + speed; // Google-Maps-Style
                else
                    newY = previewAreaOnThumbnailsImageRectangle.Y - speed; // Inverse-Style
                mouseDragStartY = e.Y;
                oldPreviewAreaOnThumbnailsImageY = previewAreaOnThumbnailsImageRectangle.Y;
            } else if (e.Y < middleAreaEndY) { // Middle area
                // Calculate new previewAreaOnThumbnailsImageY
                int moveY;
                if (scrollStyleIsGoogle)
                    moveY = mouseDragStartY - e.Y; // Google-Maps-Style
                else
                    moveY = e.Y - mouseDragStartY; // Inverse-Style
                newY = oldPreviewAreaOnThumbnailsImageY + moveY;
            } else { // Bottom area
                // Calculate new previewAreaOnThumbnailsImageY
                int speed = mouseWayLength - oldMouseWayLength;
                if (scrollStyleIsGoogle)
                    newY = previewAreaOnThumbnailsImageRectangle.Y - speed; // Google-Maps-Style
                else
                    newY = previewAreaOnThumbnailsImageRectangle.Y + speed; // Inverse-Style
                mouseDragStartY = e.Y;
                oldPreviewAreaOnThumbnailsImageY = previewAreaOnThumbnailsImageRectangle.Y;
            }
            // Clamp new drawSectionY
            newY = Math.Min(
            newY, thumbnailsImage.Height - previewAreaSize.Height);
            previewAreaOnThumbnailsImageRectangle.Y = Math.Max(
                        newY, 0);

            lastMouseX = e.X;
            lastMouseY = e.Y;
            oldMouseWayLength = mouseWayLength;

            Draw();
        }
    }
}