using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;

namespace Haxelink
{
    public class Bytecode
    {
        public byte Version { get; }
        public bool HasDebugInfo { get; }
        public int EntryPointIndex { get; }
        public int[] IntsPool { get; }
        public double[] FloatsPool { get; }
        public string[] StringsPool { get; }
        public byte[][]? BytesPool { get; }
        public string[]? DebugFiles { get; }
        public HashlinkType[] TypesPool { get; }
        public int[] GlobalsPool { get; }
        public HashlinkNative[] NativesPool { get; }
        public HashlinkMethod[] FunctionsPool { get; }
        public HashlinkConstant[] ConstantsPool { get; }

        private readonly List<byte> data = [];

        private int dataOffset = 0;

        public Bytecode(ParsedBytecode parsedBytecode, Action<string>? log = null)
        {
            Stopwatch sw = Stopwatch.StartNew();

            Version = parsedBytecode.Version;

            HasDebugInfo = true;
            DebugFiles = ["nul"];

            List<int> ints = [];
            List<double> floats = [];
            List<string> strings = [];
            List<HashlinkFunction> functions = [];
            List<int> globals = [];
            List<byte[]>? bytes = Version >= 5 ? [] : null;

            int functionIndex = 0;

            TypesPool = [
                .. parsedBytecode.Types/*.AsParallel()*/.Select(x =>
                    {
                        log?.Invoke($"Constructing type {x}");
                        return HashlinkType.FromParsedHashlinkType(x);
                    })
                ];

            Dictionary<ParsedHashlinkGlobal, int> globalsLookup = [];

            for (int i = 0; i < parsedBytecode.Globals.Length; i++)
            {
                ParsedHashlinkGlobal parsedHashlinkGlobal = parsedBytecode.Globals[i];

                globalsLookup.Add(parsedHashlinkGlobal, i + 1);
                globals.Add(GetTypeIndex(parsedHashlinkGlobal.Type));
            }

            GlobalsPool = [.. globals];

            ConstantsPool = [
                .. parsedBytecode.Constants.Select(x => new HashlinkConstant(globalsLookup[x.Global], [strings.AddOrGetIndex(x.Bytes), ints.AddOrGetIndex(x.Bytes.Length)]))
                ];

            foreach (ParsedHashlinkFunction parsedHashlinkFunction in parsedBytecode.Functions)
            {
                int funType = GetTypeIndex(parsedHashlinkFunction.Type);

                if (parsedHashlinkFunction.GetType() == typeof(ParsedHashlinkMethod))
                {
                    ParsedHashlinkMethod parsedHashlinkMethod = (ParsedHashlinkMethod)parsedHashlinkFunction;

                    log?.Invoke($"Constructing method {parsedHashlinkMethod.FullName}");

                    functions.Add(new HashlinkMethod(
                        parsedHashlinkMethod,
                        [.. parsedHashlinkMethod.Registers.Select(x => TypesPool.First(y => y.ParsedHashlinkType == x)).Select(x => Array.IndexOf(TypesPool, x))],
                        [.. parsedHashlinkMethod.Opcodes.Select(x => (0, 0))],
                        [],
                        funType,
                        functionIndex
                        ));
                }
                else
                {
                    ParsedHashlinkNative parsedHashlinkNative = (ParsedHashlinkNative)parsedHashlinkFunction;

                    log?.Invoke($"Constructing native {parsedHashlinkNative.Name} from {parsedHashlinkNative.LibraryName}");

                    functions.Add(new HashlinkNative(
                        parsedHashlinkNative,
                        strings.AddOrGetIndex(parsedHashlinkNative.LibraryName),
                        strings.AddOrGetIndex(parsedHashlinkNative.Name!),
                        funType,
                        functionIndex
                        ));
                }

                functionIndex++;
            }

            FunctionsPool = [.. functions.Where(x => x.GetType() == typeof(HashlinkMethod)).Cast<HashlinkMethod>()];
            NativesPool = [.. functions.Where(x => x.GetType() == typeof(HashlinkNative)).Cast<HashlinkNative>()];

            ConcurrentBag<int> tempInts = [];
            ConcurrentBag<double> tempFloats = [];
            ConcurrentBag<string> tempStrings = [];
            ConcurrentBag<byte[]>? tempBytes = Version >= 5 ? [] : null;

            Perpendicular.ForEach(FunctionsPool, hashlinkMethod =>
            {
                foreach(ParsedOpcode parsedOpcode in ((ParsedHashlinkMethod)hashlinkMethod.ParsedHashlinkFunction!).Opcodes)
                {
                    foreach (OpcodeArgument argument in parsedOpcode.Arguments)
                    {
                        if (argument.GetType() == typeof(ArgumentInt)) tempInts.Add(((ArgumentInt)argument).Value);
                        else if (argument.GetType() == typeof(ArgumentFloat)) tempFloats.Add(((ArgumentFloat)argument).Value);
                        else if (argument.GetType() == typeof(ArgumentString)) tempStrings.Add(((ArgumentString)argument).Value);
                        else if (argument.GetType() == typeof(ArgumentBytes)) tempBytes!.Add(((ArgumentBytes)argument).Value);
                    }
                }
            });

            foreach (int item in tempInts) if (!ints.Contains(item)) ints.Add(item);
            foreach (double item in tempFloats) if (!floats.Contains(item)) floats.Add(item);
            foreach (string item in tempStrings) if (!strings.Contains(item)) strings.Add(item);
            if (Version >= 5) foreach (byte[] item in tempBytes!) if (!bytes!.Any(x => x.SequenceEqual(item))) bytes!.Add(item);

            Perpendicular.ForEach(FunctionsPool, hashlinkMethod =>
            {
                log?.Invoke($"Constructing opcodes for {((ParsedHashlinkMethod)hashlinkMethod.ParsedHashlinkFunction!).FullName}");

                hashlinkMethod.SetupOpcodes(x =>
                {
                    if (x.GetType() == typeof(ArgumentRegister)) return ((ArgumentRegister)x).Value;
                    else if (x.GetType() == typeof(ArgumentInt)) return ints.IndexOf(((ArgumentInt)x).Value);
                    else if (x.GetType() == typeof(ArgumentFloat)) return floats.IndexOf(((ArgumentFloat)x).Value);
                    else if (x.GetType() == typeof(ArgumentBool)) return ((ArgumentBool)x).Value ? 1 : 0;
                    else if (x.GetType() == typeof(ArgumentString)) return strings.IndexOf(((ArgumentString)x).Value);
                    else if (x.GetType() == typeof(ArgumentFunction)) return GetFunctionIndex(((ArgumentFunction)x).Value);
                    else if (x.GetType() == typeof(ArgumentField)) return ((ArgumentField)x).Value;
                    else if (x.GetType() == typeof(ArgumentOffset)) return ((ArgumentOffset)x).Value;
                    else if (x.GetType() == typeof(ArgumentType)) return GetTypeIndex(((ArgumentType)x).Value);
                    else if (x.GetType() == typeof(ArgumentInlineInt)) return ((ArgumentInlineInt)x).Value;
                    else if (x.GetType() == typeof(ArgumentGlobal)) return globalsLookup[((ArgumentGlobal)x).Value];
                    else if (x.GetType() == typeof(ArgumentEnumConstruct)) return ((ArgumentEnumConstruct)x).Value;
                    else if (x.GetType() == typeof(ArgumentBytes)) return bytes!.TakeWhile(y => !y.SequenceEqual(((ArgumentBytes)x).Value)).Count();
                    else throw new Exception($"Cannot construct opcode argument {x}");
                });
            });

            IntsPool = [.. ints];
            FloatsPool = [.. floats];
            if (Version >= 5) BytesPool = [.. bytes];

            EntryPointIndex = GetFunctionIndex(parsedBytecode.EntryPoint);

            foreach (HashlinkType type in TypesPool)
            {
                type.Setup(this, strings, globalsLookup);

                if (type.GetType().IsAssignableTo(typeof(HashlinkObj)))
                    log?.Invoke($"Setting up obj {strings[((HashlinkObj)type).Name]}");
                else log?.Invoke($"Setting up type {type}");
            }

            StringsPool = [.. strings];

            sw.Stop();

            log?.Invoke($"Finished in {sw.Elapsed.TotalSeconds:.00}s");
        }

