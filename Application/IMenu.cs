using ConsoleUI;

namespace Application;

interface IMenu
{
    void Initialize(Screen screen);

    IMenu? Run(Screen screen);
}