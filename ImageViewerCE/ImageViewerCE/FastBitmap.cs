using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

namespace ImageViewerCE { 


    class FastBitmap {
        private Color[,] color;
        private int width, height;
        private Bitmap sourceBitmap;

        public FastBitmap(Bitmap sourceBitmap) {
            this.sourceBitmap = sourceBitmap;
            this.width = sourceBitmap.Width;
            this.height = sourceBitmap.Height;
            this.color = new Color[width, height];
  
            BitmapData bmpData = sourceBitmap.LockBits(new Rectangle(0, 0, width, height), 
                                                       ImageLockMode.ReadOnly,
                                                       PixelFormat.Format24bppRgb);
            
            IntPtr ptr = bmpData.Scan0;
            int stride = bmpData.Stride;
            int length = stride * height;
            byte[] rgbValues = new byte[length];
            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, length);
            sourceBitmap.UnlockBits(bmpData);
          
          
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    color[x, y] = Color.FromArgb(rgbValues[y * stride + x * 3 + 2], 
                                                 rgbValues[y * stride + x * 3 + 1], 
                                                 rgbValues[y * stride + x * 3]);

        }    
   
        public Color GetPixel(int x, int y) {
            return color[x, y];
        }

        public void SetPixel(int x, int y, Color color) {
            this.color[x, y] = color;
        }

        public int Width {
            get { return width; }
        }
        public int Height {
            get { return height; }
        }
        
    
    }
    
    
}
