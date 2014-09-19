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
using System.Windows.Threading;
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
        private Windows _win;
        private Mouse _mouse;
        private RealSenseCredentialPluginClient _client;

        private Point _lastMousePosition;

        private bool _mouseControl;
        private bool _scrollMode;
        private bool _faceMonitorActive;
        private DateTime _faceLastSeen;
        private string _currentProcess = "";

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
            StartProcessMonitor();
        }

        private void StartProcessMonitor() {
            Task.Run(async () => {
                while (true) {
                    await Task.Delay(500);
                    _currentProcess = _win.CurrentWindow.GetProcessName();
                    Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => {
                        ProcessName.Text = _currentProcess;
                    }));
                }
            });
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
            factory.Combine(camera.LeftHand, State.Closed);
            factory.Combine(camera.RightHand, State.Closed);
            CustomPose closeBothHands = factory.Build("bothHandsClosed");
            closeBothHands.Begin += n => {
                _scrollMode = true;
            };
            closeBothHands.End += n => {
                _scrollMode = false;                
            };

            SetupMouseMove(camera);
            SetUpLeftHand(camera);
            SetUpRightHand(camera);

            camera.RightHand.Closed += () => {
                if (_scrollMode) {
                    return;
                }
                _mouse.MouseLeftDown();
            };
            camera.RightHand.Opened += _mouse.MouseLeftUp;

            camera.LeftHand.Thumb.NotVisible += () => {
                Debug.WriteLine("Thumb closed");
                _mouse.MouseLeftDown();
            };
            camera.LeftHand.Thumb.Visible += _mouse.MouseLeftUp;

            camera.RightHand.NotVisible += () => {
                _mouse.MouseLeftUp();
            };

            SetUpFace(camera);
            SetUpArrows(camera);

            camera.Poses.BigFiveBegin += async () => {
                await _client.Authorize();
            };

            camera.Poses.PosePeaceBegin += () => {
                if (_mouseControl) {
                    _mouseControl = false;
                    _win.Mouse.MouseLeftUp();
                    ShowMessage("Mouse control end");                    
                }
                else {
                    _mouseControl = true;
                    ShowMessage("Mouse control begin");
                }
            };

            ShowMessage("Camera Started");

            var leftPunch = new GesturePunch(camera.LeftHand);
            leftPunch.GestureDetected += () => {
                Debug.WriteLine("LEFT PUNCH!");
            };

            var rightPunch = new GesturePunch(camera.RightHand);
            rightPunch.GestureDetected += () => {
                Debug.WriteLine("RIGHT PUNCH!");
            };

            var click = new CustomGesture(camera.RightHand);
            click.AddMovement(new MovementForward(10, TimeSpan.FromMilliseconds(500)));
            click.GestureDetected += () => _mouse.MouseLeftClick();
        }

        private void SetUpArrows(Camera camera) {
            camera.Gestures.GestureSwipeLeft += () => {
                if (_currentProcess == "chrome") {
                    _win.Keyboard.PressKey(VirtualKey.VK_CONTROL);
                    _win.Keyboard.PressKey(VirtualKey.VK_LSHIFT);
                    _win.Keyboard.PressKey(VirtualKey.VK_TAB);
                    _win.Keyboard.ReleaseKey(VirtualKey.VK_TAB);
                    _win.Keyboard.ReleaseKey(VirtualKey.VK_LSHIFT);
                    _win.Keyboard.ReleaseKey(VirtualKey.VK_CONTROL);
                }
                else {
                    _win.Keyboard.PressKey(VirtualKey.VK_LEFT);                    
                }
            };
            camera.Gestures.GestureSwipeRight += () => {
                if (_currentProcess == "chrome") {
                    _win.Keyboard.PressKey(VirtualKey.VK_CONTROL);
                    _win.Keyboard.PressKey(VirtualKey.VK_TAB);
                    _win.Keyboard.ReleaseKey(VirtualKey.VK_TAB);
                    _win.Keyboard.ReleaseKey(VirtualKey.VK_CONTROL);
                }
                else {
                    _win.Keyboard.PressKey(VirtualKey.VK_RIGHT);                    
                }
            };
            camera.Gestures.GestureSwipeUp += () => _win.Keyboard.PressKey(VirtualKey.VK_UP);
            camera.Gestures.GestureSwipeDown += () => _win.Keyboard.PressKey(VirtualKey.VK_DOWN);
        }

        private void SetUpFace(Camera camera) {
            Task.Run(async () => {
                while (true) {
                    await Task.Delay(1000);
                    if (!_faceMonitorActive) {
                        continue;
                    }
                    if (!camera.Face.IsVisible) {
                        Debug.WriteLine("Locking: " + (DateTime.Now - _faceLastSeen).TotalSeconds);
                    }
                    if (!camera.Face.IsVisible && (DateTime.Now - _faceLastSeen) > TimeSpan.FromSeconds(5)) {
                        _win.LockWorkStation();
                    }
                }
            });
            camera.Face.Moved += d => DisplayBodyPart(camera.Face, 30);
            camera.Face.Visible += () => {
                _faceLastSeen = DateTime.Now;
                Debug.WriteLine("Face visible");
            };
            camera.Face.NotVisible += () => {
                _faceLastSeen = DateTime.Now;
                Debug.WriteLine("Face not visible");
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
                var newPosition = move.MapToScreen(screenSize.Width, screenSize.Height);
                if (!(MathEx.CalcDistance(_lastMousePosition, newPosition) > 5)) return;

                if (_scrollMode) {
                    var direction = DirectionHelper.GetDirection(_lastMousePosition, newPosition);
                    if (direction == Direction.Left || direction == Direction.Right) {
                        _mouse.ScrollHorizontally(newPosition.X - _lastMousePosition.X);
                    }
                    else {
                        _mouse.ScrollVertically(newPosition.Y - _lastMousePosition.Y);                        
                    }
                }
                else {
                    _mouse.MoveCursor(newPosition.X, newPosition.Y);                    
                }
                _lastMousePosition = newPosition;
            };
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
                EnsureElipse(part, size);
                var ellipse = _parts[part];
                Color color = part.IsVisible ? Color.FromArgb(100, 100, 200, 100) : Colors.Transparent;
                ellipse.Fill = new SolidColorBrush(color);

                var p = part.Position.MapToScreen(240, 180);
                if (p.X < 0) {
                    p.X = 0;
                }
                Canvas.SetLeft(ellipse, p.X);
                Canvas.SetTop(ellipse, p.Y);
            };
            Dispatcher.Invoke(action);
        }

        private void EnsureElipse(Item part, int size) {
            if (!_parts.Keys.Contains(part)) {
                var shape = new Ellipse {
                    Width = size,
                    Height = size
                };
                _parts.Add(part, shape);
                HandCanvas.Children.Add(shape);
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            if (e.ChangedButton != MouseButton.Left) {
                return;
            }
            DragMove();
        }

        private void MenuItem_OnChecked(object sender, RoutedEventArgs e) {
            _faceMonitorActive = LockScreenMonitor.IsChecked;
        }
    }
}