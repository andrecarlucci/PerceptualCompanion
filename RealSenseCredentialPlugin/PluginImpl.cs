using System;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Microsoft.Win32;
using pGina.Shared.Interfaces;
using pGina.Shared.Types;
using StringSocket;
using XDMessaging;

namespace RealSenseCredentialPlugin {
    public class PluginImpl : IPluginAuthentication {

        private ILog _logger;
        private XDMessagingClient _client;
        private IXDListener _listener;
        private IXDBroadcaster _broadcaster;
        private bool _shouldAuthenticate;

        public PluginImpl() {
            _client = new XDMessagingClient();
            _logger = LogManager.GetLogger("pGina.Plugin.RealSensePlugin");
            _broadcaster = _client.Broadcasters.GetBroadcasterForMode(XDTransportMode.HighPerformanceUI);
        }

        public void Starting() {
        }

        public void Stopping() {
            _logger.Debug("Stopping: ");
            //_listener.Dispose();
        }

        public string Name {
            get { return "RealSense"; }
        }

        public string Description { get; private set; }

        public string Version {
            get { return Assembly.GetExecutingAssembly().GetName().Version.ToString(); }
        }

        public Guid Uuid {
            get { return new Guid("E2FD0D17-F972-4B01-BF90-D80A64506696"); }
        }

        public BooleanResult AuthenticateUser(SessionProperties properties) {
            var userInfo = properties.GetTrackedSingle<UserInformation>();
            var authorize = false;
            Task.Run(async () => {
                var client = await Client.ConnectAsync("127.0.0.1", 11000);
                client.Received += s => {
                    authorize = true;
                    client.Close();
                };
                await client.SendAsync("Authorize");
            }).Wait();

            for (int i = 0; i < 100; i++) {
                if (authorize) {
                    return new BooleanResult { Success = true };
                }
                Thread.Sleep(100);
            }
            return new BooleanResult { Success = false };
        }
    }
}