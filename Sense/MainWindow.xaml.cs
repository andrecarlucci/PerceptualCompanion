using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Hardcodet.Wpf.TaskbarNotification;
using MrWindows;
using MrWindows.KeyboardControl;
using MrWindows.Util;
using SharpPerceptual;
using SharpPerceptual.Gestures;
using Mouse = MrWindows.MouseControl.Mouse;
using Point = System.Drawing.Point;

namespace Sense {
    public partial class MainWindow : Window {
        private Camera _camera;
        private Hand _hand;

        private Windows _win;
        private Mouse _mouse;
        private RealSenseCredentialPluginClient _client;

        private Point _lastMousePosition;

        private bool _mouseControl;
        private bool _shouldLock;

        public MainWindow() {
            InitializeComponent();
            Left = SystemParameters.PrimaryScreenWidth - Width - 25;
            Top = SystemParameters.PrimaryScreenHeight - Height - 25;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e) {
            _win = new Windows();
            _mouse = _win.Mouse;
            _client = new RealSenseCredentialPluginClient();
            _client.Start();
            StartCamera(await FindCamera());
        }

        private async Task<Camera> FindCamera() {
            while (true) {
                var camera = new Camera();
                try {
                    camera.Start();
                    ShowMessage("RealSense Camera Started");
                    return camera;
                }
                catch (CameraNotFoundException) {
                    ShowMessage("Please, connect the RealSense Camera");
                }
                await Task.Delay(TimeSpan.FromSeconds(5));
            }
        }

        private void StartCamera(Camera camera) {
            var factory = new PoseFactory();
            factory.Combine(camera.LeftHand, State.Opened);
            factory.Combine(camera.RightHand, State.Opened);
            CustomPose openBothHands = factory.Build("bothHandsOpen");
            openBothHands.Begin += n => {
                if (_mouseControl) return;
                _mouseControl = true;
                ShowMessage("Mouse control begin");
            };

            factory.Combine(camera.LeftHand, State.Closed);
            factory.Combine(camera.RightHand, State.Closed);
            CustomPose closeBothHands = factory.Build("bothHandsClosed");
            closeBothHands.Begin += n => {
                if (!_mouseControl) return;
                _mouseControl = false;
                _win.Mouse.MouseLeftUp();
                ShowMessage("Mouse control end");
            };

            SetupMouseMove(camera);
            SetUpLeftHand(camera);
            SetUpRightHand(camera);
            
            camera.Poses.PosePeaceBegin += _mouse.MouseLeftClick;
            camera.RightHand.Closed += _mouse.MouseLeftDown;
            camera.RightHand.Opened += _mouse.MouseLeftUp;
            camera.RightHand.NotVisible += () => {
                _mouse.MouseLeftUp();
            };

            SetUpFace(camera);
            SetUpArrows(camera);

            camera.Poses.BigFiveBegin += async () => {
                await _client.Authorize();
            };

            ShowMessage("Camera Started");
        }

        private void SetUpArrows(Camera camera) {
            camera.Gestures.GestureSwipeLeft += () => _win.Keyboard.PressKey(VirtualKey.VK_LEFT);
            camera.Gestures.GestureSwipeRight += () => _win.Keyboard.PressKey(VirtualKey.VK_RIGHT);
            camera.Gestures.GestureSwipeUp += () => _win.Keyboard.PressKey(VirtualKey.VK_UP);
            camera.Gestures.GestureSwipeDown += () => _win.Keyboard.PressKey(VirtualKey.VK_DOWN);
        }

        private void SetUpFace(Camera camera) {
            camera.Face.Moved += d => DisplayBodyPart(camera.Face, 30);
            camera.Face.Visible += () => {
                _shouldLock = false;
                Debug.WriteLine("Face visible");
            };
            camera.Face.NotVisible += async () => {
                Debug.WriteLine("Face not visible");

                //if (_shouldLock) {
                //    return;
                //}
                //_shouldLock = true;
                //for (int i = 0; i < 10; i++) {
                //    await Task.Delay(TimeSpan.FromSeconds(10));
                //    Debug.WriteLine("Locking in " + (10 - i) + "...");
                //    if (!_shouldLock) return;
                //}
                //_win.LockWorkStation();
            };
        }

