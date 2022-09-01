using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace deployaUI.Common
{
    public static class ApplyDetails
    {
        public static string Name { get; set; }
        public static string FileName { get; set; }
        public static string IconPath { get; set; }
        public static int Index { get; set; }
        public static int DiskIndex { get; set; }
        public static bool UseEFI { get; set; }
        public static bool UseNTLDR { get; set; }
        public static bool UseRecovery { get; set; }
    }

    public static class DeploymentInfo
    {
        public static string Username { get; set; }
        public static string Password { get; set; }
        public static string CustomFilePath { get; set; }

        public static string PreConfigUserPass = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<unattend xmlns=""urn:schemas-microsoft-com:unattend"">
    <settings pass=""oobeSystem"">
        <component name=""Microsoft-Windows-Shell-Setup"" processorArchitecture=""amd64"" publicKeyToken=""31bf3856ad364e35"" language=""neutral"" versionScope=""nonSxS"" xmlns:wcm=""http://schemas.microsoft.com/WMIConfig/2002/State"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
            <UserAccounts>
                <LocalAccounts>
                    <LocalAccount wcm:action=""add"">
                        <Password>
                            <Value>{Password}</Value>
                            <PlainText>true</PlainText>
                        </Password>
                        <Name>{Username}</Name>
                        <Group>Administrators</Group>
                    </LocalAccount>
                </LocalAccounts>
            </UserAccounts>
        </component>
    </settings>
    <cpi:offlineImage cpi:source=""wim:e:/wims/win11-beta.wim#Windows 11 Pro"" xmlns:cpi=""urn:schemas-microsoft-com:cpi"" />
</unattend>";

        public static string PreConfigAdminPass = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<unattend xmlns=""urn:schemas-microsoft-com:unattend"">
    <settings pass=""oobeSystem"">
        <component name=""Microsoft-Windows-Shell-Setup"" processorArchitecture=""amd64"" publicKeyToken=""31bf3856ad364e35"" language=""neutral"" versionScope=""nonSxS"" xmlns:wcm=""http://schemas.microsoft.com/WMIConfig/2002/State"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
            <UserAccounts>
                <AdministratorPassword>
                    <Value>{Password}</Value>
                    <PlainText>true</PlainText>
                </AdministratorPassword>
            </UserAccounts>
            <AutoLogon>
                <Username>Administrator</Username>
                <Password>
                    <Value>{Password}</Value>
                    <PlainText>true</PlainText>
                </Password>
            </AutoLogon>
        </component>
    </settings>
    <cpi:offlineImage cpi:source=""wim:e:/wims/win11-beta.wim#Windows 11 Pro"" xmlns:cpi=""urn:schemas-microsoft-com:cpi"" />
</unattend>
";

        public static string PreConfigAdminWithoutPass = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<unattend xmlns=""urn:schemas-microsoft-com:unattend"">
    <settings pass=""oobeSystem"">
        <component name=""Microsoft-Windows-Shell-Setup"" processorArchitecture=""amd64"" publicKeyToken=""31bf3856ad364e35"" language=""neutral"" versionScope=""nonSxS"" xmlns:wcm=""http://schemas.microsoft.com/WMIConfig/2002/State"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
            <AutoLogon>
                <Username>Administrator</Username>
            </AutoLogon>
        </component>
    </settings>
    <cpi:offlineImage cpi:source=""wim:e:/wims/win11-beta.wim#Windows 11 Pro"" xmlns:cpi=""urn:schemas-microsoft-com:cpi"" />
</unattend>
";

    }


    public static class Debug
    {
        public static void WriteLine(string message, ConsoleColor color = ConsoleColor.White)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("[");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("*");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("] ");

            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
}