        public int GetTypeIndex(ParsedHashlinkType parsedHashlinkType)
        {
            int index = Array.FindIndex(TypesPool, x => x.ParsedHashlinkType == parsedHashlinkType);

            if (index == -1)
                throw new Exception($"Couldn't find index for type {parsedHashlinkType}");

            return index;
        }

        public int GetFunctionIndex(ParsedHashlinkFunction parsedHashlinkFunction)
        {
            HashlinkMethod? method = FunctionsPool.FirstOrDefault(x => x.ParsedHashlinkFunction == parsedHashlinkFunction);
            if (method != null) return method.Index;

            HashlinkNative? native = NativesPool.FirstOrDefault(x => x.ParsedHashlinkFunction == parsedHashlinkFunction);
            if (native != null) return native.Index;

            throw new Exception($"Couldn't find index for function {parsedHashlinkFunction.FullName}");
        }

        public void WriteToFile(string path)
        {
            data.Clear();

            data.AddRange(Encoding.ASCII.GetBytes("HLB"));
            data.Add(Version);

            data.Add((byte)(HasDebugInfo ? 1 : 0));

            WriteVarInt(IntsPool.Length);
            WriteVarInt(FloatsPool.Length);
            WriteVarInt(StringsPool.Length);
            if (Version >= 5) WriteVarInt(BytesPool!.Length);
            WriteVarInt(TypesPool.Length);
            WriteVarInt(GlobalsPool.Length);
            WriteVarInt(NativesPool.Length);
            WriteVarInt(FunctionsPool.Length);
            WriteVarInt(ConstantsPool.Length);
            WriteVarInt(EntryPointIndex);

            foreach (int value in IntsPool) data.AddRange(BitConverter.GetBytes(value));
            foreach (double value in FloatsPool) data.AddRange(BitConverter.GetBytes(value));

            WriteStrings(StringsPool);
            if (Version >= 5) WriteBytes(BytesPool!);

            if (HasDebugInfo)
            {
                WriteVarInt(DebugFiles!.Length);
                WriteStrings(DebugFiles);
            }

            foreach (HashlinkType type in TypesPool) WriteType(type);
            foreach (int global in GlobalsPool) WriteVarInt(global);
            foreach (HashlinkNative native in NativesPool)
            {
                WriteVarInt(native.LibraryName);
                WriteVarInt(native.FunctionName);
                WriteVarInt(native.Type);
                WriteVarInt(native.Index);
            }
            foreach (HashlinkMethod function in FunctionsPool) WriteFunction(function);
            foreach (HashlinkConstant constant in ConstantsPool) WriteConstant(constant);

            File.WriteAllBytes(path, [.. data]);
        }

