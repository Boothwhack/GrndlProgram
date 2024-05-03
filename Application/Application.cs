using ConsoleUI;

namespace Application;

public class Application
{
    private Screen _screen;

    public Application(ITerminal terminal)
    {
        _screen = new Screen(terminal);
    }

    public async Task Run()
    {
        _screen.Draw().Start();
        await PushMenu(new MainMenu());
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