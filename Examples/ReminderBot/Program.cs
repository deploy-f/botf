using Deployf.Botf;
using SQLite;

BotfProgram.StartBot(args, onConfigure: (svc, cfg) =>
{
    var db = new SQLiteConnection("db.sqlite");

    db.CreateTable<User>();

    svc.AddSingleton(db.Table<User>());
    svc.AddSingleton(db);
    svc.AddSingleton<IBotUserService, UserService>();
});