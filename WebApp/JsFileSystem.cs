using System;
using System.IO;
using Application;

namespace WebApp;

class SavingTextWriter(string path) : StringWriter
{
    public override void Close()
    {
        base.Close();
        Flush();
    }

    public override void Flush()
    {
        var contents = base.ToString();
        MainThread.OnMain(() => JsInterop.LocalStorageWrite(path, contents));
    }
}

public class JsFileSystem : IFileSystem
{
    public TextReader ReadFile(string path)
    {
        var contents = MainThread.OnMain(() => JsInterop.LocalStorageRead(path));
        if (contents is null) return null;

        return new StringReader(contents);
    }

    public TextWriter WriteFile(string path)
    {
        return new SavingTextWriter(path);
    }
}