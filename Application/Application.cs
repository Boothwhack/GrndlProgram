using ConsoleUI;

namespace Application;

public class Application
{
    private Screen _screen;
    private IFileSystem _fs;

    public Application(ITerminal terminal, IFileSystem fs)
    {
        _screen = new Screen(terminal);
        _fs = fs;
    }

    public async Task Run()
    {
        _screen.Draw().Start();
        await PushMenu(new MainMenu(_fs));
        _screen.StopDrawing();
    }

    async Task PushMenu(IMenu menu)
    {
        while (true)
        {
            await menu.Initialize(_screen);
            _screen.RefreshScreen();

            var nextMenu = await menu.Run(_screen);
            if (nextMenu is null)
                break;

            await PushMenu(nextMenu);
        }
    }
}