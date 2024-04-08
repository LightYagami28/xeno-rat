using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace xeno_rat_server.Forms
{
    public partial class FunMenu : Form
    {
        private readonly Node client;

        public FunMenu(Node _client)
        {
            client = _client;
            InitializeComponent();
            InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            await client.SendAsync(new byte[] { 2 });
        }

        private async void Button_Click(object sender, EventArgs e)
        {
            byte funType = 0;
            if (sender == button1)
                funType = 1;
            else if (sender == button2)
                funType = 2;
            else if (sender == button3)
                funType = 3;
            else if (sender == button4)
                funType = 4;

            await client.SendAsync(new byte[] { funType });
        }

        private async void trackBar1_Scroll(object sender, EventArgs e)
        {
            byte[] data = { 5, (byte)trackBar1.Value };
            await client.SendAsync(data);
        }
    }
}
