using Compression.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;

namespace Compression.Services
{
    /// <summary>
    /// Utility class used for compressing data.
    /// using GZip.
    /// </summary>
    public class PZipService : CompressionService
    {
        private const int _packSize = 4;
        private const int _packetSize = 9;
        #region Functions
        public static byte[] UnpackIndicies(BigInteger packed, int packetSize)
        {
            var uc = packetSize + 1;
            var bytes = new byte[packetSize];
            for (int i = 0; i < packetSize; i++)
            {
                var pow = BigInteger.Pow(packetSize + 1, i);
                pow = pow <= 0 ? BigInteger.One : pow;
                bytes[i] = (byte)(((packed / pow) % uc) - 1);
            }
            return bytes;
        }
        public static byte[] PackIndicies(byte[] bytes, int size)
        {
            var len = bytes.Length;
            var order = bytes.OrderBy(p => p).Select(p => (int)p).ToList();
            var sum = new BigInteger();
            for (var i = 0; i < len; i++)
            {
                var idx = order.IndexOf(bytes[i]);
                var s = BigInteger.Pow(len + 1, i) * (idx + 1);
                sum += s <= 0 ? BigInteger.One : s;
                order[idx] = -1;

            }
            var a = sum.ToByteArray();
            //assume 5 byts for pack of 9 bytes
            var ret = new byte[size];
            var d = ret.Length - a.Length;
            for (int i = 0; i < a.Length; i++)
            {
                ret[i + d] = a[i];
            }
            return ret;
        }
        public static byte CountBits(byte number) => (byte)(Math.Log2(number) + 1);
        public static byte Delta(byte[] bytes)
        {
            var m = 0;
            for (int i = 1; i < bytes.Length; i++)
            {
                var d = bytes[i] - bytes[i - 1];
                if (d > m) m = d;
            }
            return (byte)m;
        }

        public static bool Decode(out byte[] bytes, out byte[] pack, out byte bits, int packetSize, int packSize, BitStreamReader reader, Stream stream)
        {
            var mode = reader.ReadBits(3, stream);
            bytes = new byte[packetSize];
            pack = new byte[packSize];
            if (mode == 7)
            {
                bits = 0;
                return false;
            }
            else if (mode == 0)
            {
                bits = 0;
                var c = (byte)reader.ReadBits(8, stream);
                bytes = new byte[c];
                var b = (byte)reader.ReadBits(8, stream);
                for (int i = 0; i < c; i++)
                {
                    bytes[i] = b;
                }
            }
            else if (mode == 1)
            {
                bits = 8;
                for (int i = 0; i < packetSize; i++)
                {
                    bytes[i] = (byte)reader.ReadBits(8, stream);
                }
            }
            else
            {
                bits = (byte)(mode - 2);
                bytes[0] = (byte)reader.ReadBits(8, stream);
                for (int i = 1; i < packetSize; i++)
                {
                    var b = (byte)reader.ReadBits(bits, stream);
                    bytes[i] = (byte)(bytes[i - 1] + b);
                }
                for (int i = 0; i < packSize; i++)
                {
                    pack[i] = (byte)reader.ReadBits(8, stream);
                }
                //Reorder
                var order = UnpackIndicies(new BigInteger(pack), packetSize);
                var obytes = new byte[packetSize];
                for (int i = 0; i < packetSize; i++)
                {
                    obytes[i] = bytes[order[i]];
                }
                bytes = obytes;
            }
            return true;
        }

        //1 Bit = (9*8)-((9)+(4*8) == 72 - 41 = 31 Bits saved 28 bit actual 3 bit header
        //2 Bit = (9*8)-((2*9)+(4*8) == 72 - 50 = 22 Bits saved 19 bit actual 3 bit header
        //3 Bit = (9*8)-((3*9)+(4*8) == 72 - 59 = 13 Bits saved 10 bit actual 3 bit header
        //4 Bit = (9*8)-((4*9)+(4*8) == 72 - 68 = 4 Bits saved 1 bit actual 3 bit header
        public static int EncodePDelta(byte[] order, byte[] pack, byte bits, BitStreamWriter writer, Stream stream)
        {
            writer.WriteBits((byte)(bits + 2), 3, stream);
            writer.WriteBits(order[0], 8, stream);
            for (int i = 1; i < order.Length; i++)
                writer.WriteBits((byte)(order[i] - order[i - 1]), bits, stream);            
            for (int i = 0; i < pack.Length; i++)            
                writer.WriteBits(pack[i], 8, stream);
            return _packetSize;// 3 + (_packetSize * bits) + (_packSize * 8);
        }

