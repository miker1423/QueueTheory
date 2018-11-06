using System;

namespace TcpServer.Models
{
    public class ServerStatus
    {
        public int ReceivedClients { get; set; }
        public int ServedClients { get; set; }
        public int PendingRequests { get; set; }
        public TimeSpan AvarageWaitTime { get; set; }
        public float RPS { get; set; }
    }
}