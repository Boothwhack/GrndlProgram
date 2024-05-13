using System;
using System.Text;
using System.Threading.Tasks;
using ConsoleUI;

namespace WebApp;

public class JsTerminal : ITerminal
{
    private StringBuilder _buffer = new();
    private const string ESC = "\u001B[";

    public int WindowWidth => MainThread.OnMain(JsInterop.Width);
    public int WindowHeight => MainThread.OnMain(JsInterop.Height);

    private int ColorCode(ConsoleColor color) => color switch
    {
        ConsoleColor.Black => 30,
        ConsoleColor.DarkBlue => 34,
        ConsoleColor.DarkGreen => 32,
        ConsoleColor.DarkCyan => 36,
        ConsoleColor.DarkRed => 31,
        ConsoleColor.DarkMagenta => 35,
        ConsoleColor.DarkYellow => 33,
        ConsoleColor.Gray => 90,
        ConsoleColor.DarkGray => 37,
        ConsoleColor.Blue => 94,
        ConsoleColor.Green => 92,
        ConsoleColor.Cyan => 96,
        ConsoleColor.Red => 91,
        ConsoleColor.Magenta => 95,
        ConsoleColor.Yellow => 93,
        ConsoleColor.White => 97,
        _ => throw new ArgumentOutOfRangeException(nameof(color), color, null)
    };

    private ConsoleColor _backgroundColor = ConsoleColor.Black;

    public ConsoleColor BackgroundColor
    {
        get => _backgroundColor;
        set
        {
            _backgroundColor = value;
            var code = ColorCode(_backgroundColor) + 10;

            Write($"{ESC}{code}m");
        }
    }

    private ConsoleColor _foregroundColor = ConsoleColor.White;

    public ConsoleColor ForegroundColor
    {
        get => _foregroundColor;
        set
        {
            _foregroundColor = value;
            var code = ColorCode(_foregroundColor);

            Write($"{ESC}{code}m");
        }
    }

    public void ResetColor()
    {
        Write($"{ESC}0m");
    }

    public void SetCursorPosition(int x, int y) => Write($"{ESC}{y + 1};{x}H");

    public void Flush()
    {
        var text = _buffer.ToString();
        _buffer.Clear();
        MainThread.OnMain(() => JsInterop.Write(text));
    }

    public void Clear() => MainThread.OnMain(JsInterop.Clear);

    public void Write(string text)
    {
        _buffer.Append(text);
    }

    public async Task<ConsoleKeyInfo> ReadKey(bool intercept = false) =>
        await JsInterop.KeyChannel.Reader.ReadAsync();
}