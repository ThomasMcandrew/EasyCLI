namespace EasyCLI;

public abstract class Command
{
    public async Task<int> Invoke()
    {
        var args = Environment.GetCommandLineArgs()[1..];
        if (!args.parse(this))
            return -1;

        return await Run();
    }
    protected abstract Task<int> Run();
}