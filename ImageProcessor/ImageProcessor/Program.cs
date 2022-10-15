using CommandLine;
using Processor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Presentation
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string projectPath = Directory.GetCurrentDirectory();
            Parser.Default.ParseArguments<CommandLineOptions>(args)
                   .WithParsed<CommandLineOptions>(o =>
                   {
                       ImageProcessor.Ih.loadImage(o.loadPath);
                       //ImageProcessor.BilinearResizing(100, 100);
                       //ImageProcessor.HorizontalFlip();
                       //ImageProcessor.VerticalFlip();
                       Console.WriteLine(projectPath + "\\" + o.loadPath);
                       if (ImageProcessor.Ih.Bmp != null)
                       {
                           ImageProcessor.DiagonalFlip();
                           ImageProcessor.Ih.saveImage(projectPath + "\\" + o.savePath);
                       }
                   });
            Console.ReadLine();
        }
    }
}
