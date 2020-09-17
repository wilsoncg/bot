using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Rachael.AzureFunction.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Autofac;

namespace Rachael.AzureFunction
{
    [BotAuthentication]
    public static class Messages
    {
        [FunctionName("Messages")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]
            HttpRequestMessage req, 
            TraceWriter log)
        {
            log.Info("Messages function triggered.");

            using (BotService.Initialize())
            {
                string jsonContent = await req.Content.ReadAsStringAsync();
                var activity = JsonConvert.DeserializeObject<Activity>(jsonContent);

                if (!await BotService.Authenticator.TryAuthenticateAsync(req, new[] { activity }, CancellationToken.None))
                {
                    return BotAuthenticator.GenerateUnauthorizedResponse(req);
                }

                if (activity != null)
                {
                    // one of these will have an interface and process it
                    switch (activity.GetActivityType())
                    {
                        case ActivityTypes.Message:
                            await Conversation.SendAsync(activity, () => new AboutLuisDialog());
                            break;
                        case ActivityTypes.ConversationUpdate:
                            IConversationUpdateActivity update = activity;
                            using (var scope = DialogModule.BeginLifetimeScope(Conversation.Container, activity))
                            {
                                var client = scope.Resolve<IConnectorClient>();
                                if (update.MembersAdded.Any())
                                {
                                    var reply = activity.CreateReply();
                                    var newMembers = update.MembersAdded?.Where(t => t.Id != activity.Recipient.Id);
                                    foreach (var newMember in newMembers)
                                    {
                                        reply.Text = "Welcome";
                                        if (!string.IsNullOrEmpty(newMember.Name))
                                        {
                                            reply.Text += $" {newMember.Name}";
                                        }
                                        reply.Text += "!";
                                        await client.Conversations.ReplyToActivityAsync(reply);
                                    }
                                }
                            }
                            break;
                        case ActivityTypes.ContactRelationUpdate:
                        case ActivityTypes.Typing:
                        case ActivityTypes.DeleteUserData:
                        case ActivityTypes.Ping:
                        default:
                            log.Error($"Unknown activity type ignored: {activity.GetActivityType()}");
                            break;
                    }
                }
                return req.CreateResponse(HttpStatusCode.Accepted);
            }
        }
    }
}
