namespace Haxelink
{
    public abstract class HashlinkFunction
    {
        public int Type { get; }
        public int Index { get; }

        public ParsedHashlinkFunction? ParsedHashlinkFunction { get; }

        public HashlinkFunction(int type, int index)
        {
            Type = type;
            Index = index;
        }

        public HashlinkFunction(ParsedHashlinkFunction parsedHashlinkFunction, int type, int index)
        {
            ParsedHashlinkFunction = parsedHashlinkFunction;
            Type = type;
            Index = index;
        }
    }

    public class HashlinkNative : HashlinkFunction
    {
        public int LibraryName { get; }
        public int FunctionName { get; }

        public HashlinkNative(int libraryName, int functionName, int type, int index) : base(type, index)
        {
            LibraryName = libraryName;
            FunctionName = functionName;
        }

        public HashlinkNative(ParsedHashlinkNative parsedHashlinkNative, int libraryName, int functionName, int type, int index) : base(parsedHashlinkNative, type, index)
        {
            LibraryName = libraryName;
            FunctionName = functionName;
        }
    }

    public class HashlinkMethod : HashlinkFunction
    {
        public int[] Registers { get; }
        public Opcode[] Opcodes { get; private set; }
        public (int, int)[]? DebugInfo { get; }
        public (int, int)[]? Assigns { get; }

        public HashlinkMethod(int[] registers, Opcode[] opcodes, (int, int)[]? debugInfo, (int, int)[]? assigns, int type, int index) : base(type, index)
        {
            Registers = registers;
            Opcodes = opcodes;
            DebugInfo = debugInfo;
            Assigns = assigns;
        }

        public HashlinkMethod(ParsedHashlinkMethod parsedHashlinkMethod, int[] registers, (int, int)[]? debugInfo, (int, int)[]? assigns, int type, int index) : base(parsedHashlinkMethod, type, index)
        {
            Registers = registers;
            DebugInfo = debugInfo;
            Assigns = assigns;
        }

        public void SetupOpcodes(Func<OpcodeArgument, int> parseArgument)
        {
            Opcodes = [.. ((ParsedHashlinkMethod)ParsedHashlinkFunction!).Opcodes.Select(x => Opcode.FromParsedOpcode(x, parseArgument))];
        }
    }
}
