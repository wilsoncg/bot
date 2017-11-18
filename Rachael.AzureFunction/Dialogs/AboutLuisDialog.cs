using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rachael.AzureFunction.Attributes;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Connector;

namespace Rachael.AzureFunction.Dialogs
{
    [RachaelLuisModel]
    [Serializable]
    public class AboutLuisDialog : LuisDialog<object>
    {
        public AboutLuisDialog() { }
        public AboutLuisDialog(ILuisService service) : base(service) { }

        [LuisIntent("")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            var intro = $"I'm Rachael.";
            await context.PostAsync(intro);

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
            var gif = context.MakeMessage();
            gif.Attachments.Add(animationCard.ToAttachment());
            await context.PostAsync(gif);

            var more = "I can tell you more about me.";
            await context.PostAsync(more);
            context.Wait(MessageReceived);
        }

        [LuisIntent("About.Incept")]
        public async Task AboutIncept(IDialogContext context, LuisResult result)
        {
            await context.PostAsync($"My incept date is May 26th 2017.");
        }

        [LuisIntent("About.Model")]
        public async Task AboutModel(IDialogContext context, LuisResult result)
        {
            await context.PostAsync($"I'm a Nexus 7, N7FAA52318. More human than human.");
        }

        [LuisIntent("About.Creator")]
        public async Task AboutCreator(IDialogContext context, LuisResult result)
        {
            var creator = "I was created by Eldon Tyrell.";
            await context.PostAsync(creator);
            var createrHeroCard = new HeroCard
            {
                Title = "Dr. Eldon Tyrell",
                Images = new List<CardImage>
                {
                    new CardImage("https://vignette.wikia.nocookie.net/bladerunner/images/2/22/Tyrell_288x288.jpg/revision/latest?cb=20110421013428")
                },
                Buttons = new List<CardAction>
                {
                    new CardAction(ActionTypes.OpenUrl, "Find out more", value: "http://bladerunner.wikia.com/wiki/Eldon_Tyrell")
                }
            };
            var createrInfo = context.MakeMessage();
            createrInfo.Attachments.Add(createrHeroCard.ToAttachment());
            await context.PostAsync(createrInfo);
        }
    }
}
