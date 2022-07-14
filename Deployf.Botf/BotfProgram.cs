using Microsoft.AspNetCore;

namespace Deployf.Botf;

#if NET5_0
public class BotfProgram : BotController
{
    public static void StartBot(
        string[] args,
        bool skipHello = false,
        Action<IServiceCollection, IConfiguration>? onConfigure = null,
        Action<IApplicationBuilder, IConfiguration>? onRun = null,
        BotfOptions options = null)
    {
        if (!skipHello)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("===");
            Console.WriteLine("  DEPLOY-F BotF");
            Console.WriteLine("  Botf is a telegram bot framework with asp.net-like architecture");
            Console.WriteLine("  For more information visit https://github.com/deploy-f/botf");
            Console.WriteLine("===");
            Console.WriteLine("");
            Console.ResetColor();
        }

        var builder = WebApplication.CreateBuilder(args);

        var botOptions = options;

        if(botOptions == null && builder.Configuration["bot"] != null)
        {
            botOptions = builder.Configuration.GetSection("bot").Get<BotfOptions>();
        }

        var connectionString = builder.Configuration["botf"];
        if (botOptions == null && connectionString != null)
        {
            botOptions = ConnectionString.Parse(connectionString);
        }

        builder.Services.AddBotf(botOptions);
        builder.Services.AddHttpClient();

        onConfigure?.Invoke(builder.Services, builder.Configuration);

        var app = builder.Build();
        app.UseBotf();

        onRun?.Invoke(app, builder.Configuration);

        app.Run();
    }

    public static void StartBot<TBotService>(
        string[] args,
        bool skipHello = false,
        Action<IServiceCollection, IConfiguration>? onConfigure = null,
        Action<IApplicationBuilder, IConfiguration>? onRun = null) where TBotService : class, IBotUserService
    {
        StartBot(args, skipHello, (svc, cfg) =>
        {
            onConfigure?.Invoke(svc, cfg);
            svc.AddTransient<IBotUserService, TBotService>();
        }, onRun);
    }
}
#else
public class BotfProgram : BotController
{
    public static void StartBot(
        string[] args,
        bool skipHello = false,
        Action<IServiceCollection, IConfiguration>? onConfigure = null,
        Action<IApplicationBuilder, IConfiguration>? onRun = null,
        BotfOptions options = null)
    {
        if (!skipHello)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("===");
            Console.WriteLine("  DEPLOY-F BotF");
            Console.WriteLine("  Botf is a telegram bot framework with asp.net-like architecture");
            Console.WriteLine("  For more information visit https://github.com/deploy-f/botf");
            Console.WriteLine("===");
            Console.WriteLine("");
            Console.ResetColor();
        }

        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder
                    .ConfigureServices((context, collection) =>
                    {
                        var botOptions = options;

                        var config = context.Configuration;

                        if (botOptions == null && config["bot"] != null)
                        {
                            botOptions = config.GetSection("bot").Get<BotfOptions>();
                        }

                        var connectionString = config["botf"];
                        if (botOptions == null && connectionString != null)
                        {
                            botOptions = ConnectionString.Parse(connectionString);
                        }

                        collection.AddBotf(botOptions);
                        collection.AddHttpClient();

                        onConfigure?.Invoke(collection, config);
                    })
                    .Configure((context, builder) =>
                    {
                        builder.UseBotf();
                        onRun?.Invoke(builder, context.Configuration);
                    });
            })
            .Build()
            .Run();
    }

    public static void StartBot<TBotService>(
        string[] args,
        bool skipHello = false,
        Action<IServiceCollection, IConfiguration>? onConfigure = null,
        Action<IApplicationBuilder, IConfiguration>? onRun = null) where TBotService : class, IBotUserService
    {
        StartBot(args, skipHello, (svc, cfg) =>
        {
            onConfigure?.Invoke(svc, cfg);
            svc.AddTransient<IBotUserService, TBotService>();
        }, onRun);
    }
}
#endif