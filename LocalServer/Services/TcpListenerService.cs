using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LocalServer.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

namespace LocalServer.Services
{
    public class TcpListenerService : BackgroundService
    {
        private const int Port = 8000;
        private List<Task> _connections = new List<Task>();

        // pending connections
        private object _lock = new object();

        private IServiceScopeFactory _serviceScopeFactory;

        // sync lock

        public TcpListenerService(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await StartListener(stoppingToken);
        }

        // Handle new connection
        private Task HandleConnectionAsync(TcpClient tcpClient)
        {
            return Task.Run(async () =>
            {
                using (var networkStream = tcpClient.GetStream())
                {
                    var buffer = new byte[4096];
                    Console.WriteLine("[Server] Reading from client");
                    var byteCount = await networkStream.ReadAsync(buffer, 0, buffer.Length);
                    var request = Encoding.UTF8.GetString(buffer, 0, byteCount).Trim().Replace("\0", string.Empty);

                    Console.WriteLine("[Server] Client wrote {0}", request);

                    var data = JsonConvert.DeserializeObject<TemperatureData>(request);

                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetService<ApplicationDBContext>();
                        context.Data.Add(data);
                        context.SaveChanges();
                    }
                }
            });
        }

        // Register and handle the connection
        private async Task StartHandleConnectionAsync(TcpClient tcpClient)
        {
            // start the new connection task
            var connectionTask = HandleConnectionAsync(tcpClient);

            // add it to the list of pending task
            lock (_lock)
                _connections.Add(connectionTask);

            // catch all errors of HandleConnectionAsync
            try
            {
                await connectionTask;
                // we may be on another thread after "await"
            }
            catch (Exception ex)
            {
                // log the error
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                // remove pending task
                lock (_lock)
                    _connections.Remove(connectionTask);
            }
        }

        private async Task StartListener(CancellationToken stoppingToken)
        {
            var tcpListener = TcpListener.Create(Port);
            tcpListener.Start();

            while (!stoppingToken.IsCancellationRequested)
            {
                var tcpClient = await tcpListener.AcceptTcpClientAsync();
                Console.WriteLine("[Server] Client has connected");

                var task = StartHandleConnectionAsync(tcpClient);

                // if already faulted, re-throw any error on the calling context
                if (task.IsFaulted)
                    await task;
            }
        }
    }
}