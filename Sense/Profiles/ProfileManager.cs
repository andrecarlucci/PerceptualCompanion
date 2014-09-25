using System;
using System.Collections.Generic;
using System.Linq;
using MrWindows;
using SharpPerceptual;

namespace Sense.Profiles {
    public class ProfileManager {
        public static List<Profile> ActiveProfiles = new List<Profile>();

        public static event Action<List<Profile>> ProfileChanged;

        protected static void OnProfileChanged() {
            Action<List<Profile>> handler = ProfileChanged;
            if (handler != null) handler(ActiveProfiles);  
        }

        public static bool IsActive(string profileName) {
            return ActiveProfiles.Any(x => x.Name == profileName);
        }

        public static bool IsEmpty() {
            return !ActiveProfiles.Any();
        }

        public static bool IsActive(Profile profile) {
            return IsActive(profile.Name);
        }

        public static void Activate(params Profile[] profiles) {
            foreach (var p in ActiveProfiles) {
                p.Deactivate();
            }
            ActiveProfiles.Clear();
            ActiveProfiles.AddRange(profiles);
            OnProfileChanged();
        }

        public static void Deactivate(Profile profile) {
            if (ActiveProfiles.Contains(profile)) {
                ActiveProfiles.Remove(profile);
                profile.Deactivate();
                OnProfileChanged();
            }
        }

        public static void EnableProfile<T>() where T : Profile {
            var pars = new object[] {
                App.Container.GetInstance<Windows>(), 
                App.Container.GetInstance<Camera>(),
                App.Container.GetInstance<ProcessMonitor>()
            };
            var profile = (Profile) Activator.CreateInstance(typeof (T), pars);
            profile.Config();
        }
    }
}