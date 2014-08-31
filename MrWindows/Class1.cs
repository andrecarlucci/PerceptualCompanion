using System.Windows.Forms;

namespace MrWindows {
    public class MainScreen {
        public static int Width {
            get { return Screen.PrimaryScreen.Bounds.Width; }
        }

        public static int Height {
            get { return Screen.PrimaryScreen.Bounds.Height; }
        }
    }
}