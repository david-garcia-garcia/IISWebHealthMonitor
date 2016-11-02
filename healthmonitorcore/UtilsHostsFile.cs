using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace healthmonitorcore
{
    public class UtilsHostsFile
    {
        /// <summary>
        /// Devuelve la ubicación del fichero HOSTS
        /// </summary>
        private string HostsFile
        {
            get
            {
                return Environment.SystemDirectory + @"\drivers\etc\hosts";
            }
        }

        /// <summary>
        /// Devuelve el fichero de hosts como un conjunto de lineas
        /// </summary>
        /// <returns></returns>
        private List<string> GetHosts()
        {
            var content = System.IO.File.ReadAllText(HostsFile);
            return System.Text.RegularExpressions.Regex.Split(content, Environment.NewLine).ToList();
        }

        /// <summary>
        /// Añade un mapping al fichero hosts, o lo actualiza en función del hostname.
        /// Evita añadir duplicados o conflictivos.
        /// </summary>
        /// <param name="Address"></param>
        /// <param name="Hostname"></param>
        public void AddHostsMapping(string Address, string Hostname)
        {
            var lines = GetHosts();
            var line = Address + " " + Hostname;
            var found = false;

            // First look for the HOST name
            for (int x = 0; x < lines.Count(); x++)
            {
                // Pasamos d elos comentarios.
                if (lines[x].Trim().StartsWith("#"))
                {
                    continue;
                }

                // Si no tiene exactamente dos partes, es que hay algo raro....
                var items = lines[x].Split(" ".ToCharArray());
                if (items.Count() != 2)
                {
                    continue;
                }

                // Lo que hace que dos entradas sean iguales es que tengan
                // el mismo hostname.
                if (String.Equals(items.Last(), Hostname, StringComparison.InvariantCultureIgnoreCase))
                {
                    lines[x] = line;
                    found = true;
                    break;
                }
            }

            // If not found we need to add it
            if (!found)
            {
                lines.Add("# Automatic health monitoring hosts binding");
                lines.Add(line);
                File.WriteAllLines(HostsFile, lines, Encoding.Default);
            }
        }
    }
}
