namespace Haxelink
{
    public enum HashlinkTypeKind
    {
        Void,
        U8,
        U16,
        I32,
        I64,
        F32,
        F64,
        Bool,
        Bytes,
        Dyn,
        Fun,
        Obj,
        Array,
        Type,
        Ref,
        Virtual,
        DynObj,
        Abstract,
        Enum,
        Null,
        Method,
        Struct,
        Packed
    }

    public readonly struct ObjField(int name, int type)
    {
        public int Name { get; } = name;
        public int Type { get; } = type;
    }

    public readonly struct ObjProto(int name, int functionIndex, int pIndex)
    {
        public int Name { get; } = name;
        public int FunctionIndex { get; } = functionIndex;
        public int PIndex { get; } = pIndex;
    }

    public readonly struct ObjBinding(int field, int functionIndex)
    {
        public int Field { get; } = field;
        public int FunctionIndex { get; } = functionIndex;
    }

    public readonly struct EnumConstruct(int name, int[] parameters)
    {
        public int Name { get; } = name;
        public int[] Parameters { get; } = parameters;
    }

    public abstract class HashlinkType
    {
        public abstract HashlinkTypeKind Kind { get; }

        public ParsedHashlinkType? ParsedHashlinkType { get; }

        public HashlinkType() { }
        
        public HashlinkType(ParsedHashlinkType parsedHashlinkType)
        {
            ParsedHashlinkType = parsedHashlinkType;
        }
        
        public virtual void Setup(Bytecode bytecode, List<string> strings, IReadOnlyDictionary<ParsedHashlinkGlobal, int> globalsLookup) { }

        public static HashlinkType FromParsedHashlinkType(ParsedHashlinkType parsedHashlinkType)
        {
            Type type = parsedHashlinkType.GetType();

            if (type == typeof(ParsedHashlinkVoid)) return new HashlinkVoid((ParsedHashlinkVoid)parsedHashlinkType);
            else if (type == typeof(ParsedHashlinkU8)) return new HashlinkU8((ParsedHashlinkU8)parsedHashlinkType);
            else if (type == typeof(ParsedHashlinkU16)) return new HashlinkU16((ParsedHashlinkU16)parsedHashlinkType);
            else if (type == typeof(ParsedHashlinkI32)) return new HashlinkI32((ParsedHashlinkI32)parsedHashlinkType);
            else if (type == typeof(ParsedHashlinkI64)) return new HashlinkI64((ParsedHashlinkI64)parsedHashlinkType);
            else if (type == typeof(ParsedHashlinkF32)) return new HashlinkF32((ParsedHashlinkF32)parsedHashlinkType);
            else if (type == typeof(ParsedHashlinkF64)) return new HashlinkF64((ParsedHashlinkF64)parsedHashlinkType);
            else if (type == typeof(ParsedHashlinkBool)) return new HashlinkBool((ParsedHashlinkBool)parsedHashlinkType);
            else if (type == typeof(ParsedHashlinkBytes)) return new HashlinkBytes((ParsedHashlinkBytes)parsedHashlinkType);
            else if (type == typeof(ParsedHashlinkDyn)) return new HashlinkDyn((ParsedHashlinkDyn)parsedHashlinkType);
            else if (type == typeof(ParsedHashlinkFunType)) return new HashlinkFunType((ParsedHashlinkFunType)parsedHashlinkType);
            else if (type == typeof(ParsedHashlinkObj)) return new HashlinkObj((ParsedHashlinkObj)parsedHashlinkType);
            else if (type == typeof(ParsedHashlinkArray)) return new HashlinkArray((ParsedHashlinkArray)parsedHashlinkType);
            else if (type == typeof(ParsedHashlinkTypeType)) return new HashlinkTypeType((ParsedHashlinkTypeType)parsedHashlinkType);
            else if (type == typeof(ParsedHashlinkRef)) return new HashlinkRef((ParsedHashlinkRef)parsedHashlinkType);
            else if (type == typeof(ParsedHashlinkVirtual)) return new HashlinkVirtual((ParsedHashlinkVirtual)parsedHashlinkType);
            else if (type == typeof(ParsedHashlinkDynObj)) return new HashlinkDynObj((ParsedHashlinkDynObj)parsedHashlinkType);
            else if (type == typeof(ParsedHashlinkAbstract)) return new HashlinkAbstract((ParsedHashlinkAbstract)parsedHashlinkType);
            else if (type == typeof(ParsedHashlinkEnum)) return new HashlinkEnum((ParsedHashlinkEnum)parsedHashlinkType);
            else if (type == typeof(ParsedHashlinkNull)) return new HashlinkNull((ParsedHashlinkNull)parsedHashlinkType);
            else if (type == typeof(ParsedHashlinkMethodType)) return new HashlinkMethodType((ParsedHashlinkMethodType)parsedHashlinkType);
            else if (type == typeof(ParsedHashlinkStruct)) return new HashlinkStruct((ParsedHashlinkStruct)parsedHashlinkType);
            else if (type == typeof(ParsedHashlinkPacked)) return new HashlinkPacked((ParsedHashlinkPacked)parsedHashlinkType);
            else throw new Exception();
        }
    }

