namespace Haxelink
{
    public interface IParsedOpcodeArgument<T> where T : OpcodeArgument
    {
        public static abstract T FromRaw(ParsedBytecode parsedBytecode, Bytecode bytecode, int value);
    }

    public abstract class OpcodeArgument(object value)
    {
        public object Value { get; } = value;
    }

    public abstract class OpcodeArgument<T>(T value) : OpcodeArgument(value) where T : notnull
    {
        public new T Value { get; } = value;
    }

    public class ArgumentRegister(int value) : OpcodeArgument<int>(value), IParsedOpcodeArgument<ArgumentRegister>
    {
        public static ArgumentRegister FromRaw(ParsedBytecode parsedBytecode, Bytecode bytecode, int value) => new(value);
    }

    public class ArgumentInt(int value) : OpcodeArgument<int>(value), IParsedOpcodeArgument<ArgumentInt>
    {
        public static ArgumentInt FromRaw(ParsedBytecode parsedBytecode, Bytecode bytecode, int value) => new(bytecode.IntsPool[value]);
    }

    public class ArgumentFloat(double value) : OpcodeArgument<double>(value), IParsedOpcodeArgument<ArgumentFloat>
    {
        public static ArgumentFloat FromRaw(ParsedBytecode parsedBytecode, Bytecode bytecode, int value) => new(bytecode.FloatsPool[value]);
    }

    public class ArgumentBool(bool value) : OpcodeArgument<bool>(value), IParsedOpcodeArgument<ArgumentBool>
    {
        public static ArgumentBool FromRaw(ParsedBytecode parsedBytecode, Bytecode bytecode, int value) => new(value != 0);
    }

    public class ArgumentString(string value) : OpcodeArgument<string>(value), IParsedOpcodeArgument<ArgumentString>
    {
        public static ArgumentString FromRaw(ParsedBytecode parsedBytecode, Bytecode bytecode, int value) => new(bytecode.StringsPool[value]);
    }

    public class ArgumentFunction(ParsedHashlinkFunction value) : OpcodeArgument<ParsedHashlinkFunction>(value), IParsedOpcodeArgument<ArgumentFunction>
    {
        public static ArgumentFunction FromRaw(ParsedBytecode parsedBytecode, Bytecode bytecode, int value) => new(parsedBytecode.GetFunctionReference(value));
    }

    public class ArgumentField(int value) : OpcodeArgument<int>(value), IParsedOpcodeArgument<ArgumentField>
    {
        public static ArgumentField FromRaw(ParsedBytecode parsedBytecode, Bytecode bytecode, int value) => new(value);
    }

    public class ArgumentOffset(int value) : OpcodeArgument<int>(value), IParsedOpcodeArgument<ArgumentOffset>
    {
        public static ArgumentOffset FromRaw(ParsedBytecode parsedBytecode, Bytecode bytecode, int value) => new(value);
    }

    public class ArgumentType(ParsedHashlinkType value) : OpcodeArgument<ParsedHashlinkType>(value), IParsedOpcodeArgument<ArgumentType>
    {
        public static ArgumentType FromRaw(ParsedBytecode parsedBytecode, Bytecode bytecode, int value) => new(parsedBytecode.GetTypeReference(value));
    }

    public class ArgumentInlineInt(int value) : OpcodeArgument<int>(value), IParsedOpcodeArgument<ArgumentInlineInt>
    {
        public static ArgumentInlineInt FromRaw(ParsedBytecode parsedBytecode, Bytecode bytecode, int value) => new(value);
    }

    public class ArgumentGlobal(ParsedHashlinkGlobal value) : OpcodeArgument<ParsedHashlinkGlobal>(value), IParsedOpcodeArgument<ArgumentGlobal>
    {
        public static ArgumentGlobal FromRaw(ParsedBytecode parsedBytecode, Bytecode bytecode, int value) => new(parsedBytecode.GetGlobalReference(value));
    }

    public class ArgumentEnumConstruct(int value) : OpcodeArgument<int>(value), IParsedOpcodeArgument<ArgumentEnumConstruct>
    {
        public static ArgumentEnumConstruct FromRaw(ParsedBytecode parsedBytecode, Bytecode bytecode, int value) => new(value);
    }

    public abstract class ParsedOpcode(OpcodeType? type, params OpcodeArgument[] arguments)
    {
        public OpcodeArgument[] Arguments { get; } = arguments;
        public OpcodeType? Type { get; } = type;

