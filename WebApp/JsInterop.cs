using System;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Channels;

namespace WebApp;

public partial class JsInterop
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

    [JSImport("fs.write", "main.js")]
    public static partial void LocalStorageWrite(string path, string value);

    [JSImport("fs.read", "main.js")]
    public static partial string? LocalStorageRead(string path);
}