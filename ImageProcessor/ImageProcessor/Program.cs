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
                       //ImageProcessor.Ih.loadImage(o.loadPath);
                       Console.WriteLine("start");
                       ImageProcessor.Process(o);
                       //var image = o.images.ToList();
                       //Console.WriteLine(ImageProcessor.MaximumDifference(image[0], image[1]));
                       //ImageProcessor.AlphaTrimmedFilter();
                       //if (ImageProcessor.Ih.Bmp != null)
                       //{
                       //    ImageProcessor.Ih.saveImage(projectPath + "\\" + o.savePath);
                       //}
                   });
            Console.ReadLine();
        }
    }
}
