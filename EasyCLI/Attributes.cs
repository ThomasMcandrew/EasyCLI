namespace EasyCLI;

public class CommandName : System.Attribute
{
    public string Name { get; set; }
    public CommandName(string name) => Name = name; 
}

public class InjectAttribute : System.Attribute { }

public class HelpAttribute : System.Attribute 
{
	public string? Value { get; }
	public HelpAttribute(string val)
	{
        // I want to be able to format text how I want
        // without having to have it all the way to the other 
        //side so this will remove whitespace before the paragraph
		Value = val.Split('\n')
            .Select(x => x.Trim())
            .Aggregate((x,y) => $"{x}\n{y}");
	}
}
public interface Identifiable
{
    public string GetIdentity();
}
public class IndexedAttribute : System.Attribute , Identifiable
{
    public int Index { get; set; }
    public IndexedAttribute(int i)
    {
        Index = i;
    }
    public string GetIdentity() => $"{nameof(IndexedAttribute)}{Index}{(char)8}";
}
public abstract class FlagAttribute : System.Attribute , Identifiable
{
    public string[] Flags { get; }
    public string Id { get; }
    public string GetIdentity() => Id;
    public FlagAttribute(string id, params string[] flags) 
    { 
        Id = id;
        //make this based on all pre made flags
        if (flags.Contains("-h"))
            throw new InvalidDataException("-h is a pre-used flag");
        Flags = flags; 
    }
}

public class RequiredAttribute : FlagAttribute
{ 
    public RequiredAttribute(params string[] flags) : base(flags.Aggregate((x,y) => x+y), flags) {}
}

public class AnyAttribute : FlagAttribute
{ 
    private static string? __BaseId { get; set; }
    private static string BaseId {
        get {
            if (__BaseId == null)
                __BaseId = $"BASE_STRING::{ (char) 8} { (char) 4}";
            return __BaseId;
        }
    }
    //Should be able to accept a key or string or something so you can have multiple eithers. 
    public AnyAttribute(params string[] flags) : base(BaseId, flags) {}
    //like this then I guess it could be passed to flag attribute or just in this one, 
    //would be more versitile to go to the main wouldnt have to specify for this would just be able
    //to have one for each
    //mandatory could be like a guid for each creation
    //and not required could just be an empty string.
    //With this tho error messages wouldnt be able to be as percise so..
    public AnyAttribute(int id, params string[] flags) : base($"{id}", flags) {}
}
public class NotRequiredAttribute : FlagAttribute
{ 
    public NotRequiredAttribute(params string[] flags) : base(string.Empty, flags) {}
}
