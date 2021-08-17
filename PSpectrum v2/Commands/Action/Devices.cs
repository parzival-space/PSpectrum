using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Un4seen.Bass;
using Un4seen.BassWasapi;

namespace PSpectrum.Commands.Action
{
    class Devices
    {
        public static int Execute(Format.Devices opts)
        {
            Utils.BassSetup.Install();

            // init bass
            if (!Utils.BassGeneric.InitBass())
            {
                Console.WriteLine("Error: Failed to initiate Bass!");
                return 1;
            }

            // get list of devices
            var devices = new List<BASS_WASAPI_DEVICEINFO>();
            var id = -1;
            var offset = 0;
            for (int i = 0; i < BassWasapi.BASS_WASAPI_GetDeviceCount(); i++) devices.Add(BassWasapi.BASS_WASAPI_GetDeviceInfo(i));
            devices.ForEach((device) => { if (device.name.Length > offset) offset = device.name.Length; });

            // try to print them somewhat readable
            Console.WriteLine("ID".PadRight(4) + " " + "Name".PadRight(offset + 2) + " " + "Loopback Enabled Default I/O");
            devices.ForEach((device) =>
            {
                id++;

                // check if filters match to device
                if (opts.InputOnly && !device.IsInput) return;
                if (opts.OutputOnly && device.IsInput) return;
                if (opts.ShowEnabled && !device.IsEnabled) return;
                if (opts.ShowLoopback && !device.IsLoopback) return;
                if (!String.IsNullOrEmpty(opts.Name) && device.name != opts.Name) return;

                Console.WriteLine(
                    "{0} {1} {2} {3} {4} {5}",
                    id.ToString().PadRight(4),
                    device.name.PadRight(offset + 2),
                    (device.IsLoopback ? "X" : "").PadLeft(4).PadRight(8),
                    (device.IsEnabled ? "X" : "").PadLeft(4).PadRight(7),
                    (device.IsDefault ? "X" : "").PadLeft(4).PadRight(7),
                    device.IsInput ? " I " : " O "
                );
            });

            return 0;
        }
    }
}
