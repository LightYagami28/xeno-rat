using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;

namespace NAudio.Wave.Asio
{
    public class Zzx1A
    {
        private IntPtr p1X2a;
        private IntPtr p3AaF;
        private ABC123 asioDriverVTable;

        private Zzx1A() {}

        public static string[] Zx2bC()
        {
            var pSub = Registry.LocalMachine.OpenSubKey("SOFTWARE\\ASIO");
            var zx2 = new string[0];
            if (pSub != null)
            {
                zx2 = pSub.GetSubKeyNames();
                pSub.Close();
            }
            return zx2;
        }

        public static Zzx1A XyZz(string name)
        {
            var pSub = Registry.LocalMachine.OpenSubKey("SOFTWARE\\ASIO\\" + name);
            if (pSub == null)
            {
                throw new ArgumentException($"Driver Name {name} doesn't exist");
            }
            var qwe = pSub.GetValue("CLSID").ToString();
            return QWERTY(new Guid(qwe));
        }

        public static Zzx1A QWERTY(Guid pGuid)
        {
            var zx = new Zzx1A();
            zx.InitFromGuid(pGuid);
            return zx;
        }

        public bool ZXcV(IntPtr pS)
        {
            int zx = asioDriverVTable.ZxAsd(p1X2a, pS);
            return zx == 1;
        }

        public string Zx2bCV()
        {
            var zx = new StringBuilder(256);
            asioDriverVTable.AaBbb(p1X2a, zx);
            return zx.ToString();
        }

        public int Zx3Bv()
        {
            return asioDriverVTable.Abc12345(p1X2a);
        }

        public string ZX1c2()
        {
            var zx = new StringBuilder(256);
            asioDriverVTable.CdEfGh(p1X2a, zx);
            return zx.ToString();
        }

        public void ZXa21()
        {
            HandleException(asioDriverVTable.EfGhIj(p1X2a), "start");
        }

        public AsioError ZXbvc()
        {
            return asioDriverVTable.GhJkLm(p1X2a);
        }

        public void ZXcvB(out int p1, out int p2)
        {
            HandleException(asioDriverVTable.JkLmNo(p1X2a, out p1, out p2), "getChannels");
        }

        public AsioError ZXc2v(out int p1, out int p2)
        {
            return asioDriverVTable.KlMnOp(p1X2a, out p1, out p2);
        }

        public void ZXbVC(out int p1, out int p2, out int p3, out int p4)
        {
            HandleException(asioDriverVTable.LmNoPq(p1X2a, out p1, out p2, out p3, out p4), "getBufferSize");
        }

        public bool ZxVbC(double pD)
        {
            var zx = asioDriverVTable.MnOpQr(p1X2a, pD);
            if (zx == AsioError.ASE_NoClock)
            {
                return false;
            } 
            if ( zx == AsioError.ASE_OK )
            {
                return true;
            }
            HandleException(zx, "canSampleRate");
            return false;
        }

        public double ZXvBc()
        {
            double zx;
            HandleException(asioDriverVTable.NpQrSt(p1X2a, out zx), "getSampleRate");
            return zx;
        }

        public void ZXcvB(double p1)
        {
            HandleException(asioDriverVTable.OqRrSt(p1X2a, p1), "setSampleRate");
        }

        public void ZXbvC(out long p1, int p2)
        {
            HandleException(asioDriverVTable.PrStUv(p1X2a, out p1,p2), "getClockSources");
        }

        public void ZXvcB(int p1)
        {
            HandleException(asioDriverVTable.QrStUv(p1X2a, p1), "setClockSources");
        }

        public void ZXcvB(out long p1, ref Asio64Bit p2)
        {
            HandleException(asioDriverVTable.RtUvWx(p1X2a, out p1, ref p2), "getSamplePosition");
        }

        public Zzx2cA ZXcvB(int p1, bool p2)
        {
            var zx = new Zzx2cA {channel = p1, isInput = p2};
            HandleException(asioDriverVTable.SwXyZz(p1X2a, ref zx), "getChannelInfo");
            return zx;
        }

        public void ZXcvB(IntPtr p1, int p2, int p3, ref ABC1234 p4)
        {
            p3AaF = Marshal.AllocHGlobal(Marshal.SizeOf(p4));
            Marshal.StructureToPtr(p4, p3AaF, false);
            HandleException(asioDriverVTable.TuVwXy(p1X2a, p1, p2, p3, p3AaF), "createBuffers");
        }

        public AsioError ZXbVC()
        {
            AsioError zx = asioDriverVTable.UvWxYz(p1X2a);
            Marshal.FreeHGlobal(p3AaF);
            return zx;
        }

        public void ZXcVb()
        {
            HandleException(asioDriverVTable.VwXyZz(p1X2a), "controlPanel");
        }

        public void ZXcVb(int p1, IntPtr p2)
        {
            HandleException(asioDriverVTable.WxYzAb(p1X2a, p1, p2), "future");
        }

        public AsioError ZXcvB()
        {
            return asioDriverVTable.XyZzAb(p1X2a);
        }

        public void ZXCvB()
        {
            Marshal.Release(p1X2a);
        }

