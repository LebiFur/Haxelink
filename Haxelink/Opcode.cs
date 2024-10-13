namespace Haxelink
{
    public enum OpcodeType
    {
        Mov,
        Int,
        Float,
        Bool,
        Bytes,
        String,
        Null,
        Add,
        Sub,
        Mul,
        SDiv,
        UDiv,
        SMod,
        UMod,
        Shl,
        SShr,
        UShr,
        And,
        Or,
        Xor,
        Neg,
        Not,
        Incr,
        Decr,
        Call0,
        Call1,
        Call2,
        Call3,
        Call4,
        CallN,
        CallMethod,
        CallThis,
        CallClosure,
        StaticClosure,
        InstanceClosure,
        VirtualClosure,
        GetGlobal,
        SetGlobal,
        Field,
        SetField,
        GetThis,
        SetThis,
        DynGet,
        DynSet,
        JTrue,
        JFalse,
        JNull,
        JNotNull,
        JSLt,
        JSGte,
        JSGt,
        JSLte,
        JULt,
        JUGte,
        JNotLt,
        JNotGte,
        JEq,
        JNotEq,
        JAlways,
        ToDyn,
        ToSFloat,
        ToUFloat,
        ToInt,
        SafeCast,
        UnsafeCast,
        ToVirtual,
        Label,
        Ret,
        Throw,
        Rethrow,
        Switch,
        NullCheck,
        Trap,
        EndTrap,
        GetI8,
        GetI16,
        GetMem,
        GetArray,
        SetI8,
        SetI16,
        SetMem,
        SetArray,
        New,
        ArraySize,
        Type,
        GetType,
        GetTID,
        Ref,
        Unref,
        Setref,
        MakeEnum,
        EnumAlloc,
        EnumIndex,
        EnumField,
        SetEnumField,
        Assert,
        RefData,
        RefOffset,
        Nop,
        Prefetch,
        Asm
    }

    public class Opcode(OpcodeType type, int[] args)
    {
        public static readonly int[] opcodeArgumentsCount = [2, 2, 2, 2, 2, 2, 1, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 2, 2, 1, 1, 2, 3, 4, 5, 6, -2, -2, -2, -2, 2, 3, 3, 2, 2, 3, 3, 2, 2, 3, 3, 2, 2, 2, 2, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 1, 2, 2, 2, 2, 2, 2, 2, 0, 1, 1, 1, int.MaxValue, 1, 2, 1, 3, 3, 3, 3, 3, 3, 3, 3, 1, 2, 2, 2, 2, 2, 2, 2, -2, 2, 2, 4, 3, 0, 2, 3, 0, 3, 3];

        public OpcodeType Type { get; } = type;
        public int[] Args { get; } = args;

        public static Opcode FromParsedOpcode(ParsedOpcode parsedOpcode, Func<OpcodeArgument, int> parseArgument)
        {
            OpcodeType? type = parsedOpcode.Type;

            if (type == null)
            {
                if (parsedOpcode.GetType() != typeof(ParsedOpcodeCall))
                    throw new Exception($"Cannot construct opcode {parsedOpcode}");

                type = parsedOpcode.Arguments.Length switch
                {
                    2 => (OpcodeType?)OpcodeType.Call0,
                    3 => (OpcodeType?)OpcodeType.Call1,
                    4 => (OpcodeType?)OpcodeType.Call2,
                    5 => (OpcodeType?)OpcodeType.Call3,
                    6 => (OpcodeType?)OpcodeType.Call4,
                    _ => (OpcodeType?)OpcodeType.CallN
                };
            }

            return new(type.Value, [.. parsedOpcode.Arguments.Select(x => parseArgument(x))]);
        }
    }
}