    public abstract class HashlinkType<T> : HashlinkType where T: ParsedHashlinkType
    {
        public new T? ParsedHashlinkType { get; }

        public HashlinkType() { }

        public HashlinkType(T parsedHashlinkType) : base(parsedHashlinkType)
        {
            ParsedHashlinkType = parsedHashlinkType;
        }
    }

    public abstract class HashlinkWrapper : HashlinkType<ParsedHashlinkWrapper>
    {
        public int Type { get; private set; }

        public HashlinkWrapper(int type)
        {
            Type = type;
        }

        public HashlinkWrapper(ParsedHashlinkWrapper parsedHashlinkType) : base(parsedHashlinkType) { }

        public override void Setup(Bytecode bytecode, List<string> strings, IReadOnlyDictionary<ParsedHashlinkGlobal, int> globalsLookup)
        {
            Type = bytecode.GetTypeIndex(ParsedHashlinkType!.WrappedType);
        }
    }

    public class HashlinkVoid : HashlinkType<ParsedHashlinkVoid>
    {
        public override HashlinkTypeKind Kind => HashlinkTypeKind.Void;
        public HashlinkVoid() { }
        public HashlinkVoid(ParsedHashlinkVoid parsedHashlinkType) : base(parsedHashlinkType) { }
    }

    public class HashlinkU8 : HashlinkType<ParsedHashlinkU8>
    {
        public override HashlinkTypeKind Kind => HashlinkTypeKind.U8;
        public HashlinkU8() { }
        public HashlinkU8(ParsedHashlinkU8 parsedHashlinkType) : base(parsedHashlinkType) { }
    }

    public class HashlinkU16 : HashlinkType<ParsedHashlinkU16>
    {
        public override HashlinkTypeKind Kind => HashlinkTypeKind.U16;
        public HashlinkU16() { }
        public HashlinkU16(ParsedHashlinkU16 parsedHashlinkType) : base(parsedHashlinkType) { }
    }

    public class HashlinkI32 : HashlinkType<ParsedHashlinkI32>
    {
        public override HashlinkTypeKind Kind => HashlinkTypeKind.I32;
        public HashlinkI32() { }
        public HashlinkI32(ParsedHashlinkI32 parsedHashlinkType) : base(parsedHashlinkType) { }
    }

    public class HashlinkI64 : HashlinkType<ParsedHashlinkI64>
    {
        public override HashlinkTypeKind Kind => HashlinkTypeKind.I64;
        public HashlinkI64() { }
        public HashlinkI64(ParsedHashlinkI64 parsedHashlinkType) : base(parsedHashlinkType) { }
    }

    public class HashlinkF32 : HashlinkType<ParsedHashlinkF32>
    {
        public override HashlinkTypeKind Kind => HashlinkTypeKind.F32;
        public HashlinkF32() { }
        public HashlinkF32(ParsedHashlinkF32 parsedHashlinkType) : base(parsedHashlinkType) { }
    }

    public class HashlinkF64 : HashlinkType<ParsedHashlinkF64>
    {
        public override HashlinkTypeKind Kind => HashlinkTypeKind.F64;
        public HashlinkF64() { }
        public HashlinkF64(ParsedHashlinkF64 parsedHashlinkType) : base(parsedHashlinkType) { }
    }

    public class HashlinkBool : HashlinkType<ParsedHashlinkBool>
    {
        public override HashlinkTypeKind Kind => HashlinkTypeKind.Bool;
        public HashlinkBool() { }
        public HashlinkBool(ParsedHashlinkBool parsedHashlinkType) : base(parsedHashlinkType) { }
    }

