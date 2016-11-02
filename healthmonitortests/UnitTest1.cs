using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Security.Cryptography;
using System.Security.Principal;
using System.IO;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using healthmonitorcore;
using phphealthmonitor;

namespace healthmonitortests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestBasic()
        {
            WebHealthMonitor service1 = new WebHealthMonitor();
            service1.TestStartupAndStop(new String[0]);

            var m = new healthmonitorcore.Monitor();
            m.doMonitoring();
        }


    }
}
