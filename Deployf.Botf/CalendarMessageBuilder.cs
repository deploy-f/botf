using System.Text;

namespace Deployf.Botf;

public class CalendarMessageBuilder
{
    private StateValue _state;
    private CalendarDepth _depth { get; set; }

    private Func<int, bool>? _filterYear { get; set; }
    private Func<int, bool>? _filterMonth { get; set; }
    private Func<int, bool>? _filterDay { get; set; }
    private Func<int, bool>? _filterHour { get; set; }
    private Func<int, bool>? _filterMinute { get; set; }

    private Func<int, string>? _formatYear { get; set; }
    private Func<int, string>? _formatMonth { get; set; }
    private Func<int, string>? _formatDay { get; set; }
    private Func<int, string>? _formatHour { get; set; }
    private Func<int, string>? _formatMinute { get; set; }

    private Func<DateTime, string>? _select { get; set; }
    private Func<string, string>? _nav { get; set; }

    public CalendarMessageBuilder()
    {
        _depth = CalendarDepth.Minutes;
    }

    public CalendarMessageBuilder Year(int year)
    {
        _state.Year = year;
        return this;
    }

    public CalendarMessageBuilder Month(int month)
    {
        _state.Month = month;
        return this;
    }

    public CalendarMessageBuilder Day(int day)
    {
        _state.Day = day;
        return this;
    }

    public CalendarMessageBuilder Hour(int hour)
    {
        _state.Hour = hour;
        return this;
    }

    public CalendarMessageBuilder Minute(int minute)
    {
        _state.Minute = minute;
        return this;
    }

    public CalendarMessageBuilder Depth(CalendarDepth depth)
    {
        _depth = depth;
        return this;
    }

    public CalendarMessageBuilder FilterYear(Func<int, bool>? filter)
    {
        _filterYear = filter;
        return this;
    }

    public CalendarMessageBuilder FilterMonth(Func<int, bool>? filter)
    {
        _filterMonth = filter;
        return this;
    }

    public CalendarMessageBuilder FilterDay(Func<int, bool>? filter)
    {
        _filterDay = filter;
        return this;
    }

    public CalendarMessageBuilder FilterHour(Func<int, bool>? filter)
    {
        _filterHour = filter;
        return this;
    }

    public CalendarMessageBuilder FilterMinute(Func<int, bool>? filter)
    {
        _filterMinute = filter;
        return this;
    }

    public CalendarMessageBuilder FormatYear(Func<int, string>? format)
    {
        _formatYear = format;
        return this;
    }

    public CalendarMessageBuilder FormatMonth(Func<int, string>? format)
    {
        _formatMonth = format;
        return this;
    }

    public CalendarMessageBuilder FormaDay(Func<int, string>? format)
    {
        _formatDay = format;
        return this;
    }

    public CalendarMessageBuilder FormatHour(Func<int, string>? format)
    {
        _formatHour = format;
        return this;
    }

    public CalendarMessageBuilder FormatMinute(Func<int, string>? format)
    {
        _formatMinute = format;
        return this;
    }

    public CalendarMessageBuilder OnSelectPath(Func<DateTime, string> format)
    {
        _select = format;
        return this;
    }

    public CalendarMessageBuilder OnNavigatePath(Func<string, string> format)
    {
        _nav = format;
        return this;
    }


    public CalendarMessageBuilder SetState(string state)
    {
        _state = new StateValue(state);
        return this;
    }

    public string GetState() => _state.ToString();
    

    public void Build(MessageBuilder b, PagingService pagingService)
    {
        if(_nav == null)
        {
            throw new ArgumentException($"You should call {nameof(OnNavigatePath)} before to call `Build`");
        }

        if (_select == null)
        {
            throw new ArgumentException($"You should call {nameof(OnSelectPath)} before to call `Build`");
        }

        if(_state.Hour != null)
        {
            BuildMinute(b);
        }
        else if(_state.Day != null)
        {
            BuildHour(b);
        }
        else if (_state.Month != null)
        {
            BuildDay(b);
        }
        else if (_state.Year != null)
        {
            BuildMonth(b);
        }
        else 
        {
            BuildYear(b, pagingService);
        }
    }

    private void BuildYear(MessageBuilder b, PagingService pagingService)
    {
        var now = DateTime.Now.Year;
        var query = Enumerable.Range(now - 5, 30)
            .Select(y => y)
            .AsQueryable();
        var pager = pagingService.Paging(query, new PageFilter { Page = _state.YearPage, Count = 5 });

        b.Pager(
            pager,
            i => (_formatYear?.Invoke(i) ?? i.ToString(), click(i)),
            _nav!("{0}"),
            3
        );

        string click(int year)
        {
            if(_depth == CalendarDepth.Years)
            {
                return _select!(new DateTime(year, 1, 1));
            }

            return _nav!(_state with { Year = year });
        }
    }

