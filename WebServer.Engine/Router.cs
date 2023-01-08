using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Web;

using WebServer.Engine.Mime;

namespace WebServer.Engine
{
    internal class Router
    {
        public string DefaultPage { get; set; } = "index.html";
        public string RootFolder { get; set; }

        public Response ResolveRoute(Request request)
        {
            switch (request.Method)
            {
                case "GET":
                    return ResolveGetRespone(request);
                case "HEAD":
                    return ResolveGetRespone(request);
                default:
                    return null;
            }

        }

        private Response ResolveGetRespone(Request request)
        {            
            var fullPath = Path.Join(RootFolder, request.URL == "/" ? DefaultPage : request.URL);
            var response = new Response();
            if (!File.Exists(fullPath))
            {
                response.Status = 404;
            }
            else
            {
                response.Status = 200;
                response.File = File.Open(fullPath, FileMode.Open, FileAccess.Read);
                response.SetMimeTypeHeader(MimeMapping.GetMimeMapping(Path.GetExtension(fullPath)));
            }           

            return response;
        }
    }
}