        private void WriteBytes(byte[][] pool)
        {
            data.AddRange(BitConverter.GetBytes(pool.Sum(x => x.Length)));

            foreach (byte[] bytes in pool) data.AddRange(bytes);

            int pos = 0;

            foreach (byte[] bytes in pool)
            {
                WriteVarInt(pos);
                pos += bytes.Length;
            }
        }

        private void WriteVarInt(int value)
        {
            if (value < 0)
            {
                value = -value;

                if (value < 0x2000)
                {
                    data.Add((byte)((value >> 8) | 0xA0));
                    data.Add((byte)(value & 0xFF));
                }
                else if (value >= 0x20000000) throw new Exception("Value can't exceed 0x20000000");
                else
                {
                    data.Add((byte)((value >> 24) | 0xE0));
                    data.Add((byte)((value >> 16) & 0xFF));
                    data.Add((byte)((value >> 8) & 0xFF));
                    data.Add((byte)(value & 0xFF));
                }
            }
            else if (value < 0x80) data.Add((byte)value);
            else if (value < 0x2000)
            {
                data.Add((byte)((value >> 8) | 0x80));
                data.Add((byte)(value & 0xFF));
            }
            else if (value >= 0x20000000) throw new Exception("Value can't exceed 0x20000000");
            else
            {
                data.Add((byte)((value >> 24) | 0xC0));
                data.Add((byte)((value >> 16) & 0xFF));
                data.Add((byte)((value >> 8) & 0xFF));
                data.Add((byte)(value & 0xFF));
            }
        }

