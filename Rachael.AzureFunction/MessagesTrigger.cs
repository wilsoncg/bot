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
using Microsoft.Bot.Configuration;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector.Authentication;
using System.Configuration;
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
            _botAdapter = 
                new BotFrameworkAdapter(
                    new SimpleCredentialProvider(
                        config.GetValue<string>("MicrosoftAppId"),
                        config.GetValue<string>("MicrosoftAppPassword")))
                .UseBotState(new MessagesConversationState());
        }

        class Bot : IBot
        {
            public Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
            {
                return null;
            }
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
