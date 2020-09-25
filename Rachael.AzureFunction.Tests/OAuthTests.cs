using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Rachael.AzureFunction.Tests
{
    public class OAuthTests
    {
        [Fact]
        public void CompareWithTwitterSampleSig()
        {
            var url = "https://api.twitter.com/1.1/statuses/update.json?include_entities=true";
            var formData = new[] {
                new { key = "status" , value = "Hello Ladies + Gentlemen, a signed OAuth request!" }
            }
            .ToDictionary(x => x.key, y => y.value);
            var actual =
                new OAuthAuthenticationHeaderHandler(
                    new StubConfig(
                        "xvz1evFS4wEEPTGEFPHBog", //consumerKey
                        "370773112-GmHxMAgYyLbNEtIKZeRNFsMKPR9EyMZeS9weJAEb", //oauthToken
                        "kAcSOqF21Fu85e7zjz7ZN2U4ZRhfV3WpwPAoE3Z7kBw",
                        "LswwdoUaIvS8ltyTt5jkRh4J50vUPVVHtR2YPi5kE"), 
                    new StubDetails("kYjzVBB8Y0ZFabxSWbWovY3uYSQ2pTgmZeNu2VS4cg", DateTime.Parse("14/10/2011 20:09:18")))
                .OAuthHeader(new Uri(url), "post", formData);
            var expected = "OAuth oauth_consumer_key=\"xvz1evFS4wEEPTGEFPHBog\",oauth_token=\"370773112-GmHxMAgYyLbNEtIKZeRNFsMKPR9EyMZeS9weJAEb\",oauth_signature_method=\"HMAC-SHA1\",oauth_timestamp=\"1318622958\",oauth_nonce=\"kYjzVBB8Y0ZFabxSWbWovY3uYSQ2pTgmZeNu2VS4cg\",oauth_version=\"1.0\",oauth_signature=\"hCtSmYh%2BiHYCEqBWrE7C7hYmtUk%3D\"";

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CompareWithCapturedSigAndKeysChanged()
        {
            var url = "https://api.twitter.com/2/users/by?usernames=bydonkeys,BorisJohnson_MP";
            var actual =
                new OAuthAuthenticationHeaderHandler(
                    new StubConfig(
                        "xvz1evFS4wEEPTGEFPHBog",
                        "370773112-GmHxMAgYyLbNEtIKZeRNFsMKPR9EyMZeS9weJAEb",
                        "kAcSOqF21Fu85e7zjz7ZN2U4ZRhfV3WpwPAoE3Z7kBw",
                        "LswwdoUaIvS8ltyTt5jkRh4J50vUPVVHtR2YPi5kE"),
                    new StubDetails("l44BEKnGlM5", DateTime.Parse("24/09/2020 22:02:52")))
                .OAuthHeader(new Uri(url), "get");
            var expected = "OAuth oauth_consumer_key=\"xvz1evFS4wEEPTGEFPHBog\",oauth_token=\"370773112-GmHxMAgYyLbNEtIKZeRNFsMKPR9EyMZeS9weJAEb\",oauth_signature_method=\"HMAC-SHA1\",oauth_timestamp=\"1600984972\",oauth_nonce=\"l44BEKnGlM5\",oauth_version=\"1.0\",oauth_signature=\"%2B4Rgl%2F1TKcf7G2ZsD3JSaLLm2w0%3D\"";

            Assert.Equal(expected, actual);
        }
    }

    public class StubDetails : IOAuthDetailsFactory
    {
        private readonly string nonce;
        private readonly DateTime dateTime;

        public StubDetails(string nonce, DateTime dateTime)
        {
            this.nonce = nonce;
            this.dateTime = dateTime;
        }

        public string Nonce() => this.nonce;
        public DateTime DateTimeNow() => this.dateTime;
    }

    public class StubConfig : IConfiguration
    {
        Dictionary<string, string> _conf;
        public StubConfig(
            string consumerKey, 
            string oauthToken, 
            string consumerSecret,
            string tokenSecret)
        {
            _conf = new[] {
                new { key = "TwitterOAuthConsumerKey", value = consumerKey },
                new { key = "TwitterOAuthToken", value = oauthToken },
                new { key = "TwitterOAuthConsumerSecret", value = consumerSecret },
                new { key = "TwitterOAuthTokenSecret", value = tokenSecret }
            }.ToDictionary(x => x.key, y => y.value);
        }

        public string this[string key] { 
            get => _conf[key]; 
            set => throw new NotImplementedException(); }

        public IEnumerable<IConfigurationSection> GetChildren()
        {
            return Enumerable.Empty<IConfigurationSection>();
        }

        public IChangeToken GetReloadToken()
        {
            throw new NotImplementedException();
        }

        public IConfigurationSection GetSection(string key)
        {
            throw new NotImplementedException();
        }
    }
}
