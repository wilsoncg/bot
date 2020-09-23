using Microsoft.Bot.Builder;
using LuisRecognizerOptions = Microsoft.Bot.Builder.AI.Luis.LuisRecognizerOptionsV3;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rachael.AzureFunction.Recognizer
{
    public class RachaelLuisRecognizer : IRecognizer
    {
        bool _isConfigured = false;
        LuisApplication _app;
        LuisRecognizer _recognizer;

        public RachaelLuisRecognizer(IConfiguration config)
        {
            var kv = 
                new[] { "AppId", "SubscriptionKey", "Domain" }
                .Select(x => new { key = x, value = config[$"Luis.{x}"] })
                .ToDictionary(x => x.key, y => y.value);
            _isConfigured = kv.All(x => !string.IsNullOrEmpty(x.Value));

            _app = new LuisApplication(kv["AppId"], kv["SubscriptionKey"], kv["Domain"]);
            _recognizer = new LuisRecognizer(new LuisRecognizerOptions(_app));
        }

        public virtual bool IsConfigured => _isConfigured;

        public async Task<RecognizerResult> RecognizeAsync(ITurnContext turnContext, CancellationToken cancellationToken) =>
            await _recognizer.RecognizeAsync(turnContext, cancellationToken);

        public async Task<T> RecognizeAsync<T>(ITurnContext turnContext, CancellationToken cancellationToken) where T : IRecognizerConvert, new() =>
            await _recognizer.RecognizeAsync<T>(turnContext, cancellationToken);
    }
}
