using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace xeno_rat_server.Forms
{
    public partial class ProcessManager : Form
    {
        private readonly Node client;
        private bool paused = false;

        public ProcessManager(Node _client)
        {
            client = _client;
            InitializeComponent();
            Task.Run(RecvThread);
        }

        private async Task RecvThread()
        {
            while (client.Connected())
            {
                byte[] data = await client.ReceiveAsync();
                if (data == null)
                {
                    if (!IsDisposed)
                    {
                        Invoke((MethodInvoker)(() => Close()));
                    }
                    return;
                }
                if (!paused)
                {
                    List<ProcessNode> processList = DeserializeProcessList(data);
                    DisplayProcessTree(processList);
                }
            }
        }

        private void PopulateTreeView(ProcessNode node, TreeNode parentNode)
        {
            TreeNode treeNode = new TreeNode($"{node.Name} ({node.PID}): {node.FilePath} ({node.FileDescription})")
            {
                Tag = node
            };

            if (parentNode != null)
            {
                parentNode.Nodes.Add(treeNode);
            }
            else
            {
                treeView1.BeginUpdate();
                treeView1.Nodes.Add(treeNode);
            }

            foreach (var childNode in node.Children)
            {
                PopulateTreeView(childNode, treeNode);
            }

            if (parentNode == null)
            {
                treeView1.EndUpdate();
            }
        }

        private void DisplayProcessTree(List<ProcessNode> processList)
        {
            treeView1.BeginUpdate();
            treeView1.Nodes.Clear();

            foreach (var rootNode in processList)
            {
                PopulateTreeView(rootNode, null);
            }

            treeView1.EndUpdate();
        }

        private static List<ProcessNode> DeserializeProcessList(byte[] serializedData)
        {
            List<ProcessNode> processList = new List<ProcessNode>();

            using (MemoryStream memoryStream = new MemoryStream(serializedData))
                using (BinaryReader reader = new BinaryReader(memoryStream))
                {
                    int processCount = reader.ReadInt32();

                    for (int i = 0; i < processCount; i++)
                    {
                        ProcessNode processNode = DeserializeProcessNode(reader);
                        processList.Add(processNode);
                    }
                }

            return processList;
        }

        private static ProcessNode DeserializeProcessNode(BinaryReader reader)
        {
            int pid = reader.ReadInt32();
            int childCount = reader.ReadInt32();
            string filePath = reader.ReadString();
            string fileDescription = reader.ReadString();
            string name = reader.ReadString();

            ProcessNode processNode = new ProcessNode(filePath)
            {
                PID = pid,
                Name = name,
                FileDescription = fileDescription,
                Children = new List<ProcessNode>()
            };

            for (int i = 0; i < childCount; i++)
            {
                ProcessNode child = DeserializeProcessNode(reader);
                processNode.Children.Add(child);
            }

            return processNode;
        }

        private async void KillPid(int pid)
        {
            try
            {
                await client.SendAsync(client.sock.IntToBytes(pid));
                MessageBox.Show("Sent the kill command");
            }
            catch
            {
                MessageBox.Show("Error sending the kill command");
            }
        }

        private void TreeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                try
                {
                    ProcessNode procnode = (ProcessNode)e.Node.Tag;
                    ContextMenuStrip PopupMenu = new ContextMenuStrip();
                    PopupMenu.Items.Add($"Kill {procnode.Name}");
                    PopupMenu.ItemClicked += (object _, ToolStripItemClickedEventArgs __) => KillPid(procnode.PID);
                    PopupMenu.Show(Cursor.Position);
                }
                catch
                {
                    MessageBox.Show("Something went wrong...");
                }
            }
        }

        private async void Button1_Click(object sender, EventArgs e)
        {
            paused = !paused;
            await client.SendAsync(new byte[] { (byte)(paused ? 1 : 0) });
            button1.Text = paused ? "Unpause" : "Pause";
        }

        private void ProcessManager_Load(object sender, EventArgs e)
        {
        }
    }

    class ProcessNode
    {
        public string FilePath { get; }
        public int PID { set; get; }
        public string FileDescription { get; set; }
        public string Name { set; get; }
        public List<ProcessNode> Children { get; set; }

        public ProcessNode(string filePath)
        {
            FilePath = string.IsNullOrEmpty(filePath) ? "Unknown Path" : filePath;
        }
    }
}
