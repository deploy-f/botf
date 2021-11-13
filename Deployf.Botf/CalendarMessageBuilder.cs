namespace Deployf.Botf;

public class CalendarMessageBuilder
{
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

        if (_depth.HasFlag(CalendarDepth.Years))
        {
            b.RowButton("Back to years", _nav!(_state with { Year = null }));
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

        if (_depth.HasFlag(CalendarDepth.Months))
        {
            b.RowButton("Back to month", _nav!(_state with { Month = null }));
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

        if (_depth.HasFlag(CalendarDepth.Days))
        {
            b.RowButton("Back to days", _nav!(_state with { Day = null }));
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
            var state = _state with { Minute = min };
            var skip = _filterMinute?.Invoke(state) ?? false;
            if(skip)
            {
                continue;
            }

            list.Add((_formatMinute?.Invoke(state) ?? min.ToString("00"), _select!(state, state)));
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

        if (_depth.HasFlag(CalendarDepth.Hours))
        {
            b.RowButton("Back to hours", _nav!(_state with { Hour = null }));
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
}

public struct CalendarState
{
    public int? YearPage { get; set; }
    public int? Year { get; set; }

    public int? MonthPage { get; set; }
    public int? Month { get; set; }

    public int? DayPage { get; set; }
    public int? Day { get; set; }

    public int? HourPage { get; set; }
    public int? Hour { get; set; }

    public int? MinutePage { get; set; }
    public int? Minute { get; set; }

    public CalendarState(string state)
    {
        YearPage = null;
        Year = null;

        MonthPage = null;
        Month = null;

        DayPage = null;
        Day = null;

        HourPage = null;
        Hour = null;

        MinutePage = null;
        Minute = null;

        TryOverrideState(state);
    }

    public void TryOverrideState(string state)
    {
        if (string.IsNullOrEmpty(state) || state == ".")
        {
            return;
        }

        var lexems = Parse(state).ToList();

        if (lexems.Count == 0 && lexems.All(c => c.type == TokenType.Error))
        {
            return;
        }

        this = default;

        if (lexems.Count > 0)
        {
            var year = lexems[0];
            if (year.type == TokenType.Offset)
            {
                YearPage = year.value;
            }
            else if (year.type == TokenType.Number)
            {
                var now = DateTime.Now.Year;
                Year = now + (year.value - 30);
            }
        }

        if (lexems.Count > 1)
        {
            var month = lexems[1];
            if (month.type == TokenType.Offset)
            {
                MonthPage = month.value;
            }
            else if (month.type == TokenType.Number)
            {
                Month = month.value;
            }
        }

        if (lexems.Count > 2)
        {
            var day = lexems[2];
            if (day.type == TokenType.Offset)
            {
                DayPage = day.value;
            }
            else if (day.type == TokenType.Number)
            {
                Day = day.value;
            }
        }

        if (lexems.Count > 3)
        {
            var hour = lexems[3];
            if (hour.type == TokenType.Offset)
            {
                HourPage = hour.value;
            }
            else if (hour.type == TokenType.Number)
            {
                Hour = hour.value;
            }
        }

        if (lexems.Count > 4)
        {
            var hour = lexems[4];
            if (hour.type == TokenType.Offset)
            {
                MinutePage = hour.value;
            }
            else if (hour.type == TokenType.Number)
            {
                Minute = hour.value;
            }
        }
    }

    public override string ToString()
    {
        return Encode(Year, YearPage, true)
            + Encode(Month, MonthPage)
            + Encode(Day, DayPage)
            + Encode(Hour, HourPage)
            + Encode(Minute, MinutePage);
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

            if (Hour != null)
            {
                return CalendarDepth.Minutes;
            }

            if (Day != null)
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

    public static implicit operator string(CalendarState state)
    {
        return state.ToString();
    }

    public static implicit operator DateTime(CalendarState state)
    {
        return state.Date();
    }

    static readonly Dictionary<char, int> DecodeLookup = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWX"
        .Select((c, i) => (c, i))
        .ToDictionary(c => c.c, c => c.i);
    static readonly Dictionary<int, char> EncodeLookup = DecodeLookup.ToDictionary(c => c.Value, c => c.Key);

    ///
    /// paging: Y1
    /// offset: u (start from 30, it's encoded under `u` letter)
    /// current: u
    /// null state: .
    /// 
    /// lookup table:
    /// 0 1 2 3 4 5 6 7 8 9 10 11 12 13 14 15 16 17 18 19 20 21 22 23 24 25 26 27 28 29 30 31 32 33 34 35 36 37 38 39 40 41 42 43 44 45 46 47 48 49 50 51 52 53 54 55 56 57 58 59
    /// 0 1 2 3 4 5 6 7 8 9 a  b  c  d  e  f  g  h  i  j  k  l  m  n  o  p  q  r  s  t  u  v  w  x  y  z  A  B  C  D  E  F  G  H  I  J  K  L  M  N  O  P  Q  R  S  T  U  V  W  X
    /// 
    /// select day: 
    /// u4
    /// 
    /// select time:
    /// u8q
    /// 
    /// select minute:
    /// v2ng
    /// 
    /// selected up to minute:
    /// u7g8k
    /// 
    /// ex4(paging minute)
    /// u7g8Y2
    ///  \\\\\\
    ///   \\\\\\
    ///    \\\\\ second page 
    ///     \\\\ paging of minutes
    ///      \\\ 8 am
    ///       \\ 16'th day
    ///        \ 7'th month
    ///          0 offset of current year, just current year
    ///
    IEnumerable<(TokenType type, int value)> Parse(string input)
    {
        var enumerator = input.GetEnumerator();
        while (enumerator.MoveNext())
        {
            var ch = enumerator.Current;
            int? value;
            // paging
            if (ch == 'Y')
            {
                if (!enumerator.MoveNext())
                {
                    yield return (TokenType.Error, 0);
                }
                value = parseInt(enumerator.Current);
                if (value == null)
                {
                    yield return (TokenType.Error, 0);
                }
                else
                {
                    yield return (TokenType.Offset, value.Value);
                }
            }
            else if ((value = parseInt(ch)) != null)
            {
                yield return (TokenType.Number, value.Value);
            }
            else
            {
                yield return (TokenType.Error, 0);
            }
        }

        int? parseInt(char ch)
        {
            if (DecodeLookup.TryGetValue(ch, out int value))
            {
                return value;
            }
            return null;
        }
    }

    public string Encode(int? value, int? page, bool year = false)
    {
        if (value == null && page == null)
        {
            if (year)
            {
                return ".";
            }
            else
            {
                return "";
            }
        }

        if (page != null)
        {
            return "Y" + EncodeLookup[page.Value];
        }

        if (year)
        {
            var now = DateTime.Now.Year;
            return EncodeLookup[30 + now - value.Value].ToString();
        }
        else
        {
            return EncodeLookup[value.Value].ToString();
        }
    }

    private enum TokenType { Error, Number, Offset }
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