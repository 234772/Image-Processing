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
        [Value(index: 1, Required = false, HelpText = "Name of the output file.", Default = "output.bmp")]
        public string secondPath { get; set; }
        [Option(shortName: 'b', longName: "brightness", Required = false, HelpText = "Increase or decrease brightness of the image, by passing in a number 0-255.", Default = 0)]
        public int brightness { get; set; }
        [Option(shortName: 'c', longName: "contrast", Required = false, HelpText = "Increase contrast of the image.", Default = 0)]
        public int contrast { get; set; }
        [Option(shortName: 'n', longName: "negative", Required = false, HelpText = "Make a negative of the image.", Default = false)]
        public bool negative { get; set; }
        [Option(shortName: 'e', longName: "enlarge", Required = false, HelpText = "Enlarge image to new dimensions (width x height).")]
        public IEnumerable<int> Dimensions { get; set; }
        [Option(longName: "shrink", Required = false, HelpText = "Shrink image to new dimensions (width x height).")]
        public IEnumerable<int> DimensionsS { get; set; }
        [Option(shortName: 'h', longName: "hflip", Required = false, HelpText = "Horizontally flips the image (mirrors it).", Default = false)]
        public bool hflip { get; set; }
        [Option(shortName: 'v', longName: "vflip", Required = false, HelpText = "Flips the image vertically.", Default = false)]
        public bool vflip { get; set; }
        [Option(shortName: 'd', longName: "dflip", Required = false, HelpText = "Flips the image diagonally.", Default = false)]
        public bool dflip { get; set; }
        [Option(shortName: 'a', longName: "alpha", Required = false, HelpText = "Denoises the image using alpha-trimmed mean filter, by passing in a number [1,3].", Default = null)]
        public IEnumerable<int> ValuesA { get; set; }
        [Option(shortName: 'g', longName: "gmean", Required = false, HelpText = "Denoises the image using the geometric mean filter.", Default = null)]
        public IEnumerable<int> ValuesG { get; set; }
        [Option(shortName: 'm', longName: "mse", Required = false, HelpText = "Calculates the mean square error between two images.", Default = false)]
        public bool meanSquare { get; set; }
        [Option(shortName: 'p', longName: "pmse", Required = false, HelpText = "Calculates the peak mean square error between two images.", Default = false)]
        public bool peakMeanSquare { get; set; }
        [Option(shortName: 'z', longName: "md", Required = false, HelpText = "Calculates the maximum difference between two images.", Default = false)]
        public bool maximumDifference { get; set; }
        [Option(shortName:'s', longName: "snr", Required = false, HelpText = "Calculates the signal to noise ratio between two images.", Default = false)]
        public bool signalToNoiseRatio { get; set; }
        [Option(longName: "psnr", Required = false, HelpText = "Calculates the peak signal to noise ratio between two images.", Default = false)]
        public bool peakSignalToNoiseRatio { get; set; }
        [Option(longName: "histogram", HelpText = "Generates a histogram of specified channel of input image (Red, Green, Blue).", Default = Channel.None)]
        public Channel channel { get; set; }
        [Option(longName: "hpower", HelpText = "Improve the image quality, using Power 2/3 final probability density function.", Default = null)]
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
        [Option(longName: "sexdeti", Required = false, HelpText = "Applies the extraction of details I filter on the image.", Default = Mask.None)]
        public Mask sexdeti { get; set; }
        [Option(longName: "sexdetio", Required = false, HelpText = "Applies the extraction of details I filter on the image.", Default = false)]
        public bool sexdetio { get; set; }
        [Option(longName: "orobertsii", Required = false, HelpText = "Applies the roberts operator II filter on the image.", Default = false)]
        public bool robertsII { get; set; }
        [Option(longName: "dilation", Required = false, HelpText = "Performs the morphological operation of dilation on the image.", Default = 0)]
        public int dilationKernel { get; set; }
        [Option(longName: "erosion", Required = false, HelpText = "Performs the morphological operation of erosion on the image.", Default = 0)]
        public int erosionKernel { get; set; }
        [Option(longName: "opening", Required = false, HelpText = "Performs the morphological operation of opening on the image.", Default = 0)]
        public int openingKernel { get; set; }
        [Option(longName: "closing", Required = false, HelpText = "Performs the morphological operation of closing on the image.", Default = 0)]
        public int closingKernel { get; set; }
        [Option(longName: "hmt", Required = false, HelpText = "Performs the morphological operation of hmt on the image.", Default = 0)]
        public int hmtKernel { get; set; }
        [Option(longName: "intersection", Required = false, HelpText = "Performs the morphological operation of intersection on the image.", Default = false)]
        public bool intersection { get; set; }
        [Option(longName: "complement", Required = false, HelpText = "Compute the complement of the image", Default = false)]
        public bool complement { get; set; }
        [Option(longName: "dft", Required = false, HelpText = "Compute the complement of the image", Default = false)]
        public bool fourierTransform { get; set; }
        [Option(longName: "m3", Required = false, HelpText = "Performs the morphological operation specified in the M3 point, on the image. Use the following format: kernel x y.", Default = null)]
        public IEnumerable<int> m3 { get; set; }
        [Option(longName: "regiongg", Required = false, HelpText = "Segments the image using the region growing method.", Default = null)]
        public IEnumerable<int> region { get; set; }
        [Option(longName: "fft", Required = false, HelpText = "Return the FFT of an image", Default = false)]
        public bool fft { get; set; }
        [Option(longName: "ifft", Required = false, HelpText = "Returns the IFFT of a fourier transform, represented as an image.", Default = false)]
        public bool ifft { get; set; }
        [Option(longName: "lowpass", Required = false, HelpText = "Return the image after applying the low pass filter", Default = 1000)]
        public int lowpass { get; set; }
        [Option(longName: "highpass", Required = false, HelpText = "Return the image after applying the high pass filter", Default = 1000)]
        public int highpass { get; set; }
        [Option(longName: "bandpass", Required = false, HelpText = "Return the image after applying the band-pass filter", Default = null)]
        public IEnumerable<int> bandpass { get; set; }
        [Option(longName: "bandcut", Required = false, HelpText = "Return the image after applying the band-cut filter", Default = null)]
        public IEnumerable<int> bandcut { get; set; }
        [Option(longName: "edgehighpass", Required = false, HelpText = "Return the image after applying the high-pass with edge detection filter", Default = null)]
        public IEnumerable<int> edgehighpass { get; set; }
        [Option(longName: "phase", Required = false, HelpText = "Return the image after applying the phase modifying filter.", Default = null)]
        public IEnumerable<int> phase { get; set; }
        [Option(longName: "generatemask", Required = false, HelpText = "Return the image after applying the high-pass with edge detection filter", Default = null)]
        public bool generateMask { get; set; }

        public enum Channel
        {
            Red,
            Green,
            Blue,
            None
        }
        public enum Mask
        {
            N,
            NE,
            E,
            SE,
            None
        }
    }
}
