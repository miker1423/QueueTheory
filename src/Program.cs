using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;
using System.Collections.Generic;
using System.Text;
using TcpServer.Models;

namespace TcpServer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var server = new Server();
            server.DataChanged += DataReceived;
            Console.CancelKeyPress += (sender, arg) => {
                server.Stop();
            };
            await server.Start(2, true, 5);
        }

        static void DataReceived(object sender, ServerStatus status)
        {
            Console.WriteLine(status);
        }
    }
}
