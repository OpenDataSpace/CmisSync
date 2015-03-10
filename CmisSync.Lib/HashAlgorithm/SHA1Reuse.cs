//-----------------------------------------------------------------------
// <copyright file="SHA1Reuse.cs" company="GRAU DATA AG">
//
//   This program is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General private License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//
//   This program is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//   GNU General private License for more details.
//
//   You should have received a copy of the GNU General private License
//   along with this program. If not, see http://www.gnu.org/licenses/.
//
// </copyright>
//-----------------------------------------------------------------------

namespace CmisSync.Lib.HashAlgorithm {
    using System;
    using System.Text;
    using System.Collections;
    using System.Security.Cryptography;

    //  Reference: http://blog.csdn.net/dingwood/article/details/7506620
    public class SHA1Reuse : HashAlgorithm, HashAlgorithmReuse {
        private UInt32[] Digests = new UInt32[5];
        private const int BufferLength = 64;
        private byte[] Buffer = new byte[BufferLength];
        private int BufferOffset = 0;
        private long Length = 0;

        public SHA1Reuse() {
            Initialize();
        }

        public SHA1Reuse(SHA1Reuse from) {
            for (int i = 0; i < Digests.Length; ++i) {
                Digests[i] = from.Digests[i];
            }
            for (int i = 0; i < BufferLength; ++i) {
                Buffer[i] = from.Buffer[i];
            }
            BufferOffset = from.BufferOffset;
            Length = from.Length;
        }

        public HashAlgorithm GetHashAlgorithm() {
            return new SHA1Reuse(this);
        }

        protected override void HashCore(byte[] array, int ibStart, int cbSize) {
            for (int i = 0; i < cbSize; ++i) {
                Buffer[BufferOffset] = array[ibStart + i];
                ++BufferOffset;
                ++Length;
                if (BufferOffset >= BufferLength) {
                    SHA1_TransformBuffer();
                    BufferOffset = 0;
                }
            }
        }

        protected override byte[] HashFinal() {
            long zeros = 0;
            long ones = 1;
            long size = 0;
            long m = Length % 64;
            if (m < 56) {
                zeros = 55 - m;
                size = Length - m + 64;
            } else if (m == 56) {
                zeros = 63;
                ones = 1;
                size = Length + 8 + 64;
            } else {
                zeros = 63 - m + 56;
                size = Length + 64 - m + 64;
            }

            if (ones == 1) {
                Buffer[BufferOffset] = (byte)0x80;
                BufferOffset++;
            }

            for (int i = 0; i < zeros; i++) {
                Buffer[BufferOffset] = (byte)0;
                BufferOffset++;
            }
            UInt64 N = (UInt64)Length * 8;
            Buffer[BufferOffset] = (byte)(N >> 56);
            BufferOffset++;
            Buffer[BufferOffset] = (byte)((N >> 48) & 0xFF);
            BufferOffset++;
            Buffer[BufferOffset] = (byte)((N >> 40) & 0xFF);
            BufferOffset++;
            Buffer[BufferOffset] = (byte)((N >> 32) & 0xFF);
            BufferOffset++;
            Buffer[BufferOffset] = (byte)((N >> 24) & 0xFF);
            BufferOffset++;
            Buffer[BufferOffset] = (byte)((N >> 16) & 0xFF);
            BufferOffset++;
            Buffer[BufferOffset] = (byte)((N >> 8) & 0xFF);
            BufferOffset++;
            Buffer[BufferOffset] = (byte)(N & 0xFF);
            BufferOffset++;

            SHA1_TransformBuffer();

            return SHA1_Result;
        }

        public override void Initialize() {
            Length = 0;
            BufferOffset = 0;
            SHA1_Init();
        }

        private static UInt32 SHA1CircularShift(int bits, UInt32 word) {
            return ((word << bits) & 0xFFFFFFFF) | (word) >> (32 - (bits));
        }

        private void SHA1_Init() {
            Digests[0] = 0x67452301;
            Digests[1] = 0xEFCDAB89;
            Digests[2] = 0x98BADCFE;
            Digests[3] = 0x10325476;
            Digests[4] = 0xC3D2E1F0;
        }

