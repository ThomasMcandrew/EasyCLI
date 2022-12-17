using System.Reflection;

namespace EasyCLI;

internal static class ArgParser
{
    public static Dictionary<Type, Func<string,Object>> TypeConverter => 
        new Dictionary<Type, Func<string, object>>(){
            {typeof(int),x => int.Parse(x)},
        };

    public static object ParseByType(this Type type,string value) =>
        TypeConverter[type](value);

    public static IEnumerable<T> ToListType<T>(IEnumerable<T> collection, Type type) => 
        (IEnumerable<T>) type.GetConstructor(new[] { collection.GetType() })!
            .Invoke(new[] { collection ?? Enumerable.Empty<T>() });
        //for dictionaries if we store them as key value pairs then 
        //we can make a dictionary in the same way
        //Would be cool to have a mapping function as an attribute
        //ie [mapping(x => x.split(','))]
        //ie [mapping((x,y) => [x,y])]
    /*
     * TODO Add ability for hash maps and dicts
     */
    public static bool parse(this IEnumerable<string> args, Command command)
    {
        var flagProperties = command.GetFlagProperties();
        var indexProperties = command.GetIndexProperties();
        
        var attributeIdentities = flagProperties
            .Select(x => x.Value.GetAttributeGuid())
            .ToList();
        attributeIdentities.AddRange(
            indexProperties.Select(x => x.GetAttributeGuid())
        );
        var setStatus = attributeIdentities
            .Distinct()
            .ToDictionary(x => x, x => false);

        var tokens = args.Tokenize(command).ToArray();
        var i = 0;
        while(i < tokens.Count())
        {
            var token = tokens[i];
            PropertyInfo prop;
            if(flagProperties.ContainsKey(tokens[i].Value ?? string.Empty))
            {
                prop = flagProperties[tokens[i].Value ?? string.Empty];
                i++;
            }
            else if(indexProperties.Any())
                prop = indexProperties.Pop();
            else 
            {
                Console.WriteLine($"Unrecognized token {tokens[i].Value}");
                return false;
            }
            
            setStatus[prop.GetAttributeGuid()] = true;

            if(prop.PropertyType == typeof(bool))
            {
                prop.SetValue(command,true);
                i--;
            }
            else if(prop.PropertyType.IsPrimitive)
                prop.SetValue(command,prop.PropertyType.ParseByType(tokens[i].Value ?? string.Empty));
            else if(prop.PropertyType == typeof(string))
                prop.SetValue(command,tokens[i].Value);
            else if(prop.PropertyType.Namespace!.Contains(nameof(System.Collections)))
            {
                /*
                Looking for other enumerables in docs
                https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic?view=net-7.0
                for example Stack
                https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.stack-1?view=net-7.0
                can be initialized by an IEnumerable and I think that may be the route for each

                if generic arguments length is 1 then its a list or array
                if its 2 then it can be a dict or map or whatever, thats 
                how we can include those in this.
                */
                var enumerableType = prop.PropertyType.GetGenericArguments()[0];
                Type genericListType = typeof(List<>).MakeGenericType(enumerableType);
                var list = (System.Collections.IList)Activator.CreateInstance(genericListType)!;
                while(i+1 < tokens.Length && !tokens[i+1].IsFlag)
                {
                    list.Add(enumerableType.ParseByType(tokens[i].Value!));
                    i++;
                }
                list.Add(enumerableType.ParseByType(tokens[i].Value!));
                
                var listTypeMethod = typeof(ArgParser).GetMethod(nameof(ToListType))!.MakeGenericMethod(enumerableType);
                var listType = listTypeMethod.Invoke(null, new object[] { list, prop.PropertyType });

                prop.SetValue(command, listType);
            }
            i++;
        }
        if (setStatus.Any(x => !x.Value))
        {
            Console.WriteLine("Fields not correctly filled in");
            return false;
        }
        return true;
    }
    public static bool SetInjectors(this Command command, CliApp app)
    {
        var properties = command.GetInjectedProperties();
        foreach (var prop in properties)
            try {
                prop.SetValue(command,app.InjectorGenerators![prop.PropertyType]()); }
            catch(Exception){
                throw new NotImplementedException($"No Generator for {prop.PropertyType.Name}"); }
        return true;
    }
    public static List<PropertyInfo> GetInjectedProperties(this Command command) =>
        command.GetType()
            .GetProperties()
            .Where(x => x.GetCustomAttributes(typeof(InjectAttribute)).Any())
            .ToList();
    public static string GetAttributeGuid(this PropertyInfo property) =>
        property.GetCustomAttributes(typeof(Identifiable),false)
            .Select(x => (Identifiable) x)
            .FirstOrDefault()?
            .GetIdentity() ?? string.Empty;
    public static Stack<PropertyInfo> GetIndexProperties(this Command command) => new Stack<PropertyInfo>(
        command.GetType()
            .GetProperties()
            .Where(x => x.GetCustomAttributes(typeof(IndexedAttribute)).Any())
            .OrderBy(x => ((IndexedAttribute) (x.GetCustomAttributes(typeof(IndexedAttribute)).First())).Index)
    );
    public static Dictionary<string,PropertyInfo> GetFlagProperties(this Command command)  =>
        command.GetType()
            .GetProperties()
            .Select(x => ((FlagAttribute[]) x.GetCustomAttributes(typeof(FlagAttribute),false),x))
            .Select(x => x.Item1.Select(a => a.Flags.Select(f => (f, x.x))))
            .SelectMany(x => x)
            .SelectMany(x => x)
            .ToDictionary(x => x.f,x => x.x);

    private static List<string>? __Flags;
    private static List<string> Flags(Command command) {
        if (__Flags == null)
            __Flags = command
                .GetType()
                .GetProperties()
                .Select(p => p
                    .GetCustomAttributes()
                    .Where(x => x.GetType().IsAssignableTo(typeof(FlagAttribute)))
                    .Select(x => (FlagAttribute) x)
                    .Select(x => x.Flags)
                    .SelectMany(x => x)
                    .ToList())
                .SelectMany(x => x)
                .ToList();
        return __Flags;
    }

    public static IEnumerable<Token> Tokenize(this IEnumerable<string> args,Command command) => args
        .Select(x => 
            new Token {
                Value = x,
                Type = Flags(command).Contains(x) ? TokenType.Flag : TokenType.Value,
            });
}