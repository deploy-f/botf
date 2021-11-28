using System.Globalization;

namespace Deployf.Botf;

public class CalendarMessageBuilder
{
    #region state
    private CalendarState _state;
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

    private Func<DateTime, string, string>? _select { get; set; }
    private Func<string, string>? _nav { get; set; }

    private CultureInfo _culture;

    public CalendarMessageBuilder()
    {
        _depth = CalendarDepth.Minutes;
        _culture = CultureInfo.InvariantCulture;
        _state.Culture = _culture;
    }

    public CalendarMessageBuilder Year(int? year)
    {
        _state.Year = year;
        return this;
    }

    public CalendarMessageBuilder Month(int? month)
    {
        _state.Month = month;
        return this;
    }

    public CalendarMessageBuilder Day(int? day)
    {
        _state.Day = day;
        return this;
    }

    public CalendarMessageBuilder Hour(int? hour)
    {
        _state.Hour = hour;
        return this;
    }

    public CalendarMessageBuilder Minute(int? minute)
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

    public CalendarMessageBuilder SkipTo(string to)
    {
        return SkipTo(new CalendarState(to).Date());
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
        _select = (d, s) => format(d);
        return this;
    }

    public CalendarMessageBuilder OnSelectPath(Func<DateTime, string, string> format)
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

    public CalendarMessageBuilder Culture(CultureInfo culture)
    {
        _culture = culture;
        _state.Culture = culture;
        return this;
    }
    #endregion

    #region build
    public void Build(MessageBuilder b) => Build(b, new PagingService());

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
            var state = _state with { Year = year, YearPage = null };
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

