using System;
using System.ServiceProcess;
using System.Configuration.Install;
using System.Reflection;
using System.Diagnostics;


namespace phphealthmonitor
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            Console.WriteLine("Done");

            if (Environment.UserInteractive)
            {
                string parameter = string.Concat(args);
                Console.WriteLine("arguments: " + parameter);
                switch (parameter)
                {
                    case "--install":
                        ManagedInstallerClass.InstallHelper(new[] { Assembly.GetExecutingAssembly().Location });
                        Console.WriteLine("Install successful");
                        break;
                    case "--uninstall":
                        ManagedInstallerClass.InstallHelper(new[] { "/u", Assembly.GetExecutingAssembly().Location });
                        Console.WriteLine("Uninstall successful");
                        break;
                    case "--run":
                        var monitor = new healthmonitorcore.Monitor();
                        monitor.doMonitoring();
                        break;
                    case "--test":
                        WebHealthMonitor service1 = new WebHealthMonitor();
                        service1.TestStartupAndStop(args);
                        break;
                }
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                new WebHealthMonitor()
                };
                ServiceBase.Run(ServicesToRun);
            }


        }
    }
}
