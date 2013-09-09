using System;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.ServiceProcess;

namespace TestControlTool.ProcessKiller
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Please specify process to kill");
                
                return;
            }

            foreach (var process in Process.GetProcessesByName(args[0]))
            {
                KillProcessAndChildren(process.Id);
            }
        }

        public static void KillProcessAndChildren(int processId)
        {
            var searcher = new ManagementObjectSearcher("Select * From Win32_Process Where ParentProcessID=" + processId);
            var moc = searcher.Get();

            foreach (ManagementObject mo in moc)
            {
                KillProcessAndChildren(Convert.ToInt32(mo["ProcessID"]));
            }

            try
            {
                var process = Process.GetProcessById(processId);
                process.Kill();
            }
            catch (ArgumentException)
            {
                // Process already exited.
            }
        }
    }
}
