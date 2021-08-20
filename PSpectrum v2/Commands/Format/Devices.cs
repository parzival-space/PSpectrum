using CommandLine;

namespace PSpectrum.Commands.Format
{
    [Verb("devices", HelpText = "Displays the currently connected devices and the associated ID.")]
    internal class Devices
    {
        [Option('e', "enabled", Default = false, HelpText = "Only display devices that are enabled.")]
        public bool ShowEnabled { get; set; }

        [Option('l', "loopback", Default = false, HelpText = "Only display devices that are loopback devices.")]
        public bool ShowLoopback { get; set; }

        [Option('n', "name", Required = false, HelpText = "Only show devices that match the provided name.")]
        public string Name { get; set; }

        [Option('i', "input", Default = false, HelpText = "Only display devices that are input devices.")]
        public bool ShowInput { get; set; }

        [Option('o', "output", Default = false, HelpText = "Only display devices that are output devices.")]
        public bool ShowOutput { get; set; }

        [Option('d', "default", Default = false, HelpText = "Only display devices that are default devices.")]
        public bool ShowDefault { get; set; }
    }
}