        private void WriteStrings(string[] pool)
        {
            data.AddRange(BitConverter.GetBytes(pool.Sum(x => Encoding.UTF8.GetByteCount(x) + 1)));

            foreach (string s in pool)
            {
                data.AddRange(Encoding.UTF8.GetBytes(s));
                data.Add(0);
            }

            foreach (string s in pool) WriteVarInt(Encoding.UTF8.GetByteCount(s));
        }

        private void WriteType(HashlinkType type)
        {
            data.Add((byte)type.Kind);

            switch (type.Kind)
            {
                case HashlinkTypeKind.Method:
                case HashlinkTypeKind.Fun:
                    {
                        HashlinkFunType fun = (HashlinkFunType)type;

                        WriteVarInt(fun.Arguments.Length);

                        foreach (int arg in fun.Arguments) WriteVarInt(arg);

                        WriteVarInt(fun.Return);

                        return;
                    }
                case HashlinkTypeKind.Struct:
                case HashlinkTypeKind.Obj:
                    {
                        HashlinkObj obj = (HashlinkObj)type;

                        WriteVarInt(obj.Name);
                        WriteVarInt(obj.Super);
                        WriteVarInt(obj.Global);
                        WriteVarInt(obj.Fields.Length);
                        WriteVarInt(obj.Protos.Length);
                        WriteVarInt(obj.Bindings.Length);

                        foreach (ObjField field in obj.Fields)
                        {
                            WriteVarInt(field.Name);
                            WriteVarInt(field.Type);
                        }

                        foreach (ObjProto proto in obj.Protos)
                        {
                            WriteVarInt(proto.Name);
                            WriteVarInt(proto.FunctionIndex);
                            WriteVarInt(proto.PIndex);
                        }

                        foreach (ObjBinding binding in obj.Bindings)
                        {
                            WriteVarInt(binding.Field);
                            WriteVarInt(binding.FunctionIndex);
                        }

                        return;
                    }
                case HashlinkTypeKind.Ref:
                case HashlinkTypeKind.Null:
                case HashlinkTypeKind.Packed:
                    {
                        WriteVarInt(((HashlinkWrapper)type).Type);

                        return;
                    }
                case HashlinkTypeKind.Virtual:
                    {
                        HashlinkVirtual hashlinkVirtual = (HashlinkVirtual)type;

                        WriteVarInt(hashlinkVirtual.Fields!.Length);

                        foreach (ObjField field in hashlinkVirtual.Fields)
                        {
                            WriteVarInt(field.Name);
                            WriteVarInt(field.Type);
                        }

                        return;
                    }
                case HashlinkTypeKind.Abstract:
                    {
                        WriteVarInt(((HashlinkAbstract)type).Name);

                        return;
                    }
                case HashlinkTypeKind.Enum:
                    {
                        HashlinkEnum hashlinkEnum = (HashlinkEnum)type;

                        WriteVarInt(hashlinkEnum.Name);
                        WriteVarInt(hashlinkEnum.Global);
                        WriteVarInt(hashlinkEnum.Constructs!.Length);

                        foreach (EnumConstruct construct in hashlinkEnum.Constructs)
                        {
                            WriteVarInt(construct.Name);
                            WriteVarInt(construct.Parameters.Length);

                            foreach (int parameter in construct.Parameters) WriteVarInt(parameter);
                        }

                        return;
                    }
            }
        }

