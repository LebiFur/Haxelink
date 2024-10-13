namespace Haxelink
{
    public class ParsedObjField(string name, ParsedHashlinkType type, ParsedHashlinkType parent)
    {
        public string Name { get; } = name;
        public ParsedHashlinkType Type { get; } = type;
        public ParsedHashlinkType Parent { get; } = parent;
    }

    public readonly struct ParsedObjProto(string name, ParsedHashlinkFunction function, int pIndex)
    {
        public string Name { get; } = name;
        public ParsedHashlinkFunction Function { get; } = function;
        public int PIndex { get; } = pIndex;
    }

    public readonly struct ParsedObjBinding(int field, ParsedHashlinkFunction function)
    {
        public int Field { get; } = field;
        public ParsedHashlinkFunction Function { get; } = function;
    }

    public readonly struct ParsedEnumConstruct(string name, ParsedHashlinkType[] parameters)
    {
        public string Name { get; } = name;
        public ParsedHashlinkType[] Parameters { get; } = parameters;
    }

    public abstract class ParsedHashlinkType(HashlinkType hashlinkType)
    {
        public HashlinkType HashlinkType { get; } = hashlinkType;

        public static ParsedHashlinkType FromHashlinkType(HashlinkType hashlinkType)
        {
            return hashlinkType.Kind switch
            {
                HashlinkTypeKind.Void => new ParsedHashlinkVoid((HashlinkVoid)hashlinkType),
                HashlinkTypeKind.U8 => new ParsedHashlinkU8((HashlinkU8)hashlinkType),
                HashlinkTypeKind.U16 => new ParsedHashlinkU16((HashlinkU16)hashlinkType),
                HashlinkTypeKind.I32 => new ParsedHashlinkI32((HashlinkI32)hashlinkType),
                HashlinkTypeKind.I64 => new ParsedHashlinkI64((HashlinkI64)hashlinkType),
                HashlinkTypeKind.F32 => new ParsedHashlinkF32((HashlinkF32)hashlinkType),
                HashlinkTypeKind.F64 => new ParsedHashlinkF64((HashlinkF64)hashlinkType),
                HashlinkTypeKind.Bool => new ParsedHashlinkBool((HashlinkBool)hashlinkType),
                HashlinkTypeKind.Bytes => new ParsedHashlinkBytes((HashlinkBytes)hashlinkType),
                HashlinkTypeKind.Dyn => new ParsedHashlinkDyn((HashlinkDyn)hashlinkType),
                HashlinkTypeKind.Fun => new ParsedHashlinkFunType((HashlinkFunType)hashlinkType),
                HashlinkTypeKind.Array => new ParsedHashlinkArray((HashlinkArray)hashlinkType),
                HashlinkTypeKind.Type => new ParsedHashlinkTypeType((HashlinkTypeType)hashlinkType),
                HashlinkTypeKind.DynObj => new ParsedHashlinkDynObj((HashlinkDynObj)hashlinkType),
                HashlinkTypeKind.Method => new ParsedHashlinkMethodType((HashlinkMethodType)hashlinkType),
                HashlinkTypeKind.Ref => new ParsedHashlinkRef((HashlinkRef)hashlinkType),
                HashlinkTypeKind.Null => new ParsedHashlinkNull((HashlinkNull)hashlinkType),
                HashlinkTypeKind.Packed => new ParsedHashlinkPacked((HashlinkPacked)hashlinkType),
                HashlinkTypeKind.Obj => new ParsedHashlinkObj((HashlinkObj)hashlinkType),
                HashlinkTypeKind.Struct => new ParsedHashlinkStruct((HashlinkStruct)hashlinkType),
                HashlinkTypeKind.Virtual => new ParsedHashlinkVirtual((HashlinkVirtual)hashlinkType),
                HashlinkTypeKind.Abstract => new ParsedHashlinkAbstract((HashlinkAbstract)hashlinkType),
                HashlinkTypeKind.Enum => new ParsedHashlinkEnum((HashlinkEnum)hashlinkType),
                _ => throw new Exception(),
            };
        }

        public virtual void Setup(Bytecode bytecode, ParsedBytecode parsedBytecode) { }
    }

    public abstract class ParsedHashlinkType<T>(T hashlinkType) : ParsedHashlinkType(hashlinkType) where T : HashlinkType
    {
        public new T HashlinkType { get; } = hashlinkType;
    }

    public abstract class ParsedHashlinkWrapper(HashlinkWrapper hashlinkWrapper) : ParsedHashlinkType<HashlinkWrapper>(hashlinkWrapper)
    {
        public ParsedHashlinkType WrappedType { get; private set; }

        public override void Setup(Bytecode bytecode, ParsedBytecode parsedBytecode)
        {
            WrappedType = parsedBytecode.GetTypeReference(HashlinkType.Type);
        }
    }

    public class ParsedHashlinkVoid(HashlinkVoid hashlinkType) : ParsedHashlinkType<HashlinkVoid>(hashlinkType);
    public class ParsedHashlinkU8(HashlinkU8 hashlinkType)  : ParsedHashlinkType<HashlinkU8>(hashlinkType);
    public class ParsedHashlinkU16(HashlinkU16 hashlinkType) : ParsedHashlinkType<HashlinkU16>(hashlinkType);
    public class ParsedHashlinkI32(HashlinkI32 hashlinkType) : ParsedHashlinkType<HashlinkI32>(hashlinkType);
    public class ParsedHashlinkI64(HashlinkI64 hashlinkType) : ParsedHashlinkType<HashlinkI64>(hashlinkType);
    public class ParsedHashlinkF32(HashlinkF32 hashlinkType) : ParsedHashlinkType<HashlinkF32>(hashlinkType);
    public class ParsedHashlinkF64(HashlinkF64 hashlinkType) : ParsedHashlinkType<HashlinkF64>(hashlinkType);
    public class ParsedHashlinkBool(HashlinkBool hashlinkType) : ParsedHashlinkType<HashlinkBool>(hashlinkType);
    public class ParsedHashlinkBytes(HashlinkBytes hashlinkType) : ParsedHashlinkType<HashlinkBytes>(hashlinkType);
    public class ParsedHashlinkDyn(HashlinkDyn hashlinkType) : ParsedHashlinkType<HashlinkDyn>(hashlinkType);
    public class ParsedHashlinkArray(HashlinkArray hashlinkType) : ParsedHashlinkType<HashlinkArray>(hashlinkType);
    public class ParsedHashlinkTypeType(HashlinkTypeType hashlinkType) : ParsedHashlinkType<HashlinkTypeType>(hashlinkType);
    public class ParsedHashlinkDynObj(HashlinkDynObj hashlinkType) : ParsedHashlinkType<HashlinkDynObj>(hashlinkType);

    public class ParsedHashlinkRef(HashlinkRef hashlinkType) : ParsedHashlinkWrapper(hashlinkType);
    public class ParsedHashlinkNull(HashlinkNull hashlinkType) : ParsedHashlinkWrapper(hashlinkType);
    public class ParsedHashlinkPacked(HashlinkPacked hashlinkType) : ParsedHashlinkWrapper(hashlinkType);

    public class ParsedHashlinkMethodType(HashlinkMethodType hashlinkType) : ParsedHashlinkFunType(hashlinkType);
    public class ParsedHashlinkStruct(HashlinkStruct hashlinkType) : ParsedHashlinkObj(hashlinkType);

    public class ParsedHashlinkFunType(HashlinkFunType hashlinkType) : ParsedHashlinkType<HashlinkFunType>(hashlinkType)
    {
        public ParsedHashlinkType[] Arguments { get; private set; }
        public ParsedHashlinkType Return { get; private set; }

        public override void Setup(Bytecode bytecode, ParsedBytecode parsedBytecode)
        {
            Arguments = new ParsedHashlinkType[HashlinkType.Arguments.Length];
            for (int i = 0; i < Arguments.Length; i++)
                Arguments[i] = parsedBytecode.GetTypeReference(HashlinkType.Arguments[i]);

            Return = parsedBytecode.GetTypeReference(HashlinkType.Return);
        }
    }

    public class ParsedHashlinkObj(HashlinkObj hashlinkObj) : ParsedHashlinkType<HashlinkObj>(hashlinkObj)
    {
        public string Name { get; private set; }
        public ParsedHashlinkObj? Super { get; private set; }
        public ParsedHashlinkGlobal? Global { get; private set; }
        public ParsedObjField[] Fields { get; private set; }
        public ParsedObjField[] AllFields { get; private set; }
        public ParsedObjProto[] Protos { get; private set; }
        public ParsedObjBinding[] Bindings { get; private set; }

        public override void Setup(Bytecode bytecode, ParsedBytecode parsedBytecode)
        {
            Name = bytecode.StringsPool[HashlinkType.Name];

            Global = HashlinkType.Global == 0 ? null : parsedBytecode.GetGlobalReference(HashlinkType.Global);

            Fields = [.. HashlinkType.Fields.Select(
                x => new ParsedObjField(
                    bytecode.StringsPool[x.Name],
                    parsedBytecode.GetTypeReference(x.Type),
                    this
                )
            )];

            Protos = [.. HashlinkType.Protos.Select(x =>
            {
                ParsedHashlinkMethod method = (ParsedHashlinkMethod)parsedBytecode.GetFunctionReference(x.FunctionIndex);

                method.Name = bytecode.StringsPool[x.Name];
                method.Parent = this;

                return new ParsedObjProto(bytecode.StringsPool[x.Name], method, x.PIndex);
            })];

            if (HashlinkType.Super < 0) return;

            Super = (ParsedHashlinkObj)parsedBytecode.GetTypeReference(HashlinkType.Super);
        }

        public void SetupBindings(ParsedBytecode parsedBytecode)
        {
            AllFields = GetAllFields();

            Bindings = [.. HashlinkType.Bindings.Select(x =>
            {
                ParsedHashlinkFunction function = parsedBytecode.GetFunctionReference(x.FunctionIndex);

                if (function.GetType() == typeof(ParsedHashlinkMethod))
                {
                    if (function.Name != null) throw new Exception("Multiple bindings to the same function");
                    function.Name = AllFields[x.Field].Name;
                    ((ParsedHashlinkMethod)function).Parent = this;
                }

                return new ParsedObjBinding(x.Field, function);
            })];
        }

        private ParsedObjField[] GetAllFields()
        {
            if (Super == null) return Fields!;
            return [.. Super.GetAllFields(), .. Fields];
        }
    }

    public class ParsedHashlinkVirtual(HashlinkVirtual hashlinkVirtual) : ParsedHashlinkType<HashlinkVirtual>(hashlinkVirtual)
    {
        public ParsedObjField[] Fields { get; private set; }

        public override void Setup(Bytecode bytecode, ParsedBytecode parsedBytecode)
        {
            Fields = [.. HashlinkType.Fields.Select(
                x => new ParsedObjField(
                    bytecode.StringsPool[x.Name],
                    parsedBytecode.GetTypeReference(x.Type),
                    this
                )
            )];
        }
    }

    public class ParsedHashlinkAbstract(HashlinkAbstract hashlinkAbstract) : ParsedHashlinkType<HashlinkAbstract>(hashlinkAbstract)
    {
        public string Name { get; private set; }

        public override void Setup(Bytecode bytecode, ParsedBytecode parsedBytecode)
        {
            Name = bytecode.StringsPool[HashlinkType.Name];
        }
    }

    public class ParsedHashlinkEnum(HashlinkEnum hashlinkEnum) : ParsedHashlinkType<HashlinkEnum>(hashlinkEnum)
    {
        public string Name { get; private set; }
        public ParsedHashlinkGlobal? Global { get; private set; }
        public ParsedEnumConstruct[] Constructs { get; private set; }

        public override void Setup(Bytecode bytecode, ParsedBytecode parsedBytecode)
        {
            Name = bytecode.StringsPool[HashlinkType.Name];

            Global = HashlinkType.Global == 0 ? null : parsedBytecode.GetGlobalReference(HashlinkType.Global);

            Constructs = [.. HashlinkType.Constructs.Select(
                x => new ParsedEnumConstruct(
                    bytecode.StringsPool[x.Name],
                    [.. x.Parameters.Select(
                        y => parsedBytecode.GetTypeReference(y)
                    )]
                )
            )];
        }
    }
}
