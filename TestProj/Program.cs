// See https://aka.ms/new-console-template for more information
using EasyCLI;

return await new CliApp()
    .ConfigureInjector<string>(() => "Example")
    .Run();




[CommandName("Rename")]
public class FooDifferent : Command
{
    [Manditory("-f","--foo")]
    public string? Fum { get; set; }
    [NotRequired("-t","--gdr")]
    public List<int>? LOT { get; set; }
    [Inject]
    public string? toInject { get; set; }
    [Indexed(0)]
    public string? indexed { get; set; }
    protected override async Task<int> Run()
    {
        Console.WriteLine($"{Fum} {LOT!.Select(x => $"{x}").Aggregate((x,y) => $"{x}{y}")}{toInject}{indexed}");
        return 0;
    }
}
public class Foo : Command
{
    [Manditory("-f","--foo")]
    public string? Fum { get; set; }
    protected override async Task<int> Run()
    {
        return 0;
    }
}