        public static ParsedOpcode FromOpcode(ParsedBytecode parsedBytecode, Bytecode bytecode, Opcode opcode)
        {
            return opcode.Type switch
            {
                OpcodeType.Mov => new ParsedOpcodeMov(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[1])),
                OpcodeType.Int => new ParsedOpcodeInt(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0]), ArgumentInt.FromRaw(parsedBytecode, bytecode, opcode.Args[1])),
                OpcodeType.Float => new ParsedOpcodeFloat(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0]), ArgumentFloat.FromRaw(parsedBytecode, bytecode, opcode.Args[1])),
                OpcodeType.Bool => new ParsedOpcodeBool(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0]), ArgumentBool.FromRaw(parsedBytecode, bytecode, opcode.Args[1])),
                //case OpcodeType.Bytes:
                OpcodeType.String => new ParsedOpcodeString(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0]), ArgumentString.FromRaw(parsedBytecode, bytecode, opcode.Args[1])),
                OpcodeType.Null => new ParsedOpcodeNull(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0])),
                OpcodeType.Add => new ParsedOpcodeAdd(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[1]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[2])),
                OpcodeType.Sub => new ParsedOpcodeSub(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[1]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[2])),
                OpcodeType.Mul => new ParsedOpcodeMul(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[1]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[2])),
                OpcodeType.SDiv => new ParsedOpcodeSDiv(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[1]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[2])),
                OpcodeType.UDiv => new ParsedOpcodeUDiv(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[1]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[2])),
                OpcodeType.SMod => new ParsedOpcodeSMod(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[1]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[2])),
                OpcodeType.UMod => new ParsedOpcodeUMod(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[1]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[2])),
                OpcodeType.Shl => new ParsedOpcodeShl(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[1]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[2])),
                OpcodeType.SShr => new ParsedOpcodeSShr(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[1]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[2])),
                OpcodeType.UShr => new ParsedOpcodeUShr(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[1]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[2])),
                OpcodeType.And => new ParsedOpcodeAnd(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[1]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[2])),
                OpcodeType.Or => new ParsedOpcodeOr(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[1]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[2])),
                OpcodeType.Xor => new ParsedOpcodeXor(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[1]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[2])),
                OpcodeType.Neg => new ParsedOpcodeNeg(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[1])),
                OpcodeType.Not => new ParsedOpcodeNot(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[1])),
                OpcodeType.Incr => new ParsedOpcodeIncr(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0])),
                OpcodeType.Decr => new ParsedOpcodeDecr(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0])),
                OpcodeType.Call0 => new ParsedOpcodeCall(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0]), ArgumentFunction.FromRaw(parsedBytecode, bytecode, opcode.Args[1])),
                OpcodeType.Call1 or OpcodeType.Call2 or OpcodeType.Call3 or OpcodeType.Call4 or OpcodeType.CallN => new ParsedOpcodeCall(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0]), ArgumentFunction.FromRaw(parsedBytecode, bytecode, opcode.Args[1]), [.. opcode.Args[2..].Select(x => ArgumentRegister.FromRaw(parsedBytecode, bytecode, x))]),
                OpcodeType.CallMethod => new ParsedOpcodeCallMethod(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0]), ArgumentField.FromRaw(parsedBytecode, bytecode, opcode.Args[1]), opcode.Args.Length >= 3 ? [.. opcode.Args[2..].Select(x => ArgumentRegister.FromRaw(parsedBytecode, bytecode, x))] : null),
                OpcodeType.CallThis => new ParsedOpcodeCallThis(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0]), ArgumentField.FromRaw(parsedBytecode, bytecode, opcode.Args[1]), opcode.Args.Length >= 3 ? [.. opcode.Args[2..].Select(x => ArgumentRegister.FromRaw(parsedBytecode, bytecode, x))] : null),
                OpcodeType.CallClosure => new ParsedOpcodeCallClosure(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[1]), opcode.Args.Length >= 3 ? [.. opcode.Args[2..].Select(x => ArgumentRegister.FromRaw(parsedBytecode, bytecode, x))] : null),
                OpcodeType.StaticClosure => new ParsedOpcodeStaticClosure(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0]), ArgumentFunction.FromRaw(parsedBytecode, bytecode, opcode.Args[1])),
                OpcodeType.InstanceClosure => new ParsedOpcodeInstanceClosure(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0]), ArgumentFunction.FromRaw(parsedBytecode, bytecode, opcode.Args[1]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[2])),
                OpcodeType.VirtualClosure => new ParsedOpcodeVirtualClosure(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[1]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[2])),
                OpcodeType.GetGlobal => new ParsedOpcodeGetGlobal(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0]), ArgumentGlobal.FromRaw(parsedBytecode, bytecode, opcode.Args[1])),
                OpcodeType.SetGlobal => new ParsedOpcodeSetGlobal(ArgumentGlobal.FromRaw(parsedBytecode, bytecode, opcode.Args[0]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[1])),
                OpcodeType.Field => new ParsedOpcodeField(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[1]), ArgumentField.FromRaw(parsedBytecode, bytecode, opcode.Args[2])),
                OpcodeType.SetField => new ParsedOpcodeSetField(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0]), ArgumentField.FromRaw(parsedBytecode, bytecode, opcode.Args[1]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[2])),
                OpcodeType.GetThis => new ParsedOpcodeGetThis(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0]), ArgumentField.FromRaw(parsedBytecode, bytecode, opcode.Args[1])),
                OpcodeType.SetThis => new ParsedOpcodeSetThis(ArgumentField.FromRaw(parsedBytecode, bytecode, opcode.Args[0]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[1])),
                OpcodeType.DynGet => new ParsedOpcodeDynGet(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[1]), ArgumentString.FromRaw(parsedBytecode, bytecode, opcode.Args[2])),
                OpcodeType.DynSet => new ParsedOpcodeDynSet(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0]), ArgumentString.FromRaw(parsedBytecode, bytecode, opcode.Args[1]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[2])),
                OpcodeType.JTrue => new ParsedOpcodeJTrue(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0]), ArgumentOffset.FromRaw(parsedBytecode, bytecode, opcode.Args[1])),
                OpcodeType.JFalse => new ParsedOpcodeJFalse(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0]), ArgumentOffset.FromRaw(parsedBytecode, bytecode, opcode.Args[1])),
                OpcodeType.JNull => new ParsedOpcodeJNull(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0]), ArgumentOffset.FromRaw(parsedBytecode, bytecode, opcode.Args[1])),
                OpcodeType.JNotNull => new ParsedOpcodeJNotNull(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0]), ArgumentOffset.FromRaw(parsedBytecode, bytecode, opcode.Args[1])),
                OpcodeType.JSLt => new ParsedOpcodeJSLt(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[1]), ArgumentOffset.FromRaw(parsedBytecode, bytecode, opcode.Args[2])),
                OpcodeType.JSGte => new ParsedOpcodeJSGte(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[1]), ArgumentOffset.FromRaw(parsedBytecode, bytecode, opcode.Args[2])),
                OpcodeType.JSGt => new ParsedOpcodeJSGt(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[1]), ArgumentOffset.FromRaw(parsedBytecode, bytecode, opcode.Args[2])),
                OpcodeType.JSLte => new ParsedOpcodeJSLte(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[1]), ArgumentOffset.FromRaw(parsedBytecode, bytecode, opcode.Args[2])),
                OpcodeType.JULt => new ParsedOpcodeJULt(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[1]), ArgumentOffset.FromRaw(parsedBytecode, bytecode, opcode.Args[2])),
                OpcodeType.JUGte => new ParsedOpcodeJUGte(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[1]), ArgumentOffset.FromRaw(parsedBytecode, bytecode, opcode.Args[2])),
                OpcodeType.JNotLt => new ParsedOpcodeJNotLt(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[1]), ArgumentOffset.FromRaw(parsedBytecode, bytecode, opcode.Args[2])),
                OpcodeType.JNotGte => new ParsedOpcodeJNotGte(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[1]), ArgumentOffset.FromRaw(parsedBytecode, bytecode, opcode.Args[2])),
                OpcodeType.JEq => new ParsedOpcodeJEq(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[1]), ArgumentOffset.FromRaw(parsedBytecode, bytecode, opcode.Args[2])),
                OpcodeType.JNotEq => new ParsedOpcodeJNotEq(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[1]), ArgumentOffset.FromRaw(parsedBytecode, bytecode, opcode.Args[2])),
                OpcodeType.JAlways => new ParsedOpcodeJAlways(ArgumentOffset.FromRaw(parsedBytecode, bytecode, opcode.Args[0])),
                OpcodeType.ToDyn => new ParsedOpcodeToDyn(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[1])),
                OpcodeType.ToSFloat => new ParsedOpcodeToSFloat(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[1])),
                OpcodeType.ToUFloat => new ParsedOpcodeToUFloat(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[1])),
                OpcodeType.ToInt => new ParsedOpcodeToInt(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[1])),
                OpcodeType.SafeCast => new ParsedOpcodeSafeCast(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[1])),
                OpcodeType.UnsafeCast => new ParsedOpcodeUnsafeCast(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[1])),
                OpcodeType.ToVirtual => new ParsedOpcodeToVirtual(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[1])),
                OpcodeType.Label => new ParsedOpcodeLabel(),
                OpcodeType.Ret => new ParsedOpcodeRet(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0])),
                OpcodeType.Throw => new ParsedOpcodeThrow(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0])),
                OpcodeType.Rethrow => new ParsedOpcodeRethrow(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0])),
                OpcodeType.Switch => new ParsedOpcodeSwitch(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0]), [.. opcode.Args[1..^2].Select(x => ArgumentOffset.FromRaw(parsedBytecode, bytecode, x))], ArgumentOffset.FromRaw(parsedBytecode, bytecode, opcode.Args[^1])),
                OpcodeType.NullCheck => new ParsedOpcodeNullCheck(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0])),
                OpcodeType.Trap => new ParsedOpcodeTrap(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0]), ArgumentOffset.FromRaw(parsedBytecode, bytecode, opcode.Args[1])),
                OpcodeType.EndTrap => new ParsedOpcodeEndTrap(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0])),
                OpcodeType.GetI8 => new ParsedOpcodeGetI8(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[1]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[2])),
                OpcodeType.GetI16 => new ParsedOpcodeGetI16(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[1]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[2])),
                OpcodeType.GetMem => new ParsedOpcodeGetMem(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[1]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[2])),
                OpcodeType.GetArray => new ParsedOpcodeGetArray(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[1]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[2])),
                OpcodeType.SetI8 => new ParsedOpcodeSetI8(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[1]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[2])),
                OpcodeType.SetI16 => new ParsedOpcodeSetI16(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[1]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[2])),
                OpcodeType.SetMem => new ParsedOpcodeSetMem(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[1]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[2])),
                OpcodeType.SetArray => new ParsedOpcodeSetArray(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[1]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[2])),
                OpcodeType.New => new ParsedOpcodeNew(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0])),
                OpcodeType.ArraySize => new ParsedOpcodeArraySize(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[1])),
                OpcodeType.Type => new ParsedOpcodeType(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0]), ArgumentType.FromRaw(parsedBytecode, bytecode, opcode.Args[1])),
                OpcodeType.GetType => new ParsedOpcodeGetType(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[1])),
                OpcodeType.GetTID => new ParsedOpcodeGetTID(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[1])),
                OpcodeType.Ref => new ParsedOpcodeRef(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[1])),
                OpcodeType.Unref => new ParsedOpcodeUnref(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[1])),
                OpcodeType.Setref => new ParsedOpcodeSetref(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[1])),
                OpcodeType.MakeEnum => new ParsedOpcodeMakeEnum(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0]), ArgumentEnumConstruct.FromRaw(parsedBytecode, bytecode, opcode.Args[1]), opcode.Args.Length >= 3 ? [.. opcode.Args[2..].Select(x => ArgumentRegister.FromRaw(parsedBytecode, bytecode, x))] : null),
                OpcodeType.EnumAlloc => new ParsedOpcodeEnumAlloc(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0]), ArgumentEnumConstruct.FromRaw(parsedBytecode, bytecode, opcode.Args[1])),
                OpcodeType.EnumIndex => new ParsedOpcodeEnumIndex(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[1])),
                OpcodeType.EnumField => new ParsedOpcodeEnumField(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[1]), ArgumentEnumConstruct.FromRaw(parsedBytecode, bytecode, opcode.Args[2]), ArgumentField.FromRaw(parsedBytecode, bytecode, opcode.Args[3])),
                OpcodeType.SetEnumField => new ParsedOpcodeSetEnumField(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0]), ArgumentField.FromRaw(parsedBytecode, bytecode, opcode.Args[1]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[2])),
                OpcodeType.Assert => new ParsedOpcodeAssert(),
                OpcodeType.RefData => new ParsedOpcodeRefData(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[1])),
                OpcodeType.RefOffset => new ParsedOpcodeRefOffset(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[1]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[2])),
                OpcodeType.Nop => new ParsedOpcodeNop(),
                OpcodeType.Prefetch => new ParsedOpcodePrefetch(ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[0]), ArgumentField.FromRaw(parsedBytecode, bytecode, opcode.Args[1]), ArgumentInlineInt.FromRaw(parsedBytecode, bytecode, opcode.Args[2])),
                OpcodeType.Asm => new ParsedOpcodeAsm(ArgumentInlineInt.FromRaw(parsedBytecode, bytecode, opcode.Args[0]), ArgumentInlineInt.FromRaw(parsedBytecode, bytecode, opcode.Args[1]), ArgumentRegister.FromRaw(parsedBytecode, bytecode, opcode.Args[2])),
                _ => throw new Exception($"Opcode {opcode.Type} cannot be parsed"),
            };
        }
    }

    public class ParsedOpcodeMov(ArgumentRegister dst, ArgumentRegister src) : ParsedOpcode(OpcodeType.Mov, dst, src);
    public class ParsedOpcodeInt(ArgumentRegister dst, ArgumentInt val) : ParsedOpcode(OpcodeType.Int, dst, val);
    public class ParsedOpcodeFloat(ArgumentRegister dst, ArgumentFloat val) : ParsedOpcode(OpcodeType.Float, dst, val);
    public class ParsedOpcodeBool(ArgumentRegister dst, ArgumentBool val) : ParsedOpcode(OpcodeType.Bool, dst, val);
    //bytes
    public class ParsedOpcodeString(ArgumentRegister dst, ArgumentString val) : ParsedOpcode(OpcodeType.String, dst, val);
    public class ParsedOpcodeNull(ArgumentRegister dst) : ParsedOpcode(OpcodeType.Null, dst);
    public class ParsedOpcodeAdd(ArgumentRegister dst, ArgumentRegister a, ArgumentRegister b) : ParsedOpcode(OpcodeType.Add, dst, a, b);
    public class ParsedOpcodeSub(ArgumentRegister dst, ArgumentRegister a, ArgumentRegister b) : ParsedOpcode(OpcodeType.Sub, dst, a, b);
    public class ParsedOpcodeMul(ArgumentRegister dst, ArgumentRegister a, ArgumentRegister b) : ParsedOpcode(OpcodeType.Mul, dst, a, b);
    public class ParsedOpcodeSDiv(ArgumentRegister dst, ArgumentRegister a, ArgumentRegister b) : ParsedOpcode(OpcodeType.SDiv, dst, a, b);
    public class ParsedOpcodeUDiv(ArgumentRegister dst, ArgumentRegister a, ArgumentRegister b) : ParsedOpcode(OpcodeType.UDiv, dst, a, b);
    public class ParsedOpcodeSMod(ArgumentRegister dst, ArgumentRegister a, ArgumentRegister b) : ParsedOpcode(OpcodeType.SMod, dst, a, b);
    public class ParsedOpcodeUMod(ArgumentRegister dst, ArgumentRegister a, ArgumentRegister b) : ParsedOpcode(OpcodeType.UMod, dst, a, b);
    public class ParsedOpcodeShl(ArgumentRegister dst, ArgumentRegister a, ArgumentRegister b) : ParsedOpcode(OpcodeType.Shl, dst, a, b);
    public class ParsedOpcodeSShr(ArgumentRegister dst, ArgumentRegister a, ArgumentRegister b) : ParsedOpcode(OpcodeType.SShr, dst, a, b);
    public class ParsedOpcodeUShr(ArgumentRegister dst, ArgumentRegister a, ArgumentRegister b) : ParsedOpcode(OpcodeType.UShr, dst, a, b);
    public class ParsedOpcodeAnd(ArgumentRegister dst, ArgumentRegister a, ArgumentRegister b) : ParsedOpcode(OpcodeType.And, dst, a, b);
    public class ParsedOpcodeOr(ArgumentRegister dst, ArgumentRegister a, ArgumentRegister b) : ParsedOpcode(OpcodeType.Or, dst, a, b);
    public class ParsedOpcodeXor(ArgumentRegister dst, ArgumentRegister a, ArgumentRegister b) : ParsedOpcode(OpcodeType.Xor, dst, a, b);
    public class ParsedOpcodeNeg(ArgumentRegister dst, ArgumentRegister src) : ParsedOpcode(OpcodeType.Neg, dst, src);
    public class ParsedOpcodeNot(ArgumentRegister dst, ArgumentRegister src) : ParsedOpcode(OpcodeType.Not, dst, src);
    public class ParsedOpcodeIncr(ArgumentRegister dst) : ParsedOpcode(OpcodeType.Incr, dst);
    public class ParsedOpcodeDecr(ArgumentRegister dst) : ParsedOpcode(OpcodeType.Decr, dst);
    public class ParsedOpcodeCall(ArgumentRegister dst, ArgumentFunction fun, params ArgumentRegister[]? args) : ParsedOpcode(null, args == null ? [dst, fun] : [dst, fun, .. args]);
    public class ParsedOpcodeCallMethod(ArgumentRegister dst, ArgumentField field, params ArgumentRegister[]? args) : ParsedOpcode(OpcodeType.CallMethod, args == null ? [dst, field] : [dst, field, .. args]);
    public class ParsedOpcodeCallThis(ArgumentRegister dst, ArgumentField field, params ArgumentRegister[]? args) : ParsedOpcode(OpcodeType.CallThis, args == null ? [dst, field] : [dst, field, .. args]);
    public class ParsedOpcodeCallClosure(ArgumentRegister dst, ArgumentRegister fun, params ArgumentRegister[]? args) : ParsedOpcode(OpcodeType.CallClosure, args == null ? [dst, fun] : [dst, fun, .. args]);
    public class ParsedOpcodeStaticClosure(ArgumentRegister dst, ArgumentFunction fun) : ParsedOpcode(OpcodeType.StaticClosure, dst, fun);
    public class ParsedOpcodeInstanceClosure(ArgumentRegister dst, ArgumentFunction fun, ArgumentRegister obj) : ParsedOpcode(OpcodeType.InstanceClosure, dst, fun, obj);
    public class ParsedOpcodeVirtualClosure(ArgumentRegister dst, ArgumentRegister obj, ArgumentRegister field) : ParsedOpcode(OpcodeType.VirtualClosure, dst, obj, field);
    public class ParsedOpcodeGetGlobal(ArgumentRegister dst, ArgumentGlobal global) : ParsedOpcode(OpcodeType.GetGlobal, dst, global);
    public class ParsedOpcodeSetGlobal(ArgumentGlobal global, ArgumentRegister src) : ParsedOpcode(OpcodeType.SetGlobal, global, src);
    public class ParsedOpcodeField(ArgumentRegister dst, ArgumentRegister obj, ArgumentField field) : ParsedOpcode(OpcodeType.Field, dst, obj, field);
    public class ParsedOpcodeSetField(ArgumentRegister obj, ArgumentField field, ArgumentRegister src) : ParsedOpcode(OpcodeType.SetField, obj, field, src);
    public class ParsedOpcodeGetThis(ArgumentRegister dst, ArgumentField field) : ParsedOpcode(OpcodeType.GetThis, dst, field);
    public class ParsedOpcodeSetThis(ArgumentField field, ArgumentRegister src) : ParsedOpcode(OpcodeType.SetThis, field, src);
    public class ParsedOpcodeDynGet(ArgumentRegister dst, ArgumentRegister obj, ArgumentString field) : ParsedOpcode(OpcodeType.DynGet, dst, obj, field);
    public class ParsedOpcodeDynSet(ArgumentRegister obj, ArgumentString field, ArgumentRegister src) : ParsedOpcode(OpcodeType.DynSet, obj, field, src);
    public class ParsedOpcodeJTrue(ArgumentRegister cond, ArgumentOffset offset) : ParsedOpcode(OpcodeType.JTrue, cond, offset);
    public class ParsedOpcodeJFalse(ArgumentRegister cond, ArgumentOffset offset) : ParsedOpcode(OpcodeType.JFalse, cond, offset);
    public class ParsedOpcodeJNull(ArgumentRegister reg, ArgumentOffset offset) : ParsedOpcode(OpcodeType.JNull, reg, offset);
    public class ParsedOpcodeJNotNull(ArgumentRegister reg, ArgumentOffset offset) : ParsedOpcode(OpcodeType.JNotNull, reg, offset);
    public class ParsedOpcodeJSLt(ArgumentRegister a, ArgumentRegister b, ArgumentOffset offset) : ParsedOpcode(OpcodeType.JSLt, a, b, offset);
    public class ParsedOpcodeJSGte(ArgumentRegister a, ArgumentRegister b, ArgumentOffset offset) : ParsedOpcode(OpcodeType.JSGte, a, b, offset);
    public class ParsedOpcodeJSGt(ArgumentRegister a, ArgumentRegister b, ArgumentOffset offset) : ParsedOpcode(OpcodeType.JSGt, a, b, offset);
    public class ParsedOpcodeJSLte(ArgumentRegister a, ArgumentRegister b, ArgumentOffset offset) : ParsedOpcode(OpcodeType.JSLte, a, b, offset);
    public class ParsedOpcodeJULt(ArgumentRegister a, ArgumentRegister b, ArgumentOffset offset) : ParsedOpcode(OpcodeType.JULt, a, b, offset);
    public class ParsedOpcodeJUGte(ArgumentRegister a, ArgumentRegister b, ArgumentOffset offset) : ParsedOpcode(OpcodeType.JUGte, a, b, offset);
    public class ParsedOpcodeJNotLt(ArgumentRegister a, ArgumentRegister b, ArgumentOffset offset) : ParsedOpcode(OpcodeType.JNotLt, a, b, offset);
    public class ParsedOpcodeJNotGte(ArgumentRegister a, ArgumentRegister b, ArgumentOffset offset) : ParsedOpcode(OpcodeType.JNotGte, a, b, offset);
    public class ParsedOpcodeJEq(ArgumentRegister a, ArgumentRegister b, ArgumentOffset offset) : ParsedOpcode(OpcodeType.JEq, a, b, offset);
    public class ParsedOpcodeJNotEq(ArgumentRegister a, ArgumentRegister b, ArgumentOffset offset) : ParsedOpcode(OpcodeType.JNotEq, a, b, offset);
    public class ParsedOpcodeJAlways(ArgumentOffset offset) : ParsedOpcode(OpcodeType.JAlways, offset);
    public class ParsedOpcodeToDyn(ArgumentRegister dst, ArgumentRegister src) : ParsedOpcode(OpcodeType.ToDyn, dst, src);
    public class ParsedOpcodeToSFloat(ArgumentRegister dst, ArgumentRegister src) : ParsedOpcode(OpcodeType.ToSFloat, dst, src);
    public class ParsedOpcodeToUFloat(ArgumentRegister dst, ArgumentRegister src) : ParsedOpcode(OpcodeType.ToUFloat, dst, src);
    public class ParsedOpcodeToInt(ArgumentRegister dst, ArgumentRegister src) : ParsedOpcode(OpcodeType.ToInt, dst, src);
    public class ParsedOpcodeSafeCast(ArgumentRegister dst, ArgumentRegister src) : ParsedOpcode(OpcodeType.SafeCast, dst, src);
    public class ParsedOpcodeUnsafeCast(ArgumentRegister dst, ArgumentRegister src) : ParsedOpcode(OpcodeType.UnsafeCast, dst, src);
    public class ParsedOpcodeToVirtual(ArgumentRegister dst, ArgumentRegister src) : ParsedOpcode(OpcodeType.ToVirtual, dst, src);
    public class ParsedOpcodeLabel() : ParsedOpcode(OpcodeType.Label);
    public class ParsedOpcodeRet(ArgumentRegister ret) : ParsedOpcode(OpcodeType.Ret, ret);
    public class ParsedOpcodeThrow(ArgumentRegister exc) : ParsedOpcode(OpcodeType.Throw, exc);
    public class ParsedOpcodeRethrow(ArgumentRegister exc) : ParsedOpcode(OpcodeType.Rethrow, exc);
    public class ParsedOpcodeSwitch(ArgumentRegister reg, ArgumentOffset[] offsets, ArgumentOffset end) : ParsedOpcode(OpcodeType.Switch, [reg, .. offsets, end]);
    public class ParsedOpcodeNullCheck(ArgumentRegister reg) : ParsedOpcode(OpcodeType.NullCheck, reg);
    public class ParsedOpcodeTrap(ArgumentRegister exc, ArgumentOffset offset) : ParsedOpcode(OpcodeType.Trap, exc, offset);
    public class ParsedOpcodeEndTrap(ArgumentRegister exc) : ParsedOpcode(OpcodeType.EndTrap, exc);
    public class ParsedOpcodeGetI8(ArgumentRegister dst, ArgumentRegister bytes, ArgumentRegister index) : ParsedOpcode(OpcodeType.GetI8, dst, bytes, index);
    public class ParsedOpcodeGetI16(ArgumentRegister dst, ArgumentRegister bytes, ArgumentRegister index) : ParsedOpcode(OpcodeType.GetI16, dst, bytes, index);
    public class ParsedOpcodeGetMem(ArgumentRegister dst, ArgumentRegister bytes, ArgumentRegister index) : ParsedOpcode(OpcodeType.GetMem, dst, bytes, index);
    public class ParsedOpcodeGetArray(ArgumentRegister dst, ArgumentRegister array, ArgumentRegister index) : ParsedOpcode(OpcodeType.GetArray, dst, array, index);
    public class ParsedOpcodeSetI8(ArgumentRegister bytes, ArgumentRegister index, ArgumentRegister src) : ParsedOpcode(OpcodeType.SetI8, bytes, index, src);
    public class ParsedOpcodeSetI16(ArgumentRegister bytes, ArgumentRegister index, ArgumentRegister src) : ParsedOpcode(OpcodeType.SetI16, bytes, index, src);
    public class ParsedOpcodeSetMem(ArgumentRegister bytes, ArgumentRegister index, ArgumentRegister src) : ParsedOpcode(OpcodeType.SetMem, bytes, index, src);
    public class ParsedOpcodeSetArray(ArgumentRegister array, ArgumentRegister index, ArgumentRegister src) : ParsedOpcode(OpcodeType.SetArray, array, index, src);
    public class ParsedOpcodeNew(ArgumentRegister dst) : ParsedOpcode(OpcodeType.New, dst);
    public class ParsedOpcodeArraySize(ArgumentRegister dst, ArgumentRegister array) : ParsedOpcode(OpcodeType.ArraySize, dst, array);
    public class ParsedOpcodeType(ArgumentRegister dst, ArgumentType type) : ParsedOpcode(OpcodeType.Type, dst, type);
    public class ParsedOpcodeGetType(ArgumentRegister dst, ArgumentRegister src) : ParsedOpcode(OpcodeType.GetType, dst, src);
    public class ParsedOpcodeGetTID(ArgumentRegister dst, ArgumentRegister src) : ParsedOpcode(OpcodeType.GetTID, dst, src);
    public class ParsedOpcodeRef(ArgumentRegister dst, ArgumentRegister src) : ParsedOpcode(OpcodeType.Ref, dst, src);
    public class ParsedOpcodeUnref(ArgumentRegister dst, ArgumentRegister src) : ParsedOpcode(OpcodeType.Unref, dst, src);
    public class ParsedOpcodeSetref(ArgumentRegister dst, ArgumentRegister val) : ParsedOpcode(OpcodeType.Setref, dst, val);
    public class ParsedOpcodeMakeEnum(ArgumentRegister dst, ArgumentEnumConstruct construct, params ArgumentRegister[]? args) : ParsedOpcode(OpcodeType.MakeEnum, args == null ? [dst, construct] : [dst, construct, .. args]);
    public class ParsedOpcodeEnumAlloc(ArgumentRegister dst, ArgumentEnumConstruct construct) : ParsedOpcode(OpcodeType.EnumAlloc, dst, construct);
    public class ParsedOpcodeEnumIndex(ArgumentRegister dst, ArgumentRegister val) : ParsedOpcode(OpcodeType.EnumIndex, dst, val);
    public class ParsedOpcodeEnumField(ArgumentRegister dst, ArgumentRegister value, ArgumentEnumConstruct construct, ArgumentField field) : ParsedOpcode(OpcodeType.EnumField, dst, value, construct, field);
    public class ParsedOpcodeSetEnumField(ArgumentRegister val, ArgumentField field, ArgumentRegister src) : ParsedOpcode(OpcodeType.SetEnumField, val, field, src);
    public class ParsedOpcodeAssert() : ParsedOpcode(OpcodeType.Assert);
    public class ParsedOpcodeRefData(ArgumentRegister dst, ArgumentRegister src) : ParsedOpcode(OpcodeType.RefData, dst, src);
    public class ParsedOpcodeRefOffset(ArgumentRegister dst, ArgumentRegister reg, ArgumentRegister offset) : ParsedOpcode(OpcodeType.RefOffset, dst, reg, offset);
    public class ParsedOpcodeNop() : ParsedOpcode(OpcodeType.Nop);
    public class ParsedOpcodePrefetch(ArgumentRegister val, ArgumentField field, ArgumentInlineInt mode) : ParsedOpcode(OpcodeType.Prefetch, val, field, mode);
    public class ParsedOpcodeAsm(ArgumentInlineInt mode, ArgumentInlineInt val, ArgumentRegister reg) : ParsedOpcode(OpcodeType.Asm, mode, val, reg);
}
