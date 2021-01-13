# Rachael - C# Azure Bot 

![Rachael](rachael-icon.png)

Features:
* Running on a consumption model as an Azure function **[v3.x](https://docs.microsoft.com/en-us/azure/azure-functions/functions-versions)**
* Connected to the [LUIS](https://eu.luis.ai) language understanding service **[v2.0](https://docs.microsoft.com/en-us/azure/cognitive-services/luis/luis-migration-api-v3)**
* Bot Framework SDK **[v4](https://github.com/microsoft/botframework-sdk)**
* Connected to twitter API **[v2](https://developer.twitter.com/en/docs/twitter-api/early-access)**
* Custom OAuth [ASP.NET DelegatingHandler](https://docs.microsoft.com/en-us/aspnet/web-api/overview/advanced/http-message-handlers)

### Running as an Azure Function, requires:
* Microsoft.NET.Sdk.Functions
* Microsoft.Bot.Build.Azure

### Run botframework-cli locally
```
docker build -t wilsoncg/bf-cli .
docker run wilsoncg/bf-cli luis
```

Here you can see Rachael in the bot framework emulator [v4](https://github.com/Microsoft/BotFramework-Emulator):

![Rachael running in the emulator](rachael-emulator.png)

Useful links:

* To get started https://blogs.msdn.microsoft.com/webdev/2017/05/10/azure-function-tools-for-visual-studio-2017/
* To learn how to debug Azure Bot Service bots, please visit https://aka.ms/bf-docs-azure-debug
* Microsoft applications: https://apps.dev.microsoft.com
* LUIS applications: https://eu.luis.ai/applications/
* Bot framework: https://dev.botframework.com/bots
* Resources viewer: https://resources.azure.com
* Full list of app settings variables: https://docs.microsoft.com/en-gb/azure/azure-functions/functions-app-settings
* Azure functions authentication/keys https://github.com/Azure/azure-functions-host/wiki/Http-Functions