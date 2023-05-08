using Deployf.Botf;

BotfProgram.StartBot(args);

class MediaController : BotController
{
    /*
     * There is a picture in the 1st message, but not in the 2nd.
     * What should happen:
     * The 1st will be deleted and the second will be sent
     */
    
    #region 1st scenario

    [Action("/start")]
    void Start()
    {
        // Add the photo to message
        Photo("https://avatars.githubusercontent.com/u/59260433");
        Push("Hello from deploy-f");
        Button("Got to botf repo", Q(Test1));
    }
    
    [Action]
    void Test1()
    {
        Push("Test1");
        Button("Got to botf repo", "https://github.com/deploy-f/botf");
    }

    #endregion

    
    /*
     * There is no picture in the 1st message, but there is in the 2nd.
     * What should happen:
     * The 1st will be deleted and the second will be sent
     */
    
    #region 2й scenario

    [Action("/start2")]
    void Start2()
    {
        Push("Hello from deploy-f");
        Button("Got to botf repo", Q(Test2));
    }
    
    [Action]
    void Test2()
    {
        // Add the photo to message
        Photo("https://avatars.githubusercontent.com/u/59260433");
        Push("Test2");
        Button("Got to botf repo", "https://github.com/deploy-f/botf");
    }
    
    #endregion
    
    
    /*
     * There is no picture in the 1st message and there is no picture in the 2nd either.
     * What should happen:
     * 1st message update via without deletion
     */
    
    #region 3й scenario

    [Action("/start3")]
    void Start3()
    {
        Push("Hello from deploy-f");
        Button("Got to botf repo", Q(Test3));
    }
    
    [Action]
    void Test3()
    {
        Push("Test3");
        Button("Got to botf repo", "https://github.com/deploy-f/botf");
    }
    
    #endregion
    
    
    /*
     * In the 1st message there is a picture and in the 2nd there is.
     * What should happen:
     * The 1st message will update its text and image without deleting.
     * If you specify UpdateMessagePolicy to DeleteAndSend, so the 1st message will be deleted
     * and new message with new picture will be sent  
     * Note from Telegram: When an inline message is edited, a new file can't be uploaded;
     * use a previously uploaded file via its file_id or specify a URL.  
     */
    
    #region 4й scenario

    [Action("/start4")]
    void Start4()
    {
        // Add the photo to message
        Photo("https://avatars.githubusercontent.com/u/59260433");
        Push("Hello from deploy-f");
        Button("Got to botf repo", Q(Test4));
    }
    
    [Action]
    void Test4()
    {
        Context.SetUpdateMsgPolicy(UpdateMessagePolicy.UpdateContent);
        // Add a new photo to message
        Photo("https://icons-for-free.com/iconfiles/png/512/csharp+line-1324760527290176528.png");
        Push("Test4");
        Button("Got to botf repo", "https://github.com/deploy-f/botf");
    }
    
    #endregion
}