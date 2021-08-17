using CommandLine;

namespace PSpectrum.Commands.Format
{
    [Verb("devices", HelpText = "Displays the currently connected devices and the associated ID.")]
    class Devices
    {
        [Option('e', "enabled", Default = false, HelpText = "Only display devices that are enabled.")]
        public bool ShowEnabled { get; set; }

        [Option('l', "loopback", Default = false, HelpText = "Only display devices that are loopback devices.")]
        public bool ShowLoopback { get; set; }

        [Option('n', "name", Required = false, HelpText = "Only show devices that match the provided name.")]
        public string Name { get; set; }

        [Option('i', "input-only", Default = false, HelpText = "If enabled, only shows input devices.")]
        public bool InputOnly { get; set; }

        [Option('o', "output-only", Default = false, HelpText = "If enabled, only shows output devices.")]
        public bool OutputOnly { get; set; }
    }
}
