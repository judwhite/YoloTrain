using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using YoloTrain.Mvvm;
using YoloTrain.Utils;
using YoloTrain.Views.Controls;
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

        public MainWindow(IMainWindowViewModel viewModel)
            : base(viewModel)
        {
            InitializeComponent();

            _viewModel = viewModel;
            _viewModel.PropertyChanged += _viewModel_PropertyChanged;

            Loaded += MainWindow_Loaded;
            SizeChanged += MainWindow_SizeChanged;

            HandleEscape = false;
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateCurrentImage();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateCurrentImage();
        }

        private void _viewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_viewModel.ImageRegions))
            {
                UpdateCurrentImage();
            }
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

                var imgOffset = Coords.GetAbsolutePlacement(imgTrain);
                var img = _viewModel.CurrentBitmap;

                Point curMouseDownPoint = e.GetPosition(imgTrain);
                var box = GetMouseBoxRectangle(curMouseDownPoint, imgOffset);

                var realx = box.X - imgOffset.X;
                var realy = box.Y - imgOffset.Y;
                var scaley = img.Height / imgTrain.ActualHeight;
                var scalex = img.Width / imgTrain.ActualWidth;
                var imgy = realy * scaley;
                var imgx = realx * scalex;

                var rect = new Rectangle((int)imgx, (int)imgy, (int)(box.Width * scalex), (int)(box.Height * scaley));
                if (rect.Width == 0 || rect.Height == 0)
                    return;
            }
        }

        private void imgTrain_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isLeftMouseButtonDown)
            {
                Point curMouseDownPoint = e.GetPosition(imgTrain);
                Point imgOffset = Coords.GetAbsolutePlacement(imgTrain);

                var box = GetMouseBoxRectangle(curMouseDownPoint, imgOffset);

                Canvas.SetLeft(dragSelectionBorder, box.X);
                Canvas.SetTop(dragSelectionBorder, box.Y);
                dragSelectionBorder.Width = box.Width;
                dragSelectionBorder.Height = box.Height;

                dragSelectionCanvas.Visibility = Visibility.Visible;

                e.Handled = true;

                var img = _viewModel.CurrentBitmap;

                var yoloCoords = Coords.ViewportCoordsToYoloCoords(img, imgTrain, imgOffset, box);

                if (yoloCoords.Height <= double.Epsilon || yoloCoords.Width <= double.Epsilon)
                {
                    txtCoords.Text = "";
                    return;
                }

                txtCoords.Text = string.Format("x: {0:0.000000} y: {1:0.000000} w: {2:0.000000} h: {3:0.000000}",
                    yoloCoords.X, yoloCoords.Y, yoloCoords.Width, yoloCoords.Height);

                // close to 'box' but renders how translating back from yolo coords will look
                var rwidth = (int)(yoloCoords.Width * img.Width);
                var rheight = (int)(yoloCoords.Height * img.Height);
                var imgx = (int)(yoloCoords.X * img.Width - rwidth / 2.0);
                var imgy = (int)(yoloCoords.Y * img.Height - rheight / 2.0);

                var rect = new Rectangle(imgx, imgy, rwidth, rheight);
                var newImg = img.Clone(rect, img.PixelFormat);

                var bmpsource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                   newImg.GetHbitmap(),
                   IntPtr.Zero,
                   Int32Rect.Empty,
                   BitmapSizeOptions.FromWidthAndHeight(rwidth, rheight));
                imgPreview.Source = bmpsource;
            }
        }

        private Box GetMouseBoxRectangle(Point curMouseDownPoint, Point imgOffset)
        {
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

            return new Box { X = tmpx, Y = tmpy, Width = tmpwidth, Height = tmpheight };
        }

        private void UpdateCurrentImage()
        {
            MouseHelper.SetWaitCursor();
            try
            {
                MainCanvas.Children.Clear();

                if (_viewModel.ImageRegions == null)
                    return;

                for (int i = 0; i < _viewModel.ImageRegions.Count; i++)
                {
                    var yolo = _viewModel.ImageRegions[i];

                    var width = yolo.Width * imgTrain.ActualWidth;
                    var height = yolo.Height * imgTrain.ActualHeight;

                    var markedRegion = new MarkedRegion
                    {
                        // TODO (judwhite): check for out of bounds class number
                        Text = _viewModel.Classes[yolo.Class.Value].Replace('_', ' '),
                        RegionIndex = i,
                        YoloCoords = yolo,
                        Height = height,
                        Width = width,
                    };

                    MainCanvas.Children.Add(markedRegion);

                    Point imgOffset = Coords.GetAbsolutePlacement(imgTrain);
                    var x = yolo.X * imgTrain.ActualWidth - width / 2.0;
                    var y = yolo.Y * imgTrain.ActualHeight - height / 2.0;
                    var left = imgOffset.X + x;
                    var top = imgOffset.Y + y;

                    Canvas.SetLeft(markedRegion, left);
                    Canvas.SetTop(markedRegion, top);
                }
            }
            finally
            {
                MouseHelper.ResetCursor();
            }
        }
    }
}
