using Deployf.Botf;

BotfProgram.StartBot(args);

class MediaController : BotController
{
    [Action("/start")]
    void Start()
    {
        // Add the photo to message
        Photo("https://avatars.githubusercontent.com/u/59260433");
        Push("Hello from deploy-f");
        Button("Got to botf repo", "https://github.com/deploy-f/botf");
    }
}