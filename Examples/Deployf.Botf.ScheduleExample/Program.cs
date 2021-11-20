using Deployf.Botf;
using SQLite;

BotfProgram.StartBot(args, onConfigure: (svc, cfg) =>
{
    var db = new SQLiteConnection("db.sqlite");

    db.CreateTable<User>();
    db.CreateTable<Schedule>();

    svc.AddSingleton(db.Table<User>());
    svc.AddSingleton(db.Table<Schedule>());
    svc.AddSingleton(db);
    svc.AddTransient<SlotService>();
    svc.AddSingleton<IBotUserService, UserService>();
});