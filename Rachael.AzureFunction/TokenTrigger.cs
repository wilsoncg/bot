using System;
using System.Text;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using System.Web.Http;

namespace Rachael.AzureFunction
{
    public class TokenTrigger
    {
        private readonly IHttpClientFactory _factory;
        private readonly string _secret;

        public TokenTrigger(IHttpClientFactory factory, IConfiguration config)
        {
            _factory = factory;
            _secret = config["DirectLineSecret"];
        }

        [FunctionName("Token")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]
            HttpRequestMessage req, 
            ILogger log,
            CancellationToken hostCancellationToken)
        {
            log.LogInformation("Token function triggered.");
            try
            {
                var client = _factory.CreateClient("BotFramework");
                var url = "/v3/directline/tokens/generate";
                var userId = $"dl_{Guid.NewGuid()}";
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _secret);
                request.Content = new StringContent(
                    JsonConvert.SerializeObject(
                        new { User = new { Id = userId }}),
                        Encoding.UTF8,
                        "application/json");
                
                var response = await client.SendAsync(request);

                if(!response.IsSuccessStatusCode)
                    return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
                
                var body = await response.Content.ReadAsStringAsync();
                var directLineToken = new { conversationId = "", token = "", expires_in = 0 };
                var deserializedDirectLineToken = JsonConvert.DeserializeAnonymousType(body, directLineToken);
                var mappedWithUserId = new { 
                    conversationId = deserializedDirectLineToken.conversationId, 
                    token = deserializedDirectLineToken.token, 
                    expires_in = deserializedDirectLineToken.expires_in, 
                    userId = userId 
                };
                return new OkObjectResult(mappedWithUserId);
            }
            catch (Exception ex)
            {
                log.LogError(ex.ToString());
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}