using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PSpectrum.Utils;
using System.Threading.Tasks;
using System.Diagnostics;
using Un4seen.BassWasapi;
using Un4seen.Bass;
using Newtonsoft.Json;

namespace PSpectrum.Commands.Action
{
    class Listen
    {
        public static int Execute(Format.Listen opts)
        {
            BassSetup.Install();

            // create bass watcher
            BassGeneric.Watcher watcher = new BassGeneric.Watcher(opts.DeviceID, opts.PollingRate);
            watcher.Data += (float[] buffer) =>
            {
                var data = BassGeneric.TranslateData(buffer, 14);

                // TODO: add normalization

                Console.WriteLine(JsonConvert.SerializeObject(data));
            };
            watcher.Start();

            // wait for main processs to exit (hacky I know)
            Process.GetCurrentProcess().WaitForExit();
            return 0;
        }

        
    }
}
