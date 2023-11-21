using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dive.UI.Common
{
    public class XMLSetting
    {
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
        public partial class settings
        {

            private settingsAdministratorAccount administratorAccountField;

            private settingsOEMSupport oEMSupportField;

            private settingsDeviceInfo deviceInfoField;

            private settingsDomainInfo domainInfoField;

            private settingsOutOfBoxExperience outOfBoxExperienceField;

            /// <remarks/>
            public settingsAdministratorAccount AdministratorAccount
            {
                get
                {
                    return this.administratorAccountField;
                }
                set
                {
                    this.administratorAccountField = value;
                }
            }

            /// <remarks/>
            public settingsOEMSupport OEMSupport
            {
                get
                {
                    return this.oEMSupportField;
                }
                set
                {
                    this.oEMSupportField = value;
                }
            }

            /// <remarks/>
            public settingsDeviceInfo DeviceInfo
            {
                get
                {
                    return this.deviceInfoField;
                }
                set
                {
                    this.deviceInfoField = value;
                }
            }

            /// <remarks/>
            public settingsDomainInfo DomainInfo
            {
                get
                {
                    return this.domainInfoField;
                }
                set
                {
                    this.domainInfoField = value;
                }
            }

            /// <remarks/>
            public settingsOutOfBoxExperience OutOfBoxExperience
            {
                get
                {
                    return this.outOfBoxExperienceField;
                }
                set
                {
                    this.outOfBoxExperienceField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class settingsAdministratorAccount
        {

            private string usernameField;

            private string passwordField;

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
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class settingsOEMSupport
        {

            private string oEMLogoField;

            private string manufacturerField;

            private string modelField;

            private string supportHoursField;

            private string supportNoField;

            private string homepageField;

            /// <remarks/>
            public string OEMLogo
            {
                get
                {
                    return this.oEMLogoField;
                }
                set
                {
                    this.oEMLogoField = value;
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
            public string SupportNo
            {
                get
                {
                    return this.supportNoField;
                }
                set
                {
                    this.supportNoField = value;
                }
            }

            /// <remarks/>
            public string Homepage
            {
                get
                {
                    return this.homepageField;
                }
                set
                {
                    this.homepageField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class settingsDeviceInfo
        {

            private string deviceNameField;

            private string registeredOwnerField;

            private string registeredOrganizationField;

            private string productKeyField;

            private string timezoneField;

            /// <remarks/>
            public string DeviceName
            {
                get
                {
                    return this.deviceNameField;
                }
                set
                {
                    this.deviceNameField = value;
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
            public string Timezone
            {
                get
                {
                    return this.timezoneField;
                }
                set
                {
                    this.timezoneField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class settingsDomainInfo
        {

            private string domainField;

            private string userNameField;

            private string passwordField;

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
            public string UserName
            {
                get
                {
                    return this.userNameField;
                }
                set
                {
                    this.userNameField = value;
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
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class settingsOutOfBoxExperience
        {

            private bool hideEULAPageField;

            private bool hideOEMRegistrationScreenField;

            private bool hideOnlineAccountScreensField;

            private bool hideWirelessSetupInOOBEField;

            private bool hideLocalAccountScreenField;

            private string networkLocationField;

            private bool skipMachineOOBEField;

            private bool skipUserOOBEField;

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
        }
    }
}
