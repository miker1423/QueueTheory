using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using TcpServer.Models;

namespace TcpServer
{
    public class Server
    {
        private readonly ConcurrentQueue<Request> clients = new ConcurrentQueue<Request>();
        private readonly TcpListener listener = new TcpListener(IPAddress.Any, 5001);
        private readonly List<Task> worker = new List<Task>();
        private readonly Encoding encoder = Encoding.UTF8;
        private CancellationToken token;
        private CancellationTokenSource tokenSource;
        private ServerStatus status = new ServerStatus();
        public event EventHandler<ServerStatus> DataChanged;

        private Task Start(int workers, bool autoscale)
        {
            tokenSource = new CancellationTokenSource();
            token = tokenSource.Token;

            listener.Start();
            return Task.Run(async () => {
                while (!token.IsCancellationRequested)
                {
                    var client = await listener.AcceptTcpClientAsync();
                    var request = new Request{ Client = client, ReceivedTime = DateTime.Now };
                    clients.Enqueue(request);
                    status.ReceivedClients++;
                    status.PendingRequests = clients.Count;
                    DataChanged?.Invoke(this, status);
                }
            });
        }

        public async Task Stop()
        {
            status = new ServerStatus();
            tokenSource.Cancel();
            listener.Stop();
        }

        private async Task BuildWorkers(int workers, bool autoscale, CancellationToken token)
        {
            for (int i = 0; i < workers; i++)
                worker.Add(Serve(token));
        }

        private async Task Serve(CancellationToken token)
        {
            var helloBuffer = encoder.GetBytes("HELLO!!");
            while (!token.IsCancellationRequested)
            {
                if(clients.TryDequeue(out var client))
                {
                    var stream = client.Client.GetStream();
                    await stream.WriteAsync(helloBuffer, 0, helloBuffer.Length, token);
                    status.ServedClients++;
                    DataChanged?.Invoke(this, status);
                }
            }
        }
    }
}