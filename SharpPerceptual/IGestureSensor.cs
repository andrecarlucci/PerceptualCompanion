using System;

namespace Sense.Perceptual {
    public interface IGestureSensor {
        event Action GestureSwipeLeft;
        event Action GestureSwipeRight;
    }
}