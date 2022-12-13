using System.Reflection;

namespace EasyCLI;

public class Runner
{
    public static Dictionary<string, Type> GetCommands() => 
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