using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace WebServer.Engine.Parser
{
    internal enum RState
    {

    };
    internal static class TcpClientExtensions
    {
        public static Request? GetRequest(this TcpClient client)
        {
            var request = new Request();
            ParserStatus parserStatus = ParserStatus.METHOD;
            var myReadBuffer = new byte[client.ReceiveBufferSize];
            string myCompleteMessage = "";
            int numberOfBytesRead = 0;

            NetworkStream ns = client.GetStream();

            request.Version = "HTTP/1.1";

            string hValue = "";
            string hKey = "";


            // binary data buffer index
            int bfndx = 0;

            // Incoming message may be larger than the buffer size.
            try
            {
                do
                {
                    numberOfBytesRead = ns.Read(myReadBuffer, 0, myReadBuffer.Length);
                    myCompleteMessage =
                       string.Concat(myCompleteMessage,
                          Encoding.ASCII.GetString(myReadBuffer, 0, numberOfBytesRead));

                    // read buffer index
                    int ndx = 0;
                    do
                    {
                        switch (parserStatus)
                        {
                            case ParserStatus.METHOD:
                                if (myReadBuffer[ndx] != ' ')
                                    request.Method += (char)myReadBuffer[ndx++];
                                else
                                {
                                    ndx++;
                                    parserStatus = ParserStatus.URL;
                                }
                                break;
                            case ParserStatus.URL:
                                if (myReadBuffer[ndx] == '?')
                                {
                                    ndx++;
                                    hKey = "";
                                    request.Execute = true;
                                    parserStatus = ParserStatus.URLPARM;
                                }
                                else if (myReadBuffer[ndx] != ' ')
                                    request.URL += (char)myReadBuffer[ndx++];
                                else
                                {
                                    ndx++;
                                    request.URL = HttpUtility.UrlDecode(request.URL);
                                    parserStatus = ParserStatus.VERSION;
                                }
                                break;
                            case ParserStatus.URLPARM:
                                if (myReadBuffer[ndx] == '=')
                                {
                                    ndx++;
                                    hValue = "";
                                    parserStatus = ParserStatus.URLVALUE;
                                }
                                else if (myReadBuffer[ndx] == ' ')
                                {
                                    ndx++;

                                    request.URL = HttpUtility.UrlDecode(request.URL);
                                    parserStatus = ParserStatus.VERSION;
                                }
                                else
                                {
                                    hKey += (char)myReadBuffer[ndx++];
                                }
                                break;
                            case ParserStatus.URLVALUE:
                                if (myReadBuffer[ndx] == '&')
                                {
                                    ndx++;
                                    hKey = HttpUtility.UrlDecode(hKey);
                                    hValue = HttpUtility.UrlDecode(hValue);
                                    request.Args[hKey] = request.Args[hKey] != null ?
                                             request.Args[hKey] + ", " + hValue : hValue;
                                    hKey = "";
                                    parserStatus = ParserStatus.URLPARM;
                                }
                                else if (myReadBuffer[ndx] == ' ')
                                {
                                    ndx++;
                                    hKey = HttpUtility.UrlDecode(hKey);
                                    hValue = HttpUtility.UrlDecode(hValue);
                                    request.Args[hKey] = request.Args[hKey] != null ?
                                            request.Args[hKey] + ", " + hValue : hValue;

                                    request.URL = HttpUtility.UrlDecode(request.URL);
                                    parserStatus = ParserStatus.VERSION;
                                }
                                else
                                {
                                    hValue += (char)myReadBuffer[ndx++];
                                }
                                break;
                            case ParserStatus.VERSION:
                                if (myReadBuffer[ndx] == '\r')
                                    ndx++;
                                else if (myReadBuffer[ndx] != '\n')
                                    request.Version += (char)myReadBuffer[ndx++];
                                else
                                {
                                    ndx++;
                                    hKey = "";
                                    request.Headers = new Hashtable();
                                    parserStatus = ParserStatus.HEADERKEY;
                                }
                                break;
                            case ParserStatus.HEADERKEY:
                                if (myReadBuffer[ndx] == '\r')
                                    ndx++;
                                else if (myReadBuffer[ndx] == '\n')
                                {
                                    ndx++;
                                    if (request.Headers["Content-Length"] != null)
                                    {
                                        request.BodySize = Convert.ToInt32(request.Headers["Content-Length"]);
                                        request.BodyData = new byte[request.BodySize];
                                        parserStatus = ParserStatus.BODY;
                                    }
                                    else
                                        parserStatus = ParserStatus.OK;

                                }
                                else if (myReadBuffer[ndx] == ':')
                                    ndx++;
                                else if (myReadBuffer[ndx] != ' ')
                                    hKey += (char)myReadBuffer[ndx++];
                                else
                                {
                                    ndx++;
                                    hValue = "";
                                    parserStatus = ParserStatus.HEADERVALUE;
                                }
                                break;
                            case ParserStatus.HEADERVALUE:
                                if (myReadBuffer[ndx] == '\r')
                                    ndx++;
                                else if (myReadBuffer[ndx] != '\n')
                                    hValue += (char)myReadBuffer[ndx++];
                                else
                                {
                                    ndx++;
                                    request.Headers.Add(hKey, hValue);
                                    hKey = "";
                                    parserStatus = ParserStatus.HEADERKEY;
                                }
                                break;
                            case ParserStatus.BODY:
                                // Append to request BodyData
                                Array.Copy(myReadBuffer, ndx, request.BodyData,
                                   bfndx, numberOfBytesRead - ndx);
                                bfndx += numberOfBytesRead - ndx;
                                ndx = numberOfBytesRead;
                                if (request.BodySize <= bfndx)
                                {
                                    parserStatus = ParserStatus.OK;
                                }
                                break;
                                //default:
                                //   ndx++;
                                //   break;

                        }
                    }
                    while (ndx < numberOfBytesRead);

                }
                while (ns.DataAvailable);
            }
            catch (Exception)
            {
                request.Parsed = false;
                return request;
            }

            if (parserStatus != ParserStatus.OK)
                request.Parsed = false;
            else
                request.Parsed = true;

            return request;
        }


        public static void SendResponse(this TcpClient client, Response response)
        {
            var ns = client.GetStream();

            //response.Headers.Add("Server", Parent.Name);
            response.Headers.Add("Date", DateTime.Now.ToString("r"));
            response.Headers.Add(HttpProtocol.ContentLengthHeader, response.BodySize);

            string HeadersString = $"{response.Version}{HttpProtocol.SP}" +
                $"{response.Status}{HttpProtocol.SP}" +
                $"{HttpProtocol.GetStatusCodeMessage(response.Status)}{HttpProtocol.CRLF}";

            foreach (DictionaryEntry Header in response.Headers)
            {
                HeadersString += Header.Key + ": " + Header.Value + "\n";
            }

            HeadersString += "\n";
            byte[] bHeadersString = Encoding.ASCII.GetBytes(HeadersString);

            // Send headers
            ns.Write(bHeadersString, 0, bHeadersString.Length);

            // Send body
            if (response.BodyData != null)
                ns.Write(response.BodyData, 0, response.BodyData.Length);

            if (response.File != null)
                using (response.File)
                {
                    byte[] b = new byte[client.SendBufferSize];
                    int bytesRead;
                    while ((bytesRead = response.File.Read(b, 0, b.Length)) > 0)
                    {
                        ns.Write(b, 0, bytesRead);
                    }

                    response.File.Close();
                }
        }
    }
}
