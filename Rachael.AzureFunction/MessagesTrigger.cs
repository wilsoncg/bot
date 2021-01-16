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

        public MessagesTrigger(
            IHttpClientFactory factory, 
            IConfiguration config,
            ICredentialProvider credentialProvider,
            BotFrameworkAdapter adapter)
        {
            _factory = factory;
            _config = config;
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

            var authHeader = 
                String.Join(' ',
                    req.Headers
                    .Where(x => (x.Key.ToUpper() == "AUTHORIZATION"))
                    .Select(x => x.Value)
                    .SelectMany(x => x)
                );
            var activity = JsonConvert.DeserializeObject<Activity>(jsonContent);
            log.LogInformation($"ChannelId: {activity.ChannelId}");
            try
            {
                var response = await _botAdapter.ProcessActivityAsync(authHeader, activity, BotLogic, hostCancellationToken);
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
