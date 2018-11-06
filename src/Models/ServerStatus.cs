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
        public int Workers { get; set; }

        public override string ToString()
            => $"W: {Workers}, RC: {ReceivedClients}, SC: {ServedClients}, PR: {PendingRequests}";
    }
}