        private void WriteFunction(HashlinkMethod function)
        {
            WriteVarInt(function.Type);
            WriteVarInt(function.Index);
            WriteVarInt(function.Registers.Length);
            WriteVarInt(function.Opcodes.Length);

            foreach (int register in function.Registers) WriteVarInt(register);
            foreach (Opcode opcode in function.Opcodes) WriteOpcode(opcode);

            if (function.DebugInfo != null)
            {
                int currentFile = -1;
                int currentLine = 0;
                int i = 0;

                foreach ((int, int) info in function.DebugInfo)
                {
                    if (info.Item1 != currentFile)
                    {
                        FlushRepeat(ref currentLine, ref i, info.Item2);
                        currentFile = info.Item1;
                        data.Add((byte)((info.Item1 >> 7) | 1));
                        data.Add((byte)(info.Item1 & 0xFF));
                    }

                    if (info.Item2 != currentLine) FlushRepeat(ref currentLine, ref i, info.Item2);
                
                    if (info.Item2 == currentLine) i++;
                    else
                    {
                        int delta = info.Item2 - currentLine;

                        if (delta > 0 && delta < 32) data.Add((byte)((delta << 3) | 4));
                        else
                        {
                            data.Add((byte)(info.Item2 << 3));
                            data.Add((byte)(info.Item2 << 5));
                            data.Add((byte)(info.Item2 << 13));
                        }

                        currentLine = info.Item2;
                    }
                }

                int oldLine = currentLine;
                FlushRepeat(ref currentLine, ref i, oldLine);
            }

            if (function.Assigns != null)
            {
                WriteVarInt(function.Assigns.Length);

                foreach ((int, int) assign in function.Assigns)
                {
                    WriteVarInt(assign.Item1);
                    WriteVarInt(assign.Item2);
                }
            }
        }

        private void FlushRepeat(ref int currentLine, ref int i, int pos)
        {
            if (i > 0)
            {
                if (i > 15)
                {
                    data.Add((15 << 2) | 2);
                    i -= 15;
                    FlushRepeat(ref currentLine, ref i, pos);
                }
                else
                {
                    int delta = pos - currentLine;
                    delta = (delta > 0 && delta < 4) ? delta : 0;
                    data.Add((byte)((delta << 6) | (i << 2) | 2));
                    i = 0;
                    currentLine += delta;
                }
            }
        }

        private void WriteOpcode(Opcode opcode)
        {
            WriteVarInt((int)opcode.Type);

            if (opcode.Type == OpcodeType.Switch)
            {
                int jumpsCount = opcode.Args.Length - 2;

                WriteVarInt(opcode.Args[0]);
                WriteVarInt(jumpsCount);

                for (int i = 0; i < jumpsCount; i++) WriteVarInt(opcode.Args[i + 1]);

                WriteVarInt(opcode.Args[^1]);
            }
            else
            {
                int argumentsCount = Opcode.opcodeArgumentsCount[(int)opcode.Type];
                int absArgumentsCount = Math.Abs(argumentsCount);

                for (int i = 0; i < absArgumentsCount; i++) WriteVarInt(opcode.Args[i]);

                if (argumentsCount < 0)
                {
                    int variableArgumentsCount = opcode.Args.Length - absArgumentsCount;

                    WriteVarInt(variableArgumentsCount);

                    for (int i = 0; i < variableArgumentsCount; i++) WriteVarInt(opcode.Args[i + absArgumentsCount]);
                }
            }
        }

        private void WriteConstant(HashlinkConstant constant)
        {
            WriteVarInt(constant.Global);
            WriteVarInt(constant.Fields.Length);

            foreach (int field in constant.Fields) WriteVarInt(field);
        }

