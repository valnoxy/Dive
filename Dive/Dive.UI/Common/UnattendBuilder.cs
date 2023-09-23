using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace Dive.UI.Common
{
    public class UnattendXmlClass
    {
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:schemas-microsoft-com:unattend")]
        [System.Xml.Serialization.XmlRootAttribute(Namespace = "urn:schemas-microsoft-com:unattend", IsNullable = false)]
        public partial class unattend
        {

            private unattendSettings[] settingsField;

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("settings")]
            public unattendSettings[] settings
            {
                get
                {
                    return this.settingsField;
                }
                set
                {
                    this.settingsField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:schemas-microsoft-com:unattend")]
        public partial class unattendSettings
        {

            private unattendSettingsComponent componentField;

            private string passField;

            /// <remarks/>
            public unattendSettingsComponent component
            {
                get
                {
                    return this.componentField;
                }
                set
                {
                    this.componentField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string pass
            {
                get
                {
                    return this.passField;
                }
                set
                {
                    this.passField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:schemas-microsoft-com:unattend")]
        public partial class unattendSettingsComponent
        {

            private bool copyProfileField;

            private bool copyProfileFieldSpecified;

            private byte skuPolicyRequiredField;

            private bool skuPolicyRequiredFieldSpecified;

            private unattendSettingsComponentOEMInformation oEMInformationField;

            private unattendSettingsComponentAutoLogon autoLogonField;

            private unattendSettingsComponentUserAccounts userAccountsField;

            private unattendSettingsComponentSynchronousCommand[] firstLogonCommandsField;

            private string nameField;

            private string processorArchitectureField;

            private string publicKeyTokenField;

            private string languageField;

            private string versionScopeField;

            /// <remarks/>
            public bool CopyProfile
            {
                get
                {
                    return this.copyProfileField;
                }
                set
                {
                    this.copyProfileField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlIgnoreAttribute()]
            public bool CopyProfileSpecified
            {
                get
                {
                    return this.copyProfileFieldSpecified;
                }
                set
                {
                    this.copyProfileFieldSpecified = value;
                }
            }

            /// <remarks/>
            public byte SkuPolicyRequired
            {
                get
                {
                    return this.skuPolicyRequiredField;
                }
                set
                {
                    this.skuPolicyRequiredField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlIgnoreAttribute()]
            public bool SkuPolicyRequiredSpecified
            {
                get
                {
                    return this.skuPolicyRequiredFieldSpecified;
                }
                set
                {
                    this.skuPolicyRequiredFieldSpecified = value;
                }
            }

            /// <remarks/>
            public unattendSettingsComponentOEMInformation OEMInformation
            {
                get
                {
                    return this.oEMInformationField;
                }
                set
                {
                    this.oEMInformationField = value;
                }
            }

            /// <remarks/>
            public unattendSettingsComponentAutoLogon AutoLogon
            {
                get
                {
                    return this.autoLogonField;
                }
                set
                {
                    this.autoLogonField = value;
                }
            }

            /// <remarks/>
            public unattendSettingsComponentUserAccounts UserAccounts
            {
                get
                {
                    return this.userAccountsField;
                }
                set
                {
                    this.userAccountsField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlArrayItemAttribute("SynchronousCommand", IsNullable = false)]
            public unattendSettingsComponentSynchronousCommand[] FirstLogonCommands
            {
                get
                {
                    return this.firstLogonCommandsField;
                }
                set
                {
                    this.firstLogonCommandsField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string name
            {
                get
                {
                    return this.nameField;
                }
                set
                {
                    this.nameField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string processorArchitecture
            {
                get
                {
                    return this.processorArchitectureField;
                }
                set
                {
                    this.processorArchitectureField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string publicKeyToken
            {
                get
                {
                    return this.publicKeyTokenField;
                }
                set
                {
                    this.publicKeyTokenField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string language
            {
                get
                {
                    return this.languageField;
                }
                set
                {
                    this.languageField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string versionScope
            {
                get
                {
                    return this.versionScopeField;
                }
                set
                {
                    this.versionScopeField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:schemas-microsoft-com:unattend")]
        public partial class unattendSettingsComponentOEMInformation
        {

            private string logoField;

            private string manufacturerField;

            private string modelField;

            private string supportHoursField;

            private string supportPhoneField;

            private string supportURLField;

            /// <remarks/>
            public string Logo
            {
                get
                {
                    return this.logoField;
                }
                set
                {
                    this.logoField = value;
                }
            }

            /// <remarks/>
            public string Manufacturer
            {
                get
                {
                    return this.manufacturerField;
                }
                set
                {
                    this.manufacturerField = value;
                }
            }

            /// <remarks/>
            public string Model
            {
                get
                {
                    return this.modelField;
                }
                set
                {
                    this.modelField = value;
                }
            }

            /// <remarks/>
            public string SupportHours
            {
                get
                {
                    return this.supportHoursField;
                }
                set
                {
                    this.supportHoursField = value;
                }
            }

            /// <remarks/>
            public string SupportPhone
            {
                get
                {
                    return this.supportPhoneField;
                }
                set
                {
                    this.supportPhoneField = value;
                }
            }

            /// <remarks/>
            public string SupportURL
            {
                get
                {
                    return this.supportURLField;
                }
                set
                {
                    this.supportURLField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:schemas-microsoft-com:unattend")]
        public partial class unattendSettingsComponentAutoLogon
        {

            private unattendSettingsComponentAutoLogonPassword passwordField;

            private byte logonCountField;

            private bool enabledField;

            private string usernameField;

            /// <remarks/>
            public unattendSettingsComponentAutoLogonPassword Password
            {
                get
                {
                    return this.passwordField;
                }
                set
                {
                    this.passwordField = value;
                }
            }

            /// <remarks/>
            public byte LogonCount
            {
                get
                {
                    return this.logonCountField;
                }
                set
                {
                    this.logonCountField = value;
                }
            }

            /// <remarks/>
            public bool Enabled
            {
                get
                {
                    return this.enabledField;
                }
                set
                {
                    this.enabledField = value;
                }
            }

            /// <remarks/>
            public string Username
            {
                get
                {
                    return this.usernameField;
                }
                set
                {
                    this.usernameField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:schemas-microsoft-com:unattend")]
        public partial class unattendSettingsComponentAutoLogonPassword
        {

            private string valueField;

            private bool plainTextField;

            /// <remarks/>
            public string Value
            {
                get
                {
                    return this.valueField;
                }
                set
                {
                    this.valueField = value;
                }
            }

            /// <remarks/>
            public bool PlainText
            {
                get
                {
                    return this.plainTextField;
                }
                set
                {
                    this.plainTextField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:schemas-microsoft-com:unattend")]
        public partial class unattendSettingsComponentUserAccounts
        {

            private unattendSettingsComponentUserAccountsAdministratorPassword administratorPasswordField;

            private unattendSettingsComponentUserAccountsLocalAccounts localAccountsField;

            /// <remarks/>
            public unattendSettingsComponentUserAccountsAdministratorPassword AdministratorPassword
            {
                get
                {
                    return this.administratorPasswordField;
                }
                set
                {
                    this.administratorPasswordField = value;
                }
            }

            /// <remarks/>
            public unattendSettingsComponentUserAccountsLocalAccounts LocalAccounts
            {
                get
                {
                    return this.localAccountsField;
                }
                set
                {
                    this.localAccountsField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:schemas-microsoft-com:unattend")]
        public partial class unattendSettingsComponentUserAccountsAdministratorPassword
        {

            private string valueField;

            private bool plainTextField;

            /// <remarks/>
            public string Value
            {
                get
                {
                    return this.valueField;
                }
                set
                {
                    this.valueField = value;
                }
            }

            /// <remarks/>
            public bool PlainText
            {
                get
                {
                    return this.plainTextField;
                }
                set
                {
                    this.plainTextField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:schemas-microsoft-com:unattend")]
        public partial class unattendSettingsComponentUserAccountsLocalAccounts
        {

            private unattendSettingsComponentUserAccountsLocalAccountsLocalAccount localAccountField;

            /// <remarks/>
            public unattendSettingsComponentUserAccountsLocalAccountsLocalAccount LocalAccount
            {
                get
                {
                    return this.localAccountField;
                }
                set
                {
                    this.localAccountField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:schemas-microsoft-com:unattend")]
        public partial class unattendSettingsComponentUserAccountsLocalAccountsLocalAccount
        {

            private unattendSettingsComponentUserAccountsLocalAccountsLocalAccountPassword passwordField;

            private string nameField;

            private string groupField;

            private string actionField;

            /// <remarks/>
            public unattendSettingsComponentUserAccountsLocalAccountsLocalAccountPassword Password
            {
                get
                {
                    return this.passwordField;
                }
                set
                {
                    this.passwordField = value;
                }
            }

            /// <remarks/>
            public string Name
            {
                get
                {
                    return this.nameField;
                }
                set
                {
                    this.nameField = value;
                }
            }

            /// <remarks/>
            public string Group
            {
                get
                {
                    return this.groupField;
                }
                set
                {
                    this.groupField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified, Namespace = "http://schemas.microsoft.com/WMIConfig/2002/State")]
            public string action
            {
                get
                {
                    return this.actionField;
                }
                set
                {
                    this.actionField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:schemas-microsoft-com:unattend")]
        public partial class unattendSettingsComponentUserAccountsLocalAccountsLocalAccountPassword
        {

            private string valueField;

            private bool plainTextField;

            /// <remarks/>
            public string Value
            {
                get
                {
                    return this.valueField;
                }
                set
                {
                    this.valueField = value;
                }
            }

            /// <remarks/>
            public bool PlainText
            {
                get
                {
                    return this.plainTextField;
                }
                set
                {
                    this.plainTextField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:schemas-microsoft-com:unattend")]
        public partial class unattendSettingsComponentSynchronousCommand
        {

            private int orderField;

            private bool requiresUserInputField;

            private string commandLineField;

            private string descriptionField;

            private string actionField;

            /// <remarks/>
            public int Order
            {
                get
                {
                    return this.orderField;
                }
                set
                {
                    this.orderField = value;
                }
            }

            /// <remarks/>
            public bool RequiresUserInput
            {
                get
                {
                    return this.requiresUserInputField;
                }
                set
                {
                    this.requiresUserInputField = value;
                }
            }

            /// <remarks/>
            public string CommandLine
            {
                get
                {
                    return this.commandLineField;
                }
                set
                {
                    this.commandLineField = value;
                }
            }

            /// <remarks/>
            public string Description
            {
                get
                {
                    return this.descriptionField;
                }
                set
                {
                    this.descriptionField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified, Namespace = "http://schemas.microsoft.com/WMIConfig/2002/State")]
            public string action
            {
                get
                {
                    return this.actionField;
                }
                set
                {
                    this.actionField = value;
                }
            }
        }
    }

    public class UnattendBuilder
    {
        public static string Build(Common.UnattendMode? mode)
        {
            Common.Debug.WriteLine($"[UnattendBuilder] Entering Build process with mode {mode.Value} ...");

            var synchronousCommands = new List<UnattendXmlClass.unattendSettingsComponentSynchronousCommand>();
            var synchronousCommandCount = 0;

            var uc = new UnattendXmlClass.unattend
            {
                settings = new UnattendXmlClass.unattendSettings[3]
            };
            uc.settings[0] = new UnattendXmlClass.unattendSettings
            {
                pass = "oobeSystem",
                component = new UnattendXmlClass.unattendSettingsComponent
                {
                    name = "Microsoft-Windows-Shell-Setup",
                    processorArchitecture = "amd64",
                    publicKeyToken = "31bf3856ad364e35",
                    language = "neutral",
                    versionScope = "nonSxS"
                }
            };

            // Auto Logon for Administrator account
            if (mode is Common.UnattendMode.Admin 
                or Common.UnattendMode.AdminWithoutOem 
                or Common.UnattendMode.AdminWithoutPassword 
                or Common.UnattendMode.AdminWithoutPasswordAndOem)
            {
                Common.Debug.WriteLine("[UnattendBuilder] AutoLogon for Admin account");
                uc.settings[0].component.AutoLogon = new UnattendXmlClass.unattendSettingsComponentAutoLogon
                {
                    Enabled = true,
                    LogonCount = 1,
                    Username = "Administrator"
                };

                if (mode is not (Common.UnattendMode.AdminWithoutPassword and Common.UnattendMode.AdminWithoutPasswordAndOem))
                {
                    Common.Debug.WriteLine("[UnattendBuilder] Define Admin password");
                    uc.settings[0].component.AutoLogon.Password = new UnattendXmlClass.unattendSettingsComponentAutoLogonPassword
                    {
                        Value = Common.DeploymentInfo.Password,
                        PlainText = true
                    };
                }

            }

            // User Account
            if (mode != Common.UnattendMode.OnlyOem)
            {
                Common.Debug.WriteLine("[UnattendBuilder] User Account creation");
                uc.settings[0].component.UserAccounts = new UnattendXmlClass.unattendSettingsComponentUserAccounts();

                switch (mode)
                {
                    // Administrator Password
                    case Common.UnattendMode.Admin:
                    case Common.UnattendMode.AdminWithoutOem:
                        Common.Debug.WriteLine("[UnattendBuilder] Administrator Password");
                        uc.settings[0].component.UserAccounts.AdministratorPassword =
                            new UnattendXmlClass.unattendSettingsComponentUserAccountsAdministratorPassword
                            {
                                Value = Common.DeploymentInfo.Password,
                                PlainText = true
                            };
                        break;

                    // Local account
                    case Common.UnattendMode.User:
                    case Common.UnattendMode.UserWithoutOem:
                    case Common.UnattendMode.UserWithoutPassword:
                    case Common.UnattendMode.UserWithoutPasswordAndOem:
                    {
                        Common.Debug.WriteLine("[UnattendBuilder] Local account");
                        uc.settings[0].component.UserAccounts.LocalAccounts =
                            new UnattendXmlClass.unattendSettingsComponentUserAccountsLocalAccounts
                            {
                                LocalAccount = new UnattendXmlClass.unattendSettingsComponentUserAccountsLocalAccountsLocalAccount
                                {
                                    action = "add"
                                }
                            };

                        // Password
                        if (mode is not (Common.UnattendMode.UserWithoutPassword and Common.UnattendMode.UserWithoutPasswordAndOem))
                        {
                            Common.Debug.WriteLine("[UnattendBuilder] Local Account Password");
                            uc.settings[0].component.UserAccounts.LocalAccounts.LocalAccount.Password =
                                new UnattendXmlClass.unattendSettingsComponentUserAccountsLocalAccountsLocalAccountPassword
                                {
                                    Value = Common.DeploymentInfo.Password,
                                    PlainText = true
                                };
                        }

                        uc.settings[0].component.UserAccounts.LocalAccounts.LocalAccount.Name = Common.DeploymentInfo.Username;
                        uc.settings[0].component.UserAccounts.LocalAccounts.LocalAccount.Group = "Administrators";

                        // Add FirstLogonCommand for PasswordExpires=false
                        synchronousCommandCount++;
                        synchronousCommands.Add(new UnattendXmlClass.unattendSettingsComponentSynchronousCommand
                        {
                            action = "add",
                            Order = synchronousCommandCount,
                            RequiresUserInput = false,
                            CommandLine = $"cmd /C wmic useraccount where name=\"{Common.DeploymentInfo.Username}\" set PasswordExpires=false",
                            Description = "Password never expires"
                        });
                        break;
                    }
                }
            }

            var oemLogo = "";
            if (!string.IsNullOrEmpty(Common.OemInfo.LogoPath))
            {
                Common.Debug.WriteLine("[UnattendBuilder] OEM Logo is not Null or Empty: " + Common.OemInfo.LogoPath);
                oemLogo = "%WINDIR%\\System32\\logo.bmp";
            }

            // OEM Information
            if (mode is UnattendMode.Admin 
                or UnattendMode.AdminWithoutPassword 
                or UnattendMode.User 
                or UnattendMode.UserWithoutPassword
                or UnattendMode.OnlyOem)
            {
                Common.Debug.WriteLine("[UnattendBuilder] OEM Information");
                if (string.IsNullOrEmpty(Common.OemInfo.LogoPath))
                {
                    uc.settings[0].component.OEMInformation = new UnattendXmlClass.unattendSettingsComponentOEMInformation
                    {
                        Manufacturer = Common.OemInfo.Manufacturer,
                        Model = Common.OemInfo.Model,
                        SupportHours = Common.OemInfo.SupportHours,
                        SupportPhone = Common.OemInfo.SupportPhone,
                        SupportURL = Common.OemInfo.SupportURL
                    };
                }
                else
                {
                    uc.settings[0].component.OEMInformation = new UnattendXmlClass.unattendSettingsComponentOEMInformation
                    {
                        Logo = oemLogo,
                        Manufacturer = Common.OemInfo.Manufacturer,
                        Model = Common.OemInfo.Model,
                        SupportHours = Common.OemInfo.SupportHours,
                        SupportPhone = Common.OemInfo.SupportPhone,
                        SupportURL = Common.OemInfo.SupportURL
                    };
                }
            }

            // Add FirstLogonCommands list if synchronousCommands is not empty
            if (synchronousCommands.Count > 0)
            {
                uc.settings[0].component.FirstLogonCommands = synchronousCommands.ToArray();
            }

            // S-Mode (Windows 10 1709 and up)
            if (Common.DeploymentOption.UseSMode)
            {
                Common.Debug.WriteLine("[UnattendBuilder] Using S Mode");
                uc.settings[1] = new UnattendXmlClass.unattendSettings
                {
                    pass = "offlineServicing",
                    component = new UnattendXmlClass.unattendSettingsComponent
                    {
                        name = "Microsoft-Windows-CodeIntegrity",
                        processorArchitecture = "amd64",
                        publicKeyToken = "31bf3856ad364e35",
                        language = "neutral",
                        versionScope = "nonSxS",
                        SkuPolicyRequired = 1,
                        SkuPolicyRequiredSpecified = true
                    }
                };
            }

            // Copy Profile (only for syspreped user profiles)
            if (Common.DeploymentOption.UseCopyProfile)
            {
                Common.Debug.WriteLine("[UnattendBuilder] Using CopyProfile");
                var nextIndex = Common.DeploymentOption.UseSMode ? 2 : 1;
                uc.settings[nextIndex] = new UnattendXmlClass.unattendSettings
                {
                    pass = "specialize",
                    component = new UnattendXmlClass.unattendSettingsComponent
                    {
                        name = "Microsoft-Windows-Shell-Setup",
                        processorArchitecture = "amd64",
                        publicKeyToken = "31bf3856ad364e35",
                        language = "neutral",
                        versionScope = "nonSxS",
                        CopyProfile = true,
                        CopyProfileSpecified = true
                    }
                };
            }

            var ns = new XmlSerializerNamespaces();
            ns.Add("", "");

            // Create the serializer
            var slz = new XmlSerializer(typeof(UnattendXmlClass.unattend), "urn:schemas-microsoft-com:unattend");

            using StringWriter textWriter = new Utf8StringWriter();
            slz.Serialize(textWriter, uc, ns);

            Common.Debug.WriteLine("[UnattendBuilder] Building completed.");
            return textWriter.ToString();
        }
        
        public class Utf8StringWriter : StringWriter
        {
            public override Encoding Encoding => Encoding.UTF8;
        }
    }
}