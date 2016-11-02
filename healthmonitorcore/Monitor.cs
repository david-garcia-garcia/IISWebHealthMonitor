using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.Web.Administration;
using Microsoft.Web.Management;
using System.Security.Cryptography;
using System.Security.Principal;
using System.IO;
using System.Net;
using healthmonitorlogger;

namespace healthmonitorcore
{

    /// <summary>
    /// Implementation is a bit bizarre because
    /// we want only to use server manager when
    /// strictly necessary as this is a COM
    /// component.
    /// </summary>
    public class Monitor
    {
        /// <summary>
        /// Debug mode
        /// </summary>
        protected bool debug = false;

        protected Logger log;

        protected List<SiteInstance> sites;

        protected UtilsHostsFile hostsUtils;

        protected List<long> blacklist = new List<long>();

        public Monitor()
        {
            log = new Logger();

            if (!IsAdministrator())
            {
                log.LogError("The health monitoring tool must be run with administrator privileges.");
                return;
            }

            hostsUtils = new UtilsHostsFile();

            sites = new List<SiteInstance>();

            // Retrieve a list of active sites
            using (ServerManager manager = new ServerManager())
            {
                foreach (var s in manager.Sites)
                {
                    // Only add started sites... of course.
                    if (s.State == ObjectState.Started || s.State == ObjectState.Starting)
                    {
                        var i = new SiteInstance(s);
                        sites.Add(i);
                    }
                }
            }

            log.LogInfo(String.Format("Web health monitor started for {0} sites.", sites.Count));
        }

        public void doMonitoring()
        {
            if (!IsAdministrator())
            {
                log.LogError("The health monitoring tool must be run with administrator privileges.");
                return;
            }

            foreach (var site in sites)
            {
                monitorSitePHP(site);
            }
        }

        /// <summary>
        /// This is a specific check for a complete PHP hand
        /// where the php-process returns empty white pages
        /// with a 200OK response and 0 length, no mater
        /// what file you request.
        /// </summary>
        /// <param name="site">The site</param>
        /// <param name="host">The monitoring host</param>
        protected void monitorSitePHP(SiteInstance site)
        {
            // Do not monitor blacklisted sites.
            // Blacklist clears during a monitor reset,
            // that happens every handful of hours.
            if (blacklist.Contains(site.Id))
            {
                return;
            }

            var url = string.Format("http://{0}", site.Host);

            // Now let's try to load it.
            HttpWebResponse response = null;
            string html = null;

            var loaded = this.loadUrl(url, out response, out html);

            // If it was not loaded, try to reload.
            if (!loaded)
            {
                IISUtils.RestartSite(site.Id);
                log.LogWarning(String.Format("Could not load site, restarted and blacklisted: {0}", site.Name));
                blacklist.Add(site.Id);
                return;
            }

            // Check for a 500 status code.
            if (response.StatusCode == HttpStatusCode.InternalServerError)
            {
                IISUtils.RestartSite(site.Id);
                log.LogWarning(String.Format("Unresponsive php (Internal Server Error) restarted website: {0}", site.Name));
                return;
            }

            if (
                (response.StatusCode == HttpStatusCode.Found
                || response.StatusCode == HttpStatusCode.Moved
                || response.StatusCode == HttpStatusCode.MovedPermanently
                || response.StatusCode == HttpStatusCode.TemporaryRedirect
                )

                && response.Headers.AllKeys.Contains("Location"))
            {
                // Esto es un redirect... nada que hacer aquí.
                return;
            }

            // This is the BAD thing.... what PHP returns under weird lock
            // conditions.
            if (String.IsNullOrWhiteSpace(html) && response.ContentLength == 0)
            {
                IISUtils.RestartSite(site.Id);
                log.LogWarning(String.Format("Unresponsive php (Empty sample response) restarted website: {0}", site.Name));
                return;
            }

            // Check for a specific PHP fatal situation, some heuristics here.
            // Asume that responses with a length smaller than 250 characters
            // has a very high chance of being a PHP fatal, then look
            // for specific keywords.
            // Do this with care to prevent a specially crafted content
            // in the site to trigger an infinite boot.
            List<string> errors = new List<string>() {
                "T_ENCAPSED_AND_WHITESPACE",
                "T_STRING",
                "T_NUM_STRING",
                "T_VARIABLE",
                "<b>Fatal error</b>",
                "T_IF",
                "T_FOREACH"
            };

            // Truncate the response, PHP Fatals do not
            // output a full page (but sometimes the stack trace..)
            var truncated = html.Substring(0, html.Length > 5000 ? 5000 : html.Length);
            foreach (var e in errors)
            {
                if (truncated.Contains(e))
                {
                    IISUtils.RestartSite(site.Id);
                    log.LogWarning(String.Format("Unresponsive php (Fatal error) restarted website: {0}", site.Name));
                    return;

                }

            }
        }

        protected bool loadUrl(string url, out HttpWebResponse response, out string html)
        {
            response = null;
            html = null;

            try
            {
                // Now let's try to load it.
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.AllowAutoRedirect = false;

                try
                {
                    response = (HttpWebResponse)request.GetResponse();
                }
                catch (System.Net.WebException e)
                {
                    return false;
                }

                if (response != null)
                {
                    using (Stream data = response.GetResponseStream())
                    {

                        using (StreamReader sr = new StreamReader(data))
                        {
                            html = sr.ReadToEnd();
                        }
                    }
                }

                return true;

            }
            catch (Exception e)
            {
                return false;
            }
        }

        /// <summary>
        /// Write a text file in UTF8
        /// without BOM
        /// </summary>
        /// <param name="contents"></param>
        /// <param name="path"></param>
        protected void writeFileUtf8WithoutBom(string contents, string path)
        {
            var encoding = new UTF8Encoding(false);
            TextWriter file = new StreamWriter(path, false, encoding);
            file.Write(contents);
            file.Close();
        }



        /// <summary>
        /// Check if the current user is an administrator,
        /// otherwise many of the things here cannot be done.
        /// </summary>
        /// <returns></returns>
        protected bool IsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}
