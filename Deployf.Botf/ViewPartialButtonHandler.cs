using CsQuery;

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
