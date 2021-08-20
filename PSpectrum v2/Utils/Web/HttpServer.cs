using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace PSpectrum.Utils.Web
{
    /// <summary>
    /// Represents a simpler version of the HttpListener type.
    /// </summary>
    public class HttpServer
    {
        private HttpListener Server = new HttpListener();
        private Dictionary<string, HttpResource> Resources;
        private bool KeepAlive = true;
        private AutoResetEvent ClosedEvent = new AutoResetEvent(false);
        private Thread Handle = null;

        public delegate void ReadyEvent(HttpServer server);

        public event ReadyEvent OnReady;

        /// <summary>
        /// Creates a new HttpServer on your hostname and the selected port that optionaly listens to localhost.
        /// </summary>
        /// <param name="port">The port to listen on.</param>
        /// <param name="allowLocalhost">If true, allows incoming connections from localhost.</param>
        public HttpServer(int port, string hostname, bool allowLocalhost = true)
        {
            this.Server.Prefixes.Add("http://" + hostname + ":" + port + "/");
            if (port == 80) this.Server.Prefixes.Add("http://" + hostname + "/");

            if (allowLocalhost)
            {
                this.Server.Prefixes.Add("http://127.0.0.1:" + port + "/");
                this.Server.Prefixes.Add("http://localhost:" + port + "/");

                if (port == 80) this.Server.Prefixes.Add("http://localhost/");
                if (port == 80) this.Server.Prefixes.Add("http://127.0.0.1/");
            }
            this.Resources = new Dictionary<string, HttpResource>();
        }

        /// <summary>
        /// Creates a new HttpServer on port 80.
        /// </summary>
        /// <param name="hostname">Your hostname.</param>
        public HttpServer(string hostname)
        {
            this.Server.Prefixes.Add("http://" + hostname + "/");
            this.Server.Prefixes.Add("http://" + hostname + ":80/");
            this.Resources = new Dictionary<string, HttpResource>();
        }

        /// <summary>
        /// Creates a new HttpServer for localhost on the set port.
        /// </summary>
        /// <param name="port">Your port.</param>
        public HttpServer(int port)
        {
            this.Server.Prefixes.Add("http://127.0.0.1:" + port + "/");
            this.Server.Prefixes.Add("http://localhost:" + port + "/");

            if (port == 80) this.Server.Prefixes.Add("http://localhost/");
            if (port == 80) this.Server.Prefixes.Add("http://127.0.0.1/");
            this.Resources = new Dictionary<string, HttpResource>();
        }

        /// <summary>
        /// Maps a resource to a request path.
        /// </summary>
        /// <param name="path">The request path.</param>
        /// <param name="resource">The resulting resource.</param>
        public void AddResource(string path, HttpResource resource)
        {
            this.Resources.Add(path, resource);
        }

        /// <summary>
        /// Maps a single resource to multiple request paths.
        /// </summary>
        /// <param name="path">The request path.</param>
        /// <param name="resource">The resulting resource.</param>
        public void AddResource(string[] paths, HttpResource resource)
        {
            foreach (var path in paths)
            {
                this.Resources.Add(path, resource);
            }
        }

        /// <summary>
        /// Starts the webserver and waits for incoming connections.
        /// </summary>
        public void Listen()
        {
            // throw error if a server is already running
            if (this.Server.IsListening)
                throw new Exception("A webserver of this instance is already running! Please close the running webserver before you start another one.");

            // start http server
            this.Server.Start();

            // spawn a new worker
            this.Handle = SpawnHandle();
        }

        /// <summary>
        /// Trys to close the webserver.
        /// </summary>
        public void Close(bool force = false)
        {
            // check if the webserver is running
            if (!this.Server.IsListening) return;

            // check if force is enabled
            if (force)
            {
                // kill the running thread
                this.Handle.Abort();

                // close the webserver
                this.Server.Stop();

                // because this may break the reset logic, we do it manually here
                this.ClosedEvent.Set();
                this.KeepAlive = true;
            }
            else
            {
                // try to close the server without forcing it
                this.KeepAlive = false;
                this.ClosedEvent.WaitOne();
            }
        }

        /// <summary>
        /// Waits until the server gets closed.
        /// </summary>
        public void Wait()
        {
            this.ClosedEvent.WaitOne();
        }

        /// <summary>
        /// Handles the traffic.
        /// </summary>
        /// <returns></returns>
        private Thread SpawnHandle()
        {
            // spawn the webserver in a new thread
            Thread handler = new Thread(async () =>
            {
                // now call the ready event
                this.OnReady?.Invoke(this);

                // try to handle the request
                while (KeepAlive)
                {
                    // wait until a new request is made
                    HttpListenerContext ctx = await this.Server.GetContextAsync();

                    // get request and response
                    HttpListenerRequest req = ctx.Request;
                    HttpListenerResponse res = ctx.Response;

                    // try to find the correct request path
                    string path = "";
                    foreach (var prefix in this.Server.Prefixes)
                    {
                        // check if the current prefix matches the request
                        if (req.Url.ToString().StartsWith(prefix))
                        {
                            path = req.Url.ToString().Substring(prefix.Length - 1);
                            break;
                        }
                    }

                    // send resource if available
                    if (this.Resources.TryGetValue(path, out HttpResource resource))
                    {
                        // successfully found the resource
                        res.ContentType = resource.Type;
                        res.ContentLength64 = resource.Data.Length;
                        res.StatusCode = (int)HttpStatusCode.OK;

                        // when an encoding is given, also send encoding
                        if (resource.Encoding != null) res.ContentEncoding = resource.Encoding;

                        // finally send the resource
                        await res.OutputStream.WriteAsync(resource.Data, 0, resource.Data.Length);
                    }
                    else
                    {
                        // no resource was found => 404
                        res.StatusCode = (int)HttpStatusCode.NotFound;
                    }

                    // print some information
                    Console.WriteLine(res.StatusCode + " " + req.HttpMethod + " " + req.Url.ToString());

                    res.Close();
                }

                this.Server.Stop();
                this.ClosedEvent.Set();

                // reset the KeepAlive state
                this.KeepAlive = true;
            });

            // automatically start the handle
            handler.Start();

            return handler;
        }
    }
}