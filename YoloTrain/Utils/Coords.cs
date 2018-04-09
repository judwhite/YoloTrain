using System.Drawing;
using System.Windows;
using Image = System.Windows.Controls.Image;
using Point = System.Windows.Point;

namespace YoloTrain.Utils
{
    public static class Coords
    {
        public static YoloCoords ViewportCoordsToYoloCoords(Bitmap originalBitmap, Image imageViewport, Point imageViewportOffset, Box box)
        {
            var realx = box.X - imageViewportOffset.X;
            var realy = box.Y - imageViewportOffset.Y;
            var scaley = originalBitmap.Height / imageViewport.ActualHeight;
            var scalex = originalBitmap.Width / imageViewport.ActualWidth;

            int imgy = (int)(realy * scaley);
            int imgx = (int)(realx * scalex);
            int rwidth = (int)(box.Width * scalex);
            int rheight = (int)(box.Height * scaley);

            if (rwidth == 0 || rheight == 0)
            {
                return new YoloCoords();
            }

            return new YoloCoords
            {
                X = (rwidth / 2.0 + imgx) / originalBitmap.Width,
                Y = (rheight / 2.0 + imgy) / originalBitmap.Height,
                Width = (double)rwidth / originalBitmap.Width,
                Height = (double)rheight / originalBitmap.Height
            };
        }

        public static Point GetAbsolutePlacement(FrameworkElement element)
        {
            var absolutePos = element.PointToScreen(new Point(0, 0));
            var positionMainWindow = Application.Current.MainWindow.PointToScreen(new Point(0, 0));
            var relativePos = new Point(absolutePos.X - positionMainWindow.X, absolutePos.Y - positionMainWindow.Y);
            return relativePos;
        }
    }
}
