using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using YoloTrain.Models;
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

            SizeChanged += MainWindow_SizeChanged;
            Closed += MainWindow_Closed;

            HandleEscape = false;
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            _viewModel.SaveProject();
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateCurrentImage();
        }

        private void _viewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_viewModel.ImageRegions))
            {
                UpdateCurrentImage();
            }

            if (e.PropertyName == nameof(_viewModel.SelectedRegionIndex))
            {
                UpdateCurrentImage();
                ShowSelectedRegionProperties();
                SelectedRegionComboBox.Focus();
            }
        }

        private void ShowSelectedRegionProperties()
        {
            var idx = _viewModel.SelectedRegionIndex;
            if (idx == null)
            {
                imgPreview.Source = null;
                txtCoords.Text = "";
                return;
            }

            var region = _viewModel.ImageRegions[idx.Value];

            var yoloCoords = new YoloCoords
            {
                X = region.X,
                Y = region.Y,
                Height = region.Height,
                Width = region.Width
            };

            ShowYolo(yoloCoords);
        }

        private void ShowYolo(YoloCoords yoloCoords)
        {
            MouseHelper.SetWaitCursor();
            try
            {
                var img = _viewModel.CurrentBitmap;
                if (img == null)
                    return;

                if (yoloCoords.Height <= double.Epsilon || yoloCoords.Width <= double.Epsilon)
                {
                    imgPreview.Source = null;
                    txtCoords.Text = "";
                    return;
                }

                // render how translating back from yolo coords will look
                var rwidth = (int)(yoloCoords.Width * img.Width);
                var rheight = (int)(yoloCoords.Height * img.Height);
                var imgx = (int)(yoloCoords.X * img.Width - rwidth / 2.0);
                var imgy = (int)(yoloCoords.Y * img.Height - rheight / 2.0);

                if (imgy + rheight > img.Height)
                    rheight = img.Height - imgy;
                if (imgx + rwidth > img.Width)
                    rwidth = img.Width - imgx;

                var rect = new Rectangle(imgx, imgy, rwidth, rheight);
                var newImg = img.Clone(rect, img.PixelFormat);
                var hbitmap = newImg.GetHbitmap();

                var bmpsource = Imaging.CreateBitmapSourceFromHBitmap(
                    hbitmap,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromWidthAndHeight(rwidth, rheight)
                );

                BitmapCloner.DeleteObject(hbitmap);
                newImg.Dispose();

                txtCoords.Text = $"x: {imgx} y: {imgy} w: {rwidth}px h: {rheight}px";

                imgPreview.Source = bmpsource;
            }
            finally
            {
                MouseHelper.ResetCursor();
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
                _viewModel.SelectRegion(null);

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

                Point curMouseDownPoint = e.GetPosition(imgTrain);

                var testImgOffset = Coords.GetAbsolutePlacement(imgTrain);
                var testParentOffset = Coords.GetAbsolutePlacement(MainCanvas);
                if (testImgOffset == null || testParentOffset == null)
                    return;

                Point imgOffset = testImgOffset.Value;
                Point parentOffset = testParentOffset.Value;
                imgOffset.X -= parentOffset.X;
                imgOffset.Y -= parentOffset.Y;

                dragSelectionCanvas.Visibility = Visibility.Collapsed;

                e.Handled = true;

                var box = GetMouseBoxRectangle(curMouseDownPoint, imgOffset);
                if (box.Height < 1 || box.Width < 1)
                    return;

                var img = _viewModel.CurrentBitmap;
                var yoloCoords = Coords.ViewportCoordsToYoloCoords(img, imgTrain, imgOffset, box);

                _viewModel.ImageRegions.Add(yoloCoords);
                _viewModel.SelectRegion(_viewModel.ImageRegions.Count - 1);
            }
        }

        private void imgTrain_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isLeftMouseButtonDown)
            {
                Point curMouseDownPoint = e.GetPosition(imgTrain);

                var testImgOffset = Coords.GetAbsolutePlacement(imgTrain);
                var testParentOffset = Coords.GetAbsolutePlacement(MainCanvas);
                if (testImgOffset == null || testParentOffset == null)
                    return;

                Point imgOffset = testImgOffset.Value;
                Point parentOffset = testParentOffset.Value;
                imgOffset.X -= parentOffset.X;
                imgOffset.Y -= parentOffset.Y;

                var box = GetMouseBoxRectangle(curMouseDownPoint, imgOffset);

                Canvas.SetLeft(dragSelectionBorder, box.X);
                Canvas.SetTop(dragSelectionBorder, box.Y);
                dragSelectionBorder.Width = box.Width;
                dragSelectionBorder.Height = box.Height;

                dragSelectionCanvas.Visibility = Visibility.Visible;

                e.Handled = true;

                var img = _viewModel.CurrentBitmap;
                var yoloCoords = Coords.ViewportCoordsToYoloCoords(img, imgTrain, imgOffset, box);
                ShowYolo(yoloCoords);
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
            var testImgOffset = Coords.GetAbsolutePlacement(imgTrain);
            var testParentOffset = Coords.GetAbsolutePlacement(MainCanvas);
            if (testImgOffset == null || testParentOffset == null)
                return;

            //MouseHelper.SetWaitCursor();
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
                        Text = _viewModel.Classes[yolo.Class ?? 0].Replace('_', ' '),
                        RegionIndex = i,
                        YoloCoords = yolo,
                        Height = height,
                        Width = width,
                        IsSelected = _viewModel.SelectedRegionIndex == i
                    };

                    MainCanvas.Children.Add(markedRegion);

                    Point imgOffset = Coords.GetAbsolutePlacement(imgTrain).Value;
                    Point parentOffset = Coords.GetAbsolutePlacement(MainCanvas).Value;
                    imgOffset.X -= parentOffset.X;
                    imgOffset.Y -= parentOffset.Y;

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
                //MouseHelper.ResetCursor();
            }
        }

        private void ClassImage_Click(object sender, RoutedEventArgs e)
        {
            var dataContext = ((FrameworkElement)sender).DataContext;
            var model = (FileRegionModel)dataContext;

            MainTabControl.SelectedIndex = 0;

            string fileName = model.FileName.Replace(".txt", ".jpg");
            int idx = _viewModel.ImagePaths.IndexOf(fileName);
            if (idx == -1)
                return;

            _viewModel.CurrentImagePosition = idx + 1;
            _viewModel.SelectRegion(model.FileLineIndex);
        }
    }
}
