using Deployf.Botf.Controllers;
using Deployf.Botf.Extensions;


namespace Deployf.Botf
{
    public class BotfProgram : BotControllerBase
    {
        public static void StartBot(string[] args, bool skipHello = false, Action<IServiceCollection> onConfigure = null, Action<IApplicationBuilder> onRun = null)
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

            var botOptions = builder.Configuration.GetSection("bot").Get<BotfOptions>();
            builder.Services.AddBotf(botOptions);

            onConfigure?.Invoke(builder.Services);

            var app = builder.Build();
            app.UseBotf();

            onRun?.Invoke(app);

            app.Run();
        }
    }
}