using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Newtonsoft.Json;

using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Logging;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector.Authentication;
using System.Security.Claims;
using Rachael.AzureFunction.Dialogs;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace Rachael.AzureFunction
{
    public class MessagesTrigger
    {
        readonly BotFrameworkAdapter _botAdapter;
        readonly IHttpClientFactory _factory;
        readonly IConfiguration _config;
        readonly ClaimsIdentity _claimsIdentity;

        public MessagesTrigger(
            IHttpClientFactory factory, 
            IConfiguration config,
            ICredentialProvider credentialProvider,
            BotFrameworkAdapter adapter)
        {
            _factory = factory;
            _config = config;
            _claimsIdentity = new ClaimsIdentity(
                new List<Claim> {
                    new Claim("appid", config.GetValue<string>("MicrosoftAppId")),
                    new Claim("aud", config.GetValue<string>("MicrosoftAppId")),
                    new Claim("ver", "1.0")
                }
            );
            _botAdapter = adapter;
        }

        [FunctionName("Messages")]
        public async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]
            HttpRequestMessage req, 
            ILogger log,
            CancellationToken hostCancellationToken)
        {
            log.LogInformation("Messages function triggered.");

            var jsonContent = await req.Content.ReadAsStringAsync();
            var claimsHeaders = 
                req.Headers
                .Where(x => 
                    (x.Key.ToUpper() == "X-MS-CLIENT-PRINCIPAL-NAME") ||
                    (x.Key.ToUpper() == "X-MS-CLIENT-PRINCIPAL-ID") || 
                    (x.Key.ToUpper() == "Authorization") ||
                    (x.Key.ToUpper() == "Authentication"))
                .Select(y => $"Header[${y.Key},{y.Value}]")
                .Aggregate(
                    new System.Text.StringBuilder(),
                    (a, s) => a.Append(s))
                .ToString();
            if(string.IsNullOrEmpty(claimsHeaders))
            {
                log.LogInformation($"ClaimsPrincipal headers: <empty>");
            }
            else
            {
                log.LogInformation($"ClaimsPrincipal headers: {claimsHeaders}");
            }            

            var activity = JsonConvert.DeserializeObject<Activity>(jsonContent);
            try
            {
                var response = await _botAdapter.ProcessActivityAsync(_claimsIdentity, activity, BotLogic, hostCancellationToken);
            }
            catch (Exception ex)
            {
                log.LogError(ex.ToString());
                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }
            
            return req.CreateResponse(HttpStatusCode.Accepted);
        }

        async Task BotLogic(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            if(turnContext.Activity.Type == ActivityTypes.Message)
            {
                var state = turnContext.TurnState.Get<ConversationState>(typeof(ConversationState).FullName);
                await new RootDialog(_factory, _config).Run(
                    turnContext,
                    state.CreateProperty<DialogState>("DialogState"),
                    cancellationToken);
            }
        }
    }
}
