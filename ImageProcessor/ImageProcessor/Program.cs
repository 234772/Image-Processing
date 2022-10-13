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
            //ImageHandler ih = new ImageHandler();
            Parser.Default.ParseArguments<CommandLineOptions>(args)
                   .WithParsed<CommandLineOptions>(o =>
                   {
                       ImageProcessor.Ih.loadImage(o.loadPath);
                       ImageProcessor.ChangeBrightness(o.brightness);
                       ImageProcessor.Ih.saveImage(o.savePath);
                       //Console.WriteLine(o.loadPath);
                       //Console.WriteLine(o.savePath);
                       //var image = ih.loadImage(o.loadPath);
                       //ih.saveImage(o.savePath, image);
                       //Console.WriteLine(image.GetPixel(1, 1));
                   });
            Console.ReadLine();
        }
    }
}
