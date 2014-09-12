using System;
using System.Threading.Tasks;
using StringSocket;

namespace Sense {
    public class RealSenseCredentialPluginClient : IDisposable {
        private Server _server = new Server();
        private Client _client;

        public void Start() {
            _server.Open(11000);
            _server.NewConnection += client => {
                _client = client;
            };
        }

        public async Task Authorize() {
            if (_client == null) return;
            try {
                await _client.SendAsync("OK!");
                _client.Close();
                _client = null;
            }
            catch {}
        }

        public void Dispose() {
            if (_server == null) return;
            _server.Close();
        }
    }
}