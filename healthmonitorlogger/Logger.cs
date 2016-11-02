using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Security.Principal;
using System.IO;
using System.Net;

namespace healthmonitorlogger
{
    public class Logger
    {
        protected EventLog log;

        public Logger() {

            // None of these should have the SAME name
            // as the service otherwise you are screwed
            // when deploying.
            string sSource = "DOW Site monitor";
            string sLog = "DOW Site monitor";

            log = new EventLog(sLog);
            log.Source = sSource;

            if (!EventLog.SourceExists(sSource))
                EventLog.CreateEventSource(sSource, sLog);
        }

        /// <summary>
        /// Everything we log in this monitor application
        /// is an error.
        /// </summary>
        /// <param name="message"></param>
        public void LogError(string message)
        {
            log.WriteEntry(message, EventLogEntryType.Error);
        }

        public void LogInfo(string message)
        {
            log.WriteEntry(message, EventLogEntryType.Information);
        }

        public void LogWarning(string message)
        {
            log.WriteEntry(message, EventLogEntryType.Warning);
        }
    }
}
