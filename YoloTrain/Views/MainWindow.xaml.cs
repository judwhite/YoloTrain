using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using YoloTrain.Mvvm;
using YoloTrain.Utils;
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

            HandleEscape = false;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateCurrentImage();
        }

        private void _viewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_viewModel.CurrentImage))
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
            if (_viewModel.CurrentImage == null)
                return;

            MainCanvas.Children.Clear();

            string imageDirectory = Path.GetDirectoryName(_viewModel.CurrentImage);
            string imageBoundsFileName = Path.Combine(imageDirectory, Path.GetFileNameWithoutExtension(_viewModel.CurrentImage) + ".txt");
            if (File.Exists(imageBoundsFileName))
            {
                var lines = File.ReadAllLines(imageBoundsFileName);
                foreach (var line in lines)
                {
                    var parts = line.Split(' ');
                    if (parts.Length != 5)
                        continue;

                    var classNumber = int.Parse(parts[0]);
                    var x = double.Parse(parts[1]);
                    var y = double.Parse(parts[2]);
                    var width = double.Parse(parts[3]);
                    var height = double.Parse(parts[4]);

                    Point imgOffset = Coords.GetAbsolutePlacement(imgTrain);

                    width *= imgTrain.ActualWidth;
                    height *= imgTrain.ActualHeight;

                    x = x * imgTrain.ActualWidth - width / 2.0;
                    y = y * imgTrain.ActualHeight - height / 2.0;

                    var left = imgOffset.X + x;
                    var top = imgOffset.Y + y;

                    Canvas canvas = new Canvas();
                    canvas.Background = new SolidColorBrush(Colors.LightBlue);
                    canvas.Opacity = 0.5;
                    canvas.Visibility = Visibility.Visible;
                    canvas.HorizontalAlignment = HorizontalAlignment.Left;
                    canvas.VerticalAlignment = VerticalAlignment.Top;

                    var border = new Border();
                    border.BorderBrush = new SolidColorBrush(Colors.Blue);
                    border.BorderThickness = new Thickness(1);
                    border.CornerRadius = new CornerRadius(1);
                    border.Height = height;
                    border.Width = width;
                    canvas.Children.Add(border);

                    var textBlock = new TextBlock();
                    textBlock.Text = _viewModel.Classes[classNumber].Replace('_', ' ');
                    textBlock.Background = new SolidColorBrush(Colors.Transparent);
                    textBlock.Foreground = new SolidColorBrush(Colors.Black);
                    textBlock.SetValue(TextBlock.FontStretchProperty, FontStretches.Condensed);
                    textBlock.SetValue(TextBlock.FontWeightProperty, FontWeights.Bold);
                    textBlock.LineStackingStrategy = LineStackingStrategy.BlockLineHeight;
                    textBlock.FontSize = 11;
                    textBlock.LineHeight = 10;
                    textBlock.HorizontalAlignment = HorizontalAlignment.Left;
                    textBlock.VerticalAlignment = VerticalAlignment.Top;
                    textBlock.Padding = new Thickness(2);
                    textBlock.TextWrapping = TextWrapping.Wrap;
                    textBlock.Width = width;
                    textBlock.Height = height;
                    canvas.Children.Add(textBlock);

                    canvas.Height = height;
                    canvas.Width = width;

                    MainCanvas.Children.Add(canvas);

                    Canvas.SetLeft(canvas, left);
                    Canvas.SetTop(canvas, top);
                }
            }
        }
    }
}
