using Deployf.Botf;
using Hangfire;
using Hangfire.Storage.SQLite;
using SQLite;

BackgroundJobServer? hangfireServer = null;

BotfProgram.StartBot(args, onConfigure: (svc, cfg) =>
{
    var db = new SQLiteConnection("db.sqlite");

    db.CreateTable<User>();
    db.CreateTable<Reminder>();


    svc.AddSingleton(db.Table<User>());
    svc.AddSingleton(db.Table<Reminder>());
    svc.AddSingleton(db);
    svc.AddSingleton<IBotUserService, UserService>();

    svc.AddHangfire(c => c.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
        .UseColouredConsoleLogProvider()
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseSQLiteStorage());
    svc.AddHangfireServer();

}, onRun: (app, cfg) =>
{
    app.UseHangfireDashboard();
});

hangfireServer!.Dispose();

class HangfireActivator : JobActivator
{
    readonly IServiceProvider _serviceProvider;
    public HangfireActivator(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;
    public override object? ActivateJob(Type type) => _serviceProvider.GetService(type);
}