        public Bytecode(string path)
        {
            data = [.. File.ReadAllBytes(path)];

            if (Encoding.ASCII.GetString(ReadByteArray(3)) != "HLB") throw new Exception("Incorrect hashlink bytecode file");

            Version = ReadByte();
            if (Version < 4 && Version > 5) throw new Exception("Unsupported hashlink bytecode version");

            HasDebugInfo = ReadByte() > 0;

            IntsPool = new int[ReadVarInt()];
            FloatsPool = new double[ReadVarInt()];
            StringsPool = new string[ReadVarInt()];
            if (Version >= 5) BytesPool = new byte[ReadVarInt()][];
            TypesPool = new HashlinkType[ReadVarInt()];
            GlobalsPool = new int[ReadVarInt()];
            NativesPool = new HashlinkNative[ReadVarInt()];
            FunctionsPool = new HashlinkMethod[ReadVarInt()];
            ConstantsPool = new HashlinkConstant[ReadVarInt()];
            EntryPointIndex = ReadVarInt();

            for (int i = 0; i < IntsPool.Length; i++) IntsPool[i] = BitConverter.ToInt32(ReadByteArray(4));
            for (int i = 0; i < FloatsPool.Length; i++) FloatsPool[i] = BitConverter.ToDouble(ReadByteArray(8));

            ReadStrings(StringsPool);
            if (Version >= 5) ReadBytes(BytesPool!);

            if (HasDebugInfo)
            {
                DebugFiles = new string[ReadVarInt()];
                ReadStrings(DebugFiles);
            }

            for (int i = 0; i < TypesPool.Length; i++) TypesPool[i] = ReadType();
            for (int i = 0; i < GlobalsPool.Length; i++) GlobalsPool[i] = ReadVarInt();
            for (int i = 0; i < NativesPool.Length; i++) NativesPool[i] = new(ReadVarInt(), ReadVarInt(), ReadVarInt(), ReadVarInt());
            for (int i = 0; i < FunctionsPool.Length; i++) FunctionsPool[i] = ReadFunction();
            for (int i = 0; i < ConstantsPool.Length; i++) ConstantsPool[i] = ReadConstant();
        }
        
        private void ReadBytes(byte[][] pool)
        {
            byte[] bytesData = ReadByteArray(BitConverter.ToInt32(ReadByteArray(4)));

            int pos = ReadVarInt();

            for (int i = 0; i < pool.Length; i++)
            {
                int end = i == pool.Length - 1 ? bytesData.Length : ReadVarInt();

                pool[i] = bytesData[pos..end];

                pos = end;
            }
        }
        
        private HashlinkConstant ReadConstant()
        {
            int global = ReadVarInt();
            int[] fields = new int[ReadVarInt()];
            for (int i = 0; i < fields.Length; i++) fields[i] = ReadVarInt();
            return new(global, fields);
        }

