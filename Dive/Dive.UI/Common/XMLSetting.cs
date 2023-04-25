using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dive.UI.Common
{
    public class XMLSetting
    {
        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
        public partial class settings
        {

            private settingsAdministratorAccount administratorAccountField;

            private settingsOEMSupport oEMSupportField;

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


    }
}
