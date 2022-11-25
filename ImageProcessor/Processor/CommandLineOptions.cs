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
        [Value(index: 0, Required = true, HelpText = "Name of the bitmap you want to process.")]
        public string firstPath { get; set; }
        [Value(index: 1, Required = false, HelpText = "Name of the output file", Default = "output.bmp")]
        public string secondPath { get; set; }
        [Option(shortName: 'b', longName: "brightness", Required = false, HelpText = "Increase or decrease brightness of the image, by passing in a number 0-255", Default = 0)]
        public int brightness { get; set; }
        [Option(shortName: 'c', longName: "contrast", Required = false, HelpText = "Increase contrast of the image", Default = 0)]
        public int contrast { get; set; }
        [Option(shortName: 'n', longName: "negative", Required = false, HelpText = "Make a negative of the image", Default = false)]
        public bool negative { get; set; }
        [Option(shortName: 'e', longName: "enlarge", Required = false, HelpText = "Enlarge image to new dimensions (width x height)")]
        public IEnumerable<int> Dimensions { get; set; }
        [Option(longName: "shrink", Required = false, HelpText = "Shrink image to new dimensions (width x height)")]
        public IEnumerable<int> DimensionsS { get; set; }
        [Option(shortName: 'h', longName: "hflip", Required = false, HelpText = "Horizontally flips the image (mirrors it)", Default = false)]
        public bool hflip { get; set; }
        [Option(shortName: 'v', longName: "vflip", Required = false, HelpText = "Flips the image vertically", Default = false)]
        public bool vflip { get; set; }
        [Option(shortName: 'd', longName: "dflip", Required = false, HelpText = "Flips the image diagonally", Default = false)]
        public bool dflip { get; set; }
        [Option(shortName: 'a', longName: "alpha", Required = false, HelpText = "Denoises the image using alpha-trimmed mean filter, by passing in a number [1,3]", Default = null)]
        public IEnumerable<int> ValuesA { get; set; }
        [Option(shortName: 'g', longName: "gmean", Required = false, HelpText = "Denoises the image using the geometric mean filter", Default = null)]
        public IEnumerable<int> ValuesG { get; set; }
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
        [Option(longName: "histogram", HelpText = "Generates a histogram of specified channel of input image (Red, Green, Blue)", Default = Channel.None)]
        public Channel channel { get; set; }
        [Option(longName: "hpower", HelpText = "Improve the image quality, using Power 2/3 final probability density function", Default = null)]
        public IEnumerable<double> gs { get; set; }
        [Option(longName: "cmean", HelpText = "Calculates the mean characteristic, of a given image.", Default = false)]
        public bool mean { get; set; }
        [Option(longName: "cvariance", HelpText = "Calculates the variance characteristic of a given image.", Default = false)]
        public bool variance { get; set; }
        [Option(longName: "cstdev", HelpText = "Calculates the standard deviation characteristic of a given image.", Default = false)]
        public bool deviation { get; set; }
        [Option(longName: "cvarcoi", HelpText = "Calculates the variation coefficient I characteristic of a given image.", Default = false)]
        public bool variation { get; set; }
        [Option(longName: "casyco", HelpText = "Calculates the asymmetry coefficient characteristic of a given image.", Default = false)]
        public bool asymmetry { get; set; }
        [Option(longName: "cflatco", HelpText = "Calculates the flattening coefficient characteristic of a given image.", Default = false)]
        public bool flattening { get; set; }
        [Option(longName: "cvarcoii", HelpText = "Calculates the variation coefficient II characteristic of a given image.", Default = false)]
        public bool variation2 { get; set; }
        [Option(longName: "centropy", HelpText = "Calculates the information source entropy characteristic of a given image.", Default = false)]
        public bool entropy { get; set; }
        [Option(longName: "sexdeti", Required = false, HelpText = "Applys the extraction of details I filter on the image", Default = false)]
        public bool sexdeti { get; set; }
        [Option(longName: "sexdetio", Required = false, HelpText = "Applys the extraction of details I filter on the image", Default = false)]
        public bool sexdetio { get; set; }
        [Option(longName: "orobertsii", Required = false, HelpText = "Applys the roberts operator II filter on the image", Default = false)]
        public bool robertsII { get; set; }

        public enum Channel
        {
            Red,
            Green,
            Blue,
            None
        }
    }
}
