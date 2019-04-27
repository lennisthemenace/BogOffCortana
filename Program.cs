using System;
using System.Diagnostics;
using System.Management;
using System.Threading;

namespace BogOffCortana
{
    class Program
    {
        private static void KillProcessAndChildren(Int32 pid)
        {
            // Cannot close 'system idle process'.
            if (pid == 0)
            {
                //Log("Pid is 0, bail");
                return;
            }
            ManagementObjectSearcher searcher = new ManagementObjectSearcher
                ("Select * From Win32_Process Where ParentProcessID=" + pid);
            ManagementObjectCollection moc = searcher.Get();
            foreach (ManagementObject mo in moc)
            {
                KillProcessAndChildren(Convert.ToInt32(mo["ProcessID"]));
            }
            try
            {
                Process proc = Process.GetProcessById(pid);
                proc.Kill();
            }
            catch (Exception)
            {
                // Process already exited.
            }
        }
        static void Main(string[] args)
        {
            string[] boring = {"SkypeApp", "WinStore.App", "OneDrive", "SearchUI", "SkypeBackgroundHost", "Cortana" };
            string[] services = {"lfsvc", "DiagTrack" };
            bool lo = true;
            int count = 0;


            foreach (string service in services)
            {
                Console.WriteLine("Disabling : " + service);
                Process sc = Process.Start("sc.exe", "config " + service + " start= disabled");
                sc.WaitForExit();
                Process sc2 = Process.Start("sc.exe", "STOP " + service);
                sc2.WaitForExit();
            }
            Console.WriteLine("Finished Disabling Services");

            while (lo)
            {
                foreach (string breaker in boring)
                {
                    foreach (var process in Process.GetProcessesByName(breaker))
                    {
                        count += 1;
                        Console.WriteLine("Killing: " + breaker);
                        Console.WriteLine("Total Kills:" + count);
                        KillProcessAndChildren(process.Id);
                        
                    }
                }
                Thread.Sleep(1000);
            }
        }
    }
}