        private byte[] SHA1_Pack(byte[] input) {
            int zeros = 0;
            int ones = 1;
            int size = 0;
            int n = input.Length;
            int m = n % 64;
            if (m < 56) {
                zeros = 55 - m;
                size = n - m + 64;
            } else if (m == 56) {
                zeros = 63;
                ones = 1;
                size = n + 8 + 64;
            } else {
                zeros = 63 - m + 56;
                size = n + 64 - m + 64;
            }

            ArrayList bs = new ArrayList(input);
            if (ones == 1) {
                bs.Add((byte)0x80); // 0x80 = 10000000 
            }
            for (int i = 0; i < zeros; i++) {
                bs.Add((byte)0);
            }
            UInt64 N = (UInt64)n * 8;
            byte h8 = (byte)(N & 0xFF);
            byte h7 = (byte)((N >> 8) & 0xFF);
            byte h6 = (byte)((N >> 16) & 0xFF);
            byte h5 = (byte)((N >> 24) & 0xFF);
            byte h4 = (byte)((N >> 32) & 0xFF);
            byte h3 = (byte)((N >> 40) & 0xFF);
            byte h2 = (byte)((N >> 48) & 0xFF);
            byte h1 = (byte)(N >> 56);
            bs.Add(h1);
            bs.Add(h2);
            bs.Add(h3);
            bs.Add(h4);
            bs.Add(h5);
            bs.Add(h6);
            bs.Add(h7);
            bs.Add(h8);
            return (byte[])bs.ToArray(typeof(byte));
        }

        private byte[] SHA1_Result {
            get {
                byte[] result = new byte[20];
                for (int i = 0; i < 5; ++i) {
                    for (int j = 0; j < 4; ++j) {
                        result[i * 4 + j] = (byte)((Digests[i] >> (8 * (3 - j))) & 0xFF);
                    }
                }
                return result;
            }
        }

        private byte[] SHA1_Transform(byte[] input) {
            SHA1_Init();

            byte[] output = SHA1_Pack(input);
            for (int i = 0; i < output.Length; i += BufferLength) {
                for (int j = 0; j < BufferLength; ++j) {
                    Buffer[j] = output[i + j];
                }
                SHA1_TransformBuffer();
            }

            return SHA1_Result;
        }

        private void SHA1_TransformBuffer() {
            UInt32[] K = {
                             0x5A827999,
                             0x6ED9EBA1,
                             0x8F1BBCDC,
                             0xCA62C1D6
                         };
            int t;
            UInt32 temp;
            UInt32[] W = new UInt32[80];
            UInt32 A, B, C, D, E;

            for (int i = 0, j = 0; i < BufferLength; j++, i += 4) {
                temp = 0;
                temp = temp | (((UInt32)Buffer[i]) << 24);
                temp = temp | (((UInt32)Buffer[i + 1]) << 16);
                temp = temp | (((UInt32)Buffer[i + 2]) << 8);
                temp = temp | (((UInt32)Buffer[i + 3]));
                W[j] = temp;
            }

            for (t = 16; t < 80; t++) {
                W[t] = SHA1CircularShift(1, W[t - 3] ^ W[t - 8] ^ W[t - 14] ^ W[t - 16]);
            }

            A = Digests[0];
            B = Digests[1];
            C = Digests[2];
            D = Digests[3];
            E = Digests[4];
            for (t = 0; t < 20; t++) {
                temp = SHA1CircularShift(5, A) +
                    ((B & C) | ((~B) & D)) + E + W[t] + K[0];
                temp &= 0xFFFFFFFF;
                E = D;
                D = C;
                C = SHA1CircularShift(30, B);
                B = A;
                A = temp;
            }

            for (t = 20; t < 40; t++) {
                temp = SHA1CircularShift(5, A) + (B ^ C ^ D) + E + W[t] + K[1];
                temp &= 0xFFFFFFFF;
                E = D;
                D = C;
                C = SHA1CircularShift(30, B);
                B = A;
                A = temp;
            }
            for (t = 40; t < 60; t++) {
                temp = SHA1CircularShift(5, A) +
                    ((B & C) | (B & D) | (C & D)) + E + W[t] + K[2];
                temp &= 0xFFFFFFFF;
                E = D;
                D = C;
                C = SHA1CircularShift(30, B);
                B = A;
                A = temp;
            }

            for (t = 60; t < 80; t++) {
                temp = SHA1CircularShift(5, A) + (B ^ C ^ D) + E + W[t] + K[3];
                temp &= 0xFFFFFFFF;
                E = D;
                D = C;
                C = SHA1CircularShift(30, B);
                B = A;
                A = temp;
            }

            Digests[0] = (Digests[0] + A) & 0xFFFFFFFF;
            Digests[1] = (Digests[1] + B) & 0xFFFFFFFF;
            Digests[2] = (Digests[2] + C) & 0xFFFFFFFF;
            Digests[3] = (Digests[3] + D) & 0xFFFFFFFF;
            Digests[4] = (Digests[4] + E) & 0xFFFFFFFF;
        }

        public byte[] Compute(byte[] input) {
            return SHA1_Transform(input);
        }

        public byte[] Compute(string message) {
            char[] c = message.ToCharArray();
            byte[] b = new byte[c.Length];
            for (int i = 0; i < c.Length; i++) {
                b[i] = (byte)c[i];
            }
            return Compute(b);
        }
    }
}
