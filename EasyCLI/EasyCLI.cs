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
        var commands = Assembly.GetAssembly(typeof(CliApp))!.GetCommands();
        var userCommands = Assembly.GetEntryAssembly()!.GetCommands();
        foreach(var uc in userCommands) 
            if(!commands.TryAdd(uc.Key,uc.Value))
                throw new Exception("Invalid Command Name");

        var commandName = cliArgs[1].ToLower();
        var command = (Command) commands[commandName].GetConstructor(new Type[]{})!.Invoke(null);
        return await command.Invoke(self);
    }
    
}
