using Telegram.Bot.Framework;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

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
            app.UseTelegramBotWebhook<BotfBot>(builder);
            app.BotfEnsureWebhookSet<BotfBot>();
        }

        var bot = app.ApplicationServices.GetRequiredService<BotfBot>();
        var routes = app.ApplicationServices.GetRequiredService<BotControllerRoutes>();
        var commands = routes.Where(c => c.command.StartsWith("/"))
            .Where(c => string.IsNullOrEmpty(c.action.GetAuthPolicy()))
            .Where(c => c.action.GetParameters().Length == 0)
            .Where(c => !string.IsNullOrEmpty(c.action.GetActionDescription()))
            .Select(c => new BotCommand { Command = c.command, Description = c.action.GetActionDescription() })
            .ToList();

        bot.Client.SetMyCommandsAsync(commands).GetAwaiter().GetResult();


        return app;
    }

    public static IServiceCollection AddBotf(this IServiceCollection services, BotfOptions options)
    {
        var routes = BotControllerFactory.MakeRoutes();
        var states = BotControllerFactory.MakeStates();
        var handlers = BotControllerFactory.MakeHandlers();
        var controllerTypes = routes.ControllerTypes()
            .Concat(states.ControllerTypes())
            .Concat(handlers.ControllerTypes())
            .Distinct();

        foreach (var type in controllerTypes)
        {
            services.AddTransient(type);
        }

        services.AddSingleton(routes);
        services.AddSingleton(states);
        services.AddSingleton(handlers);
        services.AddSingleton<PagingService>();
        services.AddSingleton<IChatFSM, ChatFSM>();
        services.AddSingleton<IUserKVStorage, UserKVStorage>();
        services.AddScoped<BotControllersMiddleware>();
        services.AddScoped<BotControllersFSMMiddleware>();
        services.AddScoped<BotControllersAuthMiddleware>();
        services.AddScoped<BotControllersInvokeMiddleware>();
        services.AddScoped<BotControllersExceptionMiddleware>();
        services.AddScoped<BotControllersUnknownMiddleware>();
        services.AddScoped<BotControllersBeforeAllMiddleware>();
        services.AddScoped<BotControllersInvoker>();
        services.AddScoped<IBotContextAccessor, BotContextAccessor>();
        services.AddSingleton<BotUserService>();
        services.AddTransient<BotfBot>();
        services.AddSingleton(options);
        services.AddTransient<HttpClient>();
        services.AddTransient(ctx => ctx.GetRequiredService<BotfBot>().Client);
        services.AddTransient<MessageSender>();

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
            AllowedUpdates = new[]
            {
                    UpdateType.Message,
                    UpdateType.CallbackQuery,
                    UpdateType.EditedMessage
                }
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

            bot.Client.SetWebhookAsync(options.WebhookUrl)
                .GetAwaiter().GetResult();
        }

        return app;
    }
}