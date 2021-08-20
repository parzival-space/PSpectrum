using PSpectrum.Utils;
using System;

namespace PSpectrum.Commands.Action
{
    internal class Devices
    {
        public static int Execute(Format.Devices opts)
        {
            // init bass
            if (!BassGeneric.InitBass())
            {
                Console.WriteLine("Error: Failed to initiate Bass!");
                return 1;
            }

            // get list of devices
            var devices = BassGeneric.GetDevices();
            var offset = 0;
            devices.ForEach((device) => { if (device.Name.Length > offset) offset = device.Name.Length; });

            // try to print them somewhat readable
            Console.WriteLine("ID".PadRight(4) + " " + "Name".PadRight(offset + 2) + " " + "Loopback Enabled Default I/O");
            devices.ForEach((device) =>
            {
                // check if filters match to device
                if (opts.ShowInput && !device.Input) return;
                if (opts.ShowOutput && device.Input) return;
                if (opts.ShowEnabled && !device.Enabled) return;
                if (opts.ShowLoopback && !device.Loopback) return;
                if (opts.ShowDefault && !device.Default) return;
                if (!String.IsNullOrEmpty(opts.Name) && !device.Name.Contains(opts.Name)) return;

                Console.WriteLine(
                    "{0} {1} {2} {3} {4} {5}",
                    device.Id.ToString().PadRight(4),
                    device.Name.PadRight(offset + 2),
                    (device.Loopback ? "X" : "").PadLeft(4).PadRight(8),
                    (device.Enabled ? "X" : "").PadLeft(4).PadRight(7),
                    (device.Default ? "X" : "").PadLeft(4).PadRight(7),
                    device.Input ? " I " : " O "
                );
            });

            return 0;
        }
    }
}