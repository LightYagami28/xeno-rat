using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace XenoRatServer.Forms
{
    public partial class DebugInfo : Form
    {
        private Node client;

        public DebugInfo(Node _client)
        {
            InitializeComponent();
            client = _client;
            AsyncInit();
        }

        public async Task AsyncInit()
        {
            await ResetAndPopulateListView();
        }

        public async Task<string[]> GetDlls()
        {
            await client.SendAsync(new byte[] { 4, 0 });
            byte[] dllInfoBytes = await client.ReceiveAsync();
            string dllInfo = Encoding.UTF8.GetString(dllInfoBytes);
            return dllInfo.Split('\n');
        }

        public async Task<bool> UnloadDll(string dllName)
        {
            byte[] payload = new byte[] { 4, 1 };
            payload = client.sock.Concat(payload, Encoding.UTF8.GetBytes(dllName));
            await client.SendAsync(payload);
            bool worked = (await client.ReceiveAsync())[0] == 1;
            return worked;
        }

        public async Task<string> GetConsoleOutput()
        {
            await client.SendAsync(new byte[] { 4, 2 });
            byte[] consoleOutputBytes = await client.ReceiveAsync();
            return Encoding.UTF8.GetString(consoleOutputBytes);
        }

        public async Task ResetAndPopulateListView()
        {
            listView1.Items.Clear();
            foreach (string i in await GetDlls())
            {
                listView1.Items.Add(i);
            }
        }

        public async Task RemoveClick(string dllName)
        {
            if (await UnloadDll(dllName))
            {
                await Task.Run(() => MessageBox.Show("DLL unloaded"));
            }
            else
            {
                await Task.Run(() => MessageBox.Show("There was an issue unloading the DLL"));
            }
            await ResetAndPopulateListView();
        }

        public async Task PopulateConsoleOutput()
        {
            string consoleOutput = await GetConsoleOutput();
            richTextBox1.Text = consoleOutput;
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            await ResetAndPopulateListView();
        }

        private void listView1_MouseClick(object sender, MouseEventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;
            ContextMenuStrip contextMenu = new ContextMenuStrip();
            ToolStripMenuItem menuItem = new ToolStripMenuItem("Unload");
            menuItem.Click += new EventHandler(async (_, __) => await RemoveClick(listView1.SelectedItems[0].Text));
            contextMenu.Items.Add(menuItem);
            contextMenu.Show(Cursor.Position);
        }

        private void listView1_ItemActivate(object sender, EventArgs e)
        {

        }

        private async void button2_Click(object sender, EventArgs e)
        {
            await PopulateConsoleOutput();
        }
    }
}
