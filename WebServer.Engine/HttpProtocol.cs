using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebServer.Engine
{
    internal static class HttpProtocol
    {
        public const string SP = " ";
        public const string CRLF = "\r\n";

        public static string ContentLengthHeader = "Content-Length";

        public static string ContentTypeHeader = "Content-Type";

        public static string GetStatusCodeMessage(int statusCode)
        {
            return statusCode switch
            {
                200 => "OK",
                404 => "Not Found",
                _ => throw new NotImplementedException(),
            };
        }
    }
}
