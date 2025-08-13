using System;
using System.Collections.Generic;
using System.IO;
using System.Management;
using static System.Text.RegularExpressions.Regex;

namespace Dive.UI.Common.USMT
{
    public class RetrieveUsers
    {
        public class User
        {
            public string Username { get; set; }
            public string LastModified { get; set; }
            public string FirstCreated { get; set; }
            public string SID { get; set; }
            public string ProfilePath { get; set; }
        }

        // Temporary variables
        public const string MigrationStorePath = "C:\\Dive\\Store";
        public const bool HideBuiltInAccounts = true;
        public const bool HideUnknownSIDs = true;

        public static List<User?>? GetUsersFromHost(string host)
        {
            var users = new List<User?>();
            var manObjCol = WMI.Query("SELECT SID, LocalPath FROM Win32_UserProfile", host);
            foreach (var man in manObjCol)
            {
                var user = GetUserFromHost(host, man);
                if (user != null)
                    users.Add(user);
            }
            return users;
        }

        private static User? GetUserFromHost(string host, ManagementBaseObject userObject)
        {
            var sid = userObject.GetPropertyValue("SID").ToString();
            var username = GetUserByIdentity(sid!, host);

            if (HideBuiltInAccounts && (IsMatch(sid!, @"^S-1-5-[0-9]+$")))
            {
                return null;
            }
            if (HideUnknownSIDs && (sid == username || string.IsNullOrEmpty(username)))
            {
                return null;
            }

            var firstCreated = string.Empty;
            var lastModified = string.Empty;
            var profilePath = userObject.GetPropertyValue("LocalPath").ToString();
            if (profilePath != null)
            {
                firstCreated = Directory.GetCreationTime(profilePath).ToFileTime().ToString();
                lastModified = File.GetLastWriteTime(Path.Combine(profilePath, "NTUSER.DAT")).ToFileTime().ToString();
            }

            return new User
                {
                    SID = sid!,
                    ProfilePath = profilePath!,
                    FirstCreated = firstCreated,
                    LastModified = lastModified,
                    Username = username
                };

        }

        /// <summary>
        /// Attempts to resolve an identity (Store ID / Security ID) to a DOMAIN\USERNAME string. If failed, returns the given identity string.
        /// </summary>
        /// <param name="id">A Store ID or SID (Security Identifier) to resolve.</param>
        /// <param name="host">If this parameter is specified, then this method will treat the ID parameter as an SID and resolve it on the remote host.</param>
        /// <returns>DOMAIN\USERNAME.</returns>
        public static string GetUserByIdentity(string id, string? host = null)
        {
            if (host != null)
            {
                try
                {
                    var mo = WMI.GetInstance(host, "Win32_SID.SID=\"" + id + "\"");
                    var ntDomain = mo.GetPropertyValue("ReferencedDomainName").ToString();
                    var ntAccountName = mo.GetPropertyValue("AccountName").ToString();
                    var ntAccount = ntDomain + "\\" + ntAccountName;
                    if (ntDomain == "" || ntAccountName == "") throw new Exception();
                    return ntAccount;
                }
                catch (Exception)
                {
                    return id;
                }
            }
            else
            {
                var fNtAccount = Path.Combine(MigrationStorePath, id, "ntaccount");
                if (!File.Exists(fNtAccount)) return id;
                
                var ntAccount = File.ReadAllText(fNtAccount);
                return ntAccount != "" ? ntAccount : id;
            }
        }

    }
}
