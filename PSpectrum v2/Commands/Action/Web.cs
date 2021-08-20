using Newtonsoft.Json;
using PSpectrum.Utils;
using PSpectrum.Utils.Web;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace PSpectrum.Commands.Action
{
    internal class Web
    {
        public static int Execute(Format.Web opts)
        {
            // init bass
            if (!BassGeneric.InitBass())
            {
                Console.WriteLine("Error: Failed to initiate Bass!");
                return 1;
            }

            // create ports
            int[] ports = GeneratePorts(opts.HttpPort, opts.SocketPort);

            // create web and socket server
            HttpServer server = new HttpServer(ports[0], opts.Prefix);
            WebSocketServer socket = new WebSocketServer("127.0.0.1", ports[1]);

            // regsiter custom script if enabled
            var useCustomScript = !String.IsNullOrEmpty(opts.CustomScript) && File.Exists(opts.CustomScript);
            if (useCustomScript) server.AddResource("/custom.js", HttpResource.CreateText(File.ReadAllText(opts.CustomScript), HttpResource.TextResource.JAVASCRIPT));

            // register web content
            server.AddResource(new string[] { "/", "/index.html" }, HttpResource.CreateText(LoadResource("Resources.Web.index.html"), HttpResource.TextResource.HTML));
            server.AddResource("/style.css", HttpResource.CreateText(LoadResource("Resources.Web.style.css"), HttpResource.TextResource.CSS));
            server.AddResource("/script.js", HttpResource.CreateText(LoadResource("Resources.Web.script.js").Replace("$SOCKET_PORT$", ports[1].ToString()).Replace("$CUSTOM_SCRIPT$", useCustomScript.ToString().ToLower()), HttpResource.TextResource.JAVASCRIPT));
            server.AddResource("/favicon.ico", new HttpResource(GetOwnIcon(), "image/x-icon"));

            // start servers
            server.Listen();
            socket.Listen();

            // prepare socket and server
            server.OnReady += _ => Console.WriteLine("Server starting. Please wait...");

            // prepare normalizer
            Normalizer normalizer = new Normalizer(opts.Normalization);

            // now analyse the system audio
            BassGeneric.Watcher watcher = new BassGeneric.Watcher(opts.DeviceID, opts.PollingRate);
            watcher.Data += (float[] buffer) =>
            {
                var data = BassGeneric.TranslateData(buffer, opts.Shift, opts.BufferSize);

                // prevent overdraw
                data = normalizer.PreventOverdraw(data, (float)opts.Overdraw);

                // normalize
                data = normalizer.Next(data); ;

                // send data to all clients
                socket.Broadcast(JsonConvert.SerializeObject(data));
            };

            Console.WriteLine("Server ready! Waiting for Socket connection...");

            if (!opts.NoView) Process.Start("http://localhost:" + ports[0] + "/");

            watcher.Start();
            socket.WaitForClose();
            return 0;
        }

        /// <summary>
        /// This will generate a port in the range of 49152 - 65535 if no custom port is set.
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        private static int[] GeneratePorts(int http, int socket)
        {
            // generate a number
            var rnd = new Random();

            // return custom ports if now set to default
            return new int[] {
                (http <= 0) ? rnd.Next(49152, 65535) : http,
                (socket <= 0) ? rnd.Next(49152, 65535) : socket
            };
        }

        private static string LoadResource(string name)
        {
            var self = typeof(Program);
            var resource = self.Assembly.GetManifestResourceStream(self, name);
            using (var reader = new StreamReader(resource))
            {
                return reader.ReadToEnd();
            }
        }

        private static Color ChangeBrightness(Color color, float correctionFactor)
        {
            float red = (float)color.R;
            float green = (float)color.G;
            float blue = (float)color.B;

            if (correctionFactor < 0)
            {
                correctionFactor = 1 + correctionFactor;
                red *= correctionFactor;
                green *= correctionFactor;
                blue *= correctionFactor;
            }
            else
            {
                red = (255 - red) * correctionFactor + red;
                green = (255 - green) * correctionFactor + green;
                blue = (255 - blue) * correctionFactor + blue;
            }

            return Color.FromArgb(color.A, (int)red, (int)green, (int)blue);
        }

        private static byte[] GetOwnIcon()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                Icon.ExtractAssociatedIcon(Application.ExecutablePath).Save(ms);
                return ms.ToArray();
            }
        }
    }
}