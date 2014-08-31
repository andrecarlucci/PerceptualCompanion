using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Services;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Win32;
using MrWindows;
using Sense.Perceptual;
using SharpPerceptual;
using Point = System.Drawing.Point;

namespace Sense {
    public partial class MainWindow : Window {
        [DllImport("user32.dll")]
        static extern int GetSystemMetrics(SystemMetric smIndex);

        private static int WM_HOTKEY = 0x0312;

        private Camera _camera;
        private Hand _hand;
        private bool _mouseControl;
        private double _lastX;
        private double _lastY;
        private bool _shouldLock;
        public MainWindow() {
            InitializeComponent();
            Left = SystemParameters.PrimaryScreenWidth - Width - 25;
            Top = SystemParameters.PrimaryScreenHeight - Height - 25;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            //CameraFinder.Find();
            StartCamera();
        }

        private void StartCamera() {
            var pipeline = new Pipeline();
            pipeline.Start();

            pipeline.LeftHand.Moved += d => DisplayBodyPart(pipeline.LeftHand);
            pipeline.LeftHand.Thumb.Moved += d => DisplayBodyPart(pipeline.LeftHand.Thumb);
            pipeline.LeftHand.Index.Moved += d => DisplayBodyPart(pipeline.LeftHand.Index);
            pipeline.LeftHand.Middle.Moved += d => DisplayBodyPart(pipeline.LeftHand.Middle);
            pipeline.LeftHand.Ring.Moved += d => DisplayBodyPart(pipeline.LeftHand.Ring);
            pipeline.LeftHand.Pinky.Moved += d => DisplayBodyPart(pipeline.LeftHand.Pinky);

            pipeline.RightHand.Moved += d => DisplayBodyPart(pipeline.RightHand);
            pipeline.RightHand.Thumb.Moved += d => DisplayBodyPart(pipeline.RightHand.Thumb);
            pipeline.RightHand.Index.Moved += d => DisplayBodyPart(pipeline.RightHand.Index);
            pipeline.RightHand.Middle.Moved += d => DisplayBodyPart(pipeline.RightHand.Middle);
            pipeline.RightHand.Ring.Moved += d => DisplayBodyPart(pipeline.RightHand.Ring);
            pipeline.RightHand.Pinky.Moved += d => DisplayBodyPart(pipeline.RightHand.Pinky);

            pipeline.LeftHand.Moved += d => DisplayBodyPart(pipeline.LeftHand);

            SetupMouseMove(pipeline);

            pipeline.Poses.PosePeaceBegin += WinApi.MouseLeftClick;
            pipeline.RightHand.Closed += WinApi.MouseLeftDown;
            pipeline.RightHand.Opened += WinApi.MouseLeftUp;
            pipeline.RightHand.NotVisible += WinApi.MouseLeftUp;

            pipeline.Face.Moved += d => DisplayBodyPart(pipeline.Face);
            pipeline.Face.Visible += () => { _shouldLock = false; };
            pipeline.Face.NotVisible += async () => {
                _shouldLock = true;
                await Task.Delay(TimeSpan.FromSeconds(3));
                if (_shouldLock) {
                    WinApi.LockWorkStation();
                }
            };

            //CameraFinder.Find();
            //_hand = new Hand();
            //Camera.CameraScanInMilli = 100;
            //_camera = new Camera();
            //_camera.Monitor(_hand);
            //_hand.Visible += () => {
            //    ShowMessage("Hand Detected.", "Thumbs Up for Mouse control");
            //    Debug.WriteLine("Hand Visible");
            //    DisplayBodyPart(_hand);
            //};
            //_hand.NotVisible += () => {
            //    _mouseControl = false;
            //    WinApi.MouseLeftUp();
            //    ShowMessage("Hand NotVisible", "Mouse tracking is off", BalloonIcon.Warning);
            //    Debug.WriteLine("Hand NotVisible");
            //};
            //_hand.Moved += move => {
            //    if (!_mouseControl) return;

            //    //int screenWidth = SystemInformation.VirtualScreen.Width;
            //    //int screenHeight = SystemInformation.VirtualScreen.Height;  
            //    var point = WinApi.GetCursorPosition();
            //    double screenWidth = Screen.FromPoint(point).Bounds.Width * 1.5;
            //    double screenHeight = Screen.FromPoint(point).Bounds.Height * 1.5;

            //    double left = (screenWidth - (move.X / Camera.ResolutionWidth) * screenWidth) - (screenWidth * 0.2);
            //    double top = ((move.Y / Camera.ResolutionHeight) * screenHeight) - (screenHeight * 0.2);

            //    if (Math.Sqrt(Math.Pow(_lastX - left, 2) + Math.Pow(_lastY - top, 2)) > 3) {
            //        var middleX = (_lastX + left)/2;
            //        var middleY = (_lastY + top)/2;
            //        WinApi.SetCursorPos((int)middleX, (int)middleY);
            //        _lastX = left;
            //        _lastY = top;
            //        WinApi.SetCursorPos((int)_lastX, (int)_lastY);
            //    }
            //    //WinApi.SetCursorPos((int)left, (int)top);
            //};
            //_hand.Opened += () => {
            //    if (!_mouseControl) return;
            //    if (WinApi.IsMouseLeftDown()) {
            //        Debug.WriteLine("Mouse Up");
            //        WinApi.MouseLeftUp();
            //    }
            //};
            //_hand.Closed += () => {
            //    if (!_mouseControl) return;
            //    if (WinApi.IsMouseLeftUp()) {
            //        Debug.WriteLine("Mouse Down");
            //        WinApi.MouseLeftDown();
            //    }
            //};
            //_hand.GestureThumbsUp += () => {
            //    ShowMessage("Tracking Enabled", "Peace to disable it");
            //    _mouseControl = true;
            //};
            //_hand.GestureThumbsDown += DisableTracking;
            //_hand.GesturePeace += DisableTracking;

            //_hand.GestureSwipeLeft += () => {
            //    WinApi.ScrollHorizontally(-250);
            //};

            //_hand.GestureSwipeRight += () => {
            //    WinApi.ScrollHorizontally(250);
            //};

            //_camera.Start();
            ShowMessage("Camera Started");
        }



