using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace PSpectrum.Utils.Web
{
    internal class WebSocketServer
    {
        private IPEndPoint EndPoint;
        private Socket Server;
        private System.Timers.Timer Checker;
        public List<Socket> Clients;

        public delegate void ClientConnectedEvent(WebSocketServer socket, Socket client);

        public event ClientConnectedEvent Connect;

        public delegate void WebSocketReadyEvent(WebSocketServer socket);

        public event WebSocketReadyEvent Ready;

        public delegate void ClientDisconnectedEvent(WebSocketServer socket, Socket client);

        public event ClientDisconnectedEvent Disconnect;

        private AutoResetEvent ClosedEvent = new AutoResetEvent(false);

        public bool IsReady = false;

        public WebSocketServer(string ip, int port)
        {
            this.Server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.Clients = new List<Socket>();
            this.EndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            this.Checker = new System.Timers.Timer(250);
            this.Checker.AutoReset = true;
            this.Checker.Elapsed += (_a, _b) =>
            {
                for (int i = 0; i < this.Clients.Count; i++)
                {
                    if (!this.Clients[i].Connected)
                    {
                        this.Disconnect?.Invoke(this, this.Clients[i]);
                        this.Clients.Remove(this.Clients[i]);
                    }
                }
            };
        }

        public void Listen()
        {
            // bind port and socket
            this.Server.Bind(this.EndPoint);
            this.Server.Listen(1);

            this.Checker.Start();

            // start handling data
            this.Server.BeginAccept(HandshakeHandler, null);
        }

        public void Close()
        {
            this.Server.Close();
            this.Checker.Stop();
            this.IsReady = false;
            this.Clients.Clear();
            this.ClosedEvent.Set();
        }

        public void WaitForClose()
        {
            this.ClosedEvent.WaitOne();
        }

        public void Broadcast(string message)
        {
            // create websocket response
            byte[] data = CreateMessage(message);

            // send to all connected clients
            for (int i = 0; i < this.Clients.Count; i++)
            {
                if (this.Clients[i].Connected) try { this.Clients[i].Send(data); } catch (Exception) { }
            }
        }

        private void HandshakeHandler(IAsyncResult connection)
        {
            // if not already made, invoke ready event
            if (!this.IsReady) this.Ready?.Invoke(this);
            this.IsReady = true;

            // try to start the communication with the new client
            Socket client = this.Server.EndAccept(connection);

            // request handshake
            byte[] handshakeBuffer = new byte[1024];
            client.Receive(handshakeBuffer);

            // send handshake to the connecting client
            client.Send(Encoding.Default.GetBytes(CreateHandshake(handshakeBuffer)));

            // register client as connected
            this.Clients.Add(client);

            // call connect event
            this.Connect?.Invoke(this, client);

            // start accepting new clients
            this.Server.BeginAccept(HandshakeHandler, null);
        }

        /// <summary>
        /// Gets the http request string to send to the websocket client
        /// </summary>
        private string CreateHandshake(byte[] buffer)
        {
            string request = Encoding.Default.GetString(buffer);

            // create request key
            int keyStart = request.IndexOf("Sec-WebSocket-Key: ") + 19;
            string key = null;

            for (int i = keyStart; i < (keyStart + 24); i++)
            {
                key += request[i];
            }

            // create handshake hash
            string handshake = key + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
            byte[] result = SHA1.Create().ComputeHash(Encoding.ASCII.GetBytes(handshake));

            // return handshake
            return string.Format("HTTP/1.1 101 Switching Protocols\nUpgrade: WebSocket\nConnection: Upgrade\nSec-WebSocket-Accept: {0}\r\n\r\n", Convert.ToBase64String(result));
        }

        /// <summary>
        /// Covnverts a string into a WebSocket frame.
        /// </summary>
        /// <param name="data"></param>
        public byte[] CreateMessage(string data)
        {
            byte[] response;
            byte[] bytesRaw = Encoding.Default.GetBytes(data);
            byte[] frame = new byte[10];

            int indexStartRawData = -1;
            int length = bytesRaw.Length;

            frame[0] = (byte)(128 + 1); // 1 = text
            if (length <= 125)
            {
                frame[1] = (byte)length;
                indexStartRawData = 2;
            }
            else if (length >= 126 && length <= 65535)
            {
                frame[1] = (byte)126;
                frame[2] = (byte)((length >> 8) & 255);
                frame[3] = (byte)(length & 255);
                indexStartRawData = 4;
            }
            else
            {
                frame[1] = (byte)127;
                frame[2] = (byte)((length >> 56) & 255);
                frame[3] = (byte)((length >> 48) & 255);
                frame[4] = (byte)((length >> 40) & 255);
                frame[5] = (byte)((length >> 32) & 255);
                frame[6] = (byte)((length >> 24) & 255);
                frame[7] = (byte)((length >> 16) & 255);
                frame[8] = (byte)((length >> 8) & 255);
                frame[9] = (byte)(length & 255);

                indexStartRawData = 10;
            }

            response = new byte[indexStartRawData + length];

            int i, reponseIdx = 0;

            //Add the frame bytes to the reponse
            for (i = 0; i < indexStartRawData; i++)
            {
                response[reponseIdx] = frame[i];
                reponseIdx++;
            }

            //Add the data bytes to the response
            for (i = 0; i < length; i++)
            {
                response[reponseIdx] = bytesRaw[i];
                reponseIdx++;
            }

            return response;
        }
    }
}