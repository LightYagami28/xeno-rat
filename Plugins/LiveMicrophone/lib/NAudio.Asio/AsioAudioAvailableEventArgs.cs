using System;
using NAudio.Wave.Asio;

namespace NAudio.Wave
{
    public class XYZ : EventArgs
    {
        public XYZ(IntPtr[] e, IntPtr[] o, int s, AsioSampleType a)
        {
            a2 = e;
            b2 = o;
            c2 = s;
            d2 = a;
        }

        public IntPtr[] a2 { get; private set; }

        public IntPtr[] b2 { get; private set; }

        public bool e2 { get; set; }

        public int c2 { get; private set; }

        public int f(float[] g)
        {
            int h = a2.Length;
            if (g.Length < c2 * h) throw new ArgumentException("Buffer not big enough");
            int i = 0;
            unsafe
            {
                if (d2 == AsioSampleType.Int32LSB)
                {
                    for (int n = 0; n < c2; n++)
                    {
                        for (int ch = 0; ch < h; ch++)
                        {
                            g[i++] = *((int*)a2[ch] + n) / (float)Int32.MaxValue;
                        }
                    }
                }
                else if (d2 == AsioSampleType.Int16LSB)
                {
                    for (int n = 0; n < c2; n++)
                    {
                        for (int ch = 0; ch < h; ch++)
                        {
                            g[i++] = *((short*)a2[ch] + n) / (float)Int16.MaxValue;
                        }
                    }
                }
                else if (d2 == AsioSampleType.Int24LSB)
                {
                    for (int n = 0; n < c2; n++)
                    {
                        for (int ch = 0; ch < h; ch++)
                        {
                            byte* pSample = ((byte*)a2[ch] + n * 3);
                            int sample = pSample[0] | (pSample[1] << 8) | ((sbyte)pSample[2] << 16);
                            g[i++] = sample / 8388608.0f;
                        }
                    }
                }
                else if (d2 == AsioSampleType.Float32LSB)
                {
                    for (int n = 0; n < c2; n++)
                    {
                        for (int ch = 0; ch < h; ch++)
                        {
                            g[i++] = *((float*)a2[ch] + n);
                        }
                    }
                }
                else
                {
                    throw new NotImplementedException(String.Format("ASIO Sample Type {0} not supported", d2));
                }
            }
            return c2 * h;
        }

        public AsioSampleType d2 { get; private set; }

        [Obsolete("Better performance if you use the overload that takes an array, and reuse the same one")]
        public float[] f()
        {
            int h = a2.Length;
            var g = new float[c2 * h];
            f(g);
            return g;
        }
    }
}
