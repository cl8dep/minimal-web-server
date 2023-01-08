using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebServer.Engine
{
    internal class Response
    {
        public int Status;
        public string Version { get; set; } = "HTTP/1.1";
        public Hashtable Headers { get; set; }
        public long BodySize
        {
            get
            {
                if (BodyData != null)
                    return BodyData.Length;
                if (File != null)
                    return File.Length;
                return 0;
            }
        }
        public byte[] BodyData;
        public FileStream File { get; set; }

        public Response()
        {
            Headers = new Hashtable();
        }

        internal void SetMimeTypeHeader(string v)
        {
            Headers[HttpProtocol.ContentTypeHeader] = v;
        }
    }
}
