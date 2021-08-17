using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using PSpectrum.Commands;

namespace PSpectrum
{
    class Program
    {
        static int Main(string[] args)
        {
            return Parser.Default.ParseArguments<Commands.Format.Devices, Commands.Format.Listen>(args)
                .MapResult(
                    (Commands.Format.Devices a) => Commands.Action.Devices.Execute(a),
                    (Commands.Format.Listen  a) => Commands.Action.Listen.Execute(a),

                    errs => 1
                );
        }
    }
}
