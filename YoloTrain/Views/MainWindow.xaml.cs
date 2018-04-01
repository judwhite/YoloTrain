using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using YoloTrain.Mvvm;
using YoloTrain.Mvvm.ApplicationServices;
using Point = System.Windows.Point;

namespace YoloTrain.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : WindowView
    {
        private readonly IMainWindowViewModel _viewModel;

        private Point _origMouseDownPoint;
        private bool _isLeftMouseButtonDown;
        private double x, y, width, height;

        public MainWindow(IMainWindowViewModel viewModel)
            : base(viewModel)
        {
            InitializeComponent();

            _viewModel = viewModel;
            HandleEscape = false;
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            KeyBindingHelper.SetKeyBindings(this, MainMenu.Menu.Items);
        }

        private void imgTrain_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                _isLeftMouseButtonDown = true;
                _origMouseDownPoint = e.GetPosition(imgTrain);

                imgTrain.CaptureMouse();

                e.Handled = true;
            }
        }

        private void imgTrain_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                _isLeftMouseButtonDown = false;
                imgTrain.ReleaseMouseCapture();
                dragSelectionCanvas.Visibility = Visibility.Collapsed;
                e.Handled = true;

                var imgOffset = GetAbsolutePlacement(imgTrain);
                var img = _viewModel.CurrentImage;

                var realx = x - imgOffset.X;
                var realy = y - imgOffset.Y;
                var scaley = img.Height / imgTrain.ActualHeight;
                var scalex = img.Width / imgTrain.ActualWidth;
                var imgy = realy * scaley;
                var imgx = realx * scalex;

                var rect = new Rectangle((int)imgx, (int)imgy, (int)(width * scalex), (int)(height * scaley));
                if (rect.Width == 0 || rect.Height == 0)
                    return;
                var bmp = new Bitmap(img);
                var newImg = bmp.Clone(rect, bmp.PixelFormat);
                newImg.Save(@"f:\crop.bmp");
            }
        }

        private void imgTrain_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isLeftMouseButtonDown)
            {
                Point curMouseDownPoint = e.GetPosition(imgTrain);

                var imgOffset = GetAbsolutePlacement(imgTrain);

                var tmpx = Math.Min(_origMouseDownPoint.X, curMouseDownPoint.X) + imgOffset.X;
                var tmpy = Math.Min(_origMouseDownPoint.Y, curMouseDownPoint.Y) + imgOffset.Y;
                double overx = 0, overy = 0;
                if (tmpx < imgOffset.X)
                {
                    overx = imgOffset.X - tmpx;
                    tmpx = imgOffset.X;
                }
                if (tmpy < imgOffset.Y)
                {
                    overy = imgOffset.Y - tmpy;
                    tmpy = imgOffset.Y;
                }

                var tmpwidth = Math.Abs(_origMouseDownPoint.X - curMouseDownPoint.X) - overx;
                var tmpheight = Math.Abs(_origMouseDownPoint.Y - curMouseDownPoint.Y) - overy;

                if (tmpx + tmpwidth - imgOffset.X > imgTrain.ActualWidth)
                    tmpwidth = imgTrain.ActualWidth - tmpx + imgOffset.X;
                if (tmpy + tmpheight - imgOffset.Y > imgTrain.ActualHeight)
                    tmpheight = imgTrain.ActualHeight - tmpy + imgOffset.Y;

                x = tmpx;
                y = tmpy;
                width = tmpwidth;
                height = tmpheight;

                Canvas.SetLeft(dragSelectionBorder, x);
                Canvas.SetTop(dragSelectionBorder, y);
                dragSelectionBorder.Width = width;
                dragSelectionBorder.Height = height;

                dragSelectionCanvas.Visibility = Visibility.Visible;

                var img = _viewModel.CurrentImage;

                var realx = x - imgOffset.X;
                var realy = y - imgOffset.Y;
                var scaley = img.Height / imgTrain.ActualHeight;
                var scalex = img.Width / imgTrain.ActualWidth;

                int imgy = (int)(realy * scaley);
                int imgx = (int)(realx * scalex);
                int rwidth = (int)(width * scalex);
                int rheight = (int)(height * scaley);

                e.Handled = true;

                if (rwidth == 0 || rheight == 0)
                {
                    txtCoords.Text = "";
                    return;
                }

                double dataX, dataY, dataHeight, dataWidth;
                dataX = (rwidth / 2.0 + imgx) / (double)img.Width;
                dataY = (rheight / 2.0 + imgy) / (double)img.Height;
                dataHeight = (double)rheight / img.Height;
                dataWidth = (double)rwidth / img.Width;

                rwidth = (int)(dataWidth * img.Width);
                rheight = (int)(dataHeight * img.Height);
                imgx = (int)(dataX * img.Width - rwidth / 2.0);
                imgy = (int)(dataY * img.Height - rheight / 2.0);

                txtCoords.Text = string.Format("x: {0:0.000000} y: {1:0.000000} w: {2:0.000000} h: {3:0.000000}", dataX, dataY, dataWidth, dataHeight);

                var rect = new Rectangle(imgx, imgy, rwidth, rheight);
                //var bmp = new System.Drawing.Bitmap(_img);
                var newImg = img.Clone(rect, img.PixelFormat);

                var bmpsource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                   newImg.GetHbitmap(),
                   IntPtr.Zero,
                   Int32Rect.Empty,
                   BitmapSizeOptions.FromWidthAndHeight(rwidth, rheight));
                imgPreview.Source = bmpsource;
            }
        }

        public static Point GetAbsolutePlacement(FrameworkElement element)
        {
            var absolutePos = element.PointToScreen(new Point(0, 0));
            var posMW = Application.Current.MainWindow.PointToScreen(new Point(0, 0));
            var relativePos = new Point(absolutePos.X - posMW.X, absolutePos.Y - posMW.Y);
            return relativePos;
        }
    }
}
