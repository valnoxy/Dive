namespace deployaCore.Common
{
    public class WIMDescription
    {
        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
        public partial class WIM
        {
            private WIMIMAGE iMAGEField;

            /// <remarks/>
            public WIMIMAGE IMAGE
            {
                get
                {
                    return this.iMAGEField;
                }
                set
                {
                    this.iMAGEField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class WIMIMAGE
        {
            private string nAMEField;

            private string dESCRIPTIONField;

            private byte iNDEXField;

            /// <remarks/>
            public string NAME
            {
                get
                {
                    return this.nAMEField;
                }
                set
                {
                    this.nAMEField = value;
                }
            }

            /// <remarks/>
            public string DESCRIPTION
            {
                get
                {
                    return this.dESCRIPTIONField;
                }
                set
                {
                    this.dESCRIPTIONField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public byte INDEX
            {
                get
                {
                    return this.iNDEXField;
                }
                set
                {
                    this.iNDEXField = value;
                }
            }
        }
    }
}
