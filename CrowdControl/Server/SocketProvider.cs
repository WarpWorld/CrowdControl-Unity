using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using WebSocket4Net;

namespace WarpWorld.CrowdControl {
    public class SocketProvider : IDisposable {
        private bool _is_secure;
        private readonly SemaphoreSlim _ws_lock = new SemaphoreSlim(1);
        private readonly ManualResetEventSlim _ready = new ManualResetEventSlim(false);
        public event Action OnDisconnected;
        public WebSocket webSocket { get; private set; }

        public bool Connected => webSocket != null && webSocket.State == WebSocketState.Open;

        private bool error = false;
        private bool connected = false;
        private string socketHost = ""; 

        ~SocketProvider() => Dispose();

        public void Dispose() {
            CrowdControl.LogWarning("Dispose Stream");
            _ready.Set();
            try { webSocket?.Close(); }
            catch { }
            GC.SuppressFinalize(this);
        }

        public async Task<bool> Connect(string host, ushort port, bool secure = false) {
            using (
#if (NET35 || NET40)
                await _ws_lock.UseWaitAsync()
#else
                await _ws_lock.UseWaitAsync(new CancellationToken())
#endif
                ) {
                if (Connected) { CloseImpl(); }

                try {
                    CrowdControl.Log("CONNECTING SOCKET");
                    socketHost = host;
                    webSocket = new WebSocket(host);
                    webSocket.Opened += SocketOpened;
                    webSocket.MessageReceived += SocketMessageReceived;
                    webSocket.Closed += SocketClosed;
                    webSocket.Error += SocketTimeout;
                    webSocket.Open();

                    if (!Connected) { return false; }

                    try { }
                    catch { }
                }
                catch (Exception e) {
                    _ready.Reset();
                    CrowdControl.LogException(e);
                    CrowdControl.LogError("Failed to connect to server socket.");
                    CrowdControl.instance.ConnectError(e);
                    return false;
                }
            }

            return true;
        }

        private void SocketTimeout(object sender, EventArgs e) {
            CrowdControl.LogWarning("Socket timeout. Reconnecting");
            error = true;
            webSocket.Close();
            webSocket.Dispose();

            webSocket = new WebSocket(socketHost);
            webSocket.Opened += SocketOpened;
            webSocket.MessageReceived += SocketMessageReceived;
            webSocket.Closed += SocketClosed;
            webSocket.Error += SocketTimeout;
            webSocket.Open();
        }

        public void ErrorTest() {
            SocketTimeout(null, null);
        }

        private void SocketOpened(object sender, EventArgs e) {
            if (connected && error) {
                error = false;
                CrowdControl.LogWarning("Reconnected.");
                CrowdControl.instance.ContinueSession();
                return;
            }

            CrowdControl.Log("Connected to server socket.");
            _ready.Set();
            CrowdControl.instance.RunConnectedAction();
            error = false;
            connected = true;
        }

        private void SocketMessageReceived(object sender, MessageReceivedEventArgs e) {
            ThreadPool.QueueUserWorkItem(_ => {
                CrowdControl.instance.AddToJsonQueue(e.Message);
            });
        }

        private void SocketClosed(object sender, EventArgs e) {
            CrowdControl.Log("Server socket closed.");
            _ready.Set();
            CrowdControl.instance.RunDisconnectedAction();
            connected = false;
        }

        public void Close() {
            CrowdControl.Log("Closing Socket");
            webSocket.Opened -= SocketOpened;
            webSocket.MessageReceived -= SocketMessageReceived;
            webSocket.Closed -= SocketClosed;
            webSocket?.Close();
        }

        private bool CloseImpl() {
            _ready.Reset();
            try {
                Close();
                return true;
            }
            catch { return false; }
            finally {
                try { OnDisconnected?.Invoke(); }
                catch { }
            }
        }

        public void Send(string message) {
            CrowdControl.Log("SENT: " + message);
            webSocket.Send(message);
        }
    }
}
