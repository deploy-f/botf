using Telegram.Bot;
using Telegram.Bot.Framework;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
//
//
// #if !NET5_0
// using IApplicationBuilder = Microsoft.AspNetCore.Hosting.IWebHostBuilder;
// #endif

namespace Deployf.Botf;

public static class StartupExtensions
{
    public static IApplicationBuilder UseBotf(this IApplicationBuilder app, Action<BotBuilder>? onBeforeBuilder = null, Action<BotBuilder>? onAfterBuilder = null)
    {
        var opts = app.ApplicationServices.GetRequiredService<BotfOptions>();
        var builder = new BotBuilder();

        onBeforeBuilder?.Invoke(builder);

        builder
            .Use<BotControllersExceptionMiddleware>()
            .Use<BotControllersChainMiddleware>()
            .Use<BotControllersBeforeAllMiddleware>()
            .Use<BotControllersMiddleware>()
            .Use<BotControllersFSMMiddleware>()
            .Use<BotControllersAuthMiddleware>()
            .Use<BotControllersInvokeMiddleware>();

        onAfterBuilder?.Invoke(builder);

        builder.Use<BotControllersUnknownMiddleware>();

        if (!opts.UseWebhooks)
        {
            app.UseBotfLongPolling<BotfBot>(builder, startAfter: TimeSpan.FromSeconds(1));
        }
        else
        {
            app.UseWebhook(builder);
            app.BotfEnsureWebhookSet<BotfBot>();
        }

        var bot = app.ApplicationServices.GetRequiredService<BotfBot>();
        var routes = app.ApplicationServices.GetRequiredService<BotControllerRoutes>();
        var commands = routes.Where(c => c.command.StartsWith("/"))
            .Where(c => string.IsNullOrEmpty(c.info.Method.GetAuthPolicy()))
            .Where(c => c.info.Method.GetParameters().Length == 0)
            .Where(c => !string.IsNullOrEmpty(c.info.Method.GetActionDescription()))
            .Select(c => new BotCommand { Command = c.command, Description = c.info.Method.GetActionDescription()! })
            .ToList();

        bot.Client.SetMyCommandsAsync(commands)
            .ConfigureAwait(false)
            .GetAwaiter()
            .GetResult();


        return app;
    }

    public static IServiceCollection AddBotf(this IServiceCollection services, string connectionString)
    {
        var options = ConnectionString.Parse(connectionString);
        services.AddBotf(options);
        return services;
    }

    public static IServiceCollection AddBotf(this IServiceCollection services, BotfOptions options)
    {
        if(string.IsNullOrEmpty(options.Username))
        {
            var telegramClient = new TelegramBotClient(options.Token, baseUrl: options.ApiBaseUrl);
            var botUser = telegramClient.GetMeAsync()
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
            options.Username = botUser.Username;
        }

        var routes = BotControllerFactory.MakeRoutes(RouteStateSkipFunction.SkipFunctionFactory);

        var states = BotControllerFactory.MakeStates();
        var handlers = BotControllerFactory.MakeHandlers();
        var controllerTypes = routes.ControllerTypes()
            .Concat(states.ControllerTypes())
            .Concat(handlers.ControllerTypes())
            .Distinct();

        foreach (var type in controllerTypes)
        {
            services.AddScoped(type);
        }

        services.AddSingleton(routes);
        services.AddSingleton(states);
        services.AddSingleton(handlers);
        services.AddSingleton<PagingService>();

        services.AddSingleton<IKeyValueStorage, InMemoryKeyValueStorage>();
        services.AddSingleton<ChainStorage>();

        services.AddScoped<BotControllersMiddleware>();
        services.AddScoped<BotControllersChainMiddleware>();
        services.AddScoped<BotControllersFSMMiddleware>();
        services.AddScoped<BotControllersAuthMiddleware>();
        services.AddScoped<BotControllersInvokeMiddleware>();
        services.AddScoped<BotControllersExceptionMiddleware>();
        services.AddScoped<BotControllersUnknownMiddleware>();
        services.AddScoped<BotControllersBeforeAllMiddleware>();
        services.AddScoped<BotControllersInvoker>();

        services.AddScoped<IBotContextAccessor, BotContextAccessor>();

        services.AddScoped<BotUserService>();

        services.AddTransient<BotfBot>();
        services.AddSingleton(options);

        services.AddTransient(ctx => ctx.GetRequiredService<BotfBot>().Client);
        services.AddTransient<MessageSender>();

        services.AddSingleton<IArgumentBind, ArgumentBindInt32>();
        services.AddSingleton<IArgumentBind, ArgumentBindInt64>();
        services.AddSingleton<IArgumentBind, ArgumentBindBoolean>();
        services.AddSingleton<IArgumentBind, ArgumentBindSingle>();
        services.AddSingleton<IArgumentBind, ArgumentBindString>();
        services.AddSingleton<IArgumentBind, ArgumentBindDateTime>();
        services.AddSingleton<IArgumentBind, ArgumentBindEnum>();
        services.AddSingleton<IArgumentBind, ArgumentBindGuid>();
        services.AddSingleton<IArgumentBind, ArgumentAttributeBindState>();
        services.AddSingleton<ArgumentBinder>();

        return services;
    }

