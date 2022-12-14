namespace EasyCLI;

public class CommandName : System.Attribute
{
    public string Name { get; set; }
    public CommandName(string name) => Name = name; 
}

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
public abstract class FlagAttribute : System.Attribute 
{
    public string[] Flags { get; }
    public Guid Id { get; }
    public FlagAttribute(Guid id, params string[] flags) 
    { 
        Id = id;
        //make this based on all pre made flags
        if (flags.Contains("-h"))
            throw new InvalidDataException("-h is a pre-used flag");
        Flags = flags; 
    }
}

public class ManditoryAttribute : FlagAttribute
{ 
    public ManditoryAttribute(params string[] flags) : base(Guid.NewGuid(), flags) {}
}

public class EitherAttribute : FlagAttribute
{ 
    private static Guid __BaseId { get; set; }
    private static Guid BaseId {
        get {
            if (__BaseId == Guid.Empty)
                __BaseId = Guid.NewGuid();
            return __BaseId;
        }
    }
    //Should be able to accept a key or string or something so you can have multiple eithers. 
    public EitherAttribute(params string[] flags) : base(BaseId, flags) {}
    //like this then I guess it could be passed to flag attribute or just in this one, 
    //would be more versitile to go to the main wouldnt have to specify for this would just be able
    //to have one for each
    //mandatory could be like a guid for each creation
    //and not required could just be an empty string.
    //With this tho error messages wouldnt be able to be as percise so..
    public EitherAttribute(Guid id, params string[] flags) : base(id, flags) {}
}
public class NotRequiredAttribute : FlagAttribute
{ 
    public NotRequiredAttribute(params string[] flags) : base(Guid.Empty, flags) {}
}
