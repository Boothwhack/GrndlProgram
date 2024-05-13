using System.Threading.Channels;
using System.Threading.Tasks;
using WebApp;

async void Main()
{
    var terminal = new JsTerminal();
    var fs = new JsFileSystem();
    var app = new Application.Application(terminal, fs);
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

static class MainThread
{
    // send tasks here that need to run on the main thread
    // this is required if calling javascript functions
    public static readonly Channel<Task> TaskChannel = Channel.CreateUnbounded<Task>();
    
    public delegate T MainThreadCall<out T>();

    public delegate void MainThreadVoidCall();

    // wraps delegate to run on the main thread
    public static T OnMain<T>(MainThreadCall<T> call)
    {
        TaskCompletionSource<T> completionSource = new();
        TaskChannel.Writer.TryWrite(new Task(() => completionSource.SetResult(call())));
        completionSource.Task.Wait();
        return completionSource.Task.Result;
    }

    public static void OnMain(MainThreadVoidCall call)
    {
        var task = new Task(() => call());
        TaskChannel.Writer.TryWrite(task);
        task.Wait();
    }
}
