using System;
using System.Management;
using System.Threading.Tasks;

namespace Dive.UI.Common.USMT
{
    class WMI
    {
        /// <summary>
        /// Runs a WMI query on a remote host.
        /// </summary>
        /// <param name="queryString">Query to run.</param>
        /// <param name="host">Host to run the query on.</param>
        /// <returns>A task with results.</returns>
        public static ManagementObjectCollection Query(string queryString, string host)
        {
            try
            {
                var scope = new ManagementScope(GetBestManagementScope(host));
                scope.Connect();
                var query = new ObjectQuery(queryString);
                var searcher = new ManagementObjectSearcher(scope, query);
                var collection = searcher.Get();
                searcher.Dispose();
                return collection;
            }
            catch (Exception ex)
            {
                HandleWmiException(ex);
                return null;
            }
        }

        /// <summary>
        /// Checks if the host provided is the current machine.
        /// </summary>
        /// <param name="host">Hostname to check against.</param>
        /// <returns>True if is this machine, false if otherwise.</returns>
        public static bool IsHostThisMachine(string host)
        {
            if (host == "127.0.0.1" || host == "::1" || host.ToLower() == "localhost" || host == ".") return true;
            if (host.Equals(Environment.MachineName, StringComparison.CurrentCultureIgnoreCase))
            {
                return true;
            }
            if (host.Split('.').Length > 0 && host.Split('.')[0].ToLower() == Environment.MachineName.ToLower())
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns the best path to "ManagementScope" based on the host provided. (If the host provided is the current host then the best path is "\root\cimv2" otherwise it is "\\HOST\root\cimv2".)
        /// </summary>
        /// <param name="host">Host to check against.</param>
        /// <returns>Either "\root\cimv2" or "\\HOST\root\cimv2".</returns>
        public static string GetBestManagementScope(string host)
        {
            if (IsHostThisMachine(host))
            {
                return @"\root\cimv2";
            }
            else
            {
                return @"\\" + host + @"\root\cimv2";
            }
        }

        /// <summary>
        /// Gets the instance of a management object from a remote host.
        /// </summary>
        /// <param name="host">Host to get object from.</param>
        /// <param name="instancePath">Instance path to use.</param>
        /// <returns>A task and management object.</returns>
        public static ManagementObject GetInstance(string host, string instancePath)
        {
            try
            {
                var mo = new ManagementObject(GetBestManagementScope(host), instancePath, new ObjectGetOptions(null, TimeSpan.FromSeconds(30), true));
                mo.Get();
                return mo;
            }
            catch (Exception ex)
            {
                HandleWmiException(ex);
                return null;
            }
        }

        public static void HandleWmiException(Exception ex)
        {
            throw ex.HResult switch
            {
                // RPC server is unavailable.
                -2147023174 => new Exception("RPC Server is unavailable.", ex),
                // Access is denied.
                -2147024891 => new Exception("Access is denied.", ex),
                // WMI service is disabled.
                -2147023838 => new Exception("WMI service is disabled.", ex),
                _ => new Exception("", ex)
            };
        }
    }
}