        async void Face_NotVisible() {
            WinApi.LockWorkStation();
        }
        
        private void SetupMouseMove(Pipeline pipeline) {
            pipeline.LeftHand.Moved += move => {
                var point = WinApi.GetCursorPosition();
                double screenWidth = Screen.FromPoint(point).Bounds.Width*1.5;
                double screenHeight = Screen.FromPoint(point).Bounds.Height*1.5;

                double left = (screenWidth - (move.X/Camera.ResolutionWidth)*screenWidth) - (screenWidth*0.2);
                double top = ((move.Y/Camera.ResolutionHeight)*screenHeight) - (screenHeight*0.2);

                if (Math.Sqrt(Math.Pow(_lastX - left, 2) + Math.Pow(_lastY - top, 2)) > 3) {
                    var middleX = (_lastX + left)/2;
                    var middleY = (_lastY + top)/2;
                    WinApi.SetCursorPos((int) middleX, (int) middleY);
                    _lastX = left;
                    _lastY = top;
                    WinApi.SetCursorPos((int) _lastX, (int) _lastY);
                }
                //WinApi.SetCursorPos((int)left, (int)top);
            };
        }

        private void DisableTracking() {
            WinApi.MouseLeftUp();
            ShowMessage("Tracking Disabled", "Thumbs up to enable it", BalloonIcon.Warning);
            _mouseControl = false;
        }

        private void ShowMessage(string title, string message = " ", BalloonIcon icon = BalloonIcon.Info) {
            NotifyIcon.ShowBalloonTip(title, message, icon);
        }

        protected override void OnClosed(EventArgs e) {
            base.OnClosed(e);
            NotifyIcon.Dispose();
        }

        private Dictionary<BodyPart, Ellipse> _parts = new Dictionary<BodyPart, Ellipse>();

        private void DisplayBodyPart(BodyPart part) {
            Action action = delegate {
                if (!_parts.Keys.Contains(part)) {
                    if (!part.IsVisible) {
                        _parts.Remove(part);
                        return;
                    }
                    var shape = new Ellipse {
                        Width = 5,
                        Height = 5
                    };
                    _parts.Add(part, shape);
                    HandCanvas.Children.Add(shape);
                }
                var ellipse = _parts[part];
                Color color = part.IsOpen ? Color.FromArgb(100, 100, 200, 100) : Color.FromArgb(100, 200, 100, 100);
                ellipse.Fill = new SolidColorBrush(color);
                double left = HandCanvas.ActualWidth - (part.Position.X / 320) * HandCanvas.ActualWidth;
                double top = (part.Position.Y / 240) * HandCanvas.ActualHeight;
                Canvas.SetLeft(ellipse, left);
                Canvas.SetTop(ellipse, top);
            };
            Dispatcher.Invoke(action);
        }

        private int GetDelta(float delta) {
            var d = (int)delta;
            //if (d <= 1) return 0;
            return d * 4;
        }

        public string GetConvertableStateMode() {
            if (GetSystemMetrics(SystemMetric.SM_CONVERTABLESLATEMODE) == 0) {
                return "slate/tablet mode";
            }
            if (GetSystemMetrics(SystemMetric.SM_SYSTEMDOCKED) == 0) {
                return "docked/laptop mode";
            }
            return "Unknown state";
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e) {
            WinApi.LockWorkStation();
        }

        private async void ButtonBaseMouseLeft_OnClick(object sender, RoutedEventArgs e) {
            await Task.Delay(3000);
            var point = WinApi.GetCursorPosition();
            WinApi.SetCursorPos(point.X + 100, point.Y);
        }

        private async void DoIt(object sender, RoutedEventArgs e) {
            await Task.Delay(TimeSpan.FromSeconds(3));
            //WinApi.ScrollHorizontally(100);
            //Debug.WriteLine("ScrollHorizontally");
            WinApi.SendKeyboardEvent(Keys.Enter);
            WinApi.SendKeyboardEvent(Keys.A);
            WinApi.SendKeyboardEvent(Keys.A);
            WinApi.SendKeyboardEvent(Keys.A);
            WinApi.SendKeyboardEvent(Keys.A);
            WinApi.SendKeyboardEvent(Keys.A);
        }

        public static IEnumerable<Point> GetPointsOnLine(int x0, int y0, int x1, int y1) {
            bool steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
            if (steep) {
                int t;
                t = x0; // swap x0 and y0
                x0 = y0;
                y0 = t;
                t = x1; // swap x1 and y1
                x1 = y1;
                y1 = t;
            }
            if (x0 > x1) {
                int t;
                t = x0; // swap x0 and x1
                x0 = x1;
                x1 = t;
                t = y0; // swap y0 and y1
                y0 = y1;
                y1 = t;
            }
            int dx = x1 - x0;
            int dy = Math.Abs(y1 - y0);
            int error = dx / 2;
            int ystep = (y0 < y1) ? 1 : -1;
            int y = y0;
            for (int x = x0; x <= x1; x++) {
                yield return new Point((steep ? y : x), (steep ? x : y));
                error = error - dy;
                if (error < 0) {
                    y += ystep;
                    error += dx;
                }
            }
            yield break;
        }

        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
    }
}