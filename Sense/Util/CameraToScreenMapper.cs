using System;
using System.Drawing;
using MrWindows.Util;
using SharpPerceptual;

namespace Sense.Profiles {
    public class CameraToScreenMapper {
        private int _screenWidth;
        private int _screenHeight;
        private Point _lastPosition;

        public event Action<Point, Point> Moved;

        public CameraToScreenMapper(int screenWidth, int screenHeight, Item item) {
            _screenWidth = screenWidth;
            _screenHeight = screenHeight;
            item.Moved += FilterMove;
        }

        private void FilterMove(Point3D point3D) {
            var newPosition = point3D.MapToScreen(_screenWidth, _screenHeight);
            if (IsNoise(newPosition)) {
                return;
            }
            OnMoved(_lastPosition, newPosition);
            _lastPosition = newPosition;
        }

        private bool IsNoise(Point newPosition) {
            return MathEx.CalcDistance(_lastPosition, newPosition) <= 5;
        }

        protected virtual void OnMoved(Point from, Point to) {
            Action<Point, Point> handler = Moved;
            if (handler != null) handler(@from, to);
        }
    }
}