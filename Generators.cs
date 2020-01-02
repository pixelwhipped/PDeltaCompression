using Compression.Util;
using System;
using System.IO;

namespace Compression
{
    public static class Generators
    {
        public static (byte[] bytes, long crc) GenerateCloseData(long size)
        {
            var r = new Random();
            var bytes = new byte[size];
            for (int i = 0; i < size; i++)
                bytes[i] = (byte)(r.NextDouble() * 32);
            return (bytes, Crc.ComputeChecksum(bytes));
        }
        public static (byte[] bytes, long crc) GenerateSequentialData(long size)
        {
            var bytes = new byte[size];
            for (int i = 0; i < size; i++)
                bytes[i] = (byte)i;
            return (bytes, Crc.ComputeChecksum(bytes));
        }
        public static (byte[] bytes, long crc) GenerateMaxData(long size)
        {
            var bytes = new byte[size];
            for (int i = 0; i < bytes.Length; i++) bytes[i] = 255;
            return (bytes, Crc.ComputeChecksum(bytes));
        }
        public static (byte[] bytes, long crc) GenerateRandomData(long size)
        {
            var r = new Random();
            var bytes = new byte[size];
            r.NextBytes(bytes);
            return (bytes, Crc.ComputeChecksum(bytes));
        }
        public static (byte[] bytes, long crc) GenerateData()
        {
            var bytes = File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "world192.txt"));
            return (bytes, Crc.ComputeChecksum(bytes));
        }
        public static (byte[] bytes, long crc) GenerateData(long size)
        {
            var allBytes = File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "world192.txt"));
            var bytes = new byte[Math.Min(allBytes.Length, size)];
            Array.Copy(allBytes, 0, bytes, 0, bytes.Length);
            return (bytes, Crc.ComputeChecksum(bytes));
        }


    }
}
