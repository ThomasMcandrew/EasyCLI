// See https://aka.ms/new-console-template for more information
using EasyCLI;

Console.WriteLine("Hello, World!");
Command foo = new FooDifferent();

Runner.GetCommands();

await foo.Invoke();

Console.WriteLine("Hello, World!");

[CommandName("Rename")]
public class FooDifferent : Command
{
    [Manditory("-f","--foo")]
    public string? Fum { get; set; }
    [Manditory("-t","--gdr")]
    public List<int>? LOT { get; set; }
    protected override Task<int> Run()
    {
        throw new NotImplementedException();
    }
}
public class Foo : Command
{
    [Manditory("-f","--foo")]
    public string? Fum { get; set; }
    protected override Task<int> Run()
    {
        throw new NotImplementedException();
    }
}