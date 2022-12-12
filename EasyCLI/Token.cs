namespace EasyCLI;
internal class Token
{
    public string? Value { get; set; }
    public TokenType Type { get; set; }
    public bool IsFlag => Type == TokenType.Flag;
}
internal enum TokenType
{
    Value, Flag
}