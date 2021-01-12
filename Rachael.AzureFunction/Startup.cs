using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

[assembly: FunctionsStartup(typeof(Rachael.AzureFunction.Startup))]
namespace Rachael.AzureFunction
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services
                .AddTransient<OAuthAuthenticationHeaderHandler>()
                .AddTransient<IOAuthDetailsFactory>((sp) => new OAuthDetailsFactory());
            builder.Services
                .AddHttpClient("Twitter", c => 
                {
                    c.BaseAddress = new Uri("https://api.twitter.com/");
                })
                .AddHttpMessageHandler<OAuthAuthenticationHeaderHandler>();
        }

        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            var context = builder.GetContext();

            builder.ConfigurationBuilder
                .AddJsonFile(Path.Combine(context.ApplicationRootPath, "appsettings.json"), true)
                .AddEnvironmentVariables();
        }
    }

    public interface IOAuthDetailsFactory
    {
        string Nonce();
        DateTime DateTimeNow();
    }

    public class OAuthDetailsFactory : IOAuthDetailsFactory
    {
        public DateTime DateTimeNow()
        {
            return DateTime.UtcNow;
        }

        public string Nonce()
        {
            var r = new Random();
            var s =
                Enumerable
                .Range(1, 11)
                .Select(x => (char)r.Next(97, 122))
                .Aggregate("", (acc, c) => acc + c, (acc) => acc);
            return s;
        }
    }

    public class OAuthAuthenticationHeaderHandler : DelegatingHandler
    {
        private readonly IConfiguration _config;
        private readonly IOAuthDetailsFactory _details;
        private readonly HMACSHA1 _hash;

        public OAuthAuthenticationHeaderHandler(
            IConfiguration config,
            IOAuthDetailsFactory details)
        {
            _config = config;
            _details = details;
            var key = $"{_config["TwitterOAuthConsumerSecret"]}&{_config["TwitterOAuthTokenSecret"]}";
            _hash = new HMACSHA1(new ASCIIEncoding().GetBytes(key));
        }

        IDictionary<string, string> ToDictionary(NameValueCollection d)
        {
            return d.Cast<string>().ToDictionary(k => k, v => d[v]);
        }

        public string OAuthHeader(Uri url, string method, IDictionary<string, string> formBody = null)
        {
            var collectedParams = new[] {
                new { key = "oauth_consumer_key", value = _config["TwitterOAuthConsumerKey"] },
                new { key = "oauth_token", value = _config["TwitterOAuthToken"] },
                new { key = "oauth_signature_method", value = "HMAC-SHA1" },
                new { key = "oauth_timestamp", value = ((int)(_details.DateTimeNow() - DateTime.UnixEpoch).TotalSeconds).ToString() },
                new { key = "oauth_nonce", value = _details.Nonce() },
                new { key = "oauth_version", value = "1.0" },
            }
            .ToDictionary(x => x.key, y => y.value)
            .Concat(ToDictionary(url.ParseQueryString()))
            .Concat(formBody == null ? new Dictionary<string, string>() : formBody)
            .ToDictionary(x => x.Key, y => y.Value);
            var sig = Generate(url.AbsoluteUri, method, collectedParams);
            collectedParams.Add("oauth_signature", sig);

            return Header(collectedParams);
        }

        string Generate(string url, string method, IDictionary<string, string> parameters)
        {
            var orderedSigBase =
                string.Join(
                    "&",
                    parameters
                    .Union(parameters)
                    .Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}")
                    .OrderBy(s => s));
            var u = new Uri(url);
            var urlWithoutQueryString = $"{u.Scheme}://{u.Host}{u.AbsolutePath}";
            var baseSig = $"{method.ToUpper()}&{Uri.EscapeDataString(urlWithoutQueryString)}&{Uri.EscapeDataString(orderedSigBase)}";

            return Convert.ToBase64String(_hash.ComputeHash(new ASCIIEncoding().GetBytes(baseSig)));
        }

        string Header(IDictionary<string, string> data)
        {
            return "OAuth " +
                string.Join(",",
                    data
                        .Where(kvp => kvp.Key.StartsWith("oauth_"))
                        .Select(kv => $"{Uri.EscapeDataString(kv.Key)}=\"{Uri.EscapeDataString(kv.Value)}\""));
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.TryGetValues("application/x-www-form-urlencoded", out var formContentType);
            var body =
                formContentType != null && formContentType.Any() ?
                await request.Content.ReadAsFormDataAsync() :
                new NameValueCollection();
            request.Headers.Add("Authorization", 
                OAuthHeader(request.RequestUri, request.Method.Method, ToDictionary(body)));

            return await base.SendAsync(request, cancellationToken);
        }
    }

    public class TwitterApiClient : HttpClient
    {
    }
}
