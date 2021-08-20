using CommandLine;

namespace PSpectrum.Commands.Format
{
    [Verb("web", HelpText = "Starts an audio visualizer in your browser.")]
    internal class Web : Generic
    {
        [Option('p', "http-port", HelpText = "Sets the port for the HTTP server. By default, the port gets randomized.")]
        public int HttpPort { get; set; }

        [Option('s', "socket-port", HelpText = "Sets the port for the WebSocket server. By default, the port gets randomized.")]
        public int SocketPort { get; set; }

        [Option('v', "no-view", HelpText = "If this swtich is used, PSpectrum will not open your browser automatically once its ready.")]
        public bool NoView { get; set; }

        [Option('u', "http-prefix", Default = "127.0.0.1", HelpText = "Registers a new hostname that the server will listen at.")]
        public string Prefix { get; set; }

        [Option('c', "custom-script", HelpText = "Allows you to add a custom script. This script will be loaded into the web view. Has to be a JavaScript file.")]
        public string CustomScript { get; set; }
    }
}