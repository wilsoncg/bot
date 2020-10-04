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
using System.Linq;
using System.Net.Http;
using System.Text.Json;

namespace Rachael.AzureFunction.Dialogs
{
    public class AboutLuisDialog : LuisDialog<object>
    {
        readonly IHttpClientFactory _factory;

        public AboutLuisDialog(IHttpClientFactory factory, IConfiguration config)
            : base(
                  nameof(AboutLuisDialog), 
                  new List<ILuisService>() { GetLuisService(config) }.ToArray())
        {
            _factory = factory;
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
            /*var tweet = new Tweet
            {
                Id = "1302992864071356417",
                Text = "A 7,500sq metre reminder of the words of Vote Leave leader Michael Gove.                                                                \n(South Gare beach, Redcar, September 2019) https://t.co/pnqyZA36MB",
                CreatedAt = DateTime.Parse("2020 - 09 - 07T15: 31:21.000Z"),
                PreviewImageUrl = "https://pbs.twimg.com/ext_tw_video_thumb/1302989543709249538/pu/img/kuhetq58tdgLYiau.jpg",
                Username = "ByDonkeys",
                UserFriendlyName = "Led By Donkeys",
                UserProfileImageUrl = "https://pbs.twimg.com/profile_images/1085150433960706048/_T5T_iZY_normal.jpg"
            };*/
            var t = new Tweet(_factory);
            var recent = await t.GetTweetIdsForUserId("1073606435580325889");
            if (!recent.Any())
                return new DialogTurnResult(DialogTurnStatus.Complete);

            var tweets = await t.GetByTweetId(recent.First());
            var card = RenderTweetAsCardForSkype(tweets.First());
                        
            var message = MessageFactory.Attachment(new List<Attachment>());
            message.Text = "Here is a tweet you might find interesting:";
            message.Attachments.Add(card.ToAttachment());
            await context.Context.SendActivityAsync(message);
            return new DialogTurnResult(DialogTurnStatus.Complete);
        }

        class Tweet
        {
            private readonly IHttpClientFactory _factory;

            public Tweet()
            {
            }

            public Tweet(IHttpClientFactory factory)
            {
                _factory = factory;
            }

            public string Id { get; set; }
            public string Text { get; set; }
            public DateTime CreatedAt { get; set; }
            public string PreviewImageUrl { get; set; }
            public string Username { get; set; }
            public string UserFriendlyName { get; set; }
            public string UserProfileImageUrl { get; set; }

            public async Task<IEnumerable<long>> GetTweetIdsForUserId(string userId)
            {

                var client = _factory.CreateClient("Twitter");
                var d =
                    new Dictionary<string, string>()
                    {
                        { "user_id", userId },
                        { "count", "10" }
                    };
                var query = string.Join("&", d.Select(kv => $"{kv.Key}={kv.Value}"));
                var response = await client.GetAsync($"1.1/statuses/user_timeline.json?{query}");
                if (!response.IsSuccessStatusCode)
                    return new List<long>();

                var data = await response.Content.ReadAsStringAsync();
                var json = JsonSerializer.Deserialize<JsonTimelineData[]>(data);

                var items =
                    json == null ?
                        new List<long>() :
                        json.Select(x => x.id);
                return items;
            }

            public class JsonTimelineData { public long id { get; set; } }

            public async Task<IList<Tweet>> GetByTweetId(long tweetId)
            {
                var client = _factory.CreateClient("Twitter");
                var d = new Dictionary<string, string>()
                {
                    { "tweet.fields", "attachments,entities,author_id,created_at" },
                    { "expansions", "attachments.media_keys,author_id" },
                    { "media.fields", "duration_ms,height,media_key,preview_image_url,type,url,width" },
                    { "user.fields", "profile_image_url" },
                };
                var query = string.Join("&", d.Select(kv => $"{kv.Key}={kv.Value}"));
                var response = await client.GetAsync($"2/tweets/{tweetId}?{query}");
                if (!response.IsSuccessStatusCode)
                    return new List<Tweet>();

                var data = await response.Content.ReadAsStringAsync();
                var json = JsonSerializer.Deserialize<JSonSingleTweet>(data);

                return new List<Tweet>()
                {
                    new Tweet
                    {
                        Id = json.data.id,
                        Text = json.data.text,
                        CreatedAt = DateTime.Parse(json.data.created_at),
                        Username = json.includes.users[0].username,
                        UserFriendlyName = json.includes.users[0].name,
                        UserProfileImageUrl = json.includes.users[0].profile_image_url,
                        PreviewImageUrl = json.includes.media[0].preview_image_url
                    }
                };
            }

            class JSonSingleTweet
            {
                public JsonSingleTweet_Data data { get; set; }
                public JsonSingleTweet_Includes includes { get; set; }
            }

            class JsonSingleTweet_Data
            {
                public string id { get; set; }
                public string text { get; set; }
                public string created_at { get; set; }
            }

            class JsonSingleTweet_Includes
            {
                public Tweet_Media[] media { get; set; }
                public Tweet_Users[] users { get; set; }
            }

            class Tweet_Users
            {
                public string profile_image_url { get; set; }
                public string username { get; set; }
                public string name { get; set; }
            }

            class Tweet_Media
            {
                public string preview_image_url { get; set; }
            }
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
