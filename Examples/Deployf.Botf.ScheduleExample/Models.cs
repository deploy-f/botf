using SQLite;

public class Schedule
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public long OwnerId { get; set; }

    [Indexed]
    public long? UserId { get; set; }

    [Indexed]
    public DateTime From { get; set; }
    [Indexed]
    public DateTime To { get; set; }

    [Indexed]
    public State State { get; set; }

    public string? Comment { get; set; }
}

public class User
{
    [PrimaryKey]
    public long Id { get; set; }

    [Indexed]
    public string Username { get; set; } = String.Empty;

    public string FullName { get; set; } = String.Empty;

    public UserRole Roles { get; set; }
    public string? Timezone { get; set; }
}

public enum State
{
    Free,
    Requested,
    Booked,
    Canceled,
}

[Flags]
public enum UserRole
{
    none = 0,
    admin = 1,
    scheduler = 2
}