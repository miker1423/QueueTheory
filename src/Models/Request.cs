
using System;
using System.Net.Sockets;

namespace TcpServer.Models
{
    public class Request
    {
        public TcpClient Client { get; set; }
        public DateTime ReceivedTime { get; set; }
    }
}