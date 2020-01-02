using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Compression.Services;
using Compression.Util;

namespace Compression
{
    public class Program
    {

        public static (string name, long time, bool pass, float compression) Test(string method, (byte[] bytes, long crc) data, CompressionService service)
        {
            var stopwatch = Stopwatch.StartNew();;
            var compressed = service.Compress(data.bytes);
            var compression = 1f - ((float)compressed.Length / data.bytes.Length);
            var result = service.Decompress(compressed);
            var rcrc = Crc.ComputeChecksum(result);
            if (rcrc != data.crc)
            {
                if(data.bytes.Length!= result.Length)
                {
                    Console.Out.Write("Error in Lenght");
                }
                else
                {
                    for(int i=0;i< data.bytes.Length; i++)
                    {
                        if (data.bytes[i] != result[i])
                        {
                            Console.Out.WriteLine($"[{i}] expect {data.bytes[i]} found {result[i]}");
                        }
                    }
                }
            }
            return (method, stopwatch.ElapsedMilliseconds, rcrc == data.crc, compression);
        }

        public static void Main(string[] args)
        {
            Console.WriteLine("Testing Compression mothods");

            var data = Generators.GenerateData(64);// GenerateSequentialData(2048);// .GenerateData();

            var results = new List<(string name, long time, bool pass, float compression)>();
            var gzip = CompressionServiceFactory.GetGZipService();
            var deflate = CompressionServiceFactory.GetDeflateService();
            var pzip = CompressionServiceFactory.GetPZipService();
          
            results.Add(Test(nameof(gzip), data, gzip));
            results.Add(Test(nameof(deflate), data, deflate));
            results.Add(Test(nameof(pzip), data, pzip));

            var table = results.OrderByDescending(p => p.compression);
            Console.Out.WriteLine(string.Format("{0,10} | {1,10} | {2,9} | {3,4}|", "Name", "Time", "Comp", "Pass"));
            foreach (var r in table)
                Console.Out.WriteLine(string.Format("{0,10} | {1,10} | {2,9:p2} | {3,4}|", r.name, r.time, r.compression, r.pass));
            Console.In.Read();

        }
    }
}
