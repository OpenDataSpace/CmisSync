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
    using System.Collections;
    using System.Security.Cryptography;
    using System.Text;

    // Reference: http://blog.csdn.net/dingwood/article/details/7506620
    public class SHA1Reuse : HashAlgorithm, HashAlgorithmReuse {
        private const int BufferLength = 64;
        private UInt32[] digests = new UInt32[5];
        private byte[] buffer = new byte[BufferLength];
        private int bufferOffset = 0;
        private long length = 0;

        public SHA1Reuse() {
            this.Initialize();
        }

        public SHA1Reuse(SHA1Reuse from) {
            for (int i = 0; i < this.digests.Length; ++i) {
                this.digests[i] = from.digests[i];
            }

            for (int i = 0; i < BufferLength; ++i) {
                this.buffer[i] = from.buffer[i];
            }

            this.bufferOffset = from.bufferOffset;
            this.length = from.length;
        }

        public HashAlgorithm GetHashAlgorithm() {
            return new SHA1Reuse(this);
        }

        protected override void HashCore(byte[] array, int ibStart, int cbSize) {
            for (int i = 0; i < cbSize; ++i) {
                this.buffer[this.bufferOffset] = array[ibStart + i];
                ++this.bufferOffset;
                ++this.length;
                if (this.bufferOffset >= BufferLength) {
                    this.SHA1_TransformBuffer();
                    this.bufferOffset = 0;
                }
            }
        }

        protected override byte[] HashFinal() {
            long zeros = 0;
            long ones = 1;
            long size = 0;
            long m = this.length % 64;
            if (m < 56) {
                zeros = 55 - m;
                size = this.length - m + 64;
            } else if (m == 56) {
                zeros = 63;
                ones = 1;
                size = this.length + 8 + 64;
            } else {
                zeros = 63 - m + 56;
                size = this.length + 64 - m + 64;
            }

            if (ones == 1) {
                this.buffer[this.bufferOffset] = (byte)0x80;
                this.bufferOffset++;
            }

            for (int i = 0; i < zeros; i++) {
                this.buffer[this.bufferOffset] = (byte)0;
                this.bufferOffset++;
            }

            UInt64 n = (UInt64)this.length * 8;
            this.buffer[this.bufferOffset] = (byte)(n >> 56);
            this.bufferOffset++;
            this.buffer[this.bufferOffset] = (byte)((n >> 48) & 0xFF);
            this.bufferOffset++;
            this.buffer[this.bufferOffset] = (byte)((n >> 40) & 0xFF);
            this.bufferOffset++;
            this.buffer[this.bufferOffset] = (byte)((n >> 32) & 0xFF);
            this.bufferOffset++;
            this.buffer[this.bufferOffset] = (byte)((n >> 24) & 0xFF);
            this.bufferOffset++;
            this.buffer[this.bufferOffset] = (byte)((n >> 16) & 0xFF);
            this.bufferOffset++;
            this.buffer[this.bufferOffset] = (byte)((n >> 8) & 0xFF);
            this.bufferOffset++;
            this.buffer[this.bufferOffset] = (byte)(n & 0xFF);
            this.bufferOffset++;

            this.SHA1_TransformBuffer();

            return this.SHA1_Result;
        }

        public override void Initialize() {
            this.length = 0;
            this.bufferOffset = 0;
            this.SHA1_Init();
        }

        private static UInt32 SHA1CircularShift(int bits, UInt32 word) {
            return ((word << bits) & 0xFFFFFFFF) | (word) >> (32 - (bits));
        }

        private void SHA1_Init() {
            this.digests[0] = 0x67452301;
            this.digests[1] = 0xEFCDAB89;
            this.digests[2] = 0x98BADCFE;
            this.digests[3] = 0x10325476;
            this.digests[4] = 0xC3D2E1F0;
        }

        private byte[] SHA1_Pack(byte[] input) {
            int zeros = 0;
            int ones = 1;
            int m = input.Length % 64;
            if (m < 56) {
                zeros = 55 - m;
            } else if (m == 56) {
                zeros = 63;
                ones = 1;
            } else {
                zeros = 63 - m + 56;
            }

            ArrayList bs = new ArrayList(input);
            if (ones == 1) {
                bs.Add((byte)0x80); // 0x80 = 10000000 
            }

            for (int i = 0; i < zeros; i++) {
                bs.Add((byte)0);
            }

            UInt64 n = (UInt64)input.Length * 8;
            byte h8 = (byte)(n & 0xFF);
            byte h7 = (byte)((n >> 8) & 0xFF);
            byte h6 = (byte)((n >> 16) & 0xFF);
            byte h5 = (byte)((n >> 24) & 0xFF);
            byte h4 = (byte)((n >> 32) & 0xFF);
            byte h3 = (byte)((n >> 40) & 0xFF);
            byte h2 = (byte)((n >> 48) & 0xFF);
            byte h1 = (byte)(n >> 56);
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
                        result[i * 4 + j] = (byte)((this.digests[i] >> (8 * (3 - j))) & 0xFF);
                    }
                }

                return result;
            }
        }

        private byte[] SHA1_Transform(byte[] input) {
            this.SHA1_Init();

            byte[] output = this.SHA1_Pack(input);
            for (int i = 0; i < output.Length; i += BufferLength) {
                for (int j = 0; j < BufferLength; ++j) {
                    this.buffer[j] = output[i + j];
                }

                this.SHA1_TransformBuffer();
            }

            return this.SHA1_Result;
        }

        private void SHA1_TransformBuffer() {
            UInt32[] k = {
                0x5A827999,
                0x6ED9EBA1,
                0x8F1BBCDC,
                0xCA62C1D6
            };
            int t;
            UInt32 temp;
            UInt32[] w = new UInt32[80];
            UInt32 a, b, c, d, e;

            for (int i = 0, j = 0; i < BufferLength; j++, i += 4) {
                temp = 0;
                temp = temp | (((UInt32)this.buffer[i]) << 24);
                temp = temp | (((UInt32)this.buffer[i + 1]) << 16);
                temp = temp | (((UInt32)this.buffer[i + 2]) << 8);
                temp = temp | ((UInt32)this.buffer[i + 3]);
                w[j] = temp;
            }

            for (t = 16; t < 80; t++) {
                w[t] = SHA1CircularShift(1, w[t - 3] ^ w[t - 8] ^ w[t - 14] ^ w[t - 16]);
            }

            a = this.digests[0];
            b = this.digests[1];
            c = this.digests[2];
            d = this.digests[3];
            e = this.digests[4];
            for (t = 0; t < 20; t++) {
                temp = SHA1CircularShift(5, a) +
                    ((b & c) | ((~b) & d)) + e + w[t] + k[0];
                temp &= 0xFFFFFFFF;
                e = d;
                d = c;
                c = SHA1CircularShift(30, b);
                b = a;
                a = temp;
            }

            for (t = 20; t < 40; t++) {
                temp = SHA1CircularShift(5, a) + (b ^ c ^ d) + e + w[t] + k[1];
                temp &= 0xFFFFFFFF;
                e = d;
                d = c;
                c = SHA1CircularShift(30, b);
                b = a;
                a = temp;
            }

            for (t = 40; t < 60; t++) {
                temp = SHA1CircularShift(5, a) +
                    ((b & c) | (b & d) | (c & d)) + e + w[t] + k[2];
                temp &= 0xFFFFFFFF;
                e = d;
                d = c;
                c = SHA1CircularShift(30, b);
                b = a;
                a = temp;
            }

            for (t = 60; t < 80; t++) {
                temp = SHA1CircularShift(5, a) + (b ^ c ^ d) + e + w[t] + k[3];
                temp &= 0xFFFFFFFF;
                e = d;
                d = c;
                c = SHA1CircularShift(30, b);
                b = a;
                a = temp;
            }

            this.digests[0] = (this.digests[0] + a) & 0xFFFFFFFF;
            this.digests[1] = (this.digests[1] + b) & 0xFFFFFFFF;
            this.digests[2] = (this.digests[2] + c) & 0xFFFFFFFF;
            this.digests[3] = (this.digests[3] + d) & 0xFFFFFFFF;
            this.digests[4] = (this.digests[4] + e) & 0xFFFFFFFF;
        }

        public byte[] Compute(byte[] input) {
            return this.SHA1_Transform(input);
        }

        public byte[] Compute(string message) {
            char[] c = message.ToCharArray();
            byte[] b = new byte[c.Length];
            for (int i = 0; i < c.Length; i++) {
                b[i] = (byte)c[i];
            }

            return this.Compute(b);
        }
    }
}