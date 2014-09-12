using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace StringSocket {
    public class Client {
        private TcpClient _client;
        private TextReader _reader;
        private TextWriter _writer;

        public event Action<string> Received;

        public static Client Connect(string ip, int port) {
            var tcpClient = new TcpClient();
            tcpClient.Connect(ip, port);
            return new Client(tcpClient);
        }

        public static async Task<Client> ConnectAsync(string ip, int port) {
            var tcpClient = new TcpClient();
            await tcpClient.ConnectAsync(ip, port);
            return new Client(tcpClient);
        }

        public Client(TcpClient client) {
            _client = client;
            _reader = new StreamReader(client.GetStream());
            _writer = new StreamWriter(client.GetStream());

            Task.Run(async () => {
                while (_client != null) {
                    try {
                        var msg = await _reader.ReadLineAsync();
                        OnReceived(msg);
                    }
                    catch { }
                }
            });
        }

        public async Task SendAsync(string msg) {
            try {
                await _writer.WriteLineAsync(msg);
                await _writer.FlushAsync();
            }
            catch { }
        }

        public void Close() {
            DoIt(_reader.Dispose);
            DoIt(_writer.Dispose);
            DoIt(_client.Close);
            _reader = null;
            _writer = null;
            _client = null;
        }

        private static void DoIt(Action action) {
            try {
                action.Invoke();
            }
            catch { }
        }

        protected virtual void OnReceived(string obj) {
            Action<string> handler = Received;
            if (handler != null) handler(obj);
        }

    }
}
