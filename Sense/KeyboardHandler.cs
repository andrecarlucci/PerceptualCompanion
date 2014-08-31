using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace Sense {
    public class KeyboardHandler : IDisposable {

        public const int WM_HOTKEY = 0x0312;
        public const int VIRTUALKEYCODE_FOR_CAPS_LOCK = 0x14;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vlc);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("user32")]
        public static extern int SetCursorPos(int x, int y);

        WindowInteropHelper _host;

        public KeyboardHandler(Window mainWindow) {
            _host = new WindowInteropHelper(mainWindow);
            SetupHotKey(_host.Handle);
            ComponentDispatcher.ThreadPreprocessMessage += ComponentDispatcher_ThreadPreprocessMessage;
        }

        void ComponentDispatcher_ThreadPreprocessMessage(ref MSG msg, ref bool handled) {
            if (msg.message == WM_HOTKEY) {
                //Handle hot key kere
            }
        }

        private void SetupHotKey(IntPtr handle) {
            RegisterHotKey(handle, GetType().GetHashCode(), 0, VIRTUALKEYCODE_FOR_CAPS_LOCK);
        }

        public void Dispose() {
            UnregisterHotKey(_host.Handle, GetType().GetHashCode());
            ComponentDispatcher.ThreadPreprocessMessage -= ComponentDispatcher_ThreadPreprocessMessage;
        }
    }
}
