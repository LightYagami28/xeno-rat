using System;
using System.Runtime.InteropServices;

namespace NAudio.Wave.Asio
{
    // Define a custom obfuscated name for the struct
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Pq78R
    {
        // Obfuscate field names
        public uint aB; // Obfuscated name for hi
        public uint cD; // Obfuscated name for lo

        // Method to convert the struct to a double
        public double D3Fr()
        {
            // Combine hi and lo fields to form a double
            long combined = (Convert.ToInt64(aB) << 32) | cD;
            return BitConverter.Int64BitsToDouble(combined);
        }

        // Method to convert the struct to a long
        public long Xyz1()
        {
            // Combine hi and lo fields to form a long
            return ((long)aB << 32) | cD;
        }
    };
}
