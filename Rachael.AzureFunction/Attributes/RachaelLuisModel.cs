using System;
using System.Configuration;
using Bot.Builder.Community.Dialogs.Luis;
using Microsoft.Extensions.Configuration;

namespace Rachael.AzureFunction.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Interface, AllowMultiple = true)]
    [Serializable]
    public class RachaelLuisModelAttribute : LuisModelAttribute
    {
        public RachaelLuisModelAttribute(IConfiguration config) : base(
            config["Luis.AppId"], 
            config["Luis.SubscriptionKey"], 
            LuisApiVersion.V2, 
            config["Luis.Domain"])
        {
            // Stop Luis saving user utterances
            Log = false;
        }
    }
}
