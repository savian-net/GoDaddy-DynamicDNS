using Flurl;
using Flurl.Http;
using Flurl.Http.Testing;
using System.Text.Json.Nodes;
using static System.Console;

namespace FixGoDaddyIpAddress
{
    internal class Program
    {
        static string _domain = "mydomain.com";
        static string _aRecord = "@";
        //static string _aRecord = "info";
        static string _apiKey = "===>KEY<===";
        static string _apiSecret = "===>SECRET<===";
        static string _goDaddyCurrentIp;
        static string _detectedIp;
        static void Main(string[] args)
        {
            WriteLine("Testing GoDaddy API");
            //Test();
            GetDetectedIp();
            GetGoDaddyDnsIp();
            if (_detectedIp != _goDaddyCurrentIp)
            {
                ChangeIpAtGoDaddy();
            }
        }

        private static void Test()
        {
            _detectedIp = "1.1.1.1";
            using (var httpTest = new HttpTest())
            {
                ChangeIpAtGoDaddy();
            }
        }

        private async static void GetGoDaddyDnsIp()
        {
            var uri = $"https://api.godaddy.com/v1/domains";
            try
            {
                var response = uri
                    .WithHeader("Authorization", $"sso-key {_apiKey}:{_apiSecret}")
                    .AppendPathSegment(_domain)
                    .AppendPathSegment("records")
                    .AppendPathSegment("A")
                    .AppendPathSegment(_aRecord)
                    .GetStringAsync()
                    ;
                response.Wait();
                _goDaddyCurrentIp = JsonNode.Parse(response.Result)[0]["data"].ToString();
            }
            catch (Exception ex)
            {
                WriteLine($"Failed to get the values from GoDaddy: {ex}");
            }
        }

        private static void ChangeIpAtGoDaddy()
        {
            var uri = $"https://api.godaddy.com/v1/domains";
            var jsonArray = new[] { new { data=_detectedIp, ttl=3600} };
            try
            {
                var response = uri
                    .WithHeader("Authorization", $"sso-key {_apiKey}:{_apiSecret}")
                    .WithHeader("Accept", "application/json")
                    .WithHeader("Content-Type", "application/json")
                    .AppendPathSegment(_domain)
                    .AppendPathSegment("records")
                    .AppendPathSegment("A")
                    .AppendPathSegment(_aRecord)
                    .PutJsonAsync(jsonArray)
                    ;
                response.Wait();
                var result = response.Result;
            }
            catch (Exception ex)
            {
                WriteLine($"Failed to get the values from GoDaddy: {ex}");
            }
        }

        private static void GetDetectedIp()
        {
            try
            {
                var uri = "http://ipinfo.io/json";
                var request = uri.GetJsonAsync();
                request.Wait();
                var result = request.Result;
                _detectedIp = result.ip;
            }
            catch (Exception ex)
            {
                WriteLine($"Failed to get detected ip: {ex}");
            }
        }
    }
}
