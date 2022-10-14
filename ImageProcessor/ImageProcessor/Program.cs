using CommandLine;
using Processor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Presentation
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Parser.Default.ParseArguments<CommandLineOptions>(args)
                   .WithParsed<CommandLineOptions>(o =>
                   {
                       ImageProcessor.Ih.loadImage(o.loadPath);
                       //ImageProcessor.ChangeBrightness(o.brightness);
                       //ImageProcessor.NegativeImage();
                       //ImageProcessor.ShrinkImage();
                       ImageProcessor.BilinearResizing(2000, 2000);
                       ImageProcessor.Ih.saveImage(o.savePath);
                   });
            Console.ReadLine();
        }
    }
}
