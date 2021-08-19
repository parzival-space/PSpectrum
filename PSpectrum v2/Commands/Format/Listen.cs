using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSpectrum.Commands.Format
{
    [Verb("listen", HelpText = "Start listening and analyzing the loopback device.")]
    class Listen
    {
        [Option('r', "rate", Default = 25, HelpText = "Specifies the polling rate.")]
        public int PollingRate { get; set; }

        [Option('n', "normalization", Default = NormType.None, HelpText = "Specifies the normalization level. Can be one of the following: None, Fast, Normal, Slow")]
        public NormType Normalization { get; set; }

        [Option('d', "device", Default = 0, HelpText = "Specifies which audio device to use. You must specify the appropriate device ID. Trys to get your current output device by default.")]
        public int DeviceID { get; set; }

        public enum NormType
        {
            None,
            Fast,
            Normal,
            Slow
        }
    }
}
