using System.Drawing;

namespace MrWindows.MouseControl {
    public interface IMouse {
        void SetCursorPosition(int x, int y);
        void SetCursorPosition(Point point);
        void MoveCursor(Point point);
    }
}