    public class HashlinkBytes : HashlinkType<ParsedHashlinkBytes>
    {
        public override HashlinkTypeKind Kind => HashlinkTypeKind.Bytes;
        public HashlinkBytes() { }
        public HashlinkBytes(ParsedHashlinkBytes parsedHashlinkType) : base(parsedHashlinkType) { }
    }

    public class HashlinkDyn : HashlinkType<ParsedHashlinkDyn>
    {
        public override HashlinkTypeKind Kind => HashlinkTypeKind.Dyn;
        public HashlinkDyn() { }
        public HashlinkDyn(ParsedHashlinkDyn parsedHashlinkType) : base(parsedHashlinkType) { }
    }

    public class HashlinkFunType : HashlinkType<ParsedHashlinkFunType>
    {
        public override HashlinkTypeKind Kind => HashlinkTypeKind.Fun;

        public int[] Arguments { get; private set; }
        public int Return { get; private set; }

        public HashlinkFunType(int[] arguments, int @return)
        {
            Arguments = arguments;
            Return = @return;
        }

        public HashlinkFunType(ParsedHashlinkFunType parsedHashlinkType) : base(parsedHashlinkType) { }

        public override void Setup(Bytecode bytecode, List<string> strings, IReadOnlyDictionary<ParsedHashlinkGlobal, int> globalsLookup)
        {
            Arguments = [.. ParsedHashlinkType!.Arguments.Select(bytecode.GetTypeIndex)];
            Return = bytecode.GetTypeIndex(ParsedHashlinkType.Return);
        }
    }

    public class HashlinkObj : HashlinkType<ParsedHashlinkObj>
    {
        public override HashlinkTypeKind Kind => HashlinkTypeKind.Obj;

        public int Name { get; private set; }
        public int Super { get; private set; }
        public int Global { get; private set; }
        public ObjField[] Fields { get; private set; }
        public ObjProto[] Protos { get; private set; }
        public ObjBinding[] Bindings { get; private set; }

        public HashlinkObj(int name, int super, int global, ObjField[] fields, ObjProto[] protos, ObjBinding[] bindings)
        {
            Name = name;
            Super = super;
            Global = global;
            Fields = fields;
            Protos = protos;
            Bindings = bindings;
        }

        public HashlinkObj(ParsedHashlinkObj parsedHashlinkType) : base(parsedHashlinkType) { }
        
        public override void Setup(Bytecode bytecode, List<string> strings, IReadOnlyDictionary<ParsedHashlinkGlobal, int> globalsLookup)
        {
            Name = strings.AddOrGetIndex(ParsedHashlinkType!.Name);
            Super = ParsedHashlinkType.Super == null ? -1 : bytecode.GetTypeIndex(ParsedHashlinkType.Super);
            Global = ParsedHashlinkType.Global == null ? 0 : globalsLookup[ParsedHashlinkType.Global];

            Fields = [.. ParsedHashlinkType.Fields.Select(x => new ObjField(strings.AddOrGetIndex(x.Name), bytecode.GetTypeIndex(x.Type)))];
            Protos = [.. ParsedHashlinkType.Protos.Select(x => new ObjProto(strings.AddOrGetIndex(x.Name), bytecode.GetFunctionIndex(x.Function), x.PIndex))];
            Bindings = [.. ParsedHashlinkType.Bindings.Select(x => new ObjBinding(x.Field, bytecode.GetFunctionIndex(x.Function)))];
        }
    }

    public class HashlinkArray : HashlinkType<ParsedHashlinkArray>
    {
        public override HashlinkTypeKind Kind => HashlinkTypeKind.Array;
        public HashlinkArray() { }
        public HashlinkArray(ParsedHashlinkArray parsedHashlinkType) : base(parsedHashlinkType) { }
    }

    public class HashlinkTypeType : HashlinkType<ParsedHashlinkTypeType>
    {
        public override HashlinkTypeKind Kind => HashlinkTypeKind.Type;
        public HashlinkTypeType() { }
        public HashlinkTypeType(ParsedHashlinkTypeType parsedHashlinkType) : base(parsedHashlinkType) { }
    }

    public class HashlinkRef : HashlinkWrapper
    {
        public override HashlinkTypeKind Kind => HashlinkTypeKind.Ref;

        public HashlinkRef(int type) : base(type) { }

        public HashlinkRef(ParsedHashlinkRef parsedHashlinkType) : base(parsedHashlinkType) { }
    }

