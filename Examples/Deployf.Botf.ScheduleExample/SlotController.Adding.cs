namespace Deployf.Botf.ScheduleExample;

/// Part of adding single part
partial class SlotController
{
    [Action]
    void AddSlotFrom(string state_from)
    {
        var now = DateTime.Now;
        new CalendarMessageBuilder()
            .Year(now.Year).Month(now.Month)
            .Depth(CalendarDepth.Minutes)
            .SetState(state_from)

            .OnNavigatePath(s => Q(AddSlotFrom, s))
            .OnSelectPath(d => Q(AddSlotTo, d.ToBinary().Base64(), "."))

            .SkipTo(now)

            .FormatMinute(d => $"{d:HH:mm}")
            .FormatText((dt, depth, b) =>
            {
                var selection = depth switch
                {
                    CalendarDepth.Years => "year",
                    CalendarDepth.Months => "month",
                    CalendarDepth.Days => "day",
                    CalendarDepth.Hours => "hour",
                    CalendarDepth.Minutes => "minute",
                    _ => throw new NotImplementedException()
                };
                b.Push($"Select {selection} of the from date");
            })

            .Build(Message, new PagingService());
    }

    [Action]
    void AddSlotTo(string dt_from, string state_to)
    {
        var from = DateTime.FromBinary(dt_from.Base64());
        new CalendarMessageBuilder()
            .Year(from.Year).Month(from.Month)
            .Depth(CalendarDepth.Minutes)
            .SetState(state_to)

            .OnNavigatePath(s => Q(AddSlotTo, dt_from, s))
            .OnSelectPath(d => Q(LetsAddSlot, dt_from, d.ToBinary().Base64()))

            .SkipTo(from)

            .FormatMinute(d => $"{d:HH:mm}")
            .FormatText((dt, depth, b) =>
            {
                b.PushL($"✅The from date is {from}");

                var selection = depth switch
                {
                    CalendarDepth.Years => "year",
                    CalendarDepth.Months => "month",
                    CalendarDepth.Days => "day",
                    CalendarDepth.Hours => "hour",
                    CalendarDepth.Minutes => "minute",
                    _ => throw new NotImplementedException()
                };
                b.Push($"Select {selection} of the to date");
            })

            .Build(Message, new PagingService());
    }

    [Action]
    async Task LetsAddSlot(string dt_from, string dt_to)
    {
        var from = DateTime.FromBinary(dt_from.Base64());
        var to = DateTime.FromBinary(dt_to.Base64());
        var schedule = new Schedule
        {
            OwnerId = FromId,
            State = global::State.Free,
            From = from,
            To = to
        };
        await service.Add(schedule);

        PushL("Slot has been added");

        SlotView(schedule.Id);
    }
}