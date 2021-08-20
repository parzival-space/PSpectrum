using CommandLine;

namespace PSpectrum
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            return Parser.Default.ParseArguments<Commands.Format.Devices, Commands.Format.Listen, Commands.Format.Web>(args)
                .MapResult(
                    (Commands.Format.Devices a) => Commands.Action.Devices.Execute(a),
                    (Commands.Format.Listen a) => Commands.Action.Listen.Execute(a),
                    (Commands.Format.Web a) => Commands.Action.Web.Execute(a),

                    errs => 1
                );
        }
    }
}