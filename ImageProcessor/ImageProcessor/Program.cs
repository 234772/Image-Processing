using CommandLine;
using Processor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace Presentation
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Stopwatch stopwatch = new Stopwatch();
            string projectPath = Directory.GetCurrentDirectory();
            Parser.Default.ParseArguments<CommandLineOptions>(args)
                   .WithParsed<CommandLineOptions>(o =>
                   {
                       ImageProcessor.Ih.loadImage(o.loadPath);
                       //ImageProcessor.Contrast2();
                       //ImageProcessor.AlphaMeanFilter();
                       stopwatch.Start();
                       //ImageProcessor.ExtendBitmapByOne();
                       Console.WriteLine("start");
                       ImageProcessor.AlphaTrimmedFilter();
                       stopwatch.Stop();
                       Console.WriteLine("Elapsed time is {0} ms", stopwatch.ElapsedMilliseconds);
                       Console.WriteLine(projectPath + "\\" + o.loadPath);
                       if (ImageProcessor.Ih.Bmp != null)
                       {
                            ImageProcessor.Ih.saveImage(projectPath + "\\" + o.savePath);
                       }
                   });
            Console.ReadLine();
        }
    }
}
