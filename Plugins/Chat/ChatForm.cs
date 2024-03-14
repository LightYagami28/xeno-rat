using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using xeno_rat_client;

namespace Chat
{
    public partial class ChatForm : Form
    {
        Node server;
        bool receivedHeartbeat;

        public ChatForm(Node _server)
        {
            server = _server;
            InitializeComponent();
            textBox1.Enabled = false;
            InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            Heartbeat();
            await RecvThread();
        }

        public async Task Heartbeat()
        {
            while (server.Connected())
            {
                await Task.Delay(5000);
                if (receivedHeartbeat)
                {
                    receivedHeartbeat = false;
                    continue;
                }
                server.Disconnect();
                break;
            }
        }

        public async Task RecvThread()
        {
            while (server.Connected())
            {
                byte[] data = await server.ReceiveAsync();
                if (data == null)
                {
                    break;
                }
                if (data.Length == 1)
                {
                    if (data[0] == 4)
                    {
                        receivedHeartbeat = true;
                        continue;
                    }
                }
                string message = Encoding.UTF8.GetString(data);
                textBox1.BeginInvoke((MethodInvoker)(() =>
                {
                    textBox1.Text += "Admin: " + message + Environment.NewLine;
                    textBox1.SelectionStart = textBox1.Text.Length;
                    textBox1.ScrollToCaret();
                }));
            }
            Console.WriteLine("End!");
            if (!IsDisposed)
            {
                BeginInvoke((MethodInvoker)(() =>
                {
                    Close();
                }));
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            string message = textBox2.Text;
            textBox2.Text = "";
            await server.SendAsync(Encoding.UTF8.GetBytes(message));
            textBox1.Text += "You: " + message + Environment.NewLine;
            textBox1.SelectionStart = textBox1.Text.Length;
            textBox1.ScrollToCaret();
        }

        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                button1.PerformClick();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void ChatForm_Load(object sender, EventArgs e)
        {
            Activate();
        }
    }
}
