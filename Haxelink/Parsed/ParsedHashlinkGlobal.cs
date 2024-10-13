namespace Haxelink
{
    public class ParsedHashlinkGlobal(ParsedHashlinkType type)
    {
        public ParsedHashlinkType Type { get; } = type;
    }

    public class ParsedHashlinkConstant(ParsedHashlinkGlobal global, string bytes)
    {
        public ParsedHashlinkGlobal Global { get; } = global;
        public string Bytes { get; set; } = bytes;

        public static ParsedHashlinkConstant FromHashlinkConstant(Bytecode bytecode, ParsedBytecode parsedBytecode, ref HashlinkConstant hashlinkConstant)
        {
            return new(parsedBytecode.GetGlobalReference(hashlinkConstant.Global), bytecode.StringsPool[hashlinkConstant.Fields[0]]);
        }
    }
}
