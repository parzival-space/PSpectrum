using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Un4seen.Bass;
using Un4seen.BassWasapi;

namespace PSpectrum.Utils
{
    class BassGeneric
    {
        public static bool InitBass()
        {
            Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_UPDATETHREADS, false);
            return Bass.BASS_Init(0, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);
        }

        public static bool InitWASAPI(int device)
        {
            // WASAPI callback, required for continuous recording
            var wasapi = new WASAPIPROC((IntPtr buffer, int length, IntPtr user) => { return length; });

            // init
            return BassWasapi.BASS_WASAPI_Init(device, 0, 0, BASSWASAPIInit.BASS_WASAPI_BUFFER, 1f, 0.05f, wasapi, IntPtr.Zero);
        }
    }
}
