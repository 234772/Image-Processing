using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Processor
{
    public class CommandLineOptions
    {
        [Value(index: 0, Required = true, HelpText = "Full path to an image that you want to process.")]
        public string loadPath { get; set; }
        [Value(index: 1, Required = true, HelpText = "Full path to where you want to save the image", Default = "@C:\\")]
        public string savePath { get; set; }
        [Option(shortName: 'b', longName: "brightness", Required = false, HelpText = "Increase or decrease brightness of the image, by passing in a number 0-255", Default = 0)]
        public int brightness { get; set; }
        [Option(shortName: 'c', longName: "contrast", Required = false, HelpText = "Increase or decrease brightness of the image, by passing in a number 0-255", Default = 0)]
        public int contrast { get; set; }
        [Option(shortName: 'n', longName: "negative", Required = false, HelpText = "Increase or decrease brightness of the image, by passing in a number 0-255", Default = true)]
        public bool negative { get; set; }
        //[Option(shortName: 'h', longName: "horizontal flip", Required = false, HelpText = "Horizontal flip", Default = 0)]
        //public bool horizontalFlip { get; set; }
        //[Option(shortName: 'c', longName: "confidence", Required = false, HelpText = "Minimum confidence.", Default = 0.9f)]
        //public string Confidence { get; set; }
    }
}
