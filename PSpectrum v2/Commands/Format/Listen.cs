using CommandLine;

namespace PSpectrum.Commands.Format
{
    [Verb("listen", HelpText = "Start listening and analyzing the loopback device.")]
    internal class Listen : Generic
    { }
}