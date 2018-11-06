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
        private readonly TcpListener listener = new TcpListener(IPAddress.Loopback, 8001);
        private readonly List<Task> worker = new List<Task>();
        private readonly Encoding encoder = Encoding.UTF8;
        private CancellationToken token;
        private CancellationTokenSource tokenSource;
        private ServerStatus status = new ServerStatus();
        private Task monitor;
        public event EventHandler<ServerStatus> DataChanged;

        public Task Start(int workers, bool autoscale, int threshold)
        {
            tokenSource = new CancellationTokenSource();
            token = tokenSource.Token;

            if(autoscale)
                monitor = Task.Factory.StartNew(() => {
                    if(clients.Count > threshold)
                    {
                        worker.Add(Task.Factory.StartNew(() => Serve(token)));
                        lock (status)
                        {
                            status.Workers++;
                            DataChanged?.Invoke(this, status);
                        }
                    }
                });

            BuildWorkers(workers, autoscale, token);
            Console.WriteLine("Start now!");
            
            listener.Start();
            Console.WriteLine("Started");
            return Task.Factory.StartNew(async () => {
                while (!token.IsCancellationRequested)
                {
                    var client = await listener.AcceptTcpClientAsync();
                    var request = new Request{ Client = client, ReceivedTime = DateTime.Now };
                    clients.Enqueue(request);
                    lock(status) {
                        status.ReceivedClients++;
                        status.PendingRequests = clients.Count;
                        DataChanged?.Invoke(this, status);
                    }
                }
            });
        }

        public void Stop()
        {
            status = new ServerStatus();
            tokenSource.Cancel();
            listener.Stop();
        }

        private void BuildWorkers(int workers, bool autoscale, CancellationToken token)
        {
            for (int i = 0; i < workers; i++)
                worker.Add(Task.Factory.StartNew(() => Serve(token)));
            lock (status)
            {
                status.Workers = worker.Count;
                DataChanged?.Invoke(this, status);
            }
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
                    lock (status)
                    {  
                        status.ServedClients++;
                        DataChanged?.Invoke(this, status);
                    }
                }
            }
        }
    }
}