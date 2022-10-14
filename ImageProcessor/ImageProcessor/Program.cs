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
                       if (ImageProcessor.Ih.Bmp != null)
                       {
                            //ImageProcessor.ChangeBrightness(o.brightness);
                            ImageProcessor.NegativeImage();
                            ImageProcessor.Ih.saveImage(o.savePath);
                       }
                   });
            Console.ReadLine();
        }
    }
}