        public static bool PDeltaOrDelta(int i, byte[] bytes, byte[] chunk, byte bits, BitStreamWriter writer, Stream memory, out int read)
        {
            read = 0;
            // 3 + (_packetSize * bits) + (_packSize * 8)
            // 7 Bits = 127 + sign == 8 fail any gain
            // 6 Bits = 63 + sign = 7 Bits = 1 Bit Save per Byte
            // 5 Bits = 31 + sign = 6 Bits = 2 Bit save per Byte
            // 4 Can be compressed using PDelta but check if saving > than
            if (bits > 4)   //This will be if bits >= 7 
            {
                //Well we need to test next position where we can and how much we can save 
                writer.WriteBits(1, 3, memory);
                foreach (var b in chunk)
                    writer.WriteBits(b, 8, memory);
                read = _packetSize;
                return true;
            }
            return false;
        } 
        /// <summary>
        /// Compresses data.
        /// </summary>
        /// <param name="bytes">The byte array to be compressed.</param>
        /// <returns>A byte array of compressed data.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="bytes" /> is <c>null</c>.</exception>
        public override byte[] Compress(byte[] bytes)
        {
            if (bytes == null) throw new ArgumentNullException();
            using var memory = new MemoryStream();
            var writer = new BitStreamWriter();
            //will have to pad buffer
            int i = 0;
            byte[] finalChunk = null;
           // for (int i = 0; i < bytes.Length; i += _packetSize)
            while(i< bytes.Length)
            {
                var size = i + _packetSize > bytes.Length ? bytes.Length - i : _packetSize;
                if (size != _packetSize)
                {
                    finalChunk = bytes[i..(i + size)];// new byte[size];
                    i += size;
                }
                else
                {
                    var chunk = bytes[i..(i + _packetSize)];
                    var order = chunk.OrderBy(p => p).Select(p => p).ToArray();
                    var delta = Delta(order);
                    var bits = CountBits(delta);
                    if (bits == 0)
                    {
                        i += EncodeRLE(i, bytes, writer, memory);
                    }
                    else if (PDeltaOrDelta(i,bytes,chunk,bits,writer,memory, out int r))
                    {
                        i += r;
                    }
                    else
                    {
                        i += EncodePDelta(order, PackIndicies(chunk, _packSize), bits, writer, memory);
                    }
                }
            }
            var finalWrite = 0;
            // Check if final chunk required
            // if so set first bit 1 then len and chunks
            // then or otherwise set first bit 0 and chunks
            // May need to check lenght and padd(Flush)
            using var finalMemory = new MemoryStream();
            var finalWriter = new BitStreamWriter();

            if (finalChunk != null)
            {
                finalWrite += 5 + (finalChunk.Length * 8);
                finalWriter.WriteBits(1, 1, finalMemory);
                finalWriter.WriteBits((byte)finalChunk.Length, 4, finalMemory);
                foreach (var b in finalChunk)
                    finalWriter.WriteBits(b, 8, finalMemory);
            }
            else
            {
                finalWrite += 1;
                finalWriter.WriteBits(0, 1, finalMemory);
            }
            //xtra 3 bits for terminate in decode

            writer.WriteBits(7, 3, memory);
            writer.WriteFlush(memory);

            var memArray = memory.ToArray();
            foreach (var b in memArray)
                finalWriter.WriteBits(b, 8, finalMemory);

            finalWriter.WriteFlush(finalMemory);

            //last bit will be 0 for end and 1 for recurse but to doing now.            
            var ret = finalMemory.ToArray();
            return ret;
          /*  if(ret.Length < bytes.Length)
            {
                return Compress(ret);
            }
            return bytes; //WIll have to wrap this up*/
        }

        public static int EncodeRLE(int i, byte[] bytes, BitStreamWriter writer, MemoryStream memory)
        {
            writer.WriteBits(0, 3, memory);
            byte j;
            for (j = _packetSize; j < 255; j++)
            {
                if (bytes[i + j] != bytes[i])
                    break;
            }
            writer.WriteBits(j, 8, memory);
            writer.WriteBits(bytes[i], 8, memory);
            return j;
        }

        /// <summary>
        /// Decompresses data.
        /// </summary>
        /// <param name="bytes">The byte array to be decompressed.</param>
        /// <returns>A byte array of uncompressed data.</returns>
        public override byte[] Decompress(byte[] bytes)
        {
            var reader = new BitStreamReader();
            using var encodedMemory = new MemoryStream(bytes);
            using var ms = new MemoryStream();
            byte[] finalChunk = null;

            if (reader.ReadBits(1, encodedMemory) == 1)
            {
                var len = reader.ReadBits(4, encodedMemory);
                finalChunk = new byte[len];
                for (int i = 0; i < len; i++)
                    finalChunk[i] = (byte)reader.ReadBits(8, encodedMemory);
            }
            while (Decode(out byte[] obytes, out byte[] opack, out byte obits, _packetSize, _packSize, reader, encodedMemory))
                ms.Write(obytes, 0, obytes.Length);

            if (finalChunk != null)
                ms.Write(finalChunk, 0, finalChunk.Length);

            return ms.ToArray();
        }

        #endregion
    }
}