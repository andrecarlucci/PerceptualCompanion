using System;
using System.Diagnostics;
using MrWindows;
using MrWindows.KeyboardControl;
using SharpPerceptual;

namespace Sense.Profiles {
    public class SwipeToControlTabProfile : Profile {
        public SwipeToControlTabProfile(Windows windows, Camera camera, ProcessMonitor processMonitor)
            : base(windows, camera, processMonitor) {}

        public override string Name {
            get { return "SwipeToControlTab"; }
        }

        public override void Config() {
            ConfigGestures();
            ProcessMonitor.ActiveProcessLoop += s => {
                if (s == "chrome") {
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
            Camera.Gestures.GestureSwipeLeft +=
                () => DoIfActive(() => {
                    Debug.WriteLine("Control+Shift+Tab");
                    Windows.Keyboard.Type(VirtualKey.Control, VirtualKey.Shift, VirtualKey.Tab);
                });
            Camera.Gestures.GestureSwipeRight +=
                () => DoIfActive(() => {
                    Debug.WriteLine("Control+Tab");
                    Windows.Keyboard.Type(VirtualKey.Control, VirtualKey.Tab);
                });
        }
    }
}