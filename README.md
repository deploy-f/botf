# BotF
[![Nuget](https://img.shields.io/nuget/v/Deployf.Botf)](https://www.nuget.org/packages/Deployf.Botf) [![GitHub](https://img.shields.io/github/license/deploy-f/botf)](https://github.com/deploy-f/botf/blob/master/LICENSE) [![CI](https://github.com/deploy-f/botf/actions/workflows/dotnet.yml/badge.svg)](https://github.com/deploy-f/botf/actions/workflows/dotnet.yml) [![Telegram Group](https://img.shields.io/endpoint?url=https%3A%2F%2Ftg.sumanjay.workers.dev%2Fbotf_community)](https://t.me/botf_community)  

🤘 Make beautiful and clear telegram bots with the asp.net-like architecture!

BotF has next features:

* long pooling and webhook mode without any changes in the code
* very convinient way to work with commands and reply/keyboard buttons
* integrated pagination with buttons
* authentication and role-based authorization
* statemachine for complicated dialogs with users
* asp.net-like approach to develop bots
* automatic creating of command menu
* integrated DateTime picker
* auto sending
* good performance

 There is a good video on youtube(in russian) https://www.youtube.com/watch?v=hieLnm9wO6s

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

    // Action attribute mean that you mark method `Start`
    // as handler for user's text in message which equal to '/start'.
    // You can name method as you want
    // And also second argument of Action's attribute is a description for telegram's menu for this action
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
    public void Unknown()
    {
        // Here, we use the so-called "buffering of sending message"
        // It means you dont need to construct all message in the string and send it once
        // You can use Push to just add the text to result message, or PushL - the same but with new line after the string.
        PushL("You know.. it's very hard to recognize your command!");
        PushL("Please, write a correct text. Or use /start command");
    }
}
```

And replace content of `appsettings.json` with your bot-token:

```
{
  "botf": "123456778990:YourToken"
}
```

And that's it! Veeery easy, isn't?  
Just run the program.

Other examples you can find in `/Examples` folder.

## Documentation

Here is a documentation for all features of BotF

### Make the program

You can go in two ways:
1. Inherit from `BotfProgram` or just call static method `StartBot(args)` to make simple bot only with message handliers
2. Or construct asp.net web api application from scratch. Let's call it 'advanced method'

#### `StartBot` method

Signature of method:
```csharp
void StartBot(
     string[] args,
     bool skipHello = false,
     Action<IServiceCollection, IConfiguration>? onConfigure = null,
     Action<IApplicationBuilder, IConfiguration>? onRun = null,
     BotfOptions options = null
)
```
* `args` - _required_ pass the args from `main` methods
* `skipHello` - if you want to avoid hello message in console output so pass `true`
* `onConfigure` - _optional_ calls to register your dependencies in internal DI Contaoner, second argument of delegate - asp.net configuration service
* `onRun` - _optional_, calls to add the required middlewares in asp.net's pipeline 
* `options`- _optional_, pass your custom configuration of bot if you need it

#### Advanced method

If you have a complex project written on asp.net core you can add BotF there just with 2 calls:
1. Call `AddBotf` in `ConfigureServices` on di container in startup class
2. Call `UseBotf` in `Configure` in startup class

`AddBotf` takes botf options, you can construct it yourself or as result of the call `ConnectionString.Parse("connection_string_here")`:
```csharp
var botOptions = ConnectionString.Parse(builder.Configuration["botf"]);
services.AddBotf(botOptions);
```

### Connection string

We use prety simple connection string to configure the framework. The format of this sthing is like:
```
yout_bot_token?key1=value1&key2=value2...
```

if you only need to pass the bot token, you can only specify it in the connection string.  

You can configure next parameters:
| Key | Description |
|---|---|
| `autosend` | Determines whether the mode is on or off  <br>  <br>`1` - enabled (default value)  <br>`0` - disabled |
| `webhook` | Url to the webhook. If is not defined then will be used long pooling mode  <br>  <br>example: `https://awnfuql.ngrok.io/you_random_path_to_web_hook` |
| `api` | Url to custom telegram api. Useful if you need to work through local bot api server |

### Handling the updates

At first you need to create class controller and itherit it from BotController.
Then add method and let the `Action` attribute

Action attribute takes 2 parameters:
* `Template` - the template for message or callback data if it is a callback of the button.
If you want make a handler of the command just use full version `/command` as template
* `Description`

```charp
class FirstController : BotController {
    void Method
}
```

#### Actions

#### Buttons

### Sending the messages

### Controllers

### Message Builder

### Authorization

### Calendar

### Pagging

### Auto sending

### Handling the special cases

#### Unknown message

#### Unauthorized

#### Callbacks before and after all handlers

#### Error handling

### Chain mode


## Hosting

After you developed your bot, you can deploy it to our hosting: [deploy-f.com](https://deploy-f.com)
