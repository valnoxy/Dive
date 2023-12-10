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

            private unattendSettingsComponent[] componentField;

            private string passField;

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("component")]
            public unattendSettingsComponent[] component
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

            private unattendSettingsComponentIdentification identificationField;

            private object computerNameField;

            private string productKeyField;

            private string registeredOrganizationField;

            private string registeredOwnerField;

            private unattendSettingsComponentOOBE oOBEField;

            private unattendSettingsComponentOEMInformation oEMInformationField;

            private unattendSettingsComponentAutoLogon autoLogonField;

            private unattendSettingsComponentUserAccounts userAccountsField;

            private unattendSettingsComponentSynchronousCommand[] firstLogonCommandsField;

            private string timeZoneField;

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
            public unattendSettingsComponentIdentification Identification
            {
                get
                {
                    return this.identificationField;
                }
                set
                {
                    this.identificationField = value;
                }
            }

            /// <remarks/>
            public object ComputerName
            {
                get
                {
                    return this.computerNameField;
                }
                set
                {
                    this.computerNameField = value;
                }
            }

            /// <remarks/>
            public string ProductKey
            {
                get
                {
                    return this.productKeyField;
                }
                set
                {
                    this.productKeyField = value;
                }
            }

            /// <remarks/>
            public string RegisteredOrganization
            {
                get
                {
                    return this.registeredOrganizationField;
                }
                set
                {
                    this.registeredOrganizationField = value;
                }
            }

            /// <remarks/>
            public string RegisteredOwner
            {
                get
                {
                    return this.registeredOwnerField;
                }
                set
                {
                    this.registeredOwnerField = value;
                }
            }

            /// <remarks/>
            public unattendSettingsComponentOOBE OOBE
            {
                get
                {
                    return this.oOBEField;
                }
                set
                {
                    this.oOBEField = value;
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
            public string TimeZone
            {
                get
                {
                    return this.timeZoneField;
                }
                set
                {
                    this.timeZoneField = value;
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
        public partial class unattendSettingsComponentIdentification
        {

            private unattendSettingsComponentIdentificationCredentials credentialsField;

            private string joinDomainField;

            /// <remarks/>
            public unattendSettingsComponentIdentificationCredentials Credentials
            {
                get
                {
                    return this.credentialsField;
                }
                set
                {
                    this.credentialsField = value;
                }
            }

            /// <remarks/>
            public string JoinDomain
            {
                get
                {
                    return this.joinDomainField;
                }
                set
                {
                    this.joinDomainField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:schemas-microsoft-com:unattend")]
        public partial class unattendSettingsComponentIdentificationCredentials
        {

            private string domainField;

            private string passwordField;

            private string usernameField;

            /// <remarks/>
            public string Domain
            {
                get
                {
                    return this.domainField;
                }
                set
                {
                    this.domainField = value;
                }
            }

            /// <remarks/>
            public string Password
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
        public partial class unattendSettingsComponentOOBE
        {

            private bool hideEULAPageField;

            private bool hideOEMRegistrationScreenField;

            private bool hideOnlineAccountScreensField;

            private bool hideWirelessSetupInOOBEField;

            private string networkLocationField;

            private bool skipMachineOOBEField;

            private bool skipUserOOBEField;

            private bool hideLocalAccountScreenField;

            /// <remarks/>
            public bool HideEULAPage
            {
                get
                {
                    return this.hideEULAPageField;
                }
                set
                {
                    this.hideEULAPageField = value;
                }
            }

            /// <remarks/>
            public bool HideOEMRegistrationScreen
            {
                get
                {
                    return this.hideOEMRegistrationScreenField;
                }
                set
                {
                    this.hideOEMRegistrationScreenField = value;
                }
            }

            /// <remarks/>
            public bool HideOnlineAccountScreens
            {
                get
                {
                    return this.hideOnlineAccountScreensField;
                }
                set
                {
                    this.hideOnlineAccountScreensField = value;
                }
            }

            /// <remarks/>
            public bool HideWirelessSetupInOOBE
            {
                get
                {
                    return this.hideWirelessSetupInOOBEField;
                }
                set
                {
                    this.hideWirelessSetupInOOBEField = value;
                }
            }

            /// <remarks/>
            public string NetworkLocation
            {
                get
                {
                    return this.networkLocationField;
                }
                set
                {
                    this.networkLocationField = value;
                }
            }

            /// <remarks/>
            public bool SkipMachineOOBE
            {
                get
                {
                    return this.skipMachineOOBEField;
                }
                set
                {
                    this.skipMachineOOBEField = value;
                }
            }

            /// <remarks/>
            public bool SkipUserOOBE
            {
                get
                {
                    return this.skipUserOOBEField;
                }
                set
                {
                    this.skipUserOOBEField = value;
                }
            }

            /// <remarks/>
            public bool HideLocalAccountScreen
            {
                get
                {
                    return this.hideLocalAccountScreenField;
                }
                set
                {
                    this.hideLocalAccountScreenField = value;
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

            private int logonCountField;

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
            public int LogonCount
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
        private static readonly ApplyDetails ApplyDetailsInstance = ApplyDetails.Instance;
        private static readonly DeploymentInfo DeploymentInfoInstance = DeploymentInfo.Instance;

        public static string Build()
        {
            Common.Debug.WriteLine("[UnattendBuilder v2] Entering building process ...");
            var currentSettings = 0;

            // Calculate Settings Array Size
            var settingsArraySize = 0;
            if (DeviceInfo.UseDeviceInfo || DomainInfo.UseDomainInfo || DeploymentOption.UseCopyProfile)
                settingsArraySize++; // pass="specialize"
            if (DeviceInfo.UseDeviceInfo || OemInfo.UseOemInfo || DeploymentInfoInstance.UseUserInfo)
                settingsArraySize++; // pass="oobeSystem"
            if (DeploymentOption.UseSMode)
                settingsArraySize++; // pass="offlineServicing"

            var uc = new UnattendXmlClass.unattend
            {
                settings = new UnattendXmlClass.unattendSettings[settingsArraySize]
            };

            #region oobeSystem (single component)
            if (DeploymentInfoInstance.UseUserInfo || DeviceInfo.UseDeviceInfo || OemInfo.UseOemInfo ||
                OutOfBoxExperienceInfo.UseOOBEInfo)
            {
                Debug.WriteLine("Entered region: oobeSystem (single component)");
                uc.settings[currentSettings] = new UnattendXmlClass.unattendSettings
                {
                    pass = "oobeSystem",
                    component = new UnattendXmlClass.unattendSettingsComponent[1]
                };
                uc.settings[currentSettings].component[0] = new UnattendXmlClass.unattendSettingsComponent
                {
                    name = "Microsoft-Windows-Shell-Setup",
                    processorArchitecture = "amd64",
                    publicKeyToken = "31bf3856ad364e35",
                    language = "neutral",
                    versionScope = "nonSxS"
                };

                if (!string.IsNullOrEmpty(DeviceInfo.RegisteredOwner))
                    uc.settings[currentSettings].component[0].RegisteredOwner = DeviceInfo.RegisteredOwner;
                if (!string.IsNullOrEmpty(DeviceInfo.RegisteredOrganization))
                    uc.settings[currentSettings].component[0].RegisteredOrganization = DeviceInfo.RegisteredOrganization;

                // OOBE Information
                if (OutOfBoxExperienceInfo.UseOOBEInfo)
                {
                    // Windows Vista / 7
                    if (ApplyDetailsInstance.NTVersion == "6.0" || ApplyDetailsInstance.NTVersion == "6.1")
                        uc.settings[currentSettings].component[0].OOBE = new UnattendXmlClass.unattendSettingsComponentOOBE
                        {
                            HideEULAPage = OutOfBoxExperienceInfo.HideEULAPage,
                            HideWirelessSetupInOOBE = OutOfBoxExperienceInfo.HideWirelessSetupInOOBE,
                            NetworkLocation = OutOfBoxExperienceInfo.NetworkLocation,
                            SkipMachineOOBE = OutOfBoxExperienceInfo.SkipMachineOOBE,
                            SkipUserOOBE = OutOfBoxExperienceInfo.SkipUserOOBE,
                        };

                    // Windows 10 / 11 
                    if (ApplyDetailsInstance.NTVersion == "6.2" || ApplyDetailsInstance.NTVersion == "6.3" || ApplyDetailsInstance.NTVersion.Contains("10."))
                        uc.settings[currentSettings].component[0].OOBE = new UnattendXmlClass.unattendSettingsComponentOOBE
                        {
                            HideEULAPage = OutOfBoxExperienceInfo.HideEULAPage,
                            HideOEMRegistrationScreen = OutOfBoxExperienceInfo.HideOEMRegistrationScreen,
                            HideOnlineAccountScreens = OutOfBoxExperienceInfo.HideOnlineAccountScreens,
                            HideWirelessSetupInOOBE = OutOfBoxExperienceInfo.HideWirelessSetupInOOBE,
                            NetworkLocation = OutOfBoxExperienceInfo.NetworkLocation,
                            SkipMachineOOBE = OutOfBoxExperienceInfo.SkipMachineOOBE,
                            SkipUserOOBE = OutOfBoxExperienceInfo.SkipUserOOBE,
                            HideLocalAccountScreen = OutOfBoxExperienceInfo.HideLocalAccountScreen
                        };
                }

                // OEM Information
                if (OemInfo.UseOemInfo)
                {
                    var oemLogo = "";
                    if (!string.IsNullOrEmpty(OemInfo.LogoPath))
                    {
                        Debug.WriteLine("[UnattendBuilder v2] OEM Logo found: " + OemInfo.LogoPath);
                        oemLogo = "%WINDIR%\\System32\\logo.bmp";
                    }

                    uc.settings[currentSettings].component[0].OEMInformation =
                        new UnattendXmlClass.unattendSettingsComponentOEMInformation
                        {
                            Logo = oemLogo,
                            Manufacturer = OemInfo.Manufacturer,
                            Model = OemInfo.Model,
                            SupportHours = OemInfo.SupportHours,
                            SupportPhone = OemInfo.SupportPhone,
                            SupportURL = OemInfo.SupportURL
                        };
                }

                // User Accounts
                if (DeploymentInfoInstance.UseUserInfo)
                {
                    uc.settings[currentSettings].component[0].UserAccounts =
                        new UnattendXmlClass.unattendSettingsComponentUserAccounts();

                    if (DeploymentInfoInstance.Username == "Administrator")
                    {
                        // Administrator Password
                        uc.settings[currentSettings].component[0].UserAccounts.AdministratorPassword =
                            new UnattendXmlClass.unattendSettingsComponentUserAccountsAdministratorPassword
                            {
                                Value = DeploymentInfoInstance.Password,
                                PlainText = true
                            };
                    }
                    else
                    {
                        // Local Account
                        uc.settings[currentSettings].component[0].UserAccounts.LocalAccounts =
                            new UnattendXmlClass.unattendSettingsComponentUserAccountsLocalAccounts
                            {
                                LocalAccount = new UnattendXmlClass.unattendSettingsComponentUserAccountsLocalAccountsLocalAccount
                                {
                                    action = "add",
                                    Name = DeploymentInfoInstance.Username,
                                    Password = new UnattendXmlClass.unattendSettingsComponentUserAccountsLocalAccountsLocalAccountPassword
                                    {
                                        Value = DeploymentInfoInstance.Password,
                                        PlainText = true
                                    },
                                    Group = "Administrators"
                                }
                            };
                    }

                    // Auto Logon
                    uc.settings[currentSettings].component[0].AutoLogon =
                        new UnattendXmlClass.unattendSettingsComponentAutoLogon
                        {
                            Username = DeploymentInfoInstance.Username,
                            Password = new UnattendXmlClass.unattendSettingsComponentAutoLogonPassword
                            {
                                Value = DeploymentInfoInstance.Password,
                                PlainText = true
                            },
                            Enabled = true,
                            LogonCount = 1
                        };

                    // First Logon Command (set PasswordExpires to false)
                    if (DeploymentInfoInstance.Username != "Administrator")
                    {
                        uc.settings[currentSettings].component[0].FirstLogonCommands = new[]
                        {
                            new UnattendXmlClass.unattendSettingsComponentSynchronousCommand
                            {
                                Order = 1,
                                RequiresUserInput = false,
                                CommandLine =
                                    $"cmd /C wmic useraccount where name=\"{DeploymentInfoInstance.Username}\" set PasswordExpires=false",
                                Description = "Password never expires"
                            }
                        };
                    }
                }

                currentSettings++;
                Debug.WriteLine("Completed region: oobeSetup (single component)");
            }
            #endregion

            #region specialize (2 components)
            if (DeviceInfo.UseDeviceInfo || DomainInfo.UseDomainInfo || DeploymentOption.UseCopyProfile)
            {
                Debug.WriteLine("Entered region: specialize (2 component)");
                var currentComponent = 0;

                // Calculate component Array size
                var componentArraySize = 0;
                if (DeviceInfo.UseDeviceInfo || DeploymentOption.UseCopyProfile)
                    componentArraySize++;
                if (DomainInfo.UseDomainInfo)
                    componentArraySize++;

                uc.settings[currentSettings] = new UnattendXmlClass.unattendSettings
                {
                    pass = "specialize",
                    component = new UnattendXmlClass.unattendSettingsComponent[componentArraySize]
                };

                if (DeviceInfo.UseDeviceInfo || DeploymentOption.UseCopyProfile)
                {
                    uc.settings[currentSettings].component[currentComponent] = new UnattendXmlClass.unattendSettingsComponent
                    {
                        name = "Microsoft-Windows-Shell-Setup",
                        processorArchitecture = "amd64",
                        publicKeyToken = "31bf3856ad364e35",
                        language = "neutral",
                        versionScope = "nonSxS"
                    };

                    if (DeviceInfo.UseDeviceInfo)
                    {
                        uc.settings[currentSettings].component[currentComponent].ComputerName = DeviceInfo.DeviceName;
                        uc.settings[currentSettings].component[currentComponent].ProductKey = DeviceInfo.ProductKey;
                        uc.settings[currentSettings].component[currentComponent].RegisteredOwner = DeviceInfo.RegisteredOwner;
                        uc.settings[currentSettings].component[currentComponent].RegisteredOrganization= DeviceInfo.RegisteredOrganization;
                        uc.settings[currentSettings].component[currentComponent].TimeZone = DeviceInfo.TimeZone;
                    }

                    if (DeploymentOption.UseCopyProfile)
                    {
                        uc.settings[currentSettings].component[currentComponent].CopyProfile = true;
                        uc.settings[currentSettings].component[currentComponent].CopyProfileSpecified = true;
                    }

                    currentComponent++;
                }

                if (DomainInfo.UseDomainInfo)
                {
                    uc.settings[currentSettings].component[currentComponent] = new UnattendXmlClass.unattendSettingsComponent
                    {
                        name = "Microsoft-Windows-UnattendedJoin",
                        processorArchitecture = "amd64",
                        publicKeyToken = "31bf3856ad364e35",
                        language = "neutral",
                        versionScope = "nonSxS"
                    };
                    uc.settings[currentSettings].component[currentComponent].Identification =
                        new UnattendXmlClass.unattendSettingsComponentIdentification
                        {
                            Credentials = new UnattendXmlClass.unattendSettingsComponentIdentificationCredentials
                            {
                                Domain = DomainInfo.Domain,
                                Username = DomainInfo.UserName,
                                Password = DomainInfo.Password
                            },
                            JoinDomain = DomainInfo.Domain
                        };
                    //currentComponent++;
                }

                currentSettings++;
                Debug.WriteLine("Completed region: specialize (2 component)");
            }
            #endregion

            #region offlineServicing (single component)
            if (DeploymentOption.UseSMode)
            {
                Debug.WriteLine("Entered region: offlineServicing (single component)");

                uc.settings[currentSettings] = new UnattendXmlClass.unattendSettings
                {
                    pass = "offlineServicing",
                    component = new UnattendXmlClass.unattendSettingsComponent[1]
                };
                uc.settings[currentSettings].component[0] = new UnattendXmlClass.unattendSettingsComponent
                {
                    name = "Microsoft-Windows-CodeIntegrity",
                    processorArchitecture = "amd64",
                    publicKeyToken = "31bf3856ad364e35",
                    language = "neutral",
                    versionScope = "nonSxS",
                    SkuPolicyRequired = 1,
                    SkuPolicyRequiredSpecified = true
                };
                //currentSettings++;
                Debug.WriteLine("Completed region: offlineServicing (single component)");
            }
            #endregion

            var ns = new XmlSerializerNamespaces();
            ns.Add("", "");

            // Create the serializer
            var slz = new XmlSerializer(typeof(UnattendXmlClass.unattend), "urn:schemas-microsoft-com:unattend");

            using StringWriter textWriter = new Utf8StringWriter();
            slz.Serialize(textWriter, uc, ns);

            Debug.WriteLine("[UnattendBuilder v2] Building completed.");
            return textWriter.ToString();
        }
        
        public class Utf8StringWriter : StringWriter
        {
            public override Encoding Encoding => Encoding.UTF8;
        }
    }
}