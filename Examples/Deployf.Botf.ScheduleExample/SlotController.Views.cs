namespace Deployf.Botf.ScheduleExample;

/// Slot views and actions for manage the slot
partial class SlotController
{
    [Action]
    async Task ListCalendarDays(string uid64, int page)
    {
        var now = DateTime.Now.Date;
        var pager = await service.GetFreeDays(uid64.Base64(), now, new PageFilter { Page = page });

        if (pager.Count == 0)
        {
            Push("Nothing :(");
        }
        else
        {
            Push("Peek a day:");
        }

        Pager(pager,
            u => ($"{u:dd.MM.yyyy}", Q(ListFreeSlots, uid64, u.ToBinary().Base64(), 0)),
            Q(ListCalendarDays, uid64, "{0}")
        );
    }

    [Action]
    async Task ListFreeSlots(string uid64, string dt64, int page)
    {
        PushL("Peek free slot:");

        var user = uid64.Base64();
        var date = DateTime.FromBinary(dt64.Base64());
        var pager = await service.GetFreeSlots(user, date, new PageFilter { Page = page });

        Pager(pager,
            u => ($"{u.From:HH.mm} - {u.To:HH.mm}", Q(SlotView, u.Id)),
            Q(ListFreeSlots, uid64, dt64, "{0}"),
            3
        );

        if (pager.Count > 0 && (user == FromId || User.IsInRole("admin")))
        {
            RowButton("Cancel this day", Q(ApproveCancelDay, uid64, dt64));
        }

        RowButton("🔙 Back", Q(ListCalendarDays, uid64, 0));
    }

    [Action]
    void SlotView(int scheduleId)
    {
        var schedule = service.Get(scheduleId);

        PushL($"{schedule.State} time slot");

        PushL($"Date: {schedule.From:dd.MM.yyyy}");
        PushL($"Time: {schedule.From:HH:mm} - {schedule.To:HH:mm}");
        if (!string.IsNullOrEmpty(schedule.Comment))
        {
            PushL();
            Push(schedule.Comment);
        }

        if (FromId == schedule.OwnerId)
        {
            if (schedule.State != global::State.Canceled)
            {
                RowButton("Cancel", Q(Cancel, scheduleId));
            }
            if (schedule.State == global::State.Canceled)
            {
                RowButton("Free", Q(Free, scheduleId));
            }
        }
        else
        {
            RowButton("Book", Q(Book, scheduleId));
        }

        RowButton("🔙 Back", Q(ListFreeSlots, schedule.OwnerId.Base64(), schedule.From.Date.ToBinary().Base64(), 0));
    }

    [Action]
    async Task Book(int scheduleId)
    {
        await service.Book(scheduleId);
        PushL("Booked");
        SlotView(scheduleId);
    }

    [Action]
    async Task Cancel(int scheduleId)
    {
        await service.Cancel(scheduleId);
        PushL("Canceled");
        SlotView(scheduleId);
    }

    [Action]
    void ApproveCancelDay(string uid64, string dt64)
    {
        Push("Are you sure?");
        Button("Yes, cancel it", Q(CancelDay, uid64, dt64));
        Button("No, go back", Q(ListFreeSlots, uid64, dt64, 0));
    }

    [Action]
    async Task CancelDay(string uid64, string dt64)
    {
        var user = uid64.Base64();
        if (FromId == user || User.IsInRole("admin"))
        {
            var date = DateTime.FromBinary(dt64.Base64());
            await service.CancelDay(user, date);
            PushL("Canceled!");
            await AnswerCallback("Canceled");
        }
    }

    [Action]
    async Task Free(int scheduleId)
    {
        await service.Free(scheduleId);
        PushL("Now it is free");
        SlotView(scheduleId);
    }
}