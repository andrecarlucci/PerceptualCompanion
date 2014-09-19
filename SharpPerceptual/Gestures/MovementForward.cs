﻿using System;

namespace SharpPerceptual.Gestures {
    public class MovementForward : Movement {
        public MovementForward(double distance, TimeSpan window) : base(distance, window) {}

        protected override Point3D RemoveNoise(Point3D position) {
            position.Z = Math.Round(position.Z, 2);
            return position;
        }

        protected override bool IsRightDirection(Point3D currentLocation) {
            return currentLocation.Z <= StartPosition.Z;
        }

        protected override bool StepCompleted(Point3D currentLocation) {
            return Math.Abs(StartPosition.Z - currentLocation.Z) >= Distance;
        }
    }
}