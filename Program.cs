using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace PortScanner
{
    class Program
    {
        static int timeout = 500;
        static string[] targets;
        static List<int> ports = new List<int>();
        static string outputFile = null;
        static StreamWriter fileWriter = null;
        static object fileLock = new object();

        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: PortScanner.exe <hosts> <ports> [timeout_ms] [outfile]");
                Console.WriteLine("Example: PortScanner.exe 192.168.1.0/24 1-1000 750 results.txt");
                return;
            }

            targets = args[0].Split(',');
            ports = ExpandPorts(args[1]);

            if (args.Length >= 3)
                timeout = Math.Max(500, int.Parse(args[2]));

            if (args.Length >= 4)
            {
                outputFile = args[3];
                fileWriter = new StreamWriter(outputFile, true);
            }

            foreach (var host in ExpandTargets(targets))
            {
                ScanHost(host);
            }

            fileWriter?.Close();
        }

        static List<int> ExpandPorts(string input)
        {
            var expanded = new List<int>();
            foreach (var item in input.Split(','))
            {
                if (item.Contains('-'))
                {
                    var range = item.Split('-').Select(int.Parse).ToArray();
                    expanded.AddRange(Enumerable.Range(range[0], range[1] - range[0] + 1));
                }
                else
                {
                    expanded.Add(int.Parse(item));
                }
            }
            return expanded.Distinct().OrderBy(p => p).ToList();
        }

        static List<string> ExpandTargets(string[] inputs)
        {
            var allHosts = new List<string>();
            foreach (var input in inputs)
            {
                if (input.Contains("/"))
                    allHosts.AddRange(CIDRToIPList(input));
                else
                    allHosts.Add(input);
            }
            return allHosts;
        }

        static List<string> CIDRToIPList(string cidr)
        {
            var parts = cidr.Split('/');
            var baseIp = parts[0];
            var prefix = int.Parse(parts[1]);

            var baseBytes = baseIp.Split('.').Select(byte.Parse).ToArray();
            uint ip = (uint)(baseBytes[0] << 24 | baseBytes[1] << 16 | baseBytes[2] << 8 | baseBytes[3]);
            int hostBits = 32 - prefix;
            uint mask = (uint)((1 << hostBits) - 1);
            uint start = ip & ~mask;
            uint end = ip | mask;

            var ips = new List<string>();
            for (uint i = start + 1; i < end; i++) // skip .0 and .255
            {
                ips.Add($"{(i >> 24) & 0xFF}.{(i >> 16) & 0xFF}.{(i >> 8) & 0xFF}.{i & 0xFF}");
            }

            return ips;
        }

        static void ScanHost(string host)
        {
            Console.WriteLine($"\n[*] Scanning host: {host}");
            var openPorts = new List<int>();

            Parallel.ForEach(ports, port =>
            {
                try
                {
                    var client = new TcpClient();
                    var result = client.BeginConnect(host, port, null, null);
                    bool success = result.AsyncWaitHandle.WaitOne(timeout);

                    if (success && client.Connected)
                    {
                        lock (openPorts)
                            openPorts.Add(port);

                        client.EndConnect(result);
                        client.Close();
                    }
                }
                catch { }
            });

            if (openPorts.Count > 0)
            {
                Console.WriteLine($"[+] Open ports on {host}: {string.Join(", ", openPorts)}");

                if (fileWriter != null)
                {
                    lock (fileLock)
                    {
                        fileWriter.WriteLine($"Host: {host}");
                        foreach (var p in openPorts)
                            fileWriter.WriteLine($"    - Port {p} is open");
                        fileWriter.WriteLine();
                    }
                }
            }
            else
            {
                Console.WriteLine($"[-] No open ports on {host}");
            }
        }
    }
}
