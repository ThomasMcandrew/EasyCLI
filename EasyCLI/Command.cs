namespace EasyCLI;

public abstract class Command
{
    public async Task<int> Run(string[] args)
    {
        if (!args.parse(this))
            return -1;

        return await Run();
    }
    protected abstract Task<int> Run();
}