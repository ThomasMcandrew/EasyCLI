using System.Reflection;

namespace EasyCLI;

public class CliApp
{
    public Dictionary<Type,Func<object>>? InjectorGenerators { get; set; }
    public CliApp() 
    { 
        InjectorGenerators = new Dictionary<Type, Func<object>>();
    }
}
public static class CliAppExtensions
{
    public static CliApp ConfigureInjector<T>(this CliApp self, Func<T> generator) where T : class
    {
        self.InjectorGenerators!.Add(typeof(T),generator);
        return self;
    }
    public static async Task<int> Run(this CliApp self)
    {
        var cliArgs = Environment.GetCommandLineArgs();
        if (cliArgs.Length < 2)
            throw new InvalidOperationException("No Command Provided");
        var commands = self.GetCommands();
        var commandName = cliArgs[1];
        var command = (Command) commands[commandName].GetConstructor(new Type[]{})!.Invoke(null);
        return await command.Invoke(self);
    }
    private static Dictionary<string, Type> GetCommands(this CliApp self) => 
        Assembly.GetEntryAssembly()!
            .GetTypes()
            .Where(x => x.IsAssignableTo(typeof(Command)))
            .ToDictionary(x => 
                x.GetCustomAttributes()
                    .Where(a => a.GetType() == typeof(CommandName))
                    .Any() ? 
                x.GetCustomAttributes()
                    .Where(a => a.GetType() == typeof(CommandName))
                    .Select(a => (CommandName) a)
                    .FirstOrDefault()!
                    .Name :
                x.Name,
            x => x);
 

}
