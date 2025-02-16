using System;
using System.IO.Pipes;
using System.IO;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows;

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

                    var reader = new StreamReader(client);
                    var writer = new StreamWriter(client) { AutoFlush = true };
                    
                    // Send username to the server
                    await writer.WriteLineAsync(WindowsIdentity.GetCurrent().Name);
                    
                    // Start heartbeat in a separate task
                    _ = Task.Factory.StartNew(() => SendHeartbeats(writer), TaskCreationOptions.LongRunning);

                    // Listen for server messages
                    while (true)
                    {
                        Console.WriteLine("Reading incoming message...");
                        var message = await reader.ReadLineAsync();
                        if (message == null) break;

                        switch (message)
                        {
                            case "shutdown":
                                await Application.Current.Dispatcher.InvokeAsync(() => Application.Current.MainWindow?.Show());                                    
                                Process.Start("shutdown", "/s /f /t 300");
                                await writer.WriteLineAsync("OK");
                                break;
                            case "logoff":                                    
                                Process.Start("shutdown", "/l /t 5");
                                await writer.WriteLineAsync("OK");
                                break;
                            case string s when s.StartsWith("nextLesson_"):                                    
                                var subMessage = message.Substring(11);
                                await Application.Current.Dispatcher.InvokeAsync(() =>
                                {
                                    if (Application.Current.MainWindow is MainWindow window)
                                        window.NextLesson.Content = string.IsNullOrWhiteSpace(subMessage) ? "" : $"Nächste Stunde: {subMessage} Uhr";
                                });
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
