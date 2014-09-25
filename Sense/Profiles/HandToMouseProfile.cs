using System.Collections.Generic;
using MrWindows;
using SharpPerceptual;
using SharpPerceptual.Gestures;

namespace Sense.Profiles {
    public class HandToMouseProfile : Profile {

        public HandToMouseProfile(Windows windows, Camera camera, ProcessMonitor processMonitor) : base(windows, camera, processMonitor) {}

        public override string Name {
            get { return "HandToMouse"; }
        }
       

        public override void Config() {
            var screenSize = Windows.GetActiveScreenSize();
            var cameraToScreenMapper = new CameraToScreenMapper(
                screenSize.Width,
                screenSize.Height,
                Camera.LeftHand);
            cameraToScreenMapper.Moved += (p1, p2) => DoIfActive(() => Windows.Mouse.MoveCursor(p2.X, p2.Y));

            Camera.Poses.PosePeaceBegin += () => {
                if (ProfileManager.IsActive(this)) {
                    ProfileManager.Deactivate(this);
                    return;
                }
                ProfileManager.Activate(this);
            };

            var click = new CustomGesture(Camera.RightHand);
            click.AddMovement(Movement.Forward(8, 500));
            click.GestureDetected += () => DoIfActive(() => Windows.Mouse.MouseLeftClick());

            Camera.RightHand.Closed += () => DoIfActive(() => Windows.Mouse.MouseLeftDown());
            Camera.RightHand.Opened += () => DoIfActive(() => Windows.Mouse.MouseLeftUp());
            
            Camera.RightHand.Visible += () => DoIfActive(() => Windows.Mouse.MouseLeftUp());
            Camera.RightHand.NotVisible += () => DoIfActive(() => Windows.Mouse.MouseLeftUp());
        }

        public override void Deactivate() {
            Windows.Mouse.MouseLeftUp();
            ProfileManager.Deactivate(this);
        }
    }
}