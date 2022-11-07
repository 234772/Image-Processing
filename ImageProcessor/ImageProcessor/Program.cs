using CommandLine;
using Processor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Threading;

namespace Presentation
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Stopwatch stopwatch = new Stopwatch();
            Process proc = Process.GetCurrentProcess();
            string projectPath = Directory.GetCurrentDirectory();
            Parser.Default.ParseArguments<CommandLineOptions>(args)
                   .WithParsed<CommandLineOptions>(o =>
                   {
                       stopwatch.Start();
                       ImageProcessor.Process(o);
                       Console.WriteLine(proc.PrivateMemorySize64);
                       stopwatch.Stop();
                   });
            Console.WriteLine("Elapsed Time is {0} ms", stopwatch.ElapsedMilliseconds);
            Console.ReadLine();
        }
    }
}
