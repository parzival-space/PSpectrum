using System;
using System.Collections.Generic;
using System.IO;
using Un4seen.Bass;
using Un4seen.BassWasapi;

namespace PSpectrum.Utils
{
    internal class BassGeneric
    {
        // represents a audio device
        public class BassDevice
        {
            public string Name;
            public bool Input;
            public bool Loopback;
            public bool Default;
            public bool Enabled;
            public int Id;
        }

        // installs bass if required
        public static void Install()
        {
            // check if Bass is installed by creating a validation score (<2 means is not installed)
            var validationScore = 0;

            if (File.Exists("bass.dll")) validationScore++;
            if (File.Exists("basswasapi.dll")) validationScore++;

            if (validationScore >= 2) return;

            // install bass.dll
            using (var writer = File.OpenWrite("bass.dll"))
            {
                writer.Write(
                    Properties.Resources.bass,
                    0,
                    Properties.Resources.bass.Length
                );
            }

            // install basswasapi.dll
            using (var writer = File.OpenWrite("basswasapi.dll"))
            {
                writer.Write(
                    Properties.Resources.basswasapi,
                    0,
                    Properties.Resources.basswasapi.Length
                );
            }
        }

        // prepares Bass
        public static bool InitBass()
        {
            // install bass if needed
            Install();

            Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_UPDATETHREADS, false);
            return Bass.BASS_Init(0, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);
        }

        // does what the name says
        public static List<BassDevice> GetDevices()
        {
            var devices = new List<BassDevice>();
            for (int i = 0; i < BassWasapi.BASS_WASAPI_GetDeviceCount(); i++)
            {
                var device = new BassDevice();
                var info = BassWasapi.BASS_WASAPI_GetDeviceInfo(i);
                device.Name = info.name;
                device.Loopback = info.IsLoopback;
                device.Input = info.IsInput;
                device.Enabled = info.IsEnabled;
                device.Default = info.IsDefault;

                // okay here is the thing:
                // this bug has cost me about one day!
                // BassNet is, for some reason, not counting from 0!
                device.Id = i + 1;
                devices.Add(device);
            };

            return devices;
        }

        // translates the results of Watcher to something more understandable?
        public static float[] TranslateData(float[] buffer, int shift = 0, int bufferSize = 256)
        {
            // alternative channel selector
            float[] data = new float[bufferSize];

            // translate data into a 256 float array
            for (int i = 0; i < bufferSize / 2 + shift; i++)
            {
                if (i < shift) continue;
                float dataTemp = buffer[i];

                // get channel audio level
                int level = BassWasapi.BASS_WASAPI_GetLevel();
                var audioLeft = Un4seen.Bass.Utils.LowWord32(level);
                var audioRight = Un4seen.Bass.Utils.HighWord32(level);

                int leftMult = ushort.MaxValue / ((audioLeft == 0) ? 1 : audioLeft);
                int rightMult = ushort.MaxValue / ((audioRight == 0) ? 1 : audioRight);

                // channel left
                data[i - shift] = dataTemp * leftMult;

                // channel right
                data[i - shift + bufferSize / 2] = dataTemp * rightMult;
            }

            return data;
        }

        public class Watcher
        {
            public delegate void OnDataReady(float[] data);

            public event OnDataReady Data;

            // storage buffer
            private float[] buffer;

            // polling timer
            private EasyTimer bass;

            // prepares the audio watcher
            public Watcher(int deviceId = -1, int pollingRate = 25)
            {
                // start WASAPI process (fake callback required to support recordings)
                var wasapi = new WASAPIPROC((IntPtr _a, int length, IntPtr _b) => { return length; });

                // get a list of audio devices
                var devices = BassGeneric.GetDevices();

                // try to find default loopback by finding default output device
                var device = devices.Find(d => d.Default && d.Enabled && !d.Input);

                // select custom device if provided
                if (deviceId > 0) device = devices.Find(d => d.Id == deviceId);

                // initiate WASAPI
                if (!BassWasapi.BASS_WASAPI_Init(device.Id, 0, 0, BASSWASAPIInit.BASS_WASAPI_BUFFER, 1f, 0.05f, wasapi, IntPtr.Zero))
                {
                    throw new Exception("Error: Failed to create WASAPI!");
                };

                // beginn main loop
                bass = new EasyTimer((double)pollingRate);
                bass.Elapsed += (object _sender, System.Timers.ElapsedEventArgs _args) =>
                {
                    if (Data == null) return;

                    // clear and prepare buffer
                    buffer = new float[8192];

                    // get data and validate it
                    if (BassWasapi.BASS_WASAPI_GetData(buffer, (int)BASSData.BASS_DATA_FFT8192) <= 0) return;

                    // fire data event, if in use
                    Data.Invoke(buffer);
                };
            }

            // start / stop logic
            public void Start()
            {
                BassWasapi.BASS_WASAPI_Start();
                bass.Start();
            }

            public void Stop()
            {
                BassWasapi.BASS_WASAPI_Stop(true);
                bass.Stop();
            }
        }
    }
}