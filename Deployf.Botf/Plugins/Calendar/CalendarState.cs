using System.Globalization;

namespace Deployf.Botf;

public struct CalendarState
{
    public Span<string> Months => Culture.DateTimeFormat.MonthNames[0..12];

    public CultureInfo Culture { get; set; }

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
        Culture = CultureInfo.InvariantCulture;

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

        var culture = Culture;
        this = default;
        Culture = culture;

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
            return EncodeLookup[30 + value!.Value - now].ToString();
        }
        else
        {
            return EncodeLookup[value!.Value].ToString();
        }
    }


    public string ToStringMonth()
    {
        return $"{Year} {Months[Month!.Value - 1]}";
    }

    public string ToStringDay()
    {
        return $"{ToStringMonth()} {Day}";
    }

    public string ToStringHour()
    {
        return $"{ToStringDay()} {Hour}:00";
    }

    public CalendarState NextMonth()
    {
        var current = Date().AddMonths(1);
        return new CalendarState
        {
            Culture = Culture,
            Year = current.Year,
            Month = current.Month
        };
    }

    public CalendarState PrevMonth()
    {
        var current = Date().AddMonths(-1);
        return new CalendarState
        {
            Culture = Culture,
            Year = current.Year,
            Month = current.Month
        };
    }

    public CalendarState NextDay()
    {
        var current = Date().AddDays(1);
        return new CalendarState
        {
            Culture = Culture,
            Year = current.Year,
            Month = current.Month,
            Day = current.Day
        };
    }

    public CalendarState PrevDay()
    {
        var current = Date().AddDays(-1);
        return new CalendarState
        {
            Culture = Culture,
            Year = current.Year,
            Month = current.Month,
            Day = current.Day
        };
    }

    public CalendarState NextHour()
    {
        var current = Date().AddHours(1);
        return new CalendarState
        {
            Culture = Culture,
            Year = current.Year,
            Month = current.Month,
            Day = current.Day,
            Hour = current.Hour
        };
    }

    public CalendarState PrevHour()
    {
        var current = Date().AddHours(-1);
        return new CalendarState
        {
            Culture = Culture,
            Year = current.Year,
            Month = current.Month,
            Day = current.Day,
            Hour = current.Hour
        };
    }

    private enum TokenType { Error, Number, Offset }
}
