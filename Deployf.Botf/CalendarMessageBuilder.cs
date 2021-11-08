using System.Text;

namespace Deployf.Botf;

public class CalendarMessageBuilder
{
    private StateValue _state;
    private CalendarDepth _depth { get; set; }

    private Func<int, bool>? _filterYear { get; set; }
    private Func<DateTime, bool>? _filterMonth { get; set; }
    private Func<DateTime, bool>? _filterDay { get; set; }
    private Func<DateTime, bool>? _filterHour { get; set; }
    private Func<DateTime, bool>? _filterMinute { get; set; }

    private Func<DateTime, string>? _formatYear { get; set; }
    private Func<DateTime, string>? _formatMonth { get; set; }
    private Func<DateTime, string>? _formatDay { get; set; }
    private Func<DateTime, string>? _formatHour { get; set; }
    private Func<DateTime, string>? _formatMinute { get; set; }

    private Action<DateTime, CalendarDepth, MessageBuilder>? _formatText { get; set;}

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

    public CalendarMessageBuilder SkipYear(Func<int, bool>? filter)
    {
        _filterYear = filter;
        return this;
    }

    public CalendarMessageBuilder SkipMonth(Func<DateTime, bool>? filter)
    {
        _filterMonth = filter;
        return this;
    }

    public CalendarMessageBuilder SkipDay(Func<DateTime, bool>? filter)
    {
        _filterDay = filter;
        return this;
    }

    public CalendarMessageBuilder SkipHour(Func<DateTime, bool>? filter)
    {
        _filterHour = filter;
        return this;
    }

    public CalendarMessageBuilder SkipMinute(Func<DateTime, bool>? filter)
    {
        _filterMinute = filter;
        return this;
    }

