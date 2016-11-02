using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Web.Administration;
using System.IO;
using System.Security.AccessControl;
using System.Diagnostics;
using System.Security.Cryptography;

namespace healthmonitorcore
{
    class IISUtils
    {
        /// <summary>
        /// Find a site in a manager by name
        /// </summary>
        /// <param name="name"></param>
        /// <param name="manager"></param>
        /// <returns></returns>
        public static Site findSiteById(long Id, ServerManager manager)
        {
            return (from p in manager.Sites
                    where p.Id == Id
                    select p).FirstOrDefault();
        }

        public static ApplicationPool findPoolByName(string name, ServerManager manager)
        {
            return (from p in manager.ApplicationPools
                    where p.Name == name
                    select p).FirstOrDefault();
        }

        private static bool StopAppPool(ApplicationPool p, int timeout)
        {

            if (p.State == ObjectState.Stopped)
                return true;

            if (p.State != ObjectState.Started)
            {
                return false;
            }

            p.Stop();

            DateTime start = DateTime.Now;

            while (true)
            {
                if (p.State == ObjectState.Stopped)
                    return true;

                if ((DateTime.Now - start).TotalSeconds > timeout)
                {
                    return false;
                }

                System.Threading.Thread.Sleep(1000);
            }
        }

        private static bool StartAppPool(ApplicationPool p, int timeout)
        {

            if (p.State == ObjectState.Started)
                return true;

            if (p.State != ObjectState.Stopped)
            {
                return false;
            }

            p.Start();

            DateTime start = DateTime.Now;

            while (true)
            {
                if (p.State == ObjectState.Started)
                    return true;

                if ((DateTime.Now - start).TotalSeconds > timeout)
                {
                    return false;
                }

                System.Threading.Thread.Sleep(1000);
            }
        }

        private static bool StopSite(Site p, int timeout)
        {
            if (p.State == ObjectState.Stopped)
                return true;

            if (p.State != ObjectState.Started)
            {
                return false;
            }

            p.Stop();

            DateTime start = DateTime.Now;

            while (true)
            {
                if (p.State == ObjectState.Stopped)
                    return true;

                if ((DateTime.Now - start).TotalSeconds > timeout)
                {
                    return false;
                }

                System.Threading.Thread.Sleep(1000);
            }
        }

        private static bool StartSite(Site p, int timeout)
        {
            if (p.State == ObjectState.Started)
                return true;

            if (p.State != ObjectState.Stopped)
            {
                return false;
            }

            p.Start();

            DateTime start = DateTime.Now;

            while (true)
            {
                if (p.State == ObjectState.Started)
                    return true;

                if ((DateTime.Now - start).TotalSeconds > timeout)
                {
                    return false;
                }

                System.Threading.Thread.Sleep(1000);
            }
        }

        /// <summary>
        /// Restart a site in IIS.
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public static void RestartSite(long Id)
        {
            using (ServerManager manager = new ServerManager())
            {
                var site = findSiteById(Id, manager);

                if (site == null) {
                    return;
                }

                var app = site.Applications.First();
                var pool = findPoolByName(app.ApplicationPoolName, manager);

                // Who said this was easy....
                // severe hangs can lead to locks
                // that prevent stopping() both the
                // site and/or the application pool.

                // We first stop the site to prevent any new requests
                // from reaching the handlers/pool.
                //StopSite(site, 60);
                //StopAppPool(pool, 60);

                pool.Recycle();

                // Once stopped, starting should be "easy"
                //StartAppPool(pool, 60);
                //StartSite(site, 60);
            }
        }

        /// <summary>
        /// Get the hostname to be used to monitor
        /// this site. If none exists, it will be deployed
        /// as a local host.
        /// </summary>
        /// <param name="site"></param>
        public static string getMonitoringHostname(long Id)
        {
            var hostsUtils = new UtilsHostsFile();

            using (ServerManager manager = new ServerManager())
            {
                bool changed = false;

                var site = IISUtils.findSiteById(Id, manager);

                var ip = "127.0.0.1";
                var port = 80;

                // La raiz de la aplicación en realidad es una... aplicación con directorio virtual!
                var app = site.Applications.First();
                var vdir = app.VirtualDirectories.First();

                // We are going to create a virtual HOST
                // to do the monitoring. We use this
                var monitoringhost = String.Format("healthmonitor.{0}.local", CalculateMD5Hash(site.Name));

                // Get the list of IP addresses this site is binded to.
                Binding binding = null;

                // First look for the binding.
                foreach (var b in site.Bindings)
                {
                    if (b.Host == monitoringhost)
                    {
                        binding = b;
                        break;
                    }
                }

                // If there is no binding we need to create one.
                if (binding == null)
                {
                    var info = string.Join(":", ip, port, monitoringhost);
                    binding = site.Bindings.Add(info, "http");

                    changed = true;
                }

                // Make sure the binding is in the HOSTS file
                hostsUtils.AddHostsMapping(ip, monitoringhost);

                if (changed)
                {
                    manager.CommitChanges();
                }

                return monitoringhost;
            }
        }

        /// <summary>
        /// Calcula the MD5 has of a string
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string CalculateMD5Hash(string input)
        {
            // step 1, calculate MD5 hash from input
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }
    }
}
