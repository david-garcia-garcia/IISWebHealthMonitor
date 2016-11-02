using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Web.Administration;
using Microsoft.Web.Management;
using System.Security.Cryptography;

namespace phphealthmonitor
{
    public partial class WebHealthMonitor : ServiceBase
    {
        /// <summary>
        /// Debug mode.
        /// </summary>
        protected bool debug = false;

        /// <summary>
        /// Monitor instance.
        /// </summary>
        protected healthmonitorcore.Monitor monitor;

        /// <summary>
        /// Timer.
        /// </summary>
        protected System.Timers.Timer timer;

        /// <summary>
        /// Last time the monitor was reset.
        /// </summary>
        protected System.DateTime lastMonitorReset = DateTime.Now;

        /// <summary>
        /// The logger.
        /// </summary>
        protected healthmonitorlogger.Logger log;

        public WebHealthMonitor()
        {
            InitializeComponent();
        }

        protected string GetArgumentAt(int position, string[] args)
        {
            if (args.Length > position)
            {
                return args[position];
            }

            return null;
        }

        public void TestStartupAndStop(string[] args)
        {
            this.OnStart(args);
            Console.ReadLine();
            this.OnStop();
        }

        protected override void OnStart(string[] args)
        {
            log = new healthmonitorlogger.Logger();

            // 30 Seconds by default.
            var interval = 0;
            int.TryParse(GetArgumentAt(0, args), out interval);
            bool.TryParse(GetArgumentAt(1, args), out debug);

            if (interval == 0)
            {
                interval = 30;
            }

            monitor = new healthmonitorcore.Monitor();

            log.LogInfo(String.Format("Monitor started with interval of {0}s", interval));

            timer = new System.Timers.Timer();
            timer.Interval = interval * 1000;
            timer.Elapsed += new System.Timers.ElapsedEventHandler(this.OnTimer);
            timer.Start();

            log.LogInfo("Monitoring started");
        }

        protected override void OnStop()
        {
            timer.Stop();
            timer = null;
        }

        public void OnTimer(object sender, System.Timers.ElapsedEventArgs args)
        {
            // Stop the timer, so that the lapse
            // is counter from the next start not
            // from the previous tick.
            timer.Stop();

            // Reset the monitor every five hours.
            if ((DateTime.Now - lastMonitorReset).TotalHours > 5)
            {
                lastMonitorReset = DateTime.Now;
                monitor = new healthmonitorcore.Monitor();
                log.LogInfo("Monitor instance reset.");
            }

            try
            {
                monitor.doMonitoring();
            }
            catch (Exception ex)
            {
                // We might se exceptions if - for example - the IIS settings
                // have changed after monitor was instantiated. We need a new
                // instance to refresh those settings.
                monitor = new healthmonitorcore.Monitor();
                log.LogError(String.Format("Reseting monitor due to unhandled exception: {0}", ex.Message + Environment.NewLine + ex.StackTrace));
            }

            if (debug)
            {
                log.LogInfo("Health monitor check run");
            }

            // Restart the timer.
            timer.Start();
        }
    }
}
