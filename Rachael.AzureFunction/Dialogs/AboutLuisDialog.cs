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

        [LuisIntent("Fun.Brexit")]
        public async Task FunBrexit(IDialogContext context, LuisResult result)
        {
            var transitionEnd = new DateTime(2020, 12, 31, 23, 59, 59);
            var untilEnd = transitionEnd.Subtract(DateTime.UtcNow);
            var howLong =
                untilEnd.TotalSeconds < 0 ?
                "the transition period has ended" :
                $"there are {untilEnd.Days} days {untilEnd.Hours} hours, {untilEnd.Minutes} minutes and {untilEnd.Seconds} seconds until the transition period ends";
            var politics = $"I find politics is a waste of CPU cycles. However, {howLong}.";
            await context.PostAsync(politics);

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
                        
            var message = context.MakeMessage();
            message.Text = "Here is a tweet you might find interesting:";
            message.Attachments.Add(card.ToAttachment());
            await context.PostAsync(message);
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
                        value: $"https://twitter.com/status/{tweet.Id}")
                }
            };
        }
    }
}
