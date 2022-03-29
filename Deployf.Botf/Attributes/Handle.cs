namespace Deployf.Botf;

public enum Handle
{
    /// <summary>
    /// Means unknown command or message type from user (telegram)
    /// </summary>
    Unknown,

    /// <summary>
    /// User isn't authorized
    /// </summary>
    Unauthorized,

    /// <summary>
    /// Handle exception
    /// </summary>
    Exception,

    /// <summary>
    /// Execute action before message go to routing and whole the botf
    /// </summary>
    BeforeAll,
    ClearState,
    ChainTimeout
}