        private void SetUpRightHand(Camera camera) {
            camera.RightHand.Moved += d => DisplayBodyPart(camera.RightHand, 20);
            camera.RightHand.Thumb.Moved += d => DisplayBodyPart(camera.RightHand.Thumb);
            camera.RightHand.Index.Moved += d => DisplayBodyPart(camera.RightHand.Index);
            camera.RightHand.Middle.Moved += d => DisplayBodyPart(camera.RightHand.Middle);
            camera.RightHand.Ring.Moved += d => DisplayBodyPart(camera.RightHand.Ring);
            camera.RightHand.Pinky.Moved += d => DisplayBodyPart(camera.RightHand.Pinky);

            camera.RightHand.NotVisible += () => DisplayBodyPart(camera.RightHand, 20);
            camera.RightHand.Thumb.NotVisible += () => DisplayBodyPart(camera.RightHand.Thumb);
            camera.RightHand.Index.NotVisible += () => DisplayBodyPart(camera.RightHand.Index);
            camera.RightHand.Middle.NotVisible += () => DisplayBodyPart(camera.RightHand.Middle);
            camera.RightHand.Ring.NotVisible += () => DisplayBodyPart(camera.RightHand.Ring);
            camera.RightHand.Pinky.NotVisible += () => DisplayBodyPart(camera.RightHand.Pinky);
        }

        private void SetUpLeftHand(Camera camera) {
            camera.LeftHand.Moved += d => DisplayBodyPart(camera.LeftHand, 20);
            camera.LeftHand.Thumb.Moved += d => DisplayBodyPart(camera.LeftHand.Thumb);
            camera.LeftHand.Index.Moved += d => DisplayBodyPart(camera.LeftHand.Index);
            camera.LeftHand.Middle.Moved += d => DisplayBodyPart(camera.LeftHand.Middle);
            camera.LeftHand.Ring.Moved += d => DisplayBodyPart(camera.LeftHand.Ring);
            camera.LeftHand.Pinky.Moved += d => DisplayBodyPart(camera.LeftHand.Pinky);

            camera.LeftHand.NotVisible += () => DisplayBodyPart(camera.LeftHand, 20);
            camera.LeftHand.Thumb.NotVisible += () => DisplayBodyPart(camera.LeftHand.Thumb);
            camera.LeftHand.Index.NotVisible += () => DisplayBodyPart(camera.LeftHand.Index);
            camera.LeftHand.Middle.NotVisible += () => DisplayBodyPart(camera.LeftHand.Middle);
            camera.LeftHand.Ring.NotVisible += () => DisplayBodyPart(camera.LeftHand.Ring);
            camera.LeftHand.Pinky.NotVisible += () => DisplayBodyPart(camera.LeftHand.Pinky);
        }

        private void SetupMouseMove(Camera pipeline) {
            pipeline.LeftHand.Moved += move => {
                if (!_mouseControl) {
                    return;
                }
                var screenSize = _win.GetActiveScreenSize();

                double screenWidth = screenSize.Width*1.5;
                double screenHeight = screenSize.Height*1.5;

                double left = (screenWidth - (move.X/Camera.ResolutionWidth)*screenWidth) - (screenWidth*0.2);
                double top = ((move.Y/Camera.ResolutionHeight)*screenHeight) - (screenHeight*0.2);

                var newPosition = new Point((int) left, (int) top);
                if (!(MathEx.CalcDistance(_lastMousePosition, newPosition) > 3)) return;
                _mouse.MoveCursor(newPosition);
                _lastMousePosition = newPosition;
            };
        }

        private void DisableTracking() {
            _mouse.MouseLeftUp();
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

        private Dictionary<Item, Ellipse> _parts = new Dictionary<Item, Ellipse>();

        private void DisplayBodyPart(Item part, int size = 5) {
            Action action = delegate {
                if (!_parts.Keys.Contains(part)) {
                    var shape = new Ellipse {
                        Width = size,
                        Height = size
                    };
                    _parts.Add(part, shape);
                    HandCanvas.Children.Add(shape);
                    //Debug.WriteLine(part.ToString() + " added");
                }
                var ellipse = _parts[part];
                Color color = part.IsVisible ? Color.FromArgb(100, 100, 200, 100) : Colors.Transparent;
                ellipse.Fill = new SolidColorBrush(color);
                double left = HandCanvas.ActualWidth - (part.Position.X/320)*HandCanvas.ActualWidth;
                double top = (part.Position.Y/240)*HandCanvas.ActualHeight;
                Canvas.SetLeft(ellipse, left);
                Canvas.SetTop(ellipse, top);
            };
            Dispatcher.Invoke(action);
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            if (e.ChangedButton != MouseButton.Left) {
                return;
            }
            DragMove();
        }
    }
}