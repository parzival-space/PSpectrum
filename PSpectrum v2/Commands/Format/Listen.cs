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

        [Option('n', "normalization", Default = 0, HelpText = "Specifies the number of normalization levels.")]
        public int NormalizationLevel { get; set; }

        [Option('d', "device", Default = -1, HelpText = "Specifies which audio device to use. You must specify the appropriate device ID.")]
        public int DeviceID { get; set; }
    }
}
