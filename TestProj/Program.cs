// See https://aka.ms/new-console-template for more information
using EasyCLI;

Console.WriteLine("Hello, World!");
Command foo = new Foo();

await foo.Run(args);

Console.WriteLine("Hello, World!");

public class Foo : Command
{
    [Manditory("-f","--foo")]
    public string? Fum { get; set; }
    protected override Task<int> Run()
    {
        throw new NotImplementedException();
    }
}