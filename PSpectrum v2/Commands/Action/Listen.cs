using Newtonsoft.Json;
using PSpectrum.Utils;
using System;
using System.Diagnostics;

namespace PSpectrum.Commands.Action
{
    internal class Listen
    {
        public static int Execute(Format.Listen opts)
        {
            // init bass
            if (!BassGeneric.InitBass())
            {
                Console.WriteLine("Error: Failed to initiate Bass!");
                return 1;
            }

            // prepare normalizer
            Normalizer normalizer = new Normalizer(opts.Normalization);

            // create bass watcher
            BassGeneric.Watcher watcher = new BassGeneric.Watcher(opts.DeviceID, opts.PollingRate);
            watcher.Data += (float[] buffer) =>
            {
                var data = BassGeneric.TranslateData(buffer, opts.Shift, opts.BufferSize);

                // prevent overdraw
                data = normalizer.PreventOverdraw(data, (float)opts.Overdraw);

                // normalize
                data = normalizer.Next(data);

                Console.WriteLine(JsonConvert.SerializeObject(data));
            };
            watcher.Start();

            // wait for main processs to exit (hacky I know)
            Process.GetCurrentProcess().WaitForExit();
            return 0;
        }
    }
}