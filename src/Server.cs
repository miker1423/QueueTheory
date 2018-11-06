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
        private readonly TcpListener listener = new TcpListener(IPAddress.Any, 8001);
        private readonly List<Thread> worker = new List<Thread>();
        private readonly Encoding encoder = Encoding.UTF8;
        private CancellationToken token;
        private CancellationTokenSource tokenSource;
        private ServerStatus status = new ServerStatus();
        private Thread monitor;
        private DateTime lastReceived = DateTime.Now;
        public event EventHandler<ServerStatus> DataChanged;

        public Task Start(int workers, bool autoscale, int threshold, int maxWorkers)
        {
            tokenSource = new CancellationTokenSource();
            token = tokenSource.Token;

            if(autoscale)
            {
                monitor = new Thread(() => {
                    while(!token.IsCancellationRequested)
                    {
                        if(clients.Count > threshold && worker.Count < maxWorkers)
                        {
                            worker.Add(new Thread(async () => await Serve(token)));
                            worker[worker.Count - 1].Start();
                            lock (status)
                            {
                                status.Workers++;
                                DataChanged?.Invoke(this, status);
                            }
                        }
                    }
                });
                monitor.Start();
            }

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
                    var diff = DateTime.Now.Subtract(lastReceived);
                    lock(status) {
                        status.ReceivedClients++;
                        status.AvarageArrivalTime = CalculateAvarage(status.AvarageArrivalTime, status.ReceivedClients, diff);
                        status.PendingRequests = clients.Count;
                        DataChanged?.Invoke(this, status);
                    }
                    lastReceived = DateTime.Now;
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
            {
                worker.Add(new Thread(async () => await Serve(token)));
                worker[i].Start();
            }
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
                    await Task.Delay(TimeSpan.FromSeconds(1));
                    var stream = client.Client.GetStream();
                    await stream.WriteAsync(helloBuffer, 0, helloBuffer.Length, token);
                    var waitTime = DateTime.Now.Subtract(client.ReceivedTime);
                    lock (status)
                    {
                        status.PendingRequests = clients.Count;
                        status.ServedClients++;
                        status.AvarageWaitTime = CalculateAvarage(status.AvarageWaitTime, status.ServedClients, waitTime);
                        status.RPS = status.ServedClients / status.AvarageWaitTime.TotalSeconds;
                        DataChanged?.Invoke(this, status);
                    }
                }
            }
        }

        private TimeSpan CalculateAvarage(TimeSpan prevAvarage, int count, TimeSpan newTime)
        {
            var prevSum = prevAvarage.TotalMilliseconds * (count - 1);
            prevSum += newTime.TotalMilliseconds;
            return TimeSpan.FromMilliseconds(prevSum / count);
        }
    }
}