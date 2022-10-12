using CommandLine;
using Processor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessor
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ImageHandler ih = new ImageHandler();
            Parser.Default.ParseArguments<CommandLineOptions>(args)
                   .WithParsed<CommandLineOptions>(o =>
                   {
                       Console.WriteLine(o.loadPath);
                       Console.WriteLine(o.savePath);
                       var image = ih.loadImage(o.loadPath);
                       ih.saveImage(o.savePath, image);
                       Console.WriteLine(image.GetPixel(1, 1));
                   });
            Console.ReadLine();
        }
    }
}
