using Deployf.Botf;
using SQLite;

// arrange
// schedule
// approve or reject
// search for meets and edit it

//      /|  ---
//    -- | |   |
//   |   | | --
//    -- | |   |
//   |   | | --
//    ---  |/
//


BotfProgram.StartBot(args, onConfigure: (svc, cfg) =>
{
    var db = new SQLiteConnection("db.sqlite");

    db.CreateTable<User>();
    db.CreateTable<Schedule>();

    svc.AddSingleton(db.Table<User>());
    svc.AddSingleton(db.Table<Schedule>());
    svc.AddSingleton(db);
    svc.AddTransient<ScheduleService>();
    svc.AddSingleton<IBotUserService, UserService>();
});