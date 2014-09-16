using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Sense.Perceptual;

namespace SharpPerceptual.Gestures {
    public abstract class GestureBase {
        public event Action GestureDetected;

        protected virtual void OnGestureDetected() {
            Action handler = GestureDetected;
            if (handler != null) handler();
        }
        protected int CurrentStep;
        protected Point3D LastPosition;
        protected TimeSpan StepStart;
        protected float StepDisplacement;
        protected List<GestureStep> GestureSteps = new List<GestureStep>();

        protected GestureBase(Item item) {
            item.NotVisible += () => CurrentStep = 0;
            item.Moved += HandMoved;
        }

        private void HandMoved(Point3D position) {
            EnsureSteps();
            if (StepStart == TimeSpan.Zero) {
                StepStart = TimeSpan.FromMilliseconds(1);
                LastPosition = position;
                return;
            }
            var step = GestureSteps[CurrentStep];
            float dif;
            switch (step.Direction) {
                case Direction.Forward:
                case Direction.Backward:
                    dif = position.Z - LastPosition.Z;
                    break;
                case Direction.Up:
                case Direction.Down:
                    dif = position.Y - LastPosition.Y;
                    break;
                case Direction.Left:
                case Direction.Right:
                    dif = position.X - LastPosition.X;
                    break;
                default:
                    return;
            }
            if (step.Direction == Direction.Forward ||
                step.Direction == Direction.Up ||
                step.Direction == Direction.Left) {
                dif *= -1;
            }
            StepDisplacement += dif;
            Debug.WriteLine("Step: " + );
            if (StepDisplacement < step.Distance) {
                return;
            }
            NextStep();
            if (IsOver()) {
                OnGestureDetected();
                StartOver();
            }
        }

        private void StartOver() {
            ResetStep();
            CurrentStep = 0;
        }

        private void ResetStep() {
            StepStart = TimeSpan.Zero;
            StepDisplacement = 0;
        }

        private void NextStep() {
            ResetStep();
            CurrentStep++;
        }

        private bool IsOver() {
            return CurrentStep >= GestureSteps.Count;
        }

        private void EnsureSteps() {
            if (GestureSteps == null) {
                GestureSteps = GetGestureSteps().ToList();
            }
        }

        protected abstract IEnumerable<GestureStep> GetGestureSteps();
    }

    public class GesturePunch : GestureBase {
        public GesturePunch(Hand item) : base(item) {}
        protected override IEnumerable<GestureStep> GetGestureSteps() {

        }
    }

    public class GestureStep {
        public Direction Direction { get; set; }
        public int Distance { get; set; }
        public TimeSpan Window { get; set; }
        public string Name { get; set; }

        public GestureStep(Direction direction, int distance, TimeSpan window, string name = "") {
            Name = name;
            Direction = direction;
            Distance = distance;
            Window = window;
        }
    }
}