    private void BuildMonth(MessageBuilder b)
    {
        var data = new[]
        {
            "Jan",
            "Feb",
            "Mar",
            "Apr",
            "May",
            "Jun",
            "Jul",
            "Aug",
            "Sep",
            "Oct",
            "Nov",
            "Dec"
        };

        for (int i = 0; i < 12; i++)
        {
            if (i % 3 == 0 && i != 0)
            {
                b.MakeButtonRow();
            }

            b.Button(_formatMonth?.Invoke(i+1) ?? data[i], click(i+1));
        }
        b.RowButton("Back to years", _nav!(_state with { Year = null }));

        string click(int month)
        {
            if (_depth == CalendarDepth.Months)
            {
                return _select!(new DateTime(_state.Year.Value, month, 1));
            }

            return _nav!(_state with { Month = month });
        }
    }

    private void BuildDay(MessageBuilder b)
    {
        var daysInMonth = DateTime.DaysInMonth(_state.Year.Value, _state.Month.Value);
        for (int i = 0; i < daysInMonth; i++)
        {
            if (i % 5 == 0 && i != 0)
            {
                b.MakeButtonRow();
            }

            b.Button(_formatDay?.Invoke(i+1) ?? (i + 1).ToString(), click(i+1));
        }
        b.RowButton("Back to month", _nav!(_state with { Month = null }));

        string click(int day)
        {
            if (_depth == CalendarDepth.Days)
            {
                return _select!(new DateTime(_state.Year.Value, _state.Month.Value, day));
            }

            return _nav!(_state with { Day = day });
        }
    }

    private void BuildHour(MessageBuilder b)
    {
        for (int i = 0; i < 23; i++)
        {
            if (i % 6 == 0 && i != 0)
            {
                b.MakeButtonRow();
            }
            var hour = i + 1;
            b.Button(_formatHour?.Invoke(hour) ?? (hour).ToString("00"), click(hour));
        }
        b.RowButton("Back to days", _nav!(_state with { Day = null }));

        string click(int hour)
        {
            if (_depth == CalendarDepth.Hours)
            {
                return _select!(new DateTime(_state.Year.Value, _state.Month.Value, _state.Day.Value, hour, 0, 0));
            }

            return _nav!(_state with { Hour = hour });
        }
    }

    private void BuildMinute(MessageBuilder b)
    {
        for (int i = 0; i < 59; i++)
        {
            var minute = i + 1;

            if (i % 8 == 0 && i != 0)
            {
                b.MakeButtonRow();
            }

            b.Button(_formatMinute?.Invoke(minute) ?? minute.ToString("00"), click(minute));
        }
        b.RowButton("Back to hours", _nav!(_state with { Hour = null }));

        string click(int minute)
        {
            return _select!(new DateTime(_state.Year.Value, _state.Month.Value, _state.Day.Value, _state.Hour.Value, minute, 0));
        }
    }

    private struct StateValue
    {
        public int? YearPage { get; set; }
        public int? Year { get; set; }
        public int? Month { get; set; }
        public int? Day { get; set; }
        public int? Hour { get; set; }
        public int? Minute { get; set; }

        public StateValue(string state)
        {
            YearPage = null;
            Year = null;
            Month = null;
            Day = null;
            Hour = null;
            Minute = null;

            if (string.IsNullOrEmpty(state))
            {
                return;
            }

            if (state.Length < 4)
            {
                YearPage = int.Parse(state);
                return;
            }
            Year = int.Parse(state[0..4]);

            if (state.Length < 6)
            {
                return;
            }
            Month = int.Parse(state[4..6]);

            if (state.Length < 8)
            {
                return;
            }
            Day = int.Parse(state[6..8]);

            if (state.Length < 10)
            {
                return;
            }
            Hour = int.Parse(state[8..10]);

            if (state.Length < 12)
            {
                return;
            }
            Minute = int.Parse(state[10..12]);
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            if (Year == null)
            {
                return YearPage?.ToString() ?? "0";
            }

            builder.Append(Year.Value.ToString("0000"));

            if (Month == null)
            {
                return builder.ToString();
            }

            builder.Append(Month.Value.ToString("00"));

            if (Day == null)
            {
                return builder.ToString();
            }

            builder.Append(Day.Value.ToString("00"));

            if (Hour == null)
            {
                return builder.ToString();
            }

            builder.Append(Hour.Value.ToString("00"));

            if (Minute == null)
            {
                return builder.ToString();
            }

            builder.Append(Minute.Value.ToString("00"));

            return builder.ToString();
        }

        public static implicit operator string(StateValue state)
        {
            return state.ToString();
        }
    }
}

public enum CalendarDepth
{
    Years,
    Months,
    Days,
    Hours,
    Minutes
}