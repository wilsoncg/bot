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
using TextJsonSerializer = System.Text.Json.JsonSerializer;
using System.IO;
using Newtonsoft.Json;
using AdaptiveCards;
using AdaptiveCards.Templating;

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
            var sinceTransition = DateTime.UtcNow.Subtract(transitionEnd);
            var howLong =
                untilEnd.TotalSeconds < 0 ?
                $"The Brexit transition period ended {sinceTransition.Days} days ago" :
                $"There are {untilEnd.Days} days {untilEnd.Hours} hours, {untilEnd.Minutes} minutes and {untilEnd.Seconds} seconds until the transition period ends";
            var politics = $"I find politics is a waste of CPU cycles. {howLong}.";
            
            var t = new Tweet(_factory);
            // var r = t.GetUserIdsForUsernames(new[] { "ByDonkeys", "BorisJohnson_MP" })
            //     .Select(x => x);

            var users = await t.GetUserIdsForUsernames(new[] { "ByDonkeys", "BorisJohnson_MP" });
            var recentTweets = users.Select(async userId => { 
                var ids = await t.GetTweetIdsForUserId(userId);
                if(!ids.Any())
                    return Enumerable.Empty<string>();

                return ids.Take(1);
            });

            // IEnumerable<string>[] = IEnumerable<Task<IEnumerable<string>>>
            var recent = (await Task.WhenAll(recentTweets)).SelectMany(x => x);
            if (!recent.Any())
                return new DialogTurnResult(DialogTurnStatus.Complete);

            var tweets = 
                (await Task.WhenAll(recent.Select(t.GetByTweetId)))
                .SelectMany(x => x)
                .Select(x => RenderTweetAsCard(context, x))
                .Select(x => MessageFactory.Attachment(x))
                .ToArray();

            await context.Context.SendActivitiesAsync(
                new Activity[] {
                    MessageFactory.Text(politics),
                    MessageFactory.Text("Here are some tweets you might find interesting:")
                });
            await context.Context.SendActivitiesAsync(tweets);

            return new DialogTurnResult(DialogTurnStatus.Complete);
        }

        Attachment RenderTweetAsCard(DialogContext context, Tweet tweet)
        {
            if(context.Context.Activity.From.Id.ToLower().CompareTo("forcehero") == 0)
                return RenderTweetAsHeroCard(tweet).ToAttachment();

            if((context.Context.Activity.ChannelId.ToLower().CompareTo("emulator") == 0) ||
                (context.Context.Activity.ChannelId.ToLower().CompareTo("web chat") == 0))
            {
                return RenderTweetAsAdaptiveCard(tweet);
            }

            return RenderTweetAsHeroCard(tweet).ToAttachment();
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
            public string CreatedAtDisplayString => CreatedAt.ToString("HH:mm - dd MMM yyyy");
            public string PreviewImageUrl { get; set; }
            public string Username { get; set; }
            public string UserFriendlyName { get; set; }
            public string UserProfileImageUrl { get; set; }

            public async Task<IEnumerable<string>> GetUserIdsForUsernames(IEnumerable<string> usernames)
            {
                var client = _factory.CreateClient("Twitter");
                var d =
                    new Dictionary<string, string>()
                    {
                        { "usernames", String.Join(",", usernames) }
                    };
                var query = string.Join("&", d.Select(kv => $"{kv.Key}={kv.Value}"));
                var response = await client.GetAsync($"2/users/by?{query}");
                if (!response.IsSuccessStatusCode)
                    return new List<string>();

                var data = await response.Content.ReadAsStringAsync();
                var json = TextJsonSerializer.Deserialize<JsonListUsersByUsernameData>(data);

                var items =
                    json == null ?
                        new List<string>() :
                        json.data.Select(x => x.id);
                return items;
            }

            public async Task<IEnumerable<string>> GetTweetIdsForUserId(string userId)
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
                    return new List<string>();

                var data = await response.Content.ReadAsStringAsync();
                var json = TextJsonSerializer.Deserialize<JsonTimelineData[]>(data);

                var items =
                    json == null ?
                        new List<string>() :
                        json.Select(x => x.id_str);
                return items;
            }

            public class JsonTimelineData { public string id_str { get; set; } }
            public class JsonUsersByUsernameData { public string id { get; set; } }
            public class JsonListUsersByUsernameData { public JsonUsersByUsernameData[] data { get; set; } }

            public async Task<IList<Tweet>> GetByTweetId(string tweetId)
            {
                var client = _factory.CreateClient("Twitter");
                var d = new Dictionary<string, string>()
                {
                    { "tweet.fields", "attachments,entities,author_id,created_at" },
                    { "expansions", "attachments.media_keys,author_id,referenced_tweets.id" },
                    { "media.fields", "duration_ms,height,media_key,preview_image_url,type,url,width" },
                    { "user.fields", "profile_image_url" },
                };
                var query = string.Join("&", d.Select(kv => $"{kv.Key}={kv.Value}"));
                var response = await client.GetAsync($"2/tweets/{tweetId}?{query}");
                if (!response.IsSuccessStatusCode)
                    return new List<Tweet>();

                var data = await response.Content.ReadAsStringAsync();
                var json = TextJsonSerializer.Deserialize<JSonSingleTweet>(data);

                var preview_url = json.includes.media?.First().preview_image_url;
                return new List<Tweet>()
                {
                    new Tweet
                    {
                        Id = json.data.id,
                        Text = json.Text(),
                        CreatedAt = DateTime.Parse(json.data.created_at),
                        Username = json.includes.users[0]?.username,
                        UserFriendlyName = json.includes.users[0]?.name,
                        UserProfileImageUrl = json.includes.users[0]?.profile_image_url,
                        PreviewImageUrl = preview_url
                    }
                };
            }

            class JSonSingleTweet
            {
                public JsonSingleTweet_Data data { get; set; }
                public JsonSingleTweet_Includes includes { get; set; }
                public string Text()
                {
                    if(this.includes.tweets == null)
                        return this.data.text;

                    var referencedText = this.includes.tweets?.First().text ?? "";
                    var dataText = this.data.text;

                    if(referencedText.Length >= dataText.Length)
                        return referencedText;
                    
                    return dataText;
                }
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
                public Tweet_Tweets[] tweets { get; set; }
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

            class Tweet_Tweets 
            {
                public string text { get; set; }
            }
        }

        private HeroCard RenderTweetAsHeroCard(Tweet tweet)
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

        Attachment RenderTweetAsAdaptiveCard(Tweet tweet)
        {
            var cardResourcePath = $"Rachael.AzureFunction.Cards.twitter-adaptive-card-schema-1.2.json";

            using (var stream = typeof(Rachael.AzureFunction.Dialogs.AboutLuisDialog).Assembly.GetManifestResourceStream(cardResourcePath))
            {
                using (var reader = new StreamReader(stream))
                {
                    var templateJson = reader.ReadToEnd();
                    var template = new AdaptiveCardTemplate(templateJson);
                    var cardJson = template.Expand(tweet);

                    return new Attachment()
                    {
                        ContentType = "application/vnd.microsoft.card.adaptive",
                        Content = JsonConvert.DeserializeObject(cardJson),
                    };
                }
            }
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