        string click(CalendarState state)
        {
            if(!_depth.HasFlag(CalendarDepth.Months))
            {
                return _select!(state, state);
            }

            return _nav!(state);
        }
    }

    private void BuildMonth(MessageBuilder b)
    {
        var list = new List<(string text, string payload)>();
        for (int month = 1; month <= 12; month++)
        {
            var state = _state with { Month = month, MinutePage = null };

            var skip = _filterMonth?.Invoke(state) ?? false;
            if (skip)
            {
                continue;
            }

            list.Add((_formatMonth?.Invoke(state) ?? _culture.DateTimeFormat.MonthNames[0..12][month - 1], click(state)));
        }

        var back = _depth.HasFlag(CalendarDepth.Years)
            ? _state with { Year = null, YearPage = 0, Month = null, MonthPage = null }
            : _state;
        b.LineButton($"📅 {_state.Year}", _nav!(back));

        var preffer = PrefferToMinDiv(list.Count, 3);
        for (int i = 0; i < list.Count; i++)
        {
            if (i % preffer == 0 && i != 0)
            {
                b.MakeButtonRow();
            }
            b.Button(list[i].text, list[i].payload);
        }

        var next = _state with { Year = _state.Year + 1 };
        var prev = _state with { Year = _state.Year - 1 };

        var skipNext = _filterMonth?.Invoke(next) ?? false;
        var skipPrev = _filterMonth?.Invoke(prev) ?? false;

        if (!skipNext || !skipPrev)
        {
            b.MakeButtonRow();
            if (!skipPrev)
            {
                b.Button($"⬅️ {prev.Year}", _nav!(prev));
            }

            if (!skipNext)
            {
                b.Button($"➡️ {next.Year}", _nav!(next));
            }
        }

        string click(CalendarState state)
        {
            if (!_depth.HasFlag(CalendarDepth.Days))
            {
                return _select!(state, state);
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
            var state = _state with { Day = day, DayPage = null };
            var skip = _filterDay?.Invoke(state) ?? false;
            if (skip)
            {
                continue;
            }

            list.Add((_formatDay?.Invoke(state) ?? day.ToString(), click(state)));
        }

        var back = _depth.HasFlag(CalendarDepth.Months) ? _state with { Month = null, MonthPage = null } : _state;
        b.LineButton($"📅 {_state.ToStringMonth()}", _nav!(back));

        var preffer = PrefferToMinDiv(list.Count, 5);
        for (int i = 0; i < list.Count; i++)
        {
            if (i % preffer == 0 && i != 0)
            {
                b.MakeButtonRow();
            }
            b.Button(list[i].text, list[i].payload);
        }

        var next = _state.NextMonth();
        var prev = _state.PrevMonth();

        var skipNext = _filterDay?.Invoke(next) ?? false;
        var skipPrev = _filterDay?.Invoke(prev) ?? false;

        if (!skipNext || !skipPrev)
        {
            b.MakeButtonRow();
            if (!skipPrev)
            {
                b.Button($"⬅️ {prev.ToStringMonth()} ", _nav!(prev));
            }

            if (!skipNext)
            {
                b.Button($"{next.ToStringMonth()} ➡️", _nav!(next));
            }
        }

        string click(CalendarState state)
        {
            if (!_depth.HasFlag(CalendarDepth.Hours))
            {
                return _select!(state, state);
            }

            return _nav!(state);
        }
    }

    private void BuildHour(MessageBuilder b)
    {
        var list = new List<(string text, string payload)>();
        for (int hour = 0; hour <= 23; hour++)
        {
            var state = _state with { Hour = hour, HourPage = null };
            var skip = _filterHour?.Invoke(state) ?? false;
            if (skip)
            {
                continue;
            }

            list.Add((_formatHour?.Invoke(state) ?? hour.ToString("00"), click(state)));
        }

        var back = _depth.HasFlag(CalendarDepth.Days) ? _state with { Day = null, DayPage = null } : _state;
        b.LineButton($"📅 {_state.ToStringDay()}", _nav!(back));

        var preffer = PrefferToMinDiv(list.Count, 4);
        for (int i = 0; i < list.Count; i++)
        {
            if (i % preffer == 0 && i != 0)
            {
                b.MakeButtonRow();
            }
            b.Button(list[i].text, list[i].payload);
        }

        var next = _state.NextDay();
        var prev = _state.PrevDay();
        var skipNext = _filterHour?.Invoke(next) ?? false;
        var skipPrev = _filterHour?.Invoke(prev) ?? false;

        if (!skipNext || !skipPrev)
        {
            b.MakeButtonRow();
            if (!skipPrev)
            {
                b.Button($"⬅️ {prev.ToStringDay()} ", _nav!(prev));
            }

            if (!skipNext)
            {
                b.Button($"➡️ {next.ToStringDay()}", _nav!(next));
            }
        }

        string click(CalendarState state)
        {
            if (!_depth.HasFlag(CalendarDepth.Minutes))
            {
                return _select!(state, state);
            }

            return _nav!(state);
        }
    }

    private void BuildMinute(MessageBuilder b)
    {
        var list = new List<(string text, string payload)>();
        for (int min = 0; min <= 59; min++)
        {
            var state = _state with { Minute = min, MinutePage = null };
            var skip = _filterMinute?.Invoke(state) ?? false;
            if(skip)
            {
                continue;
            }

            list.Add((_formatMinute?.Invoke(state) ?? min.ToString("00"), _select!(state, state)));
        }

        var back = _depth.HasFlag(CalendarDepth.Hours) ? _state with { Hour = null, HourPage = null } : _state;
        b.LineButton($"📅 {_state.ToStringHour()}", _nav!(back));

        var preffer = PrefferToMinDiv(list.Count, 6);
        for (int i = 0; i < list.Count; i++)
        {
            if(i % preffer == 0 && i != 0)
            {
                b.MakeButtonRow();
            }
            b.Button(list[i].text, list[i].payload);
        }

        var next = _state.NextHour();
        var prev = _state.PrevHour();
        var skipNext = _filterMinute?.Invoke(next) ?? false;
        var skipPrev = _filterMinute?.Invoke(prev) ?? false;

        if (!skipNext || !skipPrev)
        {
            b.MakeButtonRow();
            if (!skipPrev)
            {
                b.Button($"⬅️ {prev.ToStringHour()} ", _nav!(prev));
            }

            if (!skipNext)
            {
                b.Button($"➡️ {next.ToStringHour()}", _nav!(next));
            }
        }
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

    #endregion
}

public enum CalendarDepth
{
    Years = 1,
    Months = 1 << 1,
    Days = 1 << 2,
    Hours = 1 << 3,
    Minutes = 1 << 4,

    ToYears = Years,
    ToMonths = ToYears | Months,
    ToDays = ToMonths | Days,
    ToHours = ToDays | Hours,
    ToMinutes = ToHours | Minutes,

    Time = Hours | Minutes,
    Date = ToDays
}