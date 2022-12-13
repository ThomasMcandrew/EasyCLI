using System.Reflection;

namespace EasyCLI;

internal static class ArgParser
{
    public static object ParseByType(this Type type,string value)
    {
        return "";
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
            /*
                There has to be a way to do complex 
                object arrays in this format 

                the string switch sucks would be cool 
                if there was another way to go about this.

                im thinking in the default if its a collection based on substring
                see if collection is in that or if its an array

                have a dict with string to func to do the to list part

            */ 
            switch (prop.PropertyType.ToString())
            {
                case "System.String":
                    prop.SetValue(command,tokens[i].Value);
                    break;
                case "System.Int32":
                    prop.SetValue(command,int.Parse(tokens[i].Value ?? ""));
                    break;
                case "System.Double":
                    prop.SetValue(command,double.Parse(tokens[i].Value ?? ""));
                    break;
                case "System.Single":
                    prop.SetValue(command,float.Parse(tokens[i].Value ?? ""));
                    break;
                case "System.Decimal":
                    prop.SetValue(command,decimal.Parse(tokens[i].Value ?? ""));
                    break;
                case "System.DateTime":
                    prop.SetValue(command,DateTime.Parse(tokens[i].Value ?? ""));
                    break;
                case "System.Boolean":
                    prop.SetValue(command,true);
                    i--;
                    break;
                default:
                    var enumerableType = prop.PropertyType.GetGenericArguments()[0];
                    Type genericListType = typeof(List<>).MakeGenericType(enumerableType);
                    var list = (System.Collections.IList)Activator.CreateInstance(genericListType)!;
                    while(!tokens[i+1].IsFlag)
                    {
                        list.Add(enumerableType.ParseByType(tokens[i].Value!));
                        i++;
                    }
                    list.Add(tokens[i].Value!);
                    if (prop.PropertyType.IsAssignableTo(typeof(List<>)))
                        prop.SetValue(command, list);
                    break;
                // case "System.Collections.Generic.List`1[System.Object]":
                    // var list = new List<Object>();
                    // while(!tokens[i+1].IsFlag)
                    // {
                        // list.Add(tokens[i].Value!);
                        // i++;
                    // }
                    // list.Add(tokens[i].Value!);
                    // prop.SetValue(command,list);
                    // break;
                // case "System.Collections.Generic.IEnumerable`1[System.Object]":
                    // var list2 = new List<Object>();
                    // while(!tokens[i+1].IsFlag)
                    // {
                        // list2.Add(tokens[i].Value!);
                        // i++;
                    // }
                    // list2.Add(tokens[i].Value!);
                    // prop.SetValue(command,list2.AsEnumerable());
                    // break;
                // case "System.Object[]":
                    // var list3 = new List<Object>();
                    // while(!tokens[i+1].IsFlag)
                    // {
                        // list3.Add(tokens[i].Value!);
                        // i++;
                    // }
                    // list3.Add(tokens[i].Value!);
                    // prop.SetValue(command,list3.ToArray());
                    // break;
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
        // TODO
    */
    public static IEnumerable<Token> Tokenize(this IEnumerable<string> args) => args
        .Select(x => 
            new Token {
                Value = x.Clean(),
                Type = x.StartsWith("-") ? TokenType.Flag : TokenType.Value,
            });

    public static string Clean(this string s)
    {
        if(s.StartsWith('"') && s.EndsWith('"'))
            return s[1..(s.Length-1)];
        return s;
    }
}