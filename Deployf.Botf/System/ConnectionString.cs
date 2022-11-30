namespace Deployf.Botf;

public class ConnectionString
{
    /// <summary>
    /// token?key=value&key=value...
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <exception cref="BotfException"></exception>
    public static BotfOptions Parse(string value)
    {
        if(string.IsNullOrEmpty(value))
        {
            throw new ArgumentNullException("value");
        }

        var options = new BotfOptions();

        var main = value.Split('?', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if(main.Length > 0)
        {
            options.Token = main[0];
        }
        else
        {
            throw new BotfException("Connection string for BotF (configuration string) is empty or has only whitespaces.");
        }

        if(main.Length == 1)
        {
            return options;
        }

        var values = main[1].Split('&', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var kv in values)
        {
            var cortage = kv.Split('=', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if(cortage == null || cortage.Length != 2)
            {
                throw new BotfException("Botf connection string is wrong. It must have format like `bot_token?key1=value1&key2=value2..`");
            }

            switch (cortage[0])
            {
                case "botname":
                    options.Username = cortage[1];
                    break;
                case "autosend":
                    if(bool.TryParse(cortage[1], out var autosend))
                    {
                        options.AutoSend = autosend;
                    }
                    else
                    {
                        throw new BotfException("`autosend` configuration option has wrong format, it should be a bool convertable value like true|false|1|0|True|False");
                    }
                    break;
                case "group_mode":
                    if (bool.TryParse(cortage[1], out var groupMode))
                    {
                        options.HandleOnlyMentionedInGroups = groupMode;
                    }
                    else
                    {
                        throw new BotfException("`group_mode` configuration option has wrong format, it should be a bool convertable value like true|false|1|0|True|False");
                    }
                    break;
                case "webhook":
                    options.WebhookUrl = cortage[1];
                    break;
                case "api":
                    options.ApiBaseUrl = cortage[1];
                    break;
                case "autoclean":
                    if (bool.TryParse(cortage[1], out var autoclean))
                    {
                        options.AutoCleanReplyKeyboard = autoclean;
                    }
                    else
                    {
                        throw new BotfException("`autoclean` configuration option has wrong format, it should be a bool convertable value like true|false|1|0|True|False");
                    }
                    break;
                case "keep_clean":
                    if (bool.TryParse(cortage[1], out var keepclean))
                    {
                        options.AutoCleanLastMessage = keepclean;
                    }
                    else
                    {
                        throw new BotfException("`keep_clean` configuration option has wrong format, it should be a bool convertable value like true|false|1|0|True|False");
                    }
                    break;
                case "chain_timeout":
                    options.ChainTimeout = cortage[1].TryParseTimeSpan();
                    break;
                case "webapp_url":
                    options.WebAppUrl = cortage[1];
                    break;
            }
        }

        return options;
    }
}