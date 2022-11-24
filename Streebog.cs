using System.Diagnostics;
using System.Numerics;

namespace IS_Lab_Streebog
{
    static class Streebog
    {
        const int SIZE = 64;

        private static byte[] ByteXor(this byte[] a, byte[] b)
        {
            byte[] c = new byte[SIZE];
            for (int i = 0; i < SIZE; i++)
                c[i] = (byte)(a[i] ^ b[i]);
            return c;
        }

        private static byte[] STransform(this byte[] a)
        {
            byte[] b = new byte[SIZE];
            for (int i = 0; i < SIZE; i++)
                b[i] = StreebogConstants.Pi[a[i]];
            return b;
        }

        private static byte[] PTransform(this byte[] a)
        {
            byte[] b = new byte[SIZE];
            for (int i = 0; i < SIZE; i++)
                b[i] = a[StreebogConstants.Tau[i]];

            return b;
        }

        private static byte[] LTransform(this byte[] a)
        {
            const int BIT_MASK = 0x01;
            byte[] result = Array.Empty<byte>();
            for (int i = 0; i < 8; i++)
            {
                ulong V = BitConverter.ToUInt64(a, 8 * i);
                ulong t = 0;
                for (int j = 0; j < SIZE; j++)
                    if ((BIT_MASK & (V >> j)) == 1)
                        t ^= StreebogConstants.A[SIZE - 1 - j];

                result = result.Concat(BitConverter.GetBytes(t)).ToArray();
            }
            return result;
        }

        private static byte[] Compress(byte[] N, byte[] m, byte[] h)
        {
            byte[] t = LPSX(h, N);
            t = ETransform(t, m);
            t = ByteXor(h, t);
            return ByteXor(t, m);
        }

        private static byte[] ETransform(byte[] K, byte[] m)
        {
            byte[] state = m;
            for (int i = 0; i < 12; i++)
            {
                state = LPSX(state, K);
                K = LPSX(K, StreebogConstants.C[i].Reverse().ToArray());
            }
            return ByteXor(state, K);
        }

        private static byte[] LPSX(byte[] a, byte[] b) 
            => ByteXor(a, b)
                .STransform()
                .PTransform()
                .LTransform();

        public static byte[] Hash(byte[] m, bool shit = true)
        {
            byte[] h = new byte[SIZE];
            if (!shit) 
                Array.Fill<byte>(h, 1);
            byte[] N = new byte[SIZE];
            byte[] Sigma = new byte[SIZE];
            byte[] block = new byte[SIZE];

            m = m.Reverse().ToArray();

            while (m.Length >= SIZE)
            {
                Array.Copy(m, block, SIZE);
                h = Compress(N, block, h);

                N = ((new BigInteger(N) + 512) % BigInteger.Pow(2, 512)).ToByteArray();
                Array.Resize(ref N, SIZE);
                Sigma = ((new BigInteger(Sigma) + new BigInteger(block)) % BigInteger.Pow(2, 512)).ToByteArray();
                Array.Resize(ref Sigma, SIZE);

                m = m.Skip(SIZE).ToArray();
            }

            int Length = m.Length;
            m = m.Append((byte)1).ToArray();
            m = m.Concat(new byte[SIZE - m.Length]).ToArray();

            h = Compress(N, m, h);

            N = ((new BigInteger(N) + 8 * Length) % BigInteger.Pow(2, 512)).ToByteArray();
            Array.Resize(ref N, SIZE);
            Sigma = ((new BigInteger(Sigma) + new BigInteger(m)) % BigInteger.Pow(2, 512)).ToByteArray();
            Array.Resize(ref Sigma, SIZE);

            h = Compress(new byte[SIZE], N, h);
            h = Compress(new byte[SIZE], Sigma, h);
            if (!shit)
                h = h.Skip(SIZE / 2).ToArray();

            h = h.Reverse().ToArray();

            return h;
        }
    }
}
