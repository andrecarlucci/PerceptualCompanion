using System;
using Sense.Perceptual;

namespace SharpPerceptual {
    public class GestureSensor : IGestureSensor {
  
        public event Action GestureSwipeLeft;
        public event Action GestureSwipeRight;

        public void OnGestureSwipeRight() {
            Action handler = GestureSwipeRight;
            if (handler != null) handler();
        }

        public void OnGestureSwipeLeft() {
            Action handler = GestureSwipeLeft;
            if (handler != null) handler();
        }
    }
}