using MrWindows;
using MrWindows.KeyboardControl;
using SharpPerceptual;

namespace Sense.Profiles {
    public class SwipeToArrowsProfile : Profile {
        public SwipeToArrowsProfile(Windows windows, Camera camera, ProcessMonitor processMonitor) : base(windows, camera, processMonitor) {}

        public override string Name {
            get { return "SwipeToArrows"; }
        }

        public override void Config() {
            ConfigGestures();
            ProcessMonitor.ActiveProcessLoop += s => {
                if (s != "chrome") {
                    if (ProfileManager.IsEmpty()) {
                        ProfileManager.Activate(this);                        
                    }
                }
                else {
                    ProfileManager.Deactivate(this);
                }
            };
        }

        protected void ConfigGestures() {
            Camera.Gestures.GestureSwipeLeft += () => DoIfActive(() => Windows.Keyboard.Type(VirtualKey.Left));
            Camera.Gestures.GestureSwipeRight += () => DoIfActive(() => Windows.Keyboard.Type(VirtualKey.Right));
        }
    }
}