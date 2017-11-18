using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;

namespace Rachael.AzureFunction.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Interface, AllowMultiple = true)]
    [Serializable]
    public class RachaelLuisModelAttribute : LuisModelAttribute
    {
        public RachaelLuisModelAttribute() : base(
            GetConfig("Luis.AppId", ""), 
            GetConfig("Luis.SubscriptionKey", ""), 
            LuisApiVersion.V2, 
            GetConfig("Luis.Domain", "westeurope.api.cognitive.microsoft.com"))
        {
            // Stop Luis saving user utterances
            Log = false;
        }

        private static string GetConfig(string keyName, string defaultValue)
        {
            var keyValue = ConfigurationManager.AppSettings.Get(keyName);
            if (string.IsNullOrEmpty(keyValue))
                return defaultValue;

            return keyValue;
        }
    }
}
