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
        public string firstPath { get; set; }
        [Value(index: 1, Required = true, HelpText = "Full path to where you want to save the image", Default = "@C:\\")]
        public string secondPath { get; set; }
        [Option(shortName: 'b', longName: "brightness", Required = false, HelpText = "Increase or decrease brightness of the image, by passing in a number 0-255", Default = 0)]
        public int brightness { get; set; }
        [Option(shortName: 'c', longName: "contrast", Required = false, HelpText = "Increase contrast of the image", Default = false)]
        public bool contrast { get; set; }
        [Option(shortName: 'n', longName: "negative", Required = false, HelpText = "Make a negative of the image", Default = false)]
        public bool negative { get; set; }
        [Option(shortName: 'r', longName: "resize", Required = false, HelpText = "Change image size to a new one (width x height)")]
        public IEnumerable<int> Dimensions { get; set; }
        [Option(shortName: 'h', longName: "hflip", Required = false, HelpText = "Horizontally flips the image (mirrors it)", Default = false)]
        public bool hflip { get; set; }
        [Option(shortName: 'a', longName: "alpha", Required = false, HelpText = "Denoises the image using alpha-trimmed mean filter, by passing in a number [1,3]", Default = 0)]
        public int alpha { get; set; }
        [Option(shortName: 'g', longName: "gmean", Required = false, HelpText = "Denoises the image using the geometric mean filter", Default = false)]
        public bool geometricMeanFilter { get; set; }
        [Option(shortName: 'v', longName: "vflip", Required = false, HelpText = "Flips the image vertically", Default = false)]
        public bool vflip { get; set; }
        [Option(shortName: 'd', longName: "dflip", Required = false, HelpText = "Flips the image diagonally", Default = false)]
        public bool dflip { get; set; }
        [Option(shortName: 'm', longName: "mse", Required = false, HelpText = "Calculates the mean square error between two images", Default = false)]
        public bool meanSquare { get; set; }
        [Option(shortName: 'p', longName: "pmse", Required = false, HelpText = "Calculates the peak mean square error between two images", Default = false)]
        public bool peakMeanSquare { get; set; }
        [Option(shortName: 'z', longName: "md", Required = false, HelpText = "Calculates the maximum difference between two images", Default = false)]
        public bool maximumDifference { get; set; }
        [Option(shortName:'s', longName: "snr", Required = false, HelpText = "Calculates the signal to noise ratio between two images", Default = false)]
        public bool signalToNoiseRatio { get; set; }
        [Option(longName: "psnr", Required = false, HelpText = "Calculates the peak signal to noise ratio between two images", Default = false)]
        public bool peakSignalToNoiseRatio { get; set; }
    }
}
