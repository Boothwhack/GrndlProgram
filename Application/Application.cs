using ConsoleUI;

namespace Application;

public class Application
{
    private Screen _screen;

    public Application(ITerminal terminal)
    {
        _screen = new Screen(terminal);
    }

    public void Run()
    {
        _screen.DrawThread().Start();
        PushMenu(new MainMenu());
        _screen.StopDrawing();
    }

    void PushMenu(IMenu menu)
    {
        while (true)
        {
            menu.Initialize(_screen);
            _screen.RefreshScreen();

            var nextMenu = menu.Run(_screen);
            if (nextMenu is null)
                break;

            PushMenu(nextMenu);
        }
    }
}