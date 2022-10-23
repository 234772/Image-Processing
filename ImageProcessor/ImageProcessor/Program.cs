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
            string projectPath = Directory.GetCurrentDirectory();
            Parser.Default.ParseArguments<CommandLineOptions>(args)
                   .WithParsed<CommandLineOptions>(o =>
                   {

 
                       //ImageProcessor.Ih.loadImage(o.loadPath);
                       stopwatch.Start();
                       ImageProcessor.Process(o);
                       stopwatch.Stop();
                       //var image = o.images.ToList();
                       //Console.WriteLine(ImageProcessor.MaximumDifference(image[0], image[1]));
                       //ImageProcessor.AlphaTrimmedFilter();
                       //if (ImageProcessor.Ih.Bmp != null)
                       //{
                       //    ImageProcessor.Ih.saveImage(projectPath + "\\" + o.savePath);
                       //}
                   });
            Console.WriteLine("Elapsed Time is {0} ms", stopwatch.ElapsedMilliseconds);
            Console.ReadLine();
        }
    }
}
