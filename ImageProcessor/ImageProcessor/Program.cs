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
                       ImageProcessor.BilinearResizing(2000, 2000);
                       Console.WriteLine(projectPath + "/" + o.loadPath);
                       if (ImageProcessor.Ih.Bmp != null)
                       {
                            ImageProcessor.Ih.saveImage(projectPath + "/" + o.savePath);
                       }
                   });
            Console.ReadLine();
        }
    }
}