    internal static IApplicationBuilder UseBotfLongPolling<TBot>(this IApplicationBuilder app, IBotBuilder botBuilder,
        TimeSpan startAfter = default, CancellationToken cancellationToken = default
    )
        where TBot : BotBase
    {
        if (startAfter == default)
        {
            startAfter = TimeSpan.FromSeconds(2);
        }

        var updateManager = new CustomUpdatePollingManager<TBot>(botBuilder, new BotServiceProvider(app));

        var getUpdatesRequest = new GetUpdatesRequest()
        {
            Offset = 0,
            Timeout = 100,
            AllowedUpdates = new UpdateType[0]
        };

        Task.Run(LoongPooling, cancellationToken)
            .ContinueWith(t =>
            {
                LogException(t.Exception, "Bot long pooling task error");
                throw t.Exception ?? new Exception();
            }, TaskContinuationOptions.OnlyOnFaulted);

        return app;

        async Task LoongPooling()
        {
            await Task.Delay(startAfter, cancellationToken);
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                try
                {
                    await updateManager.RunAsync(getUpdatesRequest, cancellationToken: cancellationToken);
                }
                catch (Exception e)
                {
                    LogException(e, "Bot pooling manager handled exception. Restarting after 1 sec");
                    await Task.Delay(1000, cancellationToken);
                }
            }
        }

        void LogException(Exception? e, string message)
        {
            app.ApplicationServices.GetRequiredService<ILogger<BotfBot>>().LogCritical(default, e, message);
        }
    }

    internal static IApplicationBuilder BotfEnsureWebhookSet<TBot>(
        this IApplicationBuilder app
    )
        where TBot : IBot
    {
        using (var scope = app.ApplicationServices.CreateScope())
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<BotfBot>>();
            var bot = scope.ServiceProvider.GetRequiredService<TBot>();
            var options = scope.ServiceProvider.GetRequiredService<BotfOptions>();

            logger.LogInformation("Setting webhook to URL \"{0}\"", options.WebhookUrl);

            bot.Client.SetWebhookAsync(options.WebhookUrl!)
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
        }

        return app;
    }

    internal static IApplicationBuilder UseWebhook(this IApplicationBuilder app, IBotBuilder botBuilder)
    {
        var updateDelegate = botBuilder.Build();
        var conf = app.ApplicationServices.GetRequiredService<BotfOptions>();
        app.Map((PathString)conf.WebhookPath, builder => 
        {
            var type = Type.GetType("Telegram.Bot.Framework.TelegramBotMiddleware`1[Deployf.Botf.BotfBot], Telegram.Bot.Framework");
            builder.UseMiddleware(type, new [] { updateDelegate });
        });
        return app;
    }
}