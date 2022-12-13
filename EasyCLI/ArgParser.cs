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

    public static IEnumerable<T> ToListType<T>(IEnumerable<T> collection, Type type)
    {
        if (type.IsAssignableTo(typeof(List<T>)))
            return collection.ToList();
        if (type.IsAssignableTo(typeof(T[])))
            return collection.ToArray();
        return collection.AsEnumerable();
    }
    public static bool parse(this IEnumerable<string> args, Command command)
    {
        var properties = command.GetProperties();
        var tokens = args.Tokenize().ToArray();
        var i = 0;
        while(i < tokens.Count())
        {
            if(!(tokens[i].IsFlag && properties.ContainsKey(tokens[i].Value ?? string.Empty)))
            {
                Console.WriteLine($"Unrecognized token {tokens[i].Value}");
                return false;
            }
            var prop = properties[tokens[i].Value ?? string.Empty];

            i++;

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
        /*
            run a check that 
            * all mandatory have a value or list none
            * at least one either or list none 
        */
        return true;
    }
    public static Dictionary<string,PropertyInfo> GetProperties(this Command command)  =>
        command.GetType()
            .GetProperties()
            .Where(x => x.GetCustomAttributes(false)
                .Where(y => y.GetType().IsAssignableTo(typeof(FlagAttribute)))
                .Any()
            )
            .Select(x => (
                ((FlagAttribute) x.GetCustomAttributes(false)
                    .Where(y => y.GetType().IsAssignableTo(typeof(FlagAttribute)))
                .FirstOrDefault()!)
                .Flags, x))
            .Select(x => x.Flags.Select(f => (f,x.x)))
            .SelectMany(x => x)
            .ToDictionary(x => x.f,x => x.x);


    /*
        This eventually needs to account for spaces that are contained in quotes

        This may be done via the args from the command line
        Same with clean, that may be able to be removed
        //TODO
    */
    public static IEnumerable<Token> Tokenize(this IEnumerable<string> args) => args
        .Select(x => 
            new Token {
                Value = x.Clean(),
                //I would like to update this to use values passed as flags 
                //so that they can start with anything
                //TODO
                Type = x.StartsWith("-") ? TokenType.Flag : TokenType.Value,
            });

    public static string Clean(this string s)
    {
        if(s.StartsWith('"') && s.EndsWith('"'))
            return s[1..(s.Length-1)];
        return s;
    }
}