# BotF
[![Nuget](https://img.shields.io/nuget/v/Deployf.Botf)](https://www.nuget.org/packages/Deployf.Botf) [![GitHub](https://img.shields.io/github/license/deploy-f/botf)](https://github.com/deploy-f/botf/blob/master/LICENSE) [![CI](https://github.com/deploy-f/botf/actions/workflows/dotnet.yml/badge.svg)](https://github.com/deploy-f/botf/actions/workflows/dotnet.yml) [![Telegram Group](https://img.shields.io/endpoint?url=https%3A%2F%2Ftg.sumanjay.workers.dev%2Fbotf_community)](https://t.me/botf_community)  

Make beautiful and clear telegram bots with the asp.net-like architecture!

BotF has next features:

* long pooling and webhook mode without any changes in the code
* very convinient way to work with commands and reply buttons
* integrated pagination with buttons
* authentication and role-based authorization
* statemachine for complicated dialogs with users
* asp.net-like approach to develop bots
* automatic creating of command menu
* integrated DateTime picker
* auto sending
* good performance

## Documentaion

 (Under development) Visit to [our wiki](https://github.com/deploy-f/botf/wiki) to read botf documentation

 Good video on youtube https://www.youtube.com/watch?v=hieLnm9wO6s

## Install

```bash
dotnet add package Deployf.Botf
```

## Example

Put next code into `Program.cs` file

```csharp
using Deployf.Botf;

class Program : BotfProgram
{
    // It's boilerplate program entrypoint.
    // We just simplified all usual code into static method StartBot.
    // But in this case of starting of the bot, you should add a config section under "bot" key to appsettings.json
    public static void Main(string[] args) => StartBot(args);

    // Action attribute mean that you mark async method `Start`
    // as handler for user's text in message which equal to '/start' string.
    // You can name method as you want
    // And also, second argument of Action's attribute is a description for telegram's menu for this action
    [Action("/start", "start the bot")]
    public void Start()
    {
        // Just sending a reply message to user. Very simple, isn't?
        Push($"Send `{nameof(Hello)}` to me, please!");
    }

    [Action(nameof(Hello))]
    public void Hello()
    {
        Push("Hey! Thank you! That's it.");
    }

    // Here we handle all unknown command or just text sent from user
    [On(Handle.Unknown)]
    public async Task Unknown()
    {
        // Here, we use the so-called "buffering of sending message"
        // It means you dont need to construct all message in the string and send it once
        // You can use Push to just add the text to result message, or PushL - the same but with new line after the string.
        PushL("You know.. it's very hard to recognize your command!");
        PushL("Please, write a correct text. Or use /start command");

        // And finally send buffered message
        await Send();
    }
}
```

And replace content of `appsettings.json` with your bot username and token:

```
{
  "botf": "123456778990:YourToken"
}
```

And that's it! Veeery easy, isn't?  
Just run the program :)

Other examples you can find in `/Examples` folder.

## Hosting

After you develop your bot, you can deploy it to our hosting: [deploy-f.com](https://deploy-f.com)
