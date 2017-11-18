using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace Rachael.AzureFunction.Dialogs
{
    [Serializable]
    public class AnimationDialog : IDialog<object>
    {
        private string _url;
        public AnimationDialog(string url)
        {
            _url = url;
        }

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
            context.Wait(MessageReceivedAsync);
        }

        private Attachment GetAnimationCard()
        {
            var animationCard = new AnimationCard
            {
                Media = new List<MediaUrl>
                {
                    new MediaUrl()
                    {
                        Url = _url
                    }
                }
            };

            return animationCard.ToAttachment();
        }        
    }
}
