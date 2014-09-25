using System.CodeDom;
using System.Windows;
using MrWindows;
using SharpPerceptual;
using SimpleInjector;

namespace Sense {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {

        public static Container Container;

        static App() {
            Container = new Container();
            Container.Register<Windows>(Lifestyle.Singleton);
            Container.Register<Camera>(Lifestyle.Singleton);
            Container.Register<ProcessMonitor>(Lifestyle.Singleton);
        }
    }
}