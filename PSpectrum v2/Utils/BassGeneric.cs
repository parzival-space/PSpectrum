using System;
using System.Collections.Generic;
using Un4seen.Bass;
using Un4seen.BassWasapi;

namespace PSpectrum.Utils
{
    internal class BassGeneric
    {
        public class BassDevice
        {
            public string Name;
            public bool Input;
            public bool Loopback;
            public bool Default;
            public bool Enabled;
            public int Id;
        }

        public static bool InitBass()
        {
            Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_UPDATETHREADS, false);
            return Bass.BASS_Init(0, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);
        }

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
        public static float[] TranslateData(float[] buffer, int shift = 0)
        {
            // alternative channel selector
            float[] data = new float[256];

            // translate data into a 256 float array
            for (int i = 0; i < 128 + shift; i++)
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
                data[i - shift + 128] = dataTemp * rightMult;
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
                // init bass
                Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_UPDATETHREADS, false);
                if (!Bass.BASS_Init(0, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero))
                {
                    throw new Exception("Error: Failed to create Bass!");
                }

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

                    // TODO: add shift logic

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