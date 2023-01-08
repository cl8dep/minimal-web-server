using WebServer.Engine;

var Server = new WebServer.Engine.WebServer();
Server.RootFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WebRoot");

Server.Start();

Console.ReadLine();