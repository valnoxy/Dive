using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Dive.UI.Common.Configuration
{
    public class CloudInfo
    {
        public string? Version { get; set; }
        public string? Product { get; set; }
        public List<Image>? Images { get; set; }

        public static async Task<CloudInfo?> FromUrlAsync(string url)
        {
            using var client = new HttpClient();
            var json = await client.GetStringAsync(url);
            var cloudInfo = JsonConvert.DeserializeObject<CloudInfo?>(json);
            var uri = new Uri(url);
            var domain = uri.GetLeftPart(UriPartial.Authority);
            
            // Adjust file paths if necessary
            if (cloudInfo?.Images == null) return new CloudInfo();
            foreach (var image in cloudInfo.Images)
            {
                if (image.FilePath != null && !image.FilePath.StartsWith("http://") &&
                    !image.FilePath.StartsWith("https://"))
                {
                    image.FilePath = domain.TrimEnd('/') + "/" + image.FilePath.TrimStart('/');
                }
            }

            return cloudInfo;
        }
    }

    public class Image
    {
        public string? Arch { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? InstallationType { get; set; }
        public Version? Version { get; set; }
        public string? Language { get; set; }
        public string? FilePath { get; set; }
        public string? Index { get; set; }
    }

    public class Version
    {
        public string? Major { get; set; }
        public string? Minor { get; set; }
        public string? Build { get; set; }
    }
}