        private HashlinkType ReadType()
        {
            HashlinkTypeKind kind = (HashlinkTypeKind)ReadByte();

            switch (kind)
            {
                case HashlinkTypeKind.Void: return new HashlinkVoid();
                case HashlinkTypeKind.U8: return new HashlinkU8();
                case HashlinkTypeKind.U16: return new HashlinkU16();
                case HashlinkTypeKind.I32: return new HashlinkI32();
                case HashlinkTypeKind.I64: return new HashlinkI64();
                case HashlinkTypeKind.F32: return new HashlinkF32();
                case HashlinkTypeKind.F64: return new HashlinkF64();
                case HashlinkTypeKind.Bool: return new HashlinkBool();
                case HashlinkTypeKind.Bytes: return new HashlinkBytes();
                case HashlinkTypeKind.Dyn: return new HashlinkDyn();
                case HashlinkTypeKind.Method:
                case HashlinkTypeKind.Fun:
                    {
                        int argumentsCount = ReadVarInt();

                        int[] arguments = new int[argumentsCount];
                        for (int j = 0; j < argumentsCount; j++) arguments[j] = ReadVarInt();

                        int returnType = ReadVarInt();

                        if (kind == HashlinkTypeKind.Method) return new HashlinkMethodType(arguments, returnType);
                        else return new HashlinkFunType(arguments, returnType);
                    }
                case HashlinkTypeKind.Struct:
                case HashlinkTypeKind.Obj:
                    {
                        int name = ReadVarInt();
                        int super = ReadVarInt();
                        int global = ReadVarInt();
                        int fieldsCount = ReadVarInt();
                        int protosCount = ReadVarInt();
                        int bindingsCount = ReadVarInt();

                        ObjField[] fields = new ObjField[fieldsCount];
                        for (int j = 0; j < fieldsCount; j++)
                        {
                            int fieldName = ReadVarInt();
                            int fieldType = ReadVarInt();

                            fields[j] = new(fieldName, fieldType);
                        }

                        ObjProto[] protos = new ObjProto[protosCount];
                        for (int j = 0; j < protosCount; j++)
                        {
                            int protoName = ReadVarInt();
                            int protoFunctionIndex = ReadVarInt();
                            int protoPIndex = ReadVarInt();

                            protos[j] = new(protoName, protoFunctionIndex, protoPIndex);
                        }

                        ObjBinding[] bindings = new ObjBinding[bindingsCount];
                        for (int j = 0; j < bindingsCount; j++)
                        {
                            int bindingField = ReadVarInt();
                            int bindingFunctionIndex = ReadVarInt();

                            bindings[j] = new(bindingField, bindingFunctionIndex);
                        }

                        if (kind == HashlinkTypeKind.Struct) return new HashlinkStruct(name, super, global, fields, protos, bindings);
                        else return new HashlinkObj(name, super, global, fields, protos, bindings);
                    }
                case HashlinkTypeKind.Array: return new HashlinkArray();
                case HashlinkTypeKind.Type: return new HashlinkTypeType();
                case HashlinkTypeKind.Ref: return new HashlinkRef(ReadVarInt());
                case HashlinkTypeKind.Virtual:
                    {
                        int fieldsCount = ReadVarInt();

                        ObjField[] fields = new ObjField[fieldsCount];
                        for (int j = 0; j < fieldsCount; j++)
                        {
                            int fieldName = ReadVarInt();
                            int fieldType = ReadVarInt();

                            fields[j] = new(fieldName, fieldType);
                        }

                        return new HashlinkVirtual(fields);
                    }
                case HashlinkTypeKind.DynObj: return new HashlinkDynObj();
                case HashlinkTypeKind.Abstract: return new HashlinkAbstract(ReadVarInt());
                case HashlinkTypeKind.Enum:
                    {
                        int name = ReadVarInt();
                        int global = ReadVarInt();
                        int constructsCount = ReadVarInt();

                        EnumConstruct[] constructs = new EnumConstruct[constructsCount];
                        for (int j = 0; j < constructsCount; j++)
                        {
                            int constructName = ReadVarInt();
                            int constructParametersCount = ReadVarInt();

                            int[] constructParameteres = new int[constructParametersCount];
                            for (int k = 0; k < constructParametersCount; k++) constructParameteres[k] = ReadVarInt();

                            constructs[j] = new(constructName, constructParameteres);
                        }

                        return new HashlinkEnum(name, global, constructs);
                    }
                case HashlinkTypeKind.Null: return new HashlinkNull(ReadVarInt());
                case HashlinkTypeKind.Packed: return new HashlinkPacked(ReadVarInt());
            }

            throw new Exception("Unknown type kind");
        }

