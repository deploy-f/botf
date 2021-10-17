using Deployf.TgBot.Controllers;
using Deployf.TgBot.Services;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types;

namespace Deployf.TgBot.Extensions
{
    public static class StartupExtensions
    {
        public static IServiceCollection AddDeployfTgBot(this IServiceCollection services)
        {
            var routes = BotControllerFactory.MakeRoutes();
            var states = BotControllerFactory.MakeStates();
            var controllerTypes = routes.ControllerTypes()
                .Concat(states.ControllerTypes())
                .Distinct();

            foreach (var type in controllerTypes)
            {
                services.AddTransient(type);
            }
            services.AddSingleton(routes);
            services.AddSingleton(states);
            services.AddSingleton<IChatFSM, ChatFSM>();
            services.AddScoped<BotControllersMiddleware>();
            services.AddScoped<BotControllersFSMMiddleware>();
            services.AddScoped<BotControllersAuthMiddleware>();
            services.AddScoped<BotControllersInvokeMiddleware>();
            services.AddScoped<IBotContextAccessor, BotContextAccessor>();
            services.AddSingleton<IBotUserService, BotUserService>();

            return services;
        }
    }

    public static class UpdateContextExtensions
    {
        public static long GetChatId(this IUpdateContext context)
        {
            return context.Update.Message.Chat.Id;
        }

        public static long GetUserId(this IUpdateContext context)
        {
            return context.Update.Message.From.Id;
        }

        public static long? GetSafeChatId(this IUpdateContext context)
        {
            return context.Update.Message?.Chat.Id
                   ?? context.Update.EditedMessage?.Chat.Id
                   ?? context.Update.CallbackQuery?.Message?.Chat.Id;
        }

        public static long? GetSafeUserId(this IUpdateContext context)
        {
            return context.Update.Message?.From.Id
                   ?? context.Update.EditedMessage?.From.Id
                   ?? context.Update.CallbackQuery?.From.Id;
        }

        public static long UserId(this IUpdateContext context)
        {
            var value = context.Update.Message?.From.Id
                   ?? context.Update.EditedMessage?.From.Id
                   ?? context.Update.CallbackQuery?.From.Id;

            return value.Value;
        }

        public static int? GetMessageId(this IUpdateContext context)
        {
            return context.Update.Message?.MessageId;
        }

        public static int? GetCallbackMessageId(this IUpdateContext context)
        {
            return context.Update.CallbackQuery.Message?.MessageId;
        }

        public static int? GetSafeMessageId(this IUpdateContext context)
        {
            return context.Update.Message?.MessageId
                   ?? context.Update.CallbackQuery?.Message.MessageId
                   ?? context.Update.EditedMessage?.MessageId;
        }

        public static string GetSafeTextPayload(this IUpdateContext context)
        {
            return context.Update.Message?.Text
                   ?? context.Update.CallbackQuery?.Data;
        }

        public static CallbackQuery GetCallbackQuery(this IUpdateContext context)
        {
            return context.Update.CallbackQuery;
        }

        public static long GetCallbackQueryChatId(this IUpdateContext context)
        {
            return context.Update.CallbackQuery.Message.Chat.Id;
        }

        public static string GetTypeValue(this IUpdateContext context)
        {
            return context.Update.Message?.Text
                   ?? context.Update.CallbackQuery?.Data
                   ?? context.Update.EditedMessage?.Text;
        }

        public static string GetUsername(this IUpdateContext context)
        {
            return context.Update.Message?.Chat.Username
                   ?? context.Update.CallbackQuery?.Message.Chat.Username
                   ?? context.Update.EditedMessage?.Chat.Username;
        }
    }
}
