using Dive.UI.Common.UserInterface;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Dive.UI.Common.Fun
{
    internal class WLED
    {
        public enum LedEffect
        {
            Solid = 0,
            Chase = 28
        }

        public static async Task SetEffect(LedEffect effect)
        {
            const string json = "{\"seg\":{\"fx\":{0}}}";
            var val = Convert.ChangeType(effect, effect.GetTypeCode());
            var jsonRequest = json.Replace("{0}", val.ToString());

            using var client = new HttpClient();
            var response = await client.PostAsync(
                $"http://{Common.Configuration.FunConfig.WledControllerIp}/json",
                new StringContent(jsonRequest, Encoding.UTF8, "application/json"));
            Debug.WriteLine($"Posted with: {jsonRequest}\nGot back: {response.ReasonPhrase!}");
        }

        public static async Task IncreaseProgress(int progress)
        {
            var neededLeds = (int)Math.Ceiling((decimal)((double)progress / 100 * Configuration.FunConfig.AvailableLEDs)!);
            const string json = "{\"seg\":{\"i\":[0,{0},\"FF0000\"]}}}";
            var jsonRequest = json.Replace("{0}", neededLeds.ToString());
            using var client = new HttpClient();
            var response = await client.PostAsync(
                $"http://{Common.Configuration.FunConfig.WledControllerIp}/json",
                new StringContent(jsonRequest, Encoding.UTF8, "application/json"));
            Debug.WriteLine($"Posted with: {jsonRequest}\nGot back: {response.ReasonPhrase!}");
        }
    }
}
