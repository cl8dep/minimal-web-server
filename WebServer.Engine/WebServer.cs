using System.Net;
using System.Net.Sockets;
using System.Text;
using WebServer.Engine.Parser;

namespace WebServer.Engine
{
    public class WebServer
    {
        private TcpListener listener;
        public static int maxSimultaneousConnections = 20;
        private Semaphore semaphore;
        private Router router;

        public IPAddress IPAddress { get; set; } = IPAddress.Loopback;
        public int Port { get; set; } = 8080;

        public string RootFolder { get => router.RootFolder; set => router.RootFolder = value; }

        public WebServer()
        {
            semaphore = new(maxSimultaneousConnections, maxSimultaneousConnections);
            router = new Router();
        }


        /// <summary>
        /// Starts the web server.
        /// </summary>
        public void Start()
        {
            listener = new(IPAddress, Port);
            Start(listener);
        }

        /// <summary>
        /// Start awaiting for connections, up to the "maxSimultaneousConnections" value.
        /// This code runs in a separate thread.
        /// </summary>
        private void RunServer(TcpListener listener)
        {
            while (true)
            {
                semaphore.WaitOne();
                listener.BeginAcceptTcpClient(StartConnectionListener, null);
            }
        }


        /// <summary>
        /// Await connections.
        /// </summary>
        private void StartConnectionListener(IAsyncResult res)
        {

            var client = listener.EndAcceptTcpClient(res);
            semaphore.Release();

            var request = client.GetRequest();


            var response = router.ResolveRoute(request);

            client.SendResponse(response);

            client.Close();
        }

        /// <summary>
        /// Begin listening to connections on a separate worker thread.
        /// </summary>
        private void Start(TcpListener listener)
        {
            listener.Start();
            Task.Run(() => RunServer(listener));
        }

    }
}