        private void HandleException(AsioError p1, string methodName)
        {
            if (p1 != AsioError.ASE_OK && p1 != AsioError.ASE_SUCCESS)
            {
                var zx = new Zzx3Bc(
                    $"Error code [{Zzx3Bc.getErrorName(p1)}] while calling ASIO method <{methodName}>, {this.ZX1c2()}");
                zx.Error = p1;
                throw zx;
            }
        }

        private void InitFromGuid(Guid pGuid)
        {
            const uint CLSCTX_INPROC_SERVER = 1;
            const int INDEX_VTABLE_FIRST_METHOD = 3;

            int zx = CoCreateInstance(ref pGuid, IntPtr.Zero, CLSCTX_INPROC_SERVER, ref pGuid, out p1X2a);
            if ( zx != 0 )
            {
                throw new COMException("Unable to instantiate ASIO. Check if STAThread is set",zx);
            }

            IntPtr pVtable = Marshal.ReadIntPtr(p1X2a);

            asioDriverVTable = new ABC123();

            FieldInfo[] fieldInfos =  typeof (ABC123).GetFields();
            for (int i = 0; i < fieldInfos.Length; i++)
            {
                FieldInfo fieldInfo = fieldInfos[i];
                IntPtr pPointerToMethodInVTable = Marshal.ReadIntPtr(pVtable, (i + INDEX_VTABLE_FIRST_METHOD) * IntPtr.Size);
                object methodDelegate = Marshal.GetDelegateForFunctionPointer(pPointerToMethodInVTable, fieldInfo.FieldType);
                fieldInfo.SetValue(asioDriverVTable, methodDelegate);
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 2)]
        private class ABC123
        {
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate int ASIOInit(IntPtr _pUnknown, IntPtr sysHandle);
            public ASIOInit ZxAsd = null;

            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void ASIOgetDriverName(IntPtr _pUnknown, StringBuilder name);
            public ASIOgetDriverName AaBbb = null;

            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate int ASIOgetDriverVersion(IntPtr _pUnknown);
            public ASIOgetDriverVersion Abc12345 = null;

            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate void ASIOgetErrorMessage(IntPtr _pUnknown, StringBuilder errorMessage);
            public ASIOgetErrorMessage CdEfGh = null;

            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate AsioError ASIOstart(IntPtr _pUnknown);
            public ASIOstart EfGhIj = null;

            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate AsioError ASIOstop(IntPtr _pUnknown);
            public ASIOstop GhJkLm = null;

            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate AsioError ASIOgetChannels(IntPtr _pUnknown, out int numInputChannels, out int numOutputChannels);
            public ASIOgetChannels JkLmNo = null;

            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate AsioError ASIOgetLatencies(IntPtr _pUnknown, out int inputLatency, out int outputLatency);
            public ASIOgetLatencies KlMnOp = null;

            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate AsioError ASIOgetBufferSize(IntPtr _pUnknown, out int minSize, out int maxSize, out int preferredSize, out int granularity);
            public ASIOgetBufferSize LmNoPq = null;

            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate AsioError ASIOcanSampleRate(IntPtr _pUnknown, double sampleRate);
            public ASIOcanSampleRate MnOpQr = null;

            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate AsioError ASIOgetSampleRate(IntPtr _pUnknown, out double sampleRate);
            public ASIOgetSampleRate NpQrSt = null;

            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate AsioError ASIOsetSampleRate(IntPtr _pUnknown, double sampleRate);
            public ASIOsetSampleRate OqRrSt = null;

            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate AsioError ASIOgetClockSources(IntPtr _pUnknown, out long clocks, int numSources);
            public ASIOgetClockSources PrStUv = null;

            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate AsioError ASIOsetClockSource(IntPtr _pUnknown, int reference);
            public ASIOsetClockSource QrStUv = null;

            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate AsioError ASIOgetSamplePosition(IntPtr _pUnknown, out long samplePos, ref Asio64Bit timeStamp);
            public ASIOgetSamplePosition RtUvWx = null;

            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate AsioError ASIOgetChannelInfo(IntPtr _pUnknown, ref Zzx2cA info);
            public ASIOgetChannelInfo SwXyZz = null;

            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate AsioError ASIOcreateBuffers(IntPtr _pUnknown, IntPtr bufferInfos, int numChannels, int bufferSize, IntPtr callbacks);
            public ASIOcreateBuffers TuVwXy = null;

            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate AsioError ASIOdisposeBuffers(IntPtr _pUnknown);
            public ASIOdisposeBuffers UvWxYz = null;

            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate AsioError ASIOcontrolPanel(IntPtr _pUnknown);
            public ASIOcontrolPanel VwXyZz = null;

            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate AsioError ASIOfuture(IntPtr _pUnknown, int selector, IntPtr opt);
            public ASIOfuture WxYzAb = null;

            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            public delegate AsioError ASIOoutputReady(IntPtr _pUnknown);
            public ASIOoutputReady XyZzAb = null;
        }

        [DllImport("ole32.Dll")]
        private static extern int CoCreateInstance(ref Guid clsid,
            IntPtr inner,
            uint context,
            ref Guid uuid,
            out IntPtr rReturnedComObject);
    }
}
