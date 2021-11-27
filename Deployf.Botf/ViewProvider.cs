using CsQuery;
using DotLiquid;
using System.Reflection;
using Telegram.Bot.Types.Enums;

namespace Deployf.Botf;

public record ExecuteViewParams(
    string View,
    MessageBuilder Builder,
    object? Model,
    BotControllerBase? Controller,
    MethodBase? Action
);

public interface IViewProvider
{
    string GetView(string name);
    void ExecuteView(ExecuteViewParams data);
}

public interface IViewPartialHandler
{
    CQ Handle(CQ initialDom, CQ aggregate, ExecuteViewParams data);
}

public class ViewProvider : IViewProvider
{
    readonly string BasePath;
    readonly IEnumerable<IViewPartialHandler> Handlers;

    public ViewProvider(IEnumerable<IViewPartialHandler> handlers)
    {
        BasePath = "Views";
        Handlers = handlers;
    }

    public void ExecuteView(ExecuteViewParams data)
    {
        var viewTemplate = GetView(data.View);
        var template = Template.Parse(viewTemplate);
        var view = template.Render(Hash.FromAnonymousObject(data));
        var initialDom = CQ.Create(view);
        var resultDom = initialDom;
        foreach(var handler in Handlers)
        {
            resultDom = handler.Handle(initialDom, resultDom, data);
        }

        var resultText = resultDom.RenderSelection();
        data.Builder.Push(resultText);
        data.Builder.SetParseMode(ParseMode.Html);
    }

    public string GetView(string name)
    {
        return File.ReadAllText(Path.Combine(BasePath, name));
    }
}