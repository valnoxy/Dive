using System.Linq;
using Standard.Licensing;
using Standard.Licensing.Validation;

namespace Dive.UI.Common.Licensing
{
    public class Validation
    {
        internal class Info
        {
            public static string PubKeyVerification =
                "MFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAEMMqFbJtgy0gu9tKW/ISzLHiUorE936n+ELKMyV/iu/Xej4CA574tUhf6k4/DLKgcl+diwyZlVOXAdlqHY/WIAg==";
            public static string? LicenseName { get; set; }
            public static string? LicenseEmail { get; set; }
            public static bool Valid { get; set; }
            public static bool ValidationFailed { get; set; }
        }

        public static void Validate(string licenseData)
        {
            var license = License.Load(licenseData);
            var validation = ValidateLicense(license);
            if (!validation)
            {
                Info.Valid = false;
                Info.ValidationFailed = true;
                Info.LicenseName = null;
                Info.LicenseEmail = null;
                return;
            }

            Info.LicenseName = license.Customer.Name;
            Info.LicenseEmail = license.Customer.Email;
            Info.Valid = true;
        }

        private static bool ValidateLicense(Standard.Licensing.License license)
        {
            var validationFailures =
                license.Validate()
                    .ExpirationDate()
                    .And()
                    .Signature(Info.PubKeyVerification)
                    .AssertValidLicense();

            return !validationFailures.Any();
        }
    }
}
