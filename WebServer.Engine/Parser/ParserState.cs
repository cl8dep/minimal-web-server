using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebServer.Engine.Parser
{
    internal enum ParserStatus
    {
        METHOD, URL, URLPARM, URLVALUE, VERSION,
        HEADERKEY, HEADERVALUE, BODY, OK
    }
}
