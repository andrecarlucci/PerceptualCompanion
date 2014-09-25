using System.Collections.Generic;
using System.Drawing;
using MrWindows;
using SharpPerceptual;
using SharpPerceptual.Gestures;

namespace Sense.Profiles {
    public class ScrollProfile : Profile {
        private List<Profile> _lastActiveProfiles;

        public ScrollProfile(Windows windows, Camera camera, ProcessMonitor processMonitor) : base(windows, camera, processMonitor) {}

        public override string Name {
            get { return "Scroll"; }
        }

        public override void Config() {
            var screenSize = Windows.GetActiveScreenSize();
            var cameraToScreenMapper = new CameraToScreenMapper(
                screenSize.Width,
                screenSize.Height,
                Camera.LeftHand);
            cameraToScreenMapper.Moved += LeftHandMoved;

            var factory = new PoseFactory();
            factory.Combine(Camera.LeftHand, State.Closed);
            factory.Combine(Camera.RightHand, State.Closed);
            factory.Combine(Camera.LeftHand, State.Visible);
            factory.Combine(Camera.RightHand, State.Visible);

            CustomPose closeBothHands = factory.Build("bothHandsClosed");
            closeBothHands.Begin += n => {
                _lastActiveProfiles = ProfileManager.ActiveProfiles;
                ProfileManager.Activate(this);
            };
            closeBothHands.End += n => DoIfActive(() => {
                ProfileManager.Deactivate(this);
                ProfileManager.Activate(_lastActiveProfiles.ToArray());                    
            });
        }

        public void LeftHandMoved(Point pointFrom, Point pointTo) {
            if (!ProfileManager.IsActive(this)) {
                return;
            }
            var direction = DirectionHelper.GetDirection(pointFrom, pointTo);
            if (direction == Direction.Left || direction == Direction.Right) {
                Windows.Mouse.ScrollHorizontally(pointTo.X - pointFrom.X);
            }
            else {
                Windows.Mouse.ScrollVertically(pointTo.Y - pointFrom.Y);
            }
        }

        public override void Deactivate() {
            Windows.Mouse.MouseLeftUp();
        }
    }
}