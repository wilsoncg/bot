using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace Rachael.AzureFunction.Dialogs
{
    // https://gph.is/1QQH2a2
    // incept date May 26, 2017
    [Serializable]
    public class AnimationDialog : IDialog<object>
    {
        public Task StartAsync(IDialogContext context)
        {
            try
            {
                context.Wait(MessageReceivedAsync);
            }
            catch (OperationCanceledException error)
            {
                return Task.FromCanceled(error.CancellationToken);
            }
            catch (Exception error)
            {
                return Task.FromException(error);
            }

            return Task.CompletedTask;
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = context.MakeMessage();
            var attachment = GetAnimationCard();
            message.Attachments.Add(attachment);
            await context.PostAsync(message);
            context.Wait(this.MessageReceivedAsync);
        }

        private static Attachment GetAnimationCard()
        {
            var animationCard = new AnimationCard
            {
                Media = new List<MediaUrl>
                {
                    new MediaUrl()
                    {
                        Url = "https://media.giphy.com/media/4BhmY3ZsKN5q8/giphy.gif"
                    }
                }
            };

            return animationCard.ToAttachment();
        }        
    }
}
