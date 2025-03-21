using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace WpfClient.Services
{
    public class AfkService
    {
        
        [DllImport("User32.dll")] 
        private static extern bool GetLastInputInfo(ref Lastinputinfo lastInput); 


        [StructLayout(LayoutKind.Sequential)]
        private struct Lastinputinfo 
        { 
            public Int32 cbSize; 
            public Int32 dwTime; 
        }

        private static int IdleTime 
        { 
            get 
            { 
                var lastInput = new Lastinputinfo(); 
                lastInput.cbSize = Marshal.SizeOf(lastInput); 

                var idle = 0;

                if (GetLastInputInfo(ref lastInput)) 
                    idle = Environment.TickCount - lastInput.dwTime;

                return idle; 
            } 
        }
        
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool ExitWindowsEx(uint uFlags, uint dwReason);
        
        private static bool _running;
        
        public static async Task StartAsync(int timeout)
        {
            if (_running)
                return;
            
            _running = true;
            while (true)
            {
                if (IdleTime >= timeout)
                {
                    ExitWindowsEx(4U, 0); // force logoff
                    await Task.Delay(180000); // 3 minutes
                }
                else
                    await Task.Delay(timeout - IdleTime);
            }
        }
    }
}