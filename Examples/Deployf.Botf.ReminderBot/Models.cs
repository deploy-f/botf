using SQLite;

public class Reminder
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public long ChatId { get; set; }

    [Indexed]
    public DateTime Time { get; set; }

    public WeekDay Repeating { get; set; }

    public string? Comment { get; set; }
}

public class User
{
    [PrimaryKey]
    public long Id { get; set; }

    [Indexed]
    public string Username { get; set; } = String.Empty;

    public string FullName { get; set; } = String.Empty;

    public string TimeZone { get; set; } = String.Empty;
}

public enum WeekDay
{
    Sunday = 1,
    Monday = 1 << 1,
    Tuesday = 1 << 2,
    Wednesday = 1 << 3,
    Thursday = 1 << 4,
    Friday = 1 << 5,
    Saturday = 1 << 6,
}