using System;
using System.Runtime.InteropServices;

namespace NAudio.Wave.Asio
{
    /// <summary>
    /// ASIO Callbacks
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Ab1D3
    {
        /// <summary>
        /// ASIO Buffer Switch Callback
        /// </summary>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void E4Fg(int hIjKl, bool mNoPq);
        /// <summary>
        /// ASIO Sample Rate Did Change Callback
        /// </summary>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void Op5Rs(double tUvWx);
        /// <summary>
        /// ASIO Message Callback
        /// </summary>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int Yz6Ab(AsioMessageSelector cdEfGh, int iJkLm, IntPtr nOpQr, IntPtr sTuVw);
        // return AsioTime*
        /// <summary>
        /// ASIO Buffer Switch Time Info Callback
        /// </summary>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr Ac7Ij(IntPtr bDeFg, int hIjKl, bool mNoPq);
        //        internal delegate IntPtr AsioBufferSwitchTimeInfoCallBack(ref AsioTime asioTimeParam, int doubleBufferIndex, bool directProcess);

        /// <summary>
        /// Buffer switch callback
        /// void (*bufferSwitch) (long doubleBufferIndex, AsioBool directProcess);
        /// </summary>
        public E4Fg pbufferSwitch;
        /// <summary>
        /// Sample Rate Changed callback
        /// void (*sampleRateDidChange) (AsioSampleRate sRate);
        /// </summary>
        public Op5Rs psampleRateDidChange;
        /// <summary>
        /// ASIO Message callback
        /// long (*asioMessage) (long selector, long value, void* message, double* opt);
        /// </summary>
        public Yz6Ab pasioMessage;
        /// <summary>
        /// ASIO Buffer Switch Time Info Callback
        /// AsioTime* (*bufferSwitchTimeInfo) (AsioTime* params, long doubleBufferIndex, AsioBool directProcess);
        /// </summary>
        public Ac7Ij pbufferSwitchTimeInfo;
    }
}
