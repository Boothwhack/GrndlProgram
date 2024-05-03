using ConsoleUI;

namespace Application;

interface IMenu
{
    Task Initialize(Screen screen);

    Task<IMenu?> Run(Screen screen);
}