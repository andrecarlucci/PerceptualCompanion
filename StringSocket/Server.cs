using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace StringSocket {
    public class Server {

        private TcpListener _listener;

        public event Action<Client> NewConnection;

        public void Open(int port) {
            CloseListener();
            _listener = new TcpListener(IPAddress.Any, port);
            _listener.Start();
            StartAcceptingConnections();
        }

        private void StartAcceptingConnections() {
            Task.Run(async () => {
                while (_listener != null) {
                    var tcpClient = await _listener.AcceptTcpClientAsync();
                    OnNewConnection(new Client(tcpClient));
                }
            });
        }

        protected virtual void OnNewConnection(Client client) {
            Action<Client> handler = NewConnection;
            if (handler != null) handler(client);
        }
        private void CloseListener() {
            if (_listener == null) return;
            _listener.Stop();
            _listener = null;
        }

        public void Close() {
            CloseListener();
        }
    }
}