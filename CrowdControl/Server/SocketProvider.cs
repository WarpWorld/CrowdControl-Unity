using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using WebSocket4Net;

namespace WarpWorld.CrowdControl {
    public class SocketProvider : IDisposable {
        public TcpClient _socket;

        private bool _is_secure;
        private readonly SemaphoreSlim _ws_lock = new SemaphoreSlim(1);
        private readonly ManualResetEventSlim _ready = new ManualResetEventSlim(false);
        public event Action OnDisconnected;
        private WebSocket webSocket;

        public bool Connected => webSocket != null && webSocket.State == WebSocketState.Open;

        ~SocketProvider() => Dispose(true);

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing) {
            CrowdControl.LogWarning("Dispose Stream");
            _ready.Set();
            try { webSocket?.Close(); }
            catch { }
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
                    webSocket = new WebSocket(host);
                    webSocket.Opened += SocketOpened;
                    webSocket.MessageReceived += SocketMessageReceived;
                    webSocket.Closed += SocketClosed;
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

        private void SocketOpened(object sender, EventArgs e) {
            CrowdControl.Log("Connected to server socket.");
            _ready.Set();
            CrowdControl.instance.RunConnectedAction();
        }

        private void SocketMessageReceived(object sender, MessageReceivedEventArgs e) {
            ThreadPool.QueueUserWorkItem(_ => {
                CrowdControl.jsonQueue.Enqueue(e.Message);
            });
        }

        private void SocketClosed(object sender, EventArgs e) {
            CrowdControl.Log("Connected to server socket.");
            _ready.Set();
            CrowdControl.instance.RunConnectedAction();
        }

        public void Close() {
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
