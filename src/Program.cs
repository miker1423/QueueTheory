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
        static void Main(string[] args)
        {
            var server = new Server();
            server.DataChanged += DataReceived;
        }

        static void DataReceived(object sender, ServerStatus status)
        {
            
        }
    }
}
