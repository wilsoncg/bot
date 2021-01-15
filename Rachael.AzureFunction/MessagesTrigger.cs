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

namespace Rachael.AzureFunction
{
    public class MessagesTrigger
    {
        readonly BotAdapter _botAdapter;
        readonly IHttpClientFactory _factory;
        readonly IConfiguration _config;

        public MessagesTrigger(IHttpClientFactory factory, IConfiguration config)
        {
            _factory = factory;
            _config = config;
            var simpleCredential =
                new SimpleCredentialProvider(
                        config.GetValue<string>("MicrosoftAppId"),
                        config.GetValue<string>("MicrosoftAppPassword"));
            _botAdapter = new BotFrameworkAdapter(simpleCredential)
                .UseBotState(new MessagesConversationState());
        }

        sealed class MessagesConversationState : ConversationState
        {
            public MessagesConversationState() : base(new MemoryStorage())
            {
            }

            public int TurnNumber { get; set; }
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
                    (x.Key.ToUpper() == "X-MS-CLIENT-PRINCIPAL-ID"))
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
            var claimsIdentity = new ClaimsIdentity();
            try
            {
                var response = await _botAdapter.ProcessActivityAsync(claimsIdentity, activity, BotLogic, hostCancellationToken);
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
                var state = turnContext.TurnState.Get<MessagesConversationState>(typeof(MessagesConversationState).FullName);
                await new RootDialog(_factory, _config).Run(
                    turnContext,
                    state.CreateProperty<DialogState>("DialogState"),
                    cancellationToken);
            }
        }
    }
}
