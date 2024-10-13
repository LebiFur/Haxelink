namespace Haxelink
{
    public abstract class ParsedHashlinkFunction(HashlinkFunction hashlinkFunction, ParsedBytecode parsedBytecode)
    {
        public string? Name { get; internal set; }

        public abstract string FullName { get; }

        public HashlinkFunction HashlinkFunction { get; } = hashlinkFunction;
        public ParsedHashlinkFunType Type { get; } = (ParsedHashlinkFunType)parsedBytecode.GetTypeReference(hashlinkFunction.Type);
    }

    public class ParsedHashlinkNative : ParsedHashlinkFunction
    {
        public string LibraryName { get; }

        public override string FullName => $"{LibraryName}.{Name}";

        public ParsedHashlinkNative(HashlinkNative hashlinkNative, Bytecode bytecode, ParsedBytecode parsedBytecode) : base(hashlinkNative, parsedBytecode)
        {
            Name = bytecode.StringsPool[hashlinkNative.FunctionName];
            LibraryName = bytecode.StringsPool[hashlinkNative.LibraryName];
        }
    }

    public class ParsedHashlinkMethod : ParsedHashlinkFunction
    {
        public ParsedHashlinkObj? Parent { get; internal set; }
        public ParsedHashlinkType[] Registers { get; set; }
        public ParsedOpcode[] Opcodes { get; set; }

        public override string FullName
        {
            get
            {
                string methodName = Name ?? $"anonymous@{HashlinkFunction.Index}";
                return Parent != null ? $"{Parent.Name}.{methodName}" : methodName;
            }
        }

        public ParsedHashlinkMethod(HashlinkMethod hashlinkMethod, Bytecode bytecode, ParsedBytecode parsedBytecode) : base(hashlinkMethod, parsedBytecode)
        {
            Registers = [.. hashlinkMethod.Registers.Select(parsedBytecode.GetTypeReference)];
        }

        public void ParseOpcodes(Bytecode bytecode, ParsedBytecode parsedBytecode)
        {
            Opcodes = [.. ((HashlinkMethod)HashlinkFunction).Opcodes.Select(x => ParsedOpcode.FromOpcode(parsedBytecode, bytecode, x))];
        }
    }
}
