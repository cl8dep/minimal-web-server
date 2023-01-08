using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebServer.Engine
{
    internal class Request
    {
        public string Method;
        public string URL;
        public string Version;
        public Hashtable Args;
        public bool Execute;
        public Hashtable Headers;
        public int BodySize;
        public byte[] BodyData;

        public Request()
        {
            Args = new Hashtable();
        }

        public bool Parsed { get; internal set; }
    }
}
