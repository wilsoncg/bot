using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Configuration;
using System;

namespace Rachael.AzureFunction.Dialogs
{
    [Serializable]
    public class RootDialog : ComponentDialog
    {
        public RootDialog(IConfiguration config) : base(nameof(RootDialog))
        {
            AddDialog(new AboutLuisDialog(config));
            InitialDialogId = nameof(AboutLuisDialog);
        }
    }
}
