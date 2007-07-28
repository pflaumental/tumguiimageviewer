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
        volatile bool thumbnailWorkingThreadRunning;
        volatile bool fullscreenWorkingThreadRunning;
        volatile bool killThumbnailWorkingThread;
        volatile bool killFullscreenWorkingThread;
        volatile int currentFullscreenIndexOnBuffer;
        volatile int currentFullscreenIndexOnFilenameList;

        List<string> imageFilenames;
        bool isFullscreenMode;
        bool rotateFullscreenImages;

        int navigationStepDistance;

        Size viewAreaSize;
        float viewAreaRatio;
        Rectangle viewAreaOnThumbnailsImageRectangle;
        Rectangle viewAreaOnScreenRectangle;

        Size singleThumbnailImageSize;
        Rectangle singleThumbnailImageRectangle;
        float singleThumbnailImageRatio;
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

        int imageCountFullscreen; // Muss immer eine ungerade Zahl sein
        int imageWidthFullscreen;
        int imageHeightFullscreen;

        Bitmap[] fullscreenImages;
        
        public ImageViewerCEForm() {
            InitializeComponent();
            killThumbnailWorkingThread = false;
            killFullscreenWorkingThread = false;
            imageCountFullscreen = 5;
            isFullscreenMode = false;
            currentFullscreenIndexOnBuffer = imageCountFullscreen / 2;

            fullscreenImages = new Bitmap[imageCountFullscreen];
            currentFullscreenIndexOnFilenameList = 0;

            rotateFullscreenImages = false;

            this.imageWidthFullscreen = ClientSize.Width;
            this.imageHeightFullscreen = ClientSize.Height;

            thumbnailWorkingThreadRunning = false;
            killThumbnailWorkingThread = false;

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

            viewAreaSize = new Size(ClientSize.Width, ClientSize.Height);
            viewAreaRatio = ((float)viewAreaSize.Width) / viewAreaSize.Height;
            viewAreaOnThumbnailsImageRectangle = new Rectangle(0, stopperHeight, viewAreaSize.Width, viewAreaSize.Height);
            viewAreaOnScreenRectangle = new Rectangle(0, 0, viewAreaSize.Width, viewAreaSize.Height);

            navigationStepDistance = viewAreaSize.Height / 2; // pixel

            screenG = this.CreateGraphics();

            thumbnailsPerLine = 4;
            thumbnailSpacing = 1;

            UpdateThumbnailSize();
            currentDirectory = "\\storage card";
            currentDirectoryIdString = currentDirectory.Replace('\\', '_') + "__";
            string tempPath = System.IO.Path.GetTempPath();
            tempDirectory = tempPath + "thumbnails";
            if (!System.IO.Directory.Exists(tempDirectory))
                System.IO.Directory.CreateDirectory(tempDirectory);

            // Scrolling/Clicking Stuff
            middleAreaStartY = (int)(0.15f * viewAreaSize.Height);
            middleAreaEndY = (int)(0.85f * viewAreaSize.Height);
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

            thumbnailsImageFromCurrentFolder();
        }

        private void thumbnailsImageFromCurrentFolder() {
            KillThumbnailWorkingThread();

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
                DrawThumbnailView();
                return;
            }

            imageFilenames.Sort();

            thumbnailsLineCount = (int)Math.Ceiling((double)imageFilenames.Count / thumbnailsPerLine);
            thumbnailsImage = new Bitmap(ClientSize.Width, singleThumbnailWithSpacingSize.Height * thumbnailsLineCount + 2 * stopperHeight);
            thumbnailsImageG = Graphics.FromImage(thumbnailsImage);
            drawBackground();            
            targetOnThumbnailsImageRectangle = new Rectangle(thumbnailSpacing, thumbnailSpacing + stopperHeight, singleThumbnailImageSize.Width, singleThumbnailImageSize.Height);

            lastValidX = thumbnailsImage.Width - singleThumbnailWithSpacingSize.Width;
            firstVisibleY = viewAreaOnThumbnailsImageRectangle.Y - singleThumbnailImageSize.Height + 1;
            lastVisibleY = viewAreaOnThumbnailsImageRectangle.Y + viewAreaOnThumbnailsImageRectangle.Height - 1;

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
            thumbnailWorkingThreadRunning = true;

            List<string> imageFilenames = (List<string>)val;
            foreach (string imageFilename in imageFilenames) {
                if (killThumbnailWorkingThread) {
                    thumbnailWorkingThreadRunning = false;
                    return;
                }                

                Bitmap singleThumbnailImage = null;
                Bitmap loadedImage = null;
                string thumbnailTempPath = tempDirectory
                        + "\\"
                        + currentDirectoryIdString
                        + System.IO.Path.GetFileName(imageFilename);
                if (System.IO.File.Exists(thumbnailTempPath)) {
                    loadedImage = CreateBitmap(thumbnailTempPath);
                    if (loadedImage.Width < singleThumbnailImageSize.Width
                            || loadedImage.Height < singleThumbnailImageSize.Height) {
                        loadedImage.Dispose();
                        loadedImage = CreateBitmap(imageFilename);
                    }
                }
                else {
                    loadedImage = CreateBitmap(imageFilename);
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

                    float loadedImageRatio = (float)loadedImage.Width / loadedImage.Height;
                    Rectangle destRectangleOnSingleThumbnailImage;
                    if (loadedImageRatio == singleThumbnailImageRatio) {
                        destRectangleOnSingleThumbnailImage = singleThumbnailImageRectangle;
                    } else if (loadedImageRatio < singleThumbnailImageRatio) {
                        int loadedImageOnSingleThumbnailImageWidth = (int)(singleThumbnailImage.Height * loadedImageRatio);
                        destRectangleOnSingleThumbnailImage = new Rectangle((singleThumbnailImage.Width - loadedImageOnSingleThumbnailImageWidth) / 2, 0,
                                loadedImageOnSingleThumbnailImageWidth, singleThumbnailImage.Height);
                    } else {
                        int loadedImageOnSingleThumbnailImageHeight = (int)(singleThumbnailImage.Width / loadedImageRatio);
                        destRectangleOnSingleThumbnailImage = new Rectangle(0, (singleThumbnailImage.Height - loadedImageOnSingleThumbnailImageHeight) / 2,
                                singleThumbnailImage.Width, loadedImageOnSingleThumbnailImageHeight);
                    }

                    singleThumbnailImageG.DrawImage(loadedImage, destRectangleOnSingleThumbnailImage, loadedImageRectangle, GraphicsUnit.Pixel);
                    loadedImage.Dispose();

                    singleThumbnailImage.Save(thumbnailTempPath, System.Drawing.Imaging.ImageFormat.Bmp);
                }

                if (killThumbnailWorkingThread) {
                    thumbnailWorkingThreadRunning = false;
                    return;
                }     
                this.Invoke(new WaitCallback(DrawOnThumbnailsImage), singleThumbnailImage);
                
                //thumbnailsImageG.DrawImage(singleThumbnailImage, targetOnThumbnailsImageRectangle, singleThumbnailImageRectangle, GraphicsUnit.Pixel);
                singleThumbnailImage.Dispose();

                if (targetOnThumbnailsImageRectangle.Y > firstVisibleY
                        && targetOnThumbnailsImageRectangle.Y < lastVisibleY)
                    this.Invoke(new EventHandler((DrawThumbnails_Event)));

                targetOnThumbnailsImageRectangle.X += singleThumbnailWithSpacingSize.Width;
                if (targetOnThumbnailsImageRectangle.X > lastValidX) {
                    targetOnThumbnailsImageRectangle.X = thumbnailSpacing;
                    targetOnThumbnailsImageRectangle.Y += singleThumbnailWithSpacingSize.Height;
                }
                
            }
            if (killThumbnailWorkingThread) {
                thumbnailWorkingThreadRunning = false;
                return;
            }   
            this.Invoke(new EventHandler((DrawThumbnails_Event)));

            thumbnailWorkingThreadRunning = false;

        }

        private void DrawOnThumbnailsImage(object val) {
            Bitmap singleThumbnailImage = (Bitmap) val;
            thumbnailsImageG.DrawImage(singleThumbnailImage, targetOnThumbnailsImageRectangle, singleThumbnailImageRectangle, GraphicsUnit.Pixel);
        }

        private void ThumbnailClicked(int imageIndex) {
            treeView.Visible = false;
            isFullscreenMode = true;
            this.Controls.Remove(thumbnailsToolBar);
            this.Controls.Add(fullscreenToolBar);

            currentFullscreenIndexOnFilenameList = imageIndex;

            ClearFullscreenImages();

            WaitCallback w = new WaitCallback(loadFullscreenImages);
            ThreadPool.QueueUserWorkItem(w, imageIndex);
        }
        
        private void loadFullscreenImages(object val) {
            fullscreenWorkingThreadRunning = true;          
            int imageIndex = (int)val;
            
            int fullscreenImagesMid = imageCountFullscreen / 2;
            currentFullscreenIndexOnBuffer = fullscreenImagesMid;

            int u=0;
            int d=0;
            int x = 0;
            for (int i = 0; i < imageCountFullscreen; i++) {
                if (killFullscreenWorkingThread) {
                    thumbnailWorkingThreadRunning = false;
                    return;
                }                
                if (i % 2 == 0)
                    x = d--;
                else
                    x = ++u;
                int srcIndex = imageIndex + x;
                int destIndex = fullscreenImagesMid + x;
                if (srcIndex < 0 || srcIndex >= imageFilenames.Count) {
                    fullscreenImages[destIndex] = null;
                    continue;
                }
                fullscreenImages[destIndex] = CreateBitmap(imageFilenames[srcIndex], rotateFullscreenImages);
                if (destIndex == currentFullscreenIndexOnBuffer) {
                    if (killFullscreenWorkingThread) {
                        fullscreenWorkingThreadRunning = false;
                        return;
                    }                
                    this.Invoke(new EventHandler((DrawFullscreenView_Event)));
                }
            }
            fullscreenWorkingThreadRunning = false;
        }

        private void loadSingleFullscreenImage(object val) {
            int[] values = (int[])val;
            int destImageIndex = values[0];
            int sourceImageIndex = values[1];
            if (sourceImageIndex < 0 || sourceImageIndex >= imageFilenames.Count) 
                fullscreenImages[destImageIndex] = null;
            else
                fullscreenImages[destImageIndex] = CreateBitmap(imageFilenames[sourceImageIndex], rotateFullscreenImages);
            if (destImageIndex == currentFullscreenIndexOnBuffer)
                this.Invoke(new EventHandler((DrawFullscreenView_Event)));
        }

        private void DrawFullscreenView_Event(object sender, EventArgs e) {
            DrawFullscreenView();
        }

        protected override void OnPaint(PaintEventArgs e) {
            Refresh();
        }

        public override void Refresh() {
            if (isFullscreenMode)
                DrawFullscreenView();
            else
                DrawThumbnailView();
        }

        public void DrawThumbnails_Event(object sender, EventArgs e) {
            DrawThumbnailView();
        }

        private void NavigateFullscreenView(bool forward) {
            int destImageIndex;
            int loadIndexOnFilenameList;
            int loadOffset = imageCountFullscreen / 2;
            if (forward) {
                loadIndexOnFilenameList = currentFullscreenIndexOnFilenameList + loadOffset + 1;
                currentFullscreenIndexOnFilenameList++;
                if (currentFullscreenIndexOnBuffer == imageCountFullscreen - 1) {
                    destImageIndex = currentFullscreenIndexOnBuffer - loadOffset;
                    currentFullscreenIndexOnBuffer = 0;
                }
                else {
                    destImageIndex = currentFullscreenIndexOnBuffer - loadOffset;
                    if (destImageIndex < 0)
                        destImageIndex += imageCountFullscreen;
                    currentFullscreenIndexOnBuffer++;
                }
            }
            else {
                loadIndexOnFilenameList = currentFullscreenIndexOnFilenameList - loadOffset - 1;
                currentFullscreenIndexOnFilenameList--;
                if (currentFullscreenIndexOnBuffer == 0) {
                    destImageIndex = currentFullscreenIndexOnBuffer + loadOffset;
                    currentFullscreenIndexOnBuffer = imageCountFullscreen - 1;
                }
                else {
                    destImageIndex = currentFullscreenIndexOnBuffer + loadOffset;
                    if (destImageIndex >= imageCountFullscreen)
                        destImageIndex -= imageCountFullscreen;
                    currentFullscreenIndexOnBuffer--;
                }
            }
            DrawFullscreenView();
            WaitCallback w = new WaitCallback(loadSingleFullscreenImage);
            ThreadPool.QueueUserWorkItem(w, new int[] { destImageIndex, loadIndexOnFilenameList });
        }

        public void DrawThumbnailView() {
            if (thumbnailsImage == null) {
                screenG.Clear(backgroundColor);
                this.Update();
                return;
            }
            screenG.Clear(backgroundColor);
            screenG.DrawImage(thumbnailsImage, viewAreaOnScreenRectangle, viewAreaOnThumbnailsImageRectangle, GraphicsUnit.Pixel);
            this.Update();
        }

        private void DrawFullscreenView() { 
            Bitmap currentFullscreenImage = fullscreenImages[currentFullscreenIndexOnBuffer];
            if (currentFullscreenImage == null) {
                screenG.Clear(foregroundColor);
                this.Update();
                return;
            }
            Rectangle sourceFullscreenRectangle = new Rectangle(0, 0, currentFullscreenImage.Width, currentFullscreenImage.Height);
            float sourceFullscreenRectangleRatio = ((float)sourceFullscreenRectangle.Width) / sourceFullscreenRectangle.Height;
            Rectangle destFullscreenRectangleOnScreen;
            if (sourceFullscreenRectangleRatio == viewAreaRatio) {
                destFullscreenRectangleOnScreen = viewAreaOnScreenRectangle;
            }
            else if (sourceFullscreenRectangleRatio < viewAreaRatio) {
                int fullscreenImageOnScreenWidth = (int)(viewAreaSize.Height * sourceFullscreenRectangleRatio);
                destFullscreenRectangleOnScreen = new Rectangle((viewAreaSize.Width - fullscreenImageOnScreenWidth) / 2, 0,
                        fullscreenImageOnScreenWidth, viewAreaSize.Height);
            }
            else {
                int fullscreenImageOnScreenHeight = (int)(viewAreaSize.Width / sourceFullscreenRectangleRatio);
                destFullscreenRectangleOnScreen = new Rectangle(0, (viewAreaSize.Height - fullscreenImageOnScreenHeight) / 2,
                        viewAreaSize.Width, fullscreenImageOnScreenHeight);
            }
            screenG.Clear(backgroundColor);
            screenG.DrawImage(currentFullscreenImage, destFullscreenRectangleOnScreen, sourceFullscreenRectangle, GraphicsUnit.Pixel);
            this.Update();
        }
       
        private void thumbnailsToolBar_ButtonClick(object sender, ToolBarButtonClickEventArgs e) {
            if (e.Button == browserButton) {
                treeView.Visible = !treeView.Visible;
                e.Button.Pushed = !e.Button.Pushed;
            } else if (e.Button == settingsButton) {
                settingsPanel.Visible = true;
            }
        }

        private void ImageViewerCEForm_MouseDown(object sender, MouseEventArgs e) {
            if (isFullscreenMode)
                MouseDownInFullscreenMode(e);
            else
                MouseDownInThumbnailMode(e);
        }

        private void MouseDownInFullscreenMode(MouseEventArgs e) { 
        }

        private void MouseDownInThumbnailMode(MouseEventArgs e) {
            isMouseDown = true;
            mouseDragStartY = e.Y;
            oldPreviewAreaOnThumbnailsImageY = viewAreaOnThumbnailsImageRectangle.Y;
            mouseWayLength = 0;
            oldMouseWayLength = 0;

            lastMouseX = e.X;
            lastMouseY = e.Y;
        }

        private void ImageViewerCEForm_MouseUp(object sender, MouseEventArgs e) {
            if (isFullscreenMode)
                MouseUpInFullscreenMode(e);
            else
                MouseUpInThumbnailMode(e);
        }

        private void MouseUpInFullscreenMode(MouseEventArgs e) {            
        }

        private void MouseUpInThumbnailMode(MouseEventArgs e) {
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
            if (isFullscreenMode)
                MouseMoveInFullscreenMode(e);
            else
                MouseMoveInThumbnailMode(e);
        }

        private void MouseMoveInFullscreenMode(MouseEventArgs e) { }

        private void MouseMoveInThumbnailMode(MouseEventArgs e) {
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
                    newY = viewAreaOnThumbnailsImageRectangle.Y + speed; // Google-Maps-Style
                else
                    newY = viewAreaOnThumbnailsImageRectangle.Y - speed; // Inverse-Style
                mouseDragStartY = e.Y;
                oldPreviewAreaOnThumbnailsImageY = viewAreaOnThumbnailsImageRectangle.Y;
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
                    newY = viewAreaOnThumbnailsImageRectangle.Y - speed; // Google-Maps-Style
                else
                    newY = viewAreaOnThumbnailsImageRectangle.Y + speed; // Inverse-Style
                mouseDragStartY = e.Y;
                oldPreviewAreaOnThumbnailsImageY = viewAreaOnThumbnailsImageRectangle.Y;
            }
            // Clamp new drawSectionY
            newY = Math.Min(
            newY, thumbnailsImage.Height - viewAreaSize.Height);
            viewAreaOnThumbnailsImageRectangle.Y = Math.Max(
                        newY, 0);

            lastMouseX = e.X;
            lastMouseY = e.Y;
            oldMouseWayLength = mouseWayLength;

            DrawThumbnailView();
        }

        private void ImageViewerCEForm_KeyDown(object sender, KeyEventArgs e) {
            if ((e.KeyCode == System.Windows.Forms.Keys.Up)) {
                // Rocker Up
                if (isFullscreenMode)
                    NavigateFullscreenView(true);
                else
                    NavigateThumbnailView(true);
            }
            if ((e.KeyCode == System.Windows.Forms.Keys.Down)) {
                // Rocker Down
                if (isFullscreenMode)
                    NavigateFullscreenView(false);
                else
                    NavigateThumbnailView(false);
            }
            if ((e.KeyCode == System.Windows.Forms.Keys.Left)) {
                // Rocker Left
                if (isFullscreenMode)
                    NavigateFullscreenView(false);
            }
            if ((e.KeyCode == System.Windows.Forms.Keys.Right)) {
                // Rocker Right
                if (isFullscreenMode)
                    NavigateFullscreenView(true);                
            }
            if ((e.KeyCode == System.Windows.Forms.Keys.Enter)) {
                // Enter
            }

        }

        private void fullscreenToolBar_ButtonClick(object sender, ToolBarButtonClickEventArgs e) {
            if (e.Button == thumbnailsButton) {
                treeView.Visible = thumbnailsToolBar.Buttons[0].Pushed;
                Controls.Remove(fullscreenToolBar);
                Controls.Add(thumbnailsToolBar);
                KillFullscreenWorkingThread();
            } else if (e.Button == rotateButton) {
                rotateFullscreenImages = e.Button.Pushed;
                KillFullscreenWorkingThread();
                ClearFullscreenImages();
                GC.Collect();
                loadFullscreenImages(currentFullscreenIndexOnFilenameList);
            } else if (e.Button == forwardButton) {
                NavigateFullscreenView(true);
            } else if (e.Button == backwardButton) {
                NavigateFullscreenView(false);
            }

        }

        private Bitmap CreateBitmap(string filename) {
            try {
                return new Bitmap(filename);
            }
            
            catch {
                GC.Collect();
                try {
                    return new Bitmap(filename);
                } catch (Exception e) {
                    MessageBox.Show("Bild " + filename + " konnte nicht ge�ffnet werden.\n Aufgetretener Fehler: \""
                            + e.ToString() + "\"", "Fehler");
                    return new Bitmap(1, 1);
                }
            }
        }

        private Bitmap CreateBitmap(string filename, bool rotate) {
            Bitmap sourceBitmap = CreateBitmap(filename);
            if (rotate) {
                try {
                    Bitmap destBitmap = new Bitmap(sourceBitmap.Height, sourceBitmap.Width);
                    System.Drawing.Imaging.BitmapData destData = destBitmap.LockBits(new Rectangle(0, 0, destBitmap.Width, destBitmap.Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                    IntPtr destAdress = destData.Scan0;
                    int numDestBytes = destData.Width * destData.Height * 3;
                    byte[] destRgbValues = new byte[numDestBytes];

                    System.Drawing.Imaging.BitmapData sourceData = sourceBitmap.LockBits(new Rectangle(0, 0, sourceBitmap.Width, sourceBitmap.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                    IntPtr sourceAdress = sourceData.Scan0;
                    int numSourceBytes = sourceData.Width * sourceData.Height * 3;
                    byte[] sourceRgbValues = new byte[numSourceBytes];
                    System.Runtime.InteropServices.Marshal.Copy(sourceAdress, sourceRgbValues, 0, numSourceBytes);

                    // Rotate image pixelwise
                    int destPos = 0;
                    int sourcePos = 0;
                    for (int sourceY = 0; sourceY < sourceData.Height; sourceY++) {
                        sourcePos = sourceY * sourceData.Width * 3;
                        destPos = sourceY * 3;
                        for (int sourceX = 0; sourceX < sourceData.Width; sourceX++) {
                            destRgbValues[destPos] = sourceRgbValues[sourcePos];
                            destRgbValues[destPos + 1] = sourceRgbValues[sourcePos + 1];
                            destRgbValues[destPos + 2] = sourceRgbValues[sourcePos + 2];

                            sourcePos += 3;
                            destPos += (destData.Width * 3);
                        }
                    }

                    System.Runtime.InteropServices.Marshal.Copy(destRgbValues, 0, destAdress, numDestBytes);

                    sourceBitmap.UnlockBits(sourceData);
                    destBitmap.UnlockBits(destData);

                    return destBitmap;
                } catch (Exception e) {
                    GC.Collect();
                    MessageBox.Show("Bildrotation fehlgeschlagen. Aufgetretener Fehler: \"" + e.ToString() + "\"", "Fehler");
                    return sourceBitmap;
                }
            } else
                return sourceBitmap;
        }

        private void ClearFullscreenImages() {
            for (int i = 0; i < fullscreenImages.Length; i++)
                fullscreenImages[i] = null;
        }

        private void KillThumbnailWorkingThread() {
            // Vorsicht: instabiler Synchronizations HACK !!! (lock Statement reicht hier
            // nicht und ich hab keine Ahnung wie die "h�herwertigen" Konstrukte bei C#
            // hei�en)
            killThumbnailWorkingThread = true;
            Thread.Sleep(0);
            if (thumbnailWorkingThreadRunning) {
                for (int i = 0; i < 20; i++) {
                    Thread.Sleep(100);
                    if (!thumbnailWorkingThreadRunning)
                        break;
                }
            }
            killThumbnailWorkingThread = false;
        }

        private void KillFullscreenWorkingThread() {
            isFullscreenMode = false;
            // Vorsicht: instabiler Synchronizations HACK !!! (lock Statement reicht hier
            // nicht und ich hab keine Ahnung wie die "h�herwertigen" Konstrukte bei C#
            // hei�en)
            killFullscreenWorkingThread = true;
            Thread.Sleep(0);
            if (fullscreenWorkingThreadRunning) {
                for (int i = 0; i < 20; i++) {
                    Thread.Sleep(100);
                    if (!fullscreenWorkingThreadRunning)
                        break;
                }
            }
            killFullscreenWorkingThread = false;
        }

        private void thumbnailsPerLinetrackBar_ValueChanged(object sender, EventArgs e) {
            labelThumbnailsPerLineCount.Text = thumbnailsPerLinetrackBar.Value.ToString();
        }

        private void buttonCancel_Click(object sender, EventArgs e) {
            thumbnailsPerLinetrackBar.Value = thumbnailsPerLine;
            labelThumbnailsPerLineCount.Text = thumbnailsPerLine.ToString();
            settingsPanel.Visible = false;
        }

        private void buttonOK_Click(object sender, EventArgs e) {
            thumbnailsPerLine = thumbnailsPerLinetrackBar.Value;
            settingsPanel.Visible = false;
            UpdateThumbnailSize();
            thumbnailsImageFromCurrentFolder();
        }

        private void UpdateThumbnailSize() {
            int singleThumbnailWith = (ClientSize.Width - (thumbnailsPerLine + 1) * thumbnailSpacing) / thumbnailsPerLine;
            singleThumbnailImageSize = new Size(singleThumbnailWith, singleThumbnailWith);
            singleThumbnailWithSpacingSize = new Size(singleThumbnailImageSize.Width + thumbnailSpacing, singleThumbnailImageSize.Height + thumbnailSpacing);
            singleThumbnailImageRectangle = new Rectangle(0, 0, singleThumbnailImageSize.Width, singleThumbnailImageSize.Height);
            singleThumbnailImageRatio = singleThumbnailImageSize.Width / singleThumbnailImageSize.Height;
        }

        private void NavigateThumbnailView(bool up) {
            int newY = viewAreaOnThumbnailsImageRectangle.Y + (up ? -navigationStepDistance : navigationStepDistance);
            // Clamp new drawSectionY
            newY = Math.Min(
            newY, thumbnailsImage.Height - viewAreaSize.Height);
            viewAreaOnThumbnailsImageRectangle.Y = Math.Max(
                        newY, 0);
            DrawThumbnailView();
        }
    }
}