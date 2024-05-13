namespace Application;

public interface IFileSystem
{
    TextReader? ReadFile(string path);

    TextWriter WriteFile(string path);
}

public class FileFileSystem : IFileSystem
{
    public TextReader? ReadFile(string path)
    {
        try
        {
            var file = File.OpenRead(path);
            return new StreamReader(file);
        }
        catch (IOException)
        {
            return null;
        }
    }

    public TextWriter WriteFile(string path)
    {
        var file = File.OpenWrite(path);
        return new StreamWriter(file);
    }
}