using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;

namespace WarpWorld.CrowdControl
{
    public class SocketProvider : IDisposable
    {
        public TcpClient _socket;
        public Stream _stream;

        private bool _is_secure;
        private readonly CancellationTokenSource _quitting = new CancellationTokenSource();
        private readonly SemaphoreSlim _ws_lock = new SemaphoreSlim(1);
        private readonly ManualResetEventSlim _ready = new ManualResetEventSlim(false);

        public event Action<byte[]> OnMessageReceived;
        public event Action<Exception, string> OnError;
        public event Action OnDisconnected;
        public event Action OnConnected;

        public bool Connected => _socket?.Connected ?? false;

        ~SocketProvider() => Dispose(true);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            CrowdControl.LogWarning("Dispose Stream");
            _quitting.Cancel();
            _ready.Set();
            try { _socket?.Close(); }
            catch { }
            try { _stream?.Dispose(); }
            catch { }
        }

        public SocketProvider() => Task.Factory.StartNew(ReadLoop, TaskCreationOptions.LongRunning);

        private async Task ReadLoop()
        {
            byte[] buf = new byte[4096];

            List<byte> msg = new List<byte>();
            while (!_quitting.IsCancellationRequested)
            {
                try
                {
                    _ready.Wait();

                    int bytesRead = await Task.Factory.StartNew(() => _stream.Read(buf, 0, buf.Length), _quitting.Token);

                    PlayloadPrint(buf, bytesRead, "Received");

                    msg.AddRange(buf.Take(bytesRead));

                    if (msg.Count < 2)
                    {
                        continue;
                    }

                    ushort len = (ushort)((msg[0] << 8) | (msg[1]));

                    if (msg.Count >= len)
                    {
                        byte[] nextMsg = msg.Take(len).ToArray();
                        msg.RemoveRange(0, len);
                        try
                        {
                            OnMessageReceived?.Invoke(nextMsg);
                        }
                        catch (Exception e)
                        {
                            CrowdControl.LogException(e);
                        }

                        msg.Clear();
                    }
                }
                catch (Exception e)
                {
                    OnError?.Invoke(e, e.Message);
                    CloseImpl();
                }
            }
        }

        private static bool ValidateCert(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true; // Allow untrusted certificates. (TEMP)
        }

        public async Task<bool> Connect(string host, ushort port, bool secure = true)
        {
            using (await _ws_lock.UseWaitAsync())
            {
                if (Connected) { await Close(); }
                _socket = new TcpClient();
                _is_secure = secure;

                try
                {
                    await Task.Factory.StartNew(() => _socket.Connect(host, port), _quitting.Token);

                    if (secure)
                    {
                        SslStream s;
                        _stream = s = new SslStream(_socket.GetStream(), false, new RemoteCertificateValidationCallback(ValidateCert));
                        s.AuthenticateAsClient(host);
                    }
                    else
                    {
                        _stream = _socket.GetStream();
                    }

                   CrowdControl.Log("Connected to server socket.");
                    if (!Connected) { return false; }
                    _ready.Set();
                    try { OnConnected?.Invoke(); }
                    catch {  }
                }
                catch (Exception e)
                {
                    _ready.Reset();
                    CrowdControl.LogException(e);
                    CrowdControl.LogError("Failed to connect to server socket.");
                    return false;
                }
            }
            return true;
        }

        public async Task<bool> Close()
        {
            using (await _ws_lock.UseWaitAsync()) { return CloseImpl(); }
        }

        private bool CloseImpl()
        {
            _ready.Reset();
            try
            {
                _socket?.Close();
                return true;
            }
            catch { return false; }
            finally
            {
                try { OnDisconnected?.Invoke(); }
                catch { }
            }
        }

        public async Task<bool> Send(byte[] message)
        {
            if (message[0] == 0 && message[1] == 0)
            {
                CrowdControl.LogError("CANNOT SEND STREAM OF SIZE 0!");
                return false;
            }

            using (await _ws_lock.UseWaitAsync())
            {
                if (!Connected) { return false; }
                try
                {
                    await Task.Factory.StartNew(() => _stream.Write(message, 0, message.Length), _quitting.Token);
                    PlayloadPrint(message, message.Length, "Sent");
                }
                catch {
                    return false;
                }
                return true;
            }
        }

        private void PlayloadPrint(byte [] bytes, int length, string verb)
        {
            string outputString = "";

            for (int i = 0; i < bytes.Length; i++)
                outputString += bytes[i].ToString("X") + "-";

            CrowdControl.Log(verb + ": " + outputString);
        }
    }
}
