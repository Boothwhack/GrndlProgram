using System;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using ConsoleUI;

async void Main()
{
    var terminal = new JSTerminal();
    var app = new Application.Application(terminal);
    await app.Run();
}

// start application in background task
var mainTask = new Task(Main);
mainTask.Start();

// process queued tasks on the main thread
while (true)
{
    var task = await MainThread.TaskChannel.Reader.ReadAsync();
    task.RunSynchronously();
}

class MainThread
{
    // send tasks here that need to run on the main thread
    // this is required if calling javascript functions
    public static Channel<Task> TaskChannel = Channel.CreateUnbounded<Task>();
}

class JSTerminal : ITerminal
{
    private StringBuilder _buffer = new();
    private const string ESC = "\u001B[";

    delegate T MainThreadCall<out T>();

    delegate void MainThreadVoidCall();

    // wraps delegate to run on the main thread
    private T OnMain<T>(MainThreadCall<T> call)
    {
        TaskCompletionSource<T> completionSource = new();
        MainThread.TaskChannel.Writer.TryWrite(new Task(() => completionSource.SetResult(call())));
        completionSource.Task.Wait();
        return completionSource.Task.Result;
    }

    private void OnMain(MainThreadVoidCall call)
    {
        var task = new Task(() => call());
        MainThread.TaskChannel.Writer.TryWrite(task);
        task.Wait();
    }

    public int WindowWidth => OnMain(TerminalInterop.Width);
    public int WindowHeight => OnMain(TerminalInterop.Height);

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
        OnMain(() => TerminalInterop.Write(text));
    }

    public void Clear() => OnMain(TerminalInterop.Clear);

    public void Write(string text)
    {
        _buffer.Append(text);
    }

    public async Task<ConsoleKeyInfo> ReadKey(bool intercept = false) =>
        await TerminalInterop.KeyChannel.Reader.ReadAsync();
}

public partial class TerminalInterop
{
    public static Channel<ConsoleKeyInfo> KeyChannel = Channel.CreateUnbounded<ConsoleKeyInfo>();

    [JSExport]
    public static void TerminalKey(string pressed, int keyCode)
    {
        var key = new ConsoleKeyInfo(pressed[0], (ConsoleKey)keyCode, false, false, false);
        KeyChannel.Writer.TryWrite(key);
    }

    [JSImport("terminal.write", "main.js")]
    public static partial void Write(string text);

    [JSImport("terminal.width", "main.js")]
    public static partial int Width();

    [JSImport("terminal.height", "main.js")]
    public static partial int Height();

    [JSImport("terminal.clear", "main.js")]
    public static partial void Clear();
}