namespace EasyCLI;

public abstract class FlagAttribute : System.Attribute 
{
    public string[] Flags { get; }
    public FlagAttribute(params string[] flags) { Flags = flags; }
}

public class Manditory : FlagAttribute
{ 
    public Manditory(params string[] flags) : base(flags) {}
}

public class Either : FlagAttribute
{   //Should be able to accept a key or string or something so you can have multiple eithers. 
    public Either(params string[] flags) : base(flags) {}
    //like this then I guess it could be passed to flag attribute or just in this one, 
    //would be more versitile to go to the main wouldnt have to specify for this would just be able
    //to have one for each
    //mandatory could be like a guid for each creation
    //and not required could just be an empty string.
    //With this tho error messages wouldnt be able to be as percise so..
    public Either(string id, params string[] flags) : base(flags) {}
}
public class NotRequired : FlagAttribute
{ 
    public NotRequired(params string[] flags) : base(flags) {}
}