    public CalendarMessageBuilder SkipFrom(DateTime dt)
    {
        return this.SkipYear(d => d > dt.Year)
            .SkipMonth(d => d > new DateTime(dt.Year, dt.Month, 1))
            .SkipDay(d => d > new DateTime(dt.Year, dt.Month, dt.Day))
            .SkipHour(d => d > new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, 0, 0))
            .SkipMinute(d => d > new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, 0));
    }

    public CalendarMessageBuilder SkipTo(DateTime dt)
    {
        return this.SkipYear(d => d < dt.Year)
            .SkipMonth(d => d < new DateTime(dt.Year, dt.Month, 1))
            .SkipDay(d => d < new DateTime(dt.Year, dt.Month, dt.Day))
            .SkipHour(d => d < new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, 0, 0))
            .SkipMinute(d => d < new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, 0));
    }

    public CalendarMessageBuilder FormatYear(Func<DateTime, string>? format)
    {
        _formatYear = format;
        return this;
    }

    public CalendarMessageBuilder FormatMonth(Func<DateTime, string>? format)
    {
        _formatMonth = format;
        return this;
    }

    public CalendarMessageBuilder FormaDay(Func<DateTime, string>? format)
    {
        _formatDay = format;
        return this;
    }

    public CalendarMessageBuilder FormatHour(Func<DateTime, string>? format)
    {
        _formatHour = format;
        return this;
    }

    public CalendarMessageBuilder FormatMinute(Func<DateTime, string>? format)
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

    public CalendarMessageBuilder FormatText(Action<DateTime, CalendarDepth, MessageBuilder> format)
    {
        _formatText = format;
        return this;
    }


    public CalendarMessageBuilder SetState(string state)
    {
        _state.TryOverrideState(state);
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

        _formatText?.Invoke(_state, _state.Depth, b);

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
            .Where(y => !(_filterYear?.Invoke(y) ?? false))
            .AsQueryable();
        var pager = pagingService.Paging(query, new PageFilter { Page = _state.YearPage, Count = 6 });

        var itemButton = (int year) =>
        {
            var state = _state with { Year = year };
            var label = _formatYear?.Invoke(state) ?? year.ToString();
            var callback = click(state);
            return (label, callback);
        };

        b.Pager(
            pager,
            itemButton,
            _nav!("{0}"),
            3
        );

        string click(StateValue state)
        {
            if(_depth == CalendarDepth.Years)
            {
                return _select!(state);
            }

            return _nav!(state);
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
        var list = new List<(string text, string payload)>();
        for (int month = 1; month <= 12; month++)
        {
            var state = _state with { Month = month };

            var skip = _filterMonth?.Invoke(state) ?? false;
            if (skip)
            {
                continue;
            }

            list.Add((_formatMonth?.Invoke(state) ?? data[month - 1], click(state)));
        }

        var preffer = PrefferToMinDiv(list.Count, 3);
        for (int i = 0; i < list.Count; i++)
        {
            if (i % preffer == 0 && i != 0)
            {
                b.MakeButtonRow();
            }
            b.Button(list[i].text, list[i].payload);
        }

        b.RowButton("Back to years", _nav!(_state with { Year = null }));

        string click(StateValue state)
        {
            if (_depth == CalendarDepth.Months)
            {
                return _select!(state);
            }

            return _nav!(state);
        }
    }

    private void BuildDay(MessageBuilder b)
    {
        var list = new List<(string text, string payload)>();
        var daysInMonth = DateTime.DaysInMonth(_state.Year.GetValueOrDefault(), _state.Month.GetValueOrDefault());
        for (int day = 1; day <= daysInMonth; day++)
        {
            var state = _state with { Day = day };
            var skip = _filterDay?.Invoke(state) ?? false;
            if (skip)
            {
                continue;
            }

            list.Add((_formatDay?.Invoke(state) ?? day.ToString(), click(state)));
        }

        var preffer = PrefferToMinDiv(list.Count, 5);
        for (int i = 0; i < list.Count; i++)
        {
            if (i % preffer == 0 && i != 0)
            {
                b.MakeButtonRow();
            }
            b.Button(list[i].text, list[i].payload);
        }

        b.RowButton("Back to month", _nav!(_state with { Month = null }));

        string click(StateValue state)
        {
            if (_depth == CalendarDepth.Days)
            {
                return _select!(state);
            }

            return _nav!(state);
        }
    }

    private void BuildHour(MessageBuilder b)
    {
        var list = new List<(string text, string payload)>();
        for (int hour = 0; hour < 23; hour++)
        {
            var state = _state with { Hour = hour };
            var skip = _filterHour?.Invoke(state) ?? false;
            if (skip)
            {
                continue;
            }

            list.Add((_formatHour?.Invoke(state) ?? hour.ToString("00"), click(state)));
        }

        var preffer = PrefferToMinDiv(list.Count, 4);
        for (int i = 0; i < list.Count; i++)
        {
            if (i % preffer == 0 && i != 0)
            {
                b.MakeButtonRow();
            }
            b.Button(list[i].text, list[i].payload);
        }


        b.RowButton("Back to days", _nav!(_state with { Day = null }));

        string click(StateValue state)
        {
            if (_depth == CalendarDepth.Hours)
            {
                return _select!(state);
            }

            return _nav!(state);
        }
    }

    private void BuildMinute(MessageBuilder b)
    {
        var list = new List<(string text, string payload)>();
        for (int min = 0; min < 59; min++)
        {
            var state = _state with { Minute = min };
            var skip = _filterMinute?.Invoke(state) ?? false;
            if(skip)
            {
                continue;
            }

            list.Add((_formatMinute?.Invoke(state) ?? min.ToString("00"), _select!(state)));
        }

        var preffer = PrefferToMinDiv(list.Count, 6);
        for (int i = 0; i < list.Count; i++)
        {
            if(i % preffer == 0 && i != 0)
            {
                b.MakeButtonRow();
            }
            b.Button(list[i].text, list[i].payload);
        }

        b.RowButton("Back to hours", _nav!(_state with { Hour = null }));

    }

    private int PrefferToMinDiv(int size, int collums)
    {
        if(size < 2)
        {
            return collums;
        }

        var variants = Enumerable.Range(collums - 5, 10)
            .Where(c => c > 2)
            .Select(c => (number: c, rate: size % c))
            .OrderBy(c => c.rate)
            .ThenBy(c => Math.Abs(c.number - collums)).ToArray();

        if(variants.Any(c => c.rate == 0))
        {
            return variants.First().number;
        }

        return collums;
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

            TryOverrideState(state);
        }

        public void TryOverrideState(string state)
        {
            if (string.IsNullOrEmpty(state) || state == ".")
            {
                return;
            }

            if (state.Length < 4)
            {
                YearPage = int.Parse(state);
                Year = Month = Day = Hour = Minute = null;
                return;
            }
            Year = int.Parse(state[0..4]);

            if (state.Length < 6)
            {
                Month = Day = Hour = Minute = null;
                return;
            }
            Month = int.Parse(state[4..6]);

            if (state.Length < 8)
            {
                Day = Hour = Minute = null;
                return;
            }
            Day = int.Parse(state[6..8]);

            if (state.Length < 10)
            {
                Hour = Minute = null;
                return;
            }
            Hour = int.Parse(state[8..10]);

            if (state.Length < 12)
            {
                Minute = null;
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

        public DateTime Date()
        {
            return new DateTime(
                Year.GetValueOrDefault(1970),
                Month.GetValueOrDefault(1),
                Day.GetValueOrDefault(1),
                Hour.GetValueOrDefault(),
                Minute.GetValueOrDefault(),
                0
            );
        }

        public CalendarDepth Depth
        {
            get
            {
                if (Minute != null)
                {
                    return CalendarDepth.Minutes;
                }

                if(Hour != null)
                {
                    return CalendarDepth.Minutes;
                }

                if(Day != null)
                {
                    return CalendarDepth.Hours;
                }

                if (Month != null)
                {
                    return CalendarDepth.Days;
                }

                if (Year != null)
                {
                    return CalendarDepth.Months;
                }

                return CalendarDepth.Years;
            }
        }

        public static implicit operator string(StateValue state)
        {
            return state.ToString();
        }

        public static implicit operator DateTime(StateValue state)
        {
            return state.Date();
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