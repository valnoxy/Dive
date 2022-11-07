using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace deployaUI.Common
{
    public class UnattendXmlClass
    {
        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:schemas-microsoft-com:unattend")]
        [System.Xml.Serialization.XmlRootAttribute(Namespace = "urn:schemas-microsoft-com:unattend", IsNullable = false)]
        public partial class unattend
        {

            private unattendSettings settingsField;

            /// <remarks/>
            public unattendSettings settings
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

            private unattendSettingsComponentAutoLogon autoLogonField;

            private unattendSettingsComponentOEMInformation oEMInformationField;

            private unattendSettingsComponentUserAccounts userAccountsField;

            private string nameField;

            private string processorArchitectureField;

            private string publicKeyTokenField;

            private string languageField;

            private string versionScopeField;

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
        public partial class unattendSettingsComponentAutoLogon
        {

            private bool enabledField;

            private byte logonCountField;

            private string usernameField;

            private unattendSettingsComponentAutoLogonPassword passwordField;

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
    }

    public class UnattendBuilder
    {
        public static string Build(Common.UnattendMode? mode)
        {
            var uc = new UnattendXmlClass.unattend();

            switch (mode)
            {
                case Common.UnattendMode.Admin:
                    uc = new UnattendXmlClass.unattend
                    {
                        settings = new UnattendXmlClass.unattendSettings
                        {
                            pass = "oobeSystem",
                            component = new UnattendXmlClass.unattendSettingsComponent
                            {
                                name = "Microsoft-Windows-Shell-Setup",
                                processorArchitecture = "amd64",
                                publicKeyToken = "31bf3856ad364e35",
                                language = "neutral",
                                versionScope = "nonSxS",
                                // AutoLogin
                                AutoLogon = new UnattendXmlClass.unattendSettingsComponentAutoLogon
                                {
                                    Enabled = true,
                                    LogonCount = 1,
                                    Username = "Administrator",
                                    Password = new UnattendXmlClass.unattendSettingsComponentAutoLogonPassword
                                    {
                                        Value = Common.DeploymentInfo.Password,
                                        PlainText = true
                                    }
                                },
                                // Oem Information
                                OEMInformation = new UnattendXmlClass.unattendSettingsComponentOEMInformation
                                {
                                    Logo = "C:\\Windows\\System32\\logo.bmp",
                                    Manufacturer = Common.OemInfo.Manufacturer,
                                    Model = Common.OemInfo.Model,
                                    SupportHours = Common.OemInfo.SupportHours,
                                    SupportPhone = Common.OemInfo.SupportPhone,
                                    SupportURL = Common.OemInfo.SupportURL
                                },
                                // Administrator Password
                                UserAccounts = new UnattendXmlClass.unattendSettingsComponentUserAccounts
                                {
                                    AdministratorPassword = new UnattendXmlClass.unattendSettingsComponentUserAccountsAdministratorPassword
                                    {
                                        Value = Common.DeploymentInfo.Password,
                                        PlainText = true
                                    }
                                }
                            }
                        }
                    };
                    break;
                case UnattendMode.AdminWithoutOem:
                    uc = new UnattendXmlClass.unattend
                    {
                        settings = new UnattendXmlClass.unattendSettings
                        {
                            pass = "oobeSystem",
                            component = new UnattendXmlClass.unattendSettingsComponent
                            {
                                name = "Microsoft-Windows-Shell-Setup",
                                processorArchitecture = "amd64",
                                publicKeyToken = "31bf3856ad364e35",
                                language = "neutral",
                                versionScope = "nonSxS",
                                // AutoLogin
                                AutoLogon = new UnattendXmlClass.unattendSettingsComponentAutoLogon
                                {
                                    Enabled = true,
                                    LogonCount = 1,
                                    Username = "Administrator",
                                    Password = new UnattendXmlClass.unattendSettingsComponentAutoLogonPassword
                                    {
                                        Value = Common.DeploymentInfo.Password,
                                        PlainText = true
                                    }
                                },
                                // Administrator Password
                                UserAccounts = new UnattendXmlClass.unattendSettingsComponentUserAccounts
                                {
                                    AdministratorPassword = new UnattendXmlClass.unattendSettingsComponentUserAccountsAdministratorPassword
                                    {
                                        Value = Common.DeploymentInfo.Password,
                                        PlainText = true
                                    }
                                }
                            }
                        }
                    };
                    break;
                case UnattendMode.AdminWithoutPassword:
                    uc = new UnattendXmlClass.unattend
                    {
                        settings = new UnattendXmlClass.unattendSettings
                        {
                            pass = "oobeSystem",
                            component = new UnattendXmlClass.unattendSettingsComponent
                            {
                                name = "Microsoft-Windows-Shell-Setup",
                                processorArchitecture = "amd64",
                                publicKeyToken = "31bf3856ad364e35",
                                language = "neutral",
                                versionScope = "nonSxS",
                                // AutoLogin
                                AutoLogon = new UnattendXmlClass.unattendSettingsComponentAutoLogon
                                {
                                    Enabled = true,
                                    LogonCount = 1,
                                    Username = "Administrator"
                                },
                                // Oem Information
                                OEMInformation = new UnattendXmlClass.unattendSettingsComponentOEMInformation
                                {
                                    Logo = "C:\\Windows\\System32\\logo.bmp",
                                    Manufacturer = Common.OemInfo.Manufacturer,
                                    Model = Common.OemInfo.Model,
                                    SupportHours = Common.OemInfo.SupportHours,
                                    SupportPhone = Common.OemInfo.SupportPhone,
                                    SupportURL = Common.OemInfo.SupportURL
                                }
                            }
                        }
                    };
                    break;
                case UnattendMode.AdminWithoutPasswordAndOem:
                    uc = new UnattendXmlClass.unattend
                    {
                        settings = new UnattendXmlClass.unattendSettings
                        {
                            pass = "oobeSystem",
                            component = new UnattendXmlClass.unattendSettingsComponent
                            {
                                name = "Microsoft-Windows-Shell-Setup",
                                processorArchitecture = "amd64",
                                publicKeyToken = "31bf3856ad364e35",
                                language = "neutral",
                                versionScope = "nonSxS",
                                // AutoLogin
                                AutoLogon = new UnattendXmlClass.unattendSettingsComponentAutoLogon
                                {
                                    Enabled = true,
                                    LogonCount = 1,
                                    Username = "Administrator"
                                }
                            }
                        }
                    };
                    break;
                case UnattendMode.User:
                    uc = new UnattendXmlClass.unattend
                    {
                        settings = new UnattendXmlClass.unattendSettings
                        {
                            pass = "oobeSystem",
                            component = new UnattendXmlClass.unattendSettingsComponent
                            {
                                name = "Microsoft-Windows-Shell-Setup",
                                processorArchitecture = "amd64",
                                publicKeyToken = "31bf3856ad364e35",
                                language = "neutral",
                                versionScope = "nonSxS",
                                // Oem Information
                                OEMInformation = new UnattendXmlClass.unattendSettingsComponentOEMInformation
                                {
                                    Logo = "C:\\Windows\\System32\\logo.bmp",
                                    Manufacturer = Common.OemInfo.Manufacturer,
                                    Model = Common.OemInfo.Model,
                                    SupportHours = Common.OemInfo.SupportHours,
                                    SupportPhone = Common.OemInfo.SupportPhone,
                                    SupportURL = Common.OemInfo.SupportURL
                                },
                                // User Accounts
                                UserAccounts = new UnattendXmlClass.unattendSettingsComponentUserAccounts
                                {
                                    // Local Account
                                    LocalAccounts = new UnattendXmlClass.unattendSettingsComponentUserAccountsLocalAccounts
                                    {
                                        LocalAccount = new UnattendXmlClass.unattendSettingsComponentUserAccountsLocalAccountsLocalAccount
                                        {
                                            action = "add",
                                            Password = new UnattendXmlClass.unattendSettingsComponentUserAccountsLocalAccountsLocalAccountPassword
                                            {
                                                Value = Common.DeploymentInfo.Password,
                                                PlainText = true
                                            },
                                            Name = Common.DeploymentInfo.Username,
                                            Group = "Administrators"
                                        }
                                    }
                                }
                            }
                        }
                    };
                    break;
                case UnattendMode.UserWithoutOem:
                    uc = new UnattendXmlClass.unattend
                    {
                        settings = new UnattendXmlClass.unattendSettings
                        {
                            pass = "oobeSystem",
                            component = new UnattendXmlClass.unattendSettingsComponent
                            {
                                name = "Microsoft-Windows-Shell-Setup",
                                processorArchitecture = "amd64",
                                publicKeyToken = "31bf3856ad364e35",
                                language = "neutral",
                                versionScope = "nonSxS",
                                // User Accounts
                                UserAccounts = new UnattendXmlClass.unattendSettingsComponentUserAccounts
                                {
                                    // Local Account
                                    LocalAccounts = new UnattendXmlClass.unattendSettingsComponentUserAccountsLocalAccounts
                                    {
                                        LocalAccount = new UnattendXmlClass.unattendSettingsComponentUserAccountsLocalAccountsLocalAccount
                                        {
                                            action = "add",
                                            Password = new UnattendXmlClass.unattendSettingsComponentUserAccountsLocalAccountsLocalAccountPassword
                                            {
                                                Value = Common.DeploymentInfo.Password,
                                                PlainText = true
                                            },
                                            Name = Common.DeploymentInfo.Username,
                                            Group = "Administrators"
                                        }
                                    }
                                }
                            }
                        }
                    };
                    break;
                case UnattendMode.UserWithoutPassword:
                    uc = new UnattendXmlClass.unattend
                    {
                        settings = new UnattendXmlClass.unattendSettings
                        {
                            pass = "oobeSystem",
                            component = new UnattendXmlClass.unattendSettingsComponent
                            {
                                name = "Microsoft-Windows-Shell-Setup",
                                processorArchitecture = "amd64",
                                publicKeyToken = "31bf3856ad364e35",
                                language = "neutral",
                                versionScope = "nonSxS",
                                // Oem Information
                                OEMInformation = new UnattendXmlClass.unattendSettingsComponentOEMInformation
                                {
                                    Logo = "C:\\Windows\\System32\\logo.bmp",
                                    Manufacturer = Common.OemInfo.Manufacturer,
                                    Model = Common.OemInfo.Model,
                                    SupportHours = Common.OemInfo.SupportHours,
                                    SupportPhone = Common.OemInfo.SupportPhone,
                                    SupportURL = Common.OemInfo.SupportURL
                                },
                                // User Accounts
                                UserAccounts = new UnattendXmlClass.unattendSettingsComponentUserAccounts
                                {
                                    // Local Account
                                    LocalAccounts = new UnattendXmlClass.unattendSettingsComponentUserAccountsLocalAccounts
                                    {
                                        LocalAccount = new UnattendXmlClass.unattendSettingsComponentUserAccountsLocalAccountsLocalAccount
                                        {
                                            action = "add",
                                            Name = Common.DeploymentInfo.Username,
                                            Group = "Administrators"
                                        }
                                    }
                                }
                            }
                        }
                    };
                    break;
                case UnattendMode.UserWithoutPasswordAndOem:
                    uc = new UnattendXmlClass.unattend
                    {
                        settings = new UnattendXmlClass.unattendSettings
                        {
                            pass = "oobeSystem",
                            component = new UnattendXmlClass.unattendSettingsComponent
                            {
                                name = "Microsoft-Windows-Shell-Setup",
                                processorArchitecture = "amd64",
                                publicKeyToken = "31bf3856ad364e35",
                                language = "neutral",
                                versionScope = "nonSxS",
                                // User Accounts
                                UserAccounts = new UnattendXmlClass.unattendSettingsComponentUserAccounts
                                {
                                    // Local Account
                                    LocalAccounts = new UnattendXmlClass.unattendSettingsComponentUserAccountsLocalAccounts
                                    {
                                        LocalAccount = new UnattendXmlClass.unattendSettingsComponentUserAccountsLocalAccountsLocalAccount
                                        {
                                            action = "add",
                                            Name = Common.DeploymentInfo.Username,
                                            Group = "Administrators"
                                        }
                                    }
                                }
                            }
                        }
                    }; 
                    break;
                default:
                    return "";
            }

            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "");

            //Create the serializer
            XmlSerializer slz = new XmlSerializer(typeof(UnattendXmlClass.unattend), "urn:schemas-microsoft-com:unattend");

            using (StringWriter textWriter = new Utf8StringWriter())
            {
                slz.Serialize(textWriter, uc, ns);
                return textWriter.ToString();
            }
        }
        
        public class Utf8StringWriter : StringWriter
        {
            public override Encoding Encoding => Encoding.UTF8;
        }
    }
}
