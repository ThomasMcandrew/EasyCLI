using System.Reflection;

namespace EasyCLI;

[CommandName("[GenerateAlias]")]
public class GenerateAlias : Command
{
    [NotRequired("-f","--file")]
    public string? Filename { get; set; }

    protected override Task<int> Run()
    {
        var aliases = GetAliases();
        if(Filename == null)
            Console.WriteLine(aliases);
        else    
            File.WriteAllText(Filename,aliases);

        return Task.FromResult(0);
    }
    private string GetAliases() =>
        GetCommandNames().Select(BashFunc).Aggregate((x,y) => $"{x}{Environment.NewLine}{y}");
    private Func<string,string> BashFunc => x => $"alias {x}=\"{GetProgramName()} {x}\"";
    private string GetProgramName() => Environment.GetCommandLineArgs()[0];
    private IEnumerable<string> GetCommandNames() => 
        Assembly.GetEntryAssembly()!.GetCommands().Select(x => x.Key);
}