        private HashlinkMethod ReadFunction()
        {
            int type = ReadVarInt();
            int index = ReadVarInt();
            int[] registers = new int[ReadVarInt()];
            Opcode[] opcodes = new Opcode[ReadVarInt()];
            (int, int)[]? debugInfo = null;
            (int, int)[]? assigns = null;

            for (int i = 0; i < registers.Length; i++) registers[i] = ReadVarInt();
            for (int i = 0; i < opcodes.Length; i++) opcodes[i] = ReadOpcode();

            if (HasDebugInfo)
            {
                List<(int, int)> temp = new(opcodes.Length);
                int currentFile = -1;
                int currentLine = 0;
                int i = 0;

                while (i < opcodes.Length)
                {
                    int a = ReadByte();

                    if ((a & 1) != 0)
                    {
                        a >>= 1;
                        currentFile = (a << 8) | ReadByte();
                    }
                    else if ((a & 2) != 0)
                    {
                        int delta = a >> 6;
                        int count = (a >> 2) & 15;

                        while (count > 0)
                        {
                            count--;
                            temp.Add((currentFile, currentLine));
                            i++;
                        }

                        currentLine += delta;
                    }
                    else if ((a & 4) != 0)
                    {
                        currentLine += a >> 3;
                        temp.Add((currentFile, currentLine));
                        i++;
                    }
                    else
                    {
                        int b = ReadByte();
                        int c = ReadByte();
                        currentLine = (a >> 3) | (b << 5) | (c << 13);
                        temp.Add((currentFile, currentLine));
                        i++;
                    }
                }

                debugInfo = [.. temp];

                assigns = new (int, int)[ReadVarInt()];
                for (int j = 0; j < assigns.Length; j++) assigns[j] = (ReadVarInt(), ReadVarInt());
            }

            return new(registers, opcodes, debugInfo, assigns, type, index);
        }

        private Opcode ReadOpcode()
        {
            int type = ReadVarInt();
            int argumentsCount = Opcode.opcodeArgumentsCount[type];
            int[] arguments;

            if ((OpcodeType)type == OpcodeType.Switch)
            {
                int a = ReadVarInt();
                int b = ReadVarInt();

                arguments = new int[2 + b];
                for (int i = 0; i < b; i++) arguments[i + 1] = ReadVarInt();

                arguments[0] = a;
                arguments[^1] = ReadVarInt();
            }
            else
            {
                if (argumentsCount == int.MaxValue) throw new Exception("Cosmic ray bit flip");

                int[] firstArguments = new int[Math.Abs(argumentsCount)];

                for (int i = 0; i < firstArguments.Length; i++) firstArguments[i] = ReadVarInt();

                if (argumentsCount >= 0) arguments = firstArguments;
                else
                {
                    int variableArgumentsCount = ReadVarInt();
                    arguments = new int[firstArguments.Length + variableArgumentsCount];

                    for (int i = 0; i < variableArgumentsCount; i++) arguments[i + firstArguments.Length] = ReadVarInt();
                    for (int i = 0; i < firstArguments.Length; i++) arguments[i] = firstArguments[i];
                }
            }

            return new((OpcodeType)type, arguments);
        }

        private void ReadStrings(string[] pool)
        {
            byte[] stringData = ReadByteArray(BitConverter.ToInt32(ReadByteArray(4)));

            int offset = 0;

            for (int i = 0; i < pool.Length; i++)
            {
                int length = ReadVarInt();

                pool[i] = Encoding.UTF8.GetString(stringData[offset..(offset + length)]);

                offset += length + 1;
            }
        }

        private byte ReadByte()
        {
            byte value = data[dataOffset];

            dataOffset++;

            return value;
        }

        private byte[] ReadByteArray(int length)
        {
            byte[] bytes = [.. data[dataOffset..(dataOffset + length)]];

            dataOffset += length;

            return bytes;
        }

        private int ReadVarInt()
        {
            int first = ReadByte();

            if ((first & 0x80) == 0) return first & 0x7F;
            else if ((first & 0x40) == 0)
            {
                int value = ReadByte() | (first & 31) << 8;

                if ((first & 0x20) == 0) return value;
                else return -value;
            }
            else
            {
                int value = ((first & 31) << 24) | (ReadByte() << 16) | (ReadByte() << 8) | ReadByte();

                if ((first & 0x20) == 0) return value;
                else return -value;
            }
        }
    }
}