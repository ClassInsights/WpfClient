using System;
using System.IO.Pipes;
using System.IO;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows;
using Newtonsoft.Json;
using WpfClient.Models;

namespace WpfClient.Services
{
    public class PipeService
    {
        private static readonly TimeSpan HeartbeatInterval = TimeSpan.FromSeconds(5);
        private static readonly TimeSpan ReconnectInterval = TimeSpan.FromSeconds(3);
        
        public async Task RunClient()
        {
            while (true)
            {                
                var client = new NamedPipeClientStream(".", "ClassInsights", PipeDirection.InOut, PipeOptions.Asynchronous, TokenImpersonationLevel.Impersonation);
                try
                {
                    await client.ConnectAsync(); // Connect to the server

                    var writer = new StreamWriter(client) { AutoFlush = true };
                    
                    // Send username to the server
                    await writer.WriteLineAsync(WindowsIdentity.GetCurrent().Name);
                    
                    // Start heartbeat in a separate task
                    _ = Task.Factory.StartNew(() => SendHeartbeats(writer), TaskCreationOptions.LongRunning);

                    var reader = new StreamReader(client);
                    // Listen for server messages
                    while (client.IsConnected)
                    {
                        Console.WriteLine("Reading incoming message...");
                        var message = await reader.ReadLineAsync();
                        if (message == null) continue;
                        var packet = JsonConvert.DeserializeObject<PipeModels.Packet>(message);
                        if (packet == null)
                            continue;
                        
                        switch (packet.PacketType)
                        {
                            case PipeModels.Type.Shutdown:
                                var shutdownData = JsonConvert.DeserializeObject<PipeModels.Packet<PipeModels.ShutdownData>>(message)?.Data;
                                if (shutdownData == null) break;
                                
                                await Application.Current.Dispatcher.InvokeAsync(() =>
                                {
                                    if (Application.Current.MainWindow is MainWindow window)
                                        window.NextLesson.Content = string.IsNullOrWhiteSpace(shutdownData.NextLesson) ? "" : $"Nächste Stunde: {shutdownData.NextLesson} Uhr";
                                    Application.Current.MainWindow?.Show();
                                });
                                
                                Process.Start("shutdown", "/s /f /t 300");
                                await writer.WriteLineAsync("OK");
                                break;
                            case PipeModels.Type.Logoff:                                    
                                Process.Start("shutdown", "/l /t 5");
                                await writer.WriteLineAsync("OK");
                                break;
                            case PipeModels.Type.Afk:
                                var afkData = JsonConvert.DeserializeObject<PipeModels.Packet<PipeModels.AfkData>>(message)?.Data;
                                if (afkData == null) break;
                                
                                var timeout = afkData.Timeout;
                                _ = Task.Run(() => AfkService.StartAsync(timeout * 60_000));
                                break;
                        }

                        Console.WriteLine($@"Message from server: {message}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($@"Connection error: {ex.Message}. Reconnecting in {ReconnectInterval.TotalSeconds} seconds...");
                }
                finally
                {
                    client.Dispose();
                    await Task.Delay(ReconnectInterval); // Wait before attempting to reconnect
                }                
            }
        }

        private static async Task SendHeartbeats(StreamWriter writer)
        {
            while (true)
            {
                try
                {
                    Console.WriteLine("Sending heartbeat...");
                    await writer.WriteLineAsync("HEARTBEAT");
                    await Task.Delay(HeartbeatInterval);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($@"Failed to send heartbeat: {ex.Message}");
                    break;
                }
            }
        }
    }
}
