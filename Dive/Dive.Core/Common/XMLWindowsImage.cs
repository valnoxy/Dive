namespace Dive.Core.Common
{
    public class WIMDescription
    {
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
        public partial class WIMIMAGE
        {

            private string nAMEField;

            private string dESCRIPTIONField;

            private WIMIMAGEWINDOWS wINDOWSField;

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
            public WIMIMAGEWINDOWS WINDOWS
            {
                get
                {
                    return this.wINDOWSField;
                }
                set
                {
                    this.wINDOWSField = value;
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

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class WIMIMAGEWINDOWS
        {

            private int aRCHField;

            private WIMIMAGEWINDOWSVERSION vERSIONField;

            /// <remarks/>
            public int ARCH
            {
                get
                {
                    return this.aRCHField;
                }
                set
                {
                    this.aRCHField = value;
                }
            }

            /// <remarks/>
            public WIMIMAGEWINDOWSVERSION VERSION
            {
                get
                {
                    return this.vERSIONField;
                }
                set
                {
                    this.vERSIONField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class WIMIMAGEWINDOWSVERSION
        {

            private ushort mAJORField;

            private ushort mINORField;

            private ushort bUILDField;

            private ushort sPBUILDField;

            private string bRANCHField;

            /// <remarks/>
            public ushort MAJOR
            {
                get
                {
                    return this.mAJORField;
                }
                set
                {
                    this.mAJORField = value;
                }
            }

            /// <remarks/>
            public ushort MINOR
            {
                get
                {
                    return this.mINORField;
                }
                set
                {
                    this.mINORField = value;
                }
            }

            /// <remarks/>
            public ushort BUILD
            {
                get
                {
                    return this.bUILDField;
                }
                set
                {
                    this.bUILDField = value;
                }
            }

            /// <remarks/>
            public ushort SPBUILD
            {
                get
                {
                    return this.sPBUILDField;
                }
                set
                {
                    this.sPBUILDField = value;
                }
            }

            /// <remarks/>
            public string BRANCH
            {
                get
                {
                    return this.bRANCHField;
                }
                set
                {
                    this.bRANCHField = value;
                }
            }
        }
    }
}
