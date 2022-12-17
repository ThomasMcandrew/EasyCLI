# EasyCLI
EasyCLI is a project intended to make creating CLI tools easier.

## Getting Started
Basic setup is quite simple.
```
dotnet add package TM.EasyCLI --version 0.1.0
```
In the program.cs file put the following code.
```cs 
using EasyCLI;

return await new CliApp().Run();
```

Then to add a command just create a class that extends the Command object

```cs 
public class Example : Command
{
    protected override async Task<int> Run()
    {
        Console.WriteLine("Hello, World!");
        return 0;
    }
}
```

To Run this command in the terminal
** NOTE All examples of the command running are after building, "a" being the compiled executable
```cmd
a.exe example
```
```bash
./a example
```
* Note the command name is case insensitive

### Rename Command
To rename the command there is a CommandName Attribute that can be added to the class
```cs 
[CommandName("UpdatedName")]
public class Example : Command
{
    protected override async Task<int> Run()
    {
        Console.WriteLine("Hello, World!");
        return 0;
    }
}
```
Then to run:
```bash
./a updatedName
```

### Accepting Arguments
Having a simple way to accept arguments was the inspiration for this project.
I wanted a way that was easy to integrate and parse information, to expand on 
our example class:

```cs 
[CommandName("UpdatedName")]
public class Example : Command
{
    [NotRequired("-i","--input")]
    public string? InputArgument { get; set; }
    protected override async Task<int> Run()
    {
        Console.WriteLine("Hello, World!");
        return 0;
    }
}
```
Then to run: (Each of the following would result in the same operation)
```bash
./a updatedName -i string_to_input
./a updatedName -i "string to input" //To get spaces you need to wrap in quotes
./a updatedName -input string_to_input
./a updatedName -input "string to input"
```

#### More Arguments
As of now this accepts most types of arguments
```cs 
[CommandName("UpdatedName")]
public class Example : Command
{
    [NotRequired("-f","--float")]
    public float FloatExample { get; set; }
    [NotRequired("-b",)] //The attributes can accept any number of flag values
    public bool BoolExample { get; set; }
    [NotRequired("-l","--list")]
    public List<int>? ListExample { get; set; }
    [NotRequired("-a","--array")]
    public string[]? ArrayExample { get; set; }
    protected override async Task<int> Run()
    {
        Console.WriteLine("Hello, World!");
        return 0;
    }
}
```

```bash
./a updatedName -f 3.23 -l 23 43 53 23 -b -a str "string example" 
```
**Note 
IEnumerable<> arguments will continue to accept the arguments until the end or until another 
flag is reached
Boolean flags to not accept an input rather they are false if not inputed and true if inputed.

#### index input methods
```cs 
[CommandName("UpdatedName")]
public class Example : Command
{
    [Indexed(0)]
    public float indexExample { get; set; }
    [Indexed(3)]
    public float indexExample2 { get; set; }
 
    protected override async Task<int> Run()
    {
        Console.WriteLine("Hello, World!");
        return 0;
    }
}
```

```bash
./a updatedName 34.12 645.345
```
with index inputs there is not a need for flags. they will be accepted 
in the order they are put in with the value passed into the indexed attribute
The attributes are sorted by the number passed in, not all numbers need to be used and the argument does not 
need to be in the index that is passed in, for example 0 is the first non flag argument and 3 being the next smallest 
would accept the next non flag argument, that means you can mix flag and indexed arguements.
Example:
```cs 
[CommandName("UpdatedName")]
public class Example : Command
{
    [Indexed(0)]
    public float indexExample { get; set; }
    [Indexed(3)]
    public float indexExample2 { get; set; }
    [NotRequired("-s")]
    public string stringExample { get; set; }
    [NotRequired("-b",)] 
    public bool BoolExample { get; set; }
    protected override async Task<int> Run()
    {
        Console.WriteLine("Hello, World!");
        return 0;
    }
}
```

```bash
./a updatedName -b 34.12 -s "Input String" 645.345
```


### Required flags

Index flags are required by default. Other flags are declared based on weather or not they are required
#### [NotRequired]
Does not need to be inputed the program will run without it
#### [Required]
Will trow an error if user does not declare flag
#### [Any]
Any is an in between where the user can pass in any flag that has an [Any] tag on it. as long as one is supplied 
then the validation will pass

Any can be extended with numeric id's to have multiple Any Attributes.
Example:
```cs
[CommandName("UpdatedName")]
public class Example : Command
{
    [Any("-i")]
    public float indexExample { get; set; }
    [Any("-i2")]
    public float indexExample2 { get; set; }
    [Any(1,"-s")]
    public string stringExample { get; set; }
    [Any(1,"-s2")] 
    public string stringExample2 { get; set; }
    protected override async Task<int> Run()
    {
        Console.WriteLine("Hello, World!");
        return 0;
    }
}
```
As long as one string is passed in and one float then the verification process will work as expected.

## Injection

To inject other classed into the Command There is a Inject Attribute

```cs
[CommandName("UpdatedName")]
public class Example : Command
{
    [Inject]
    public List<int> InjectedList { get; set; }
    protected override async Task<int> Run()
    {
        Console.WriteLine("Hello, World!");
        return 0;
    }
}
```

Then to set the generator function for the List<int> add to the program setup.

```cs 
using EasyCLI;

return await 
    new CliApp()
        .ConfigureInjector<List<int>>(() => new List<int>(){23,34,234})
        .Run();
```

This will automatically inject the list provided in the generator function into the command program when run is called.
This is based on type so registering List<int> will be injected into any List<int> with a [Inject] attribute

### Generating Aliases
The main goal of this project is to have a simple place to collect all your cli programs so to make them easier I reccommend 
aliasing the command name to ignore the executable itselfs name so
```bash
./a updatedname
```
becomes 
```bash
updatedname
```

this can be done with an alias in any shell to make that easier there is a built in command "[GenerateAlias]"

```bash
$ ./a [generatealias]
alias updatedname={pathtoprogram} updatedname
//followed by each custom command created.
```
these values can then be added to your bashrc or whatever shell you use config file.
can also output the generate alias command to a file by using -f or --file flags with a file name passed into it.

```bash
$ ./a [generatealias] -f newFile.txt
$ ls
newFile.txt
$ cat newFile.txt
alias updatedname={pathtoprogram} updatedname
```