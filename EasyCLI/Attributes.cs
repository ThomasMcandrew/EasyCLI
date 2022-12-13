namespace EasyCLI;

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
    public FlagAttribute(params string[] flags) { Flags = flags; }
}

public class ManditoryAttribute : FlagAttribute
{ 
    public Manditory(params string[] flags) : base(flags) {}
}

public class EitherAttribute : FlagAttribute
{   //Should be able to accept a key or string or something so you can have multiple eithers. 
    public Either(params string[] flags) : base(flags) {}
    //like this then I guess it could be passed to flag attribute or just in this one, 
    //would be more versitile to go to the main wouldnt have to specify for this would just be able
    //to have one for each
    //mandatory could be like a guid for each creation
    //and not required could just be an empty string.
    //With this tho error messages wouldnt be able to be as percise so..
    public Either(Guid id, params string[] flags) : base(flags) {}
}
public class NotRequiredAttribute : FlagAttribute
{ 
    public NotRequired(params string[] flags) : base(flags) {}
}
