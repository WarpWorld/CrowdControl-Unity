using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using WebSocket4Net;

namespace WarpWorld.CrowdControl {
    public class SocketProvider : IDisposable {
        public TcpClient _socket;
        public Stream _stream;

        private bool _is_secure;
        private readonly CancellationTokenSource _quitting = new CancellationTokenSource();
        private readonly SemaphoreSlim _ws_lock = new SemaphoreSlim(1);
        private readonly ManualResetEventSlim _ready = new ManualResetEventSlim(false);

        public event Action<string> OnMessageReceived;
        public event Action<Exception, string> OnError;
        public event Action OnDisconnected;

        public bool Connected => webSocket != null && webSocket.State == WebSocketState.Open;

        ~SocketProvider() => Dispose(true);

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing) {
            CrowdControl.LogWarning("Dispose Stream");
            _quitting.Cancel();
            _ready.Set();
            try { webSocket?.Close(); }
            catch { }
        }

        //public SocketProvider() => Task.Factory.StartNew(ReadLoop, TaskCreationOptions.LongRunning);

        private async Task ReadLoop() {
            byte[] buf = new byte[4096];

            List<byte> msg = new List<byte>();
            while (!_quitting.IsCancellationRequested) {
                try {
                    _ready.Wait();
#if (NET35 || NET40) 
                    int bytesRead = await Task.Factory.StartNew(() => _stream.Read(buf, 0, buf.Length), _quitting.Token);
#else
                    int bytesRead = await _stream.ReadAsync(buf, 0, buf.Length, _quitting.Token);
#endif
                    PlayloadPrint(buf, bytesRead, "Received");

                    msg.AddRange(buf.Take(bytesRead));

                    if (msg.Count < 2)
                        continue;

                    ushort len = (ushort)((msg[0] << 8) | (msg[1]));

                    if (msg.Count >= len) {
                        byte[] nextMsg = msg.Take(len).ToArray();
                        msg.RemoveRange(0, len);
                        try {
                           // OnMessageReceived?.Invoke(nextMsg);
                        }
                        catch (Exception e) {
                            CrowdControl.LogException(e);
                        }

                        msg.Clear();
                    }
                }
                catch (Exception e) {
                    try { OnError?.Invoke(e, e.Message); }
                    catch (Exception ex) { CrowdControl.LogException(ex); }
                    CloseImpl();
                }
            }
        }

        private static bool ValidateCert(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) {
            return true; // Allow untrusted certificates. (TEMP)
        }

        WebSocket webSocket;

        public async Task<bool> Connect(string host, ushort port, bool secure = false) {
            using (
#if (NET35 || NET40)
                await _ws_lock.UseWaitAsync()
#else
                await _ws_lock.UseWaitAsync(new CancellationToken())
#endif
                )
            {
                if (Connected) { CloseImpl(); }

                try {
                    webSocket = new WebSocket(host);
                    
                    webSocket.Opened += (sender, e) => {
                        CrowdControl.Log("Connected to server socket.");
                        _ready.Set();
                        CrowdControl.instance.RunConnectedAction();
                    };

                    webSocket.MessageReceived += (sender, e) => {
                        CrowdControl.jsonQueue.Enqueue(e.Message);
                    };

                    webSocket.Closed += (sender, e) => {
                        CrowdControl.Log("Closed server socket.");
                    };

                    webSocket.Open();

                    if (!Connected) { return false; }
                    
                    try {  }
                    catch { }
                }
                catch (Exception e) {
                    _ready.Reset();
                    CrowdControl.LogException(e);
                    CrowdControl.LogError("Failed to connect to server socket.");
                    return false;
                }
            }
            return true;
        }

        public async Task<bool> Close() {
            webSocket?.Close();
            return true;
        }

        private bool CloseImpl() {
            _ready.Reset();
            try {
                webSocket?.Close();
                return true;
            }
            catch { return false; }
            finally {
                try { OnDisconnected?.Invoke(); }
                catch { }
            }
        }

        public bool QuickSend(byte[] message) {
            if (message[0] == 0 && message[1] == 0) {
                CrowdControl.LogError("CANNOT SEND STREAM OF SIZE 0!");
                return false;
            }

            if (!Connected) { return false; }
            try {

#if (NET35 || NET40)
                Task.Factory.StartNew(() => _stream.Write(message, 0, message.Length), _quitting.Token);
#else
                _stream.WriteAsync(message, 0, message.Length, _quitting.Token);
#endif
                PlayloadPrint(message, message.Length, "Sent");
            }
            catch {
                return false;
            }

            return true;
        }

        public async Task<bool> Send(byte[] message) {
            using (
#if (NET35 || NET40)
                await _ws_lock.UseWaitAsync()
#else
                await _ws_lock.UseWaitAsync(new CancellationToken())
#endif
                )
            {
                if (!Connected) { return false; }
                try
                {
#if (NET35 || NET40)
                    await Task.Factory.StartNew(() => _stream.Write(message, 0, message.Length), _quitting.Token);
#else
                    await _stream.WriteAsync(message, 0, message.Length, _quitting.Token);
#endif
                    PlayloadPrint(message, message.Length, "Sent");
                }
                catch
                {
                    return false;
                }

                return true;
            }
        }

        public async Task<bool> Send(string json)
        {
            CrowdControl.Log("SENT: " + json);
            webSocket.Send(json);
            return true;
        }

        private void PlayloadPrint(byte [] bytes, int length, string verb) {
            string outputString = "";

            for (int i = 0; i < bytes.Length; i++)
                outputString += bytes[i].ToString("X") + "-";

            CrowdControl.Log(verb + ": " + outputString);
        }
    }
}
