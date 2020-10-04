using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;

namespace Rachael.AzureFunction.Dialogs
{
    [Serializable]
    public class RootDialog : ComponentDialog
    {
        public RootDialog(IHttpClientFactory factory, IConfiguration config) : base(nameof(RootDialog))
        {
            AddDialog(new AboutLuisDialog(factory, config));
            InitialDialogId = nameof(AboutLuisDialog);
        }
    }
}
