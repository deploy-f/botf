namespace Deployf.Botf.ScheduleExample;

public class ReminderJob
{
    readonly MessageBuilder Message;
    readonly MessageSender Sender;
    readonly ILogger<ReminderJob> Log;

    public ReminderJob(MessageSender sender, ILogger<ReminderJob> log)
    {
        Message = new MessageBuilder();
        Sender = sender;
        Log = log;
    }

    public async Task Exec(Reminder reminder)
    {
        try
        {
            Message.Push(reminder.Comment!);
            Message.SetChatId(reminder.ChatId);
            await Sender.Send(Message);
        }
        catch(Exception ex)
        {
            Log.LogError(ex, "Exception in ReminderJob");
        }
    }
}