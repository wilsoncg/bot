using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Rachael.AzureFunction.Attributes;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Bot.Builder.Community.Dialogs.Luis;
using Bot.Builder.Community.Dialogs.Luis.Models;
using Microsoft.Extensions.Configuration;

namespace Rachael.AzureFunction.Dialogs
{
    public class AboutLuisDialog : LuisDialog<object>
    {
        public AboutLuisDialog(IConfiguration config)
            : base(
                  nameof(AboutLuisDialog), 
                  new List<ILuisService>() { GetLuisService(config) }.ToArray())
        {
        }

        static ILuisService GetLuisService(IConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            var luisModel = new RachaelLuisModelAttribute(configuration);
            return new LuisService(luisModel);
        }

        [LuisIntent("")]
        public async Task<DialogTurnResult> None(DialogContext context, LuisResult result)
        {
            var intro = $"I'm Rachael.";
            await context.Context.SendActivityAsync(intro);

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
            var gif = MessageFactory.Attachment(new List<Attachment>());
            gif.Attachments.Add(animationCard.ToAttachment());
            await context.Context.SendActivityAsync(gif);
            return new DialogTurnResult(DialogTurnStatus.Complete);
        }

        [LuisIntent("About.Incept")]
        public async Task<DialogTurnResult> AboutIncept(DialogContext context, LuisResult result)
        {
            await context.Context.SendActivityAsync($"My incept date is May 26th 2017.");
            return new DialogTurnResult(DialogTurnStatus.Complete);
        }

        [LuisIntent("About.Model")]
        public async Task<DialogTurnResult> AboutModel(DialogContext context, LuisResult result)
        {
            await context.Context.SendActivityAsync($"I'm a Nexus 7, N7FAA52318. More human than human.");
            return new DialogTurnResult(DialogTurnStatus.Complete);
        }

        [LuisIntent("About.Creator")]
        public async Task<DialogTurnResult> AboutCreator(DialogContext context, LuisResult result)
        {
            var creator = "I was created by Eldon Tyrell.";
            await context.Context.SendActivityAsync(creator);
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
            var createrInfo = MessageFactory.Attachment(new List<Attachment>());
            createrInfo.Attachments.Add(createrHeroCard.ToAttachment());
            await context.Context.SendActivityAsync(createrInfo);
            return new DialogTurnResult(DialogTurnStatus.Complete);
        }

        [LuisIntent("Fun.Brexit")]
        public async Task<DialogTurnResult> FunBrexit(DialogContext context, LuisResult result)
        {
            var transitionEnd = new DateTime(2020, 12, 31, 23, 59, 59);
            var untilEnd = transitionEnd.Subtract(DateTime.UtcNow);
            var howLong =
                untilEnd.TotalSeconds < 0 ?
                "the transition period has ended" :
                $"there are {untilEnd.Days} days {untilEnd.Hours} hours, {untilEnd.Minutes} minutes and {untilEnd.Seconds} seconds until the transition period ends";
            var politics = $"I find politics is a waste of CPU cycles. However, {howLong}.";
            await context.Context.SendActivityAsync(politics);

            //if (context.Activity.ChannelId == "skype")
            //{
                var tweet = new Tweet
                {
                    Id = "1302992864071356417",
                    Text = "A 7,500sq metre reminder of the words of Vote Leave leader Michael Gove.                                                                \n(South Gare beach, Redcar, September 2019) https://t.co/pnqyZA36MB",
                    CreatedAt = DateTime.Parse("2020 - 09 - 07T15: 31:21.000Z"),
                    PreviewImageUrl = "https://pbs.twimg.com/ext_tw_video_thumb/1302989543709249538/pu/img/kuhetq58tdgLYiau.jpg",
                    Username = "ByDonkeys",
                    UserFriendlyName = "Led By Donkeys",
                    UserProfileImageUrl = "https://pbs.twimg.com/profile_images/1085150433960706048/_T5T_iZY_normal.jpg"
                };
                var card = RenderTweetAsCardForSkype(tweet);
            //}
                        
            var message = MessageFactory.Attachment(new List<Attachment>());
            message.Text = "Here is a tweet you might find interesting:";
            message.Attachments.Add(card.ToAttachment());
            await context.Context.SendActivityAsync(message);
            return new DialogTurnResult(DialogTurnStatus.Complete);
        }

        class Tweet
        {
            public string Id { get; set; }
            public string Text { get; set; }
            public DateTime CreatedAt { get; set; }
            public string PreviewImageUrl { get; set; }
            public string Username { get; set; }
            public string UserFriendlyName { get; set; }
            public string UserProfileImageUrl { get; set; }
        }

        private HeroCard RenderTweetAsCardForSkype(Tweet tweet)
        {
            var month = tweet.CreatedAt.ToString("MMM");
            return new HeroCard
            {
                Title = $"{tweet.UserFriendlyName} @{tweet.Username} - {month} {tweet.CreatedAt.Day}",
                Text = tweet.Text,
                Images = new List<CardImage>
                {
                    new CardImage($"{tweet.PreviewImageUrl}")
                },
                Buttons = new List<CardAction>
                {
                    new CardAction(ActionTypes.OpenUrl, "View tweet",
                        value: $"https://twitter.com/{tweet.Username}/status/{tweet.Id}")
                }
            };
        }

        protected override async Task<DialogTurnResult> OnContinueDialogAsync(
            DialogContext innerDc, 
            CancellationToken cancellationToken = default)
        {
            var childResult = await innerDc.ContinueDialogAsync(cancellationToken);
            var result = childResult.Result as DialogTurnResult;
            return await base.OnContinueDialogAsync(innerDc, cancellationToken);
        }
    }
}
