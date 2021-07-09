using NAudio.CoreAudioApi;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Un4seen.Bass;
using Un4seen.BassWasapi;
using static PLib.Generic;

namespace PSpectrum
{
    #region IGNORED

    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.Title = "PSpectrum by Malte Linke";

            // extract required files
            if (!File.Exists("bass.dll")) File.WriteAllBytes("bass.dll", Properties.Resources.bass);
            if (!File.Exists("basswasapi.dll")) File.WriteAllBytes("basswasapi.dll", Properties.Resources.basswasapi);

            var analyser = new PAnalyser();
            analyser.DataReady += (s, d) =>
            {
                Console.WriteLine(JsonConvert.SerializeObject(d));
            };
            analyser.Start();
            Process.GetCurrentProcess().WaitForExit();
        }
    }

    #endregion IGNORED

    internal class PAnalyser
    {
        private WASAPIPROC _process;
        private EasyTimer _t;
        private float[] _buffer;

        public delegate void OnDataReady(object sender, float[] data);

        public event OnDataReady DataReady;

        public PAnalyser()
        {
            // prepare bass api?
            Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_UPDATETHREADS, false);
            var result = Bass.BASS_Init(0, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);
            if (!result) throw new Exception("Init Error");

            // prepare buffer
            _buffer = new float[8192];

            // initialize WASAPI-Process?
            _process = new WASAPIPROC(Process);

            // get my device index
            var devIndex = 0;
            var d = new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            for (int i = 0; i < BassWasapi.BASS_WASAPI_GetDeviceCount(); i++)
            {
                var device = BassWasapi.BASS_WASAPI_GetDeviceInfo(i);
                if (device.IsEnabled && device.IsLoopback && device.name == d.FriendlyName)
                {
                    if (Environment.GetCommandLineArgs().Contains("--print-device") || Environment.GetCommandLineArgs().Contains("-pd")) Console.WriteLine("Selected Device: {0}", device.name);
                    devIndex = i;
                }
            }
            result = BassWasapi.BASS_WASAPI_Init(devIndex, 0, 0, BASSWASAPIInit.BASS_WASAPI_BUFFER, 1f, 0.05f, _process, IntPtr.Zero);
            if (!result)
            {
                var error = Bass.BASS_ErrorGetCode();
                MessageBox.Show(error.ToString());
            }

            // init polling timer?
            _t = new EasyTimer(20);
            _t.Stop();
            _t.Elapsed += (s, e) =>
            {
                // get raw data
                int dataRaw = BassWasapi.BASS_WASAPI_GetData(_buffer, (int)BASSData.BASS_DATA_FFT8192);  //get ch.annel fft data
                if (dataRaw < -1) return;

                int shift = 15;

                // convert raw data to 128 channels (64 left / 64 right)
                /*float[] data = new float[128];
                int range = (_buffer.Length / 3) / (data.Length / 2); // calculate the range of frequencys
                for (int i = 0; i < (data.Length / 2) + shift; i++)
                {
                    if (i < shift) continue;
                    float dataTemp = 0;
                    for (int j = 0; j < range; j++)
                    {
                        dataTemp += (_buffer[j + (range * i)] < 0) ? 0 : _buffer[j + (range * i)];
                    }

                    // get channel audio level
                    int level = BassWasapi.BASS_WASAPI_GetLevel();
                    var audioLeft = Utils.LowWord32(level);
                    var audioRight = Utils.HighWord32(level);

                    int leftMult = ushort.MaxValue / ((audioLeft == 0) ? 1 : audioLeft);
                    int rightMult = ushort.MaxValue / ((audioRight == 0) ? 1 : audioRight);


                    // channel left
                    data[i-shift] = dataTemp * leftMult;

                    // channel right
                    data[i- shift + 64] = dataTemp * rightMult;
                }*/

                // alternative channel selector
                float[] data = new float[256];
                for (int i = 0; i < 128 + shift; i++)
                {
                    if (i < shift) continue;
                    float dataTemp = _buffer[i];

                    // get channel audio level
                    int level = BassWasapi.BASS_WASAPI_GetLevel();
                    var audioLeft = Utils.LowWord32(level);
                    var audioRight = Utils.HighWord32(level);

                    int leftMult = ushort.MaxValue / ((audioLeft == 0) ? 1 : audioLeft);
                    int rightMult = ushort.MaxValue / ((audioRight == 0) ? 1 : audioRight);


                    // channel left
                    data[i - shift] = dataTemp * leftMult;

                    // channel right
                    data[i - shift + 128] = dataTemp * rightMult;
                }

                // send to user
                if (DataReady != null) DataReady(this, data);
            };
            _t.Interval = 25; // 40Hz
        }

        // WASAPI callback, required for continuous recording
        private int Process(IntPtr buffer, int length, IntPtr user)
        {
            return length;
        }

        public void Start()
        {
            BassWasapi.BASS_WASAPI_Start();
            _t.Start();
        }

        public void Stop()
        {
            BassWasapi.BASS_WASAPI_Stop(true);
            _t.Stop();
        }
    }
}