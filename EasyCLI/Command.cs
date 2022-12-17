namespace EasyCLI;

public abstract class Command
{
    public async Task<int> Invoke(CliApp cli)
    {
        if(!this.SetInjectors(cli))
            return -1;
        var args = Environment.GetCommandLineArgs()[2..];
        if (!args.parse(this))
            return -1;

        return await Run();
    }
    protected abstract Task<int> Run();
}