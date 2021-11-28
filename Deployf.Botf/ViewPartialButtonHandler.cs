using CsQuery;
using System.Globalization;

namespace Deployf.Botf;

public class ViewPartialButtonHandler : IViewPartialHandler
{
    readonly IPathQuery query;

    public ViewPartialButtonHandler(IPathQuery query)
    {
        this.query = query;
    }

    public CQ Handle(CQ initialDom, CQ aggregate, ExecuteViewParams data)
    {
        var buttons = aggregate["BUTTON"];

        foreach(var button in buttons)
        {
            var payload = button.Attributes["_payload"];
            var text = button.InnerText;

            var actionData = button.Attributes["action"];
            if (!string.IsNullOrEmpty(actionData))
            {
                var path = actionData;
                if(!actionData.Contains("/"))
                {
                    path = data.Controller!.GetType().Name + "/" + actionData;
                }

                var arguments = button.Attributes
                    .Where(c => c.Key != "_payload"
                                && c.Key != "action"
                                && c.Key != "row")
                    .ToDictionary(c => c.Key, c => (object)c.Value);

                payload = query.GetPath(path, arguments);
            }
            
            data.Builder.RowButton(text, payload);
        }

        return new CQ(aggregate.Where(c => c.NodeName != "BUTTON"));
    }
}

public class ViewPartialCalendarHandler : IViewPartialHandler
{
    readonly IPathQuery query;

    public ViewPartialCalendarHandler(IPathQuery query)
    {
        this.query = query;
    }

    public CQ Handle(CQ initialDom, CQ aggregate, ExecuteViewParams data)
    {
        var calendar = aggregate.FirstOrDefault(c => c.NodeName == "CALENDAR");

        if (calendar == null)
        {
            return aggregate;
        }

        var builder = new CalendarMessageBuilder();

        var dateAttr = calendar.Attributes["date"];
        if (dateAttr != null)
        {
            var date = dateAttr == "now" ? DateTime.Now : DateTime.Parse(dateAttr);
            builder = builder.Year(date.Year).Month(date.Month).Day(date.Day);
        }

        var yearAttr = calendar.Attributes["year"];
        if (yearAttr != null)
        {
            builder = builder.Year(int.Parse(yearAttr));
        }

        var monthAttr = calendar.Attributes["month"];
        if (monthAttr != null)
        {
            builder = builder.Month(int.Parse(monthAttr));
        }

        var dayAttr = calendar.Attributes["day"];
        if (dayAttr != null)
        {
            builder = builder.Day(int.Parse(dayAttr));
        }

        var cultureAttr = calendar.Attributes["culture"];
        if (cultureAttr != null)
        {
            builder = builder.Culture(CultureInfo.GetCultureInfo(cultureAttr));
        }

        var depthAttr = calendar.Attributes["mode"];
        if (depthAttr != null)
        {
            builder = builder.Depth(Enum.Parse<CalendarDepth>(depthAttr));
        }

        var stateAttr = calendar.Attributes["state"];
        if (stateAttr != null)
        {
            builder = builder.SetState(stateAttr);
        }

        var skipToAttr = calendar.Attributes["skipto"];
        if (skipToAttr != null)
        {
            var date = DateTime.Parse(skipToAttr);
            builder = builder.SkipTo(date);
        }

        var skipFromAttr = calendar.Attributes["skipfrom"];
        if (skipFromAttr != null)
        {
            var date = DateTime.Parse(skipFromAttr);
            builder = builder.SkipFrom(date);
        }

        var navigate = calendar.Attributes["navigate"];
        if (navigate != null)
        {
            builder = builder.OnNavigatePath(f => navigate.Replace("{0}", f));
        }

        var click = calendar.Attributes["click"];
        if (click != null)
        {
            builder = builder.OnSelectPath(f => click.Replace("{0}", f.ToBinary().ToString()));
        }

        return new CQ(aggregate.Where(c => c.NodeName != "CALENDAR"));
    }
}