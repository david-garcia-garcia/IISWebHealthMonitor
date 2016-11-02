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


namespace healthmonitorcore
{
    public class SiteInstance
    {
        public SiteInstance(Site site)
        {

            var app = site.Applications.First();
            var vdir = app.VirtualDirectories.First();

            Id = site.Id;
            Host = IISUtils.getMonitoringHostname(site.Id);
            Root = vdir.PhysicalPath; ;
            ApplicationPoolName = app.ApplicationPoolName;
            Name = site.Name;
        }

        public string Name { get; set; }

        public long Id { get; set; }

        public string Host { get; set; }

        public string Root { get; set; }

        public string ApplicationPoolName { get; set; }
    }
}