    public class HashlinkVirtual : HashlinkType<ParsedHashlinkVirtual>
    {
        public override HashlinkTypeKind Kind => HashlinkTypeKind.Virtual;

        public ObjField[] Fields { get; private set; }

        public HashlinkVirtual(ObjField[] fields)
        {
            Fields = fields;
        }

        public HashlinkVirtual(ParsedHashlinkVirtual parsedHashlinkType) : base(parsedHashlinkType) { }

        public override void Setup(Bytecode bytecode, List<string> strings, IReadOnlyDictionary<ParsedHashlinkGlobal, int> globalsLookup)
        {
            Fields = [.. ParsedHashlinkType!.Fields.Select(x => new ObjField(strings.AddOrGetIndex(x.Name), bytecode.GetTypeIndex(x.Type)))];
        }
    }

    public class HashlinkDynObj : HashlinkType<ParsedHashlinkDynObj>
    {
        public override HashlinkTypeKind Kind => HashlinkTypeKind.DynObj;
        public HashlinkDynObj() { }
        public HashlinkDynObj(ParsedHashlinkDynObj parsedHashlinkType) : base(parsedHashlinkType) { }
    }

    public class HashlinkAbstract : HashlinkType<ParsedHashlinkAbstract>
    {
        public override HashlinkTypeKind Kind => HashlinkTypeKind.Abstract;

        public int Name { get; private set; }

        public HashlinkAbstract(int name)
        {
            Name = name;
        }

        public HashlinkAbstract(ParsedHashlinkAbstract parsedHashlinkType) : base(parsedHashlinkType) { }

        public override void Setup(Bytecode bytecode, List<string> strings, IReadOnlyDictionary<ParsedHashlinkGlobal, int> globalsLookup)
        {
            Name = strings.AddOrGetIndex(ParsedHashlinkType!.Name);
        }
    }

    public class HashlinkEnum : HashlinkType<ParsedHashlinkEnum>
    {
        public override HashlinkTypeKind Kind => HashlinkTypeKind.Enum;

        public int Name { get; private set; }
        public int Global { get; private set; }
        public EnumConstruct[] Constructs { get; private set; }

        public HashlinkEnum(int name, int global, EnumConstruct[] constructs)
        {
            Name = name;
            Global = global;
            Constructs = constructs;
        }

        public HashlinkEnum(ParsedHashlinkEnum parsedHashlinkType) : base(parsedHashlinkType) { }

        public override void Setup(Bytecode bytecode, List<string> strings, IReadOnlyDictionary<ParsedHashlinkGlobal, int> globalsLookup)
        {
            Name = strings.AddOrGetIndex(ParsedHashlinkType!.Name);
            Global = ParsedHashlinkType.Global == null ? 0 : globalsLookup[ParsedHashlinkType.Global];
            Constructs = [.. ParsedHashlinkType.Constructs.Select(x => new EnumConstruct(strings.AddOrGetIndex(x.Name), [.. x.Parameters.Select(y => bytecode.GetTypeIndex(y))]))];
        }
    }

    public class HashlinkNull : HashlinkWrapper
    {
        public override HashlinkTypeKind Kind => HashlinkTypeKind.Null;

        public HashlinkNull(int type) : base(type) { }

        public HashlinkNull(ParsedHashlinkNull parsedHashlinkType) : base(parsedHashlinkType) { }
    }

    public class HashlinkMethodType : HashlinkFunType
    {
        public override HashlinkTypeKind Kind => HashlinkTypeKind.Method;

        public HashlinkMethodType(int[] arguments, int @return) : base(arguments, @return) { }

        public HashlinkMethodType(ParsedHashlinkMethodType parsedHashlinkType) : base(parsedHashlinkType) { }
    }

    public class HashlinkStruct : HashlinkObj
    {
        public override HashlinkTypeKind Kind => HashlinkTypeKind.Struct;

        public HashlinkStruct(int name, int super, int global, ObjField[] fields, ObjProto[] protos, ObjBinding[] bindings) : base(name, super, global, fields, protos, bindings) { }

        public HashlinkStruct(ParsedHashlinkStruct parsedHashlinkType) : base(parsedHashlinkType) { }
    }

    public class HashlinkPacked : HashlinkWrapper
    {
        public override HashlinkTypeKind Kind => HashlinkTypeKind.Packed;

        public HashlinkPacked(int type) : base(type) { }

        public HashlinkPacked(ParsedHashlinkPacked parsedHashlinkType) : base(parsedHashlinkType) { }
    }
}
