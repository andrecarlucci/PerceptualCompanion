using System.Drawing;
using System.Windows.Forms;
using MrWindows.KeyboardControl;
using MrWindows.MouseControl;

namespace MrWindows {
    public class Windows {

        private Mouse _mouse = new Mouse();
        private Keyboard _keyboard = new Keyboard();

        public Mouse Mouse {
            get { return _mouse; }
        }

        public Keyboard Keyboard {
            get { return _keyboard; }
        }

        public void LockWorkStation() {
            WindowsPInvoke.LockWorkStation();
        }

        public Size MainScreenSize {
            get {
                return new Size(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            }
        }

        public Size GetActiveScreenSize() {
            var point = Mouse.CursorLocation;
            int screenWidth = Screen.FromPoint(point).Bounds.Width;
            int screenHeight = Screen.FromPoint(point).Bounds.Height;
            return new Size(screenWidth, screenHeight);
        }
    }
}