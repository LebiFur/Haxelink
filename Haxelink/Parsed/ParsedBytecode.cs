using System.Diagnostics;

namespace Haxelink
{
    public class ParsedBytecode
    {
        public byte Version { get; }
        public ParsedHashlinkFunction EntryPoint { get; }
        public ParsedHashlinkType[] Types { get; }
        public ParsedHashlinkFunction[] Functions { get; }
        public ParsedHashlinkGlobal[] Globals { get; }
        public ParsedHashlinkConstant[] Constants { get; }

        private readonly Bytecode bytecode;

        public ParsedBytecode(Bytecode bytecode, Action<string>? log = null)
        {
            //uncommenting all these ".AsParallel()" will result in segmentation fault

            Stopwatch sw = Stopwatch.StartNew();

            this.bytecode = bytecode;

            Version = bytecode.Version;

            Types = [
                .. bytecode.TypesPool/*.AsParallel()*/.Select(x =>
                    {
                        log?.Invoke($"Parsing type {x}");
                        return ParsedHashlinkType.FromHashlinkType(x);
                    })
                ];

            Functions = [
                .. bytecode.FunctionsPool/*.AsParallel()*/.Select(x =>
                    {
                        log?.Invoke($"Parsing function@{x.Index}");
                        return new ParsedHashlinkMethod(x, bytecode, this);
                    }),
                .. bytecode.NativesPool/*.AsParallel()*/.Select(x =>
                    {
                        log?.Invoke($"Parsing native {bytecode.StringsPool[x.FunctionName]} from {bytecode.StringsPool[x.LibraryName]}");
                        return new ParsedHashlinkNative(x, bytecode, this);
                    })
                ];

            Globals = [
                .. bytecode.GlobalsPool/*.AsParallel()*/.Select<int, ParsedHashlinkGlobal>((x, i) =>
                    {
                        log?.Invoke($"Parsing global@{i}");
                        return new(GetTypeReference(x));
                    })
                ];

            Constants = [
                .. bytecode.ConstantsPool/*.AsParallel()*/.Select((x, i) =>
                    {
                        log?.Invoke($"Parsing constant@{i}");
                        return ParsedHashlinkConstant.FromHashlinkConstant(bytecode, this, ref x);
                    })
                ];

            Perpendicular.ForEach(Types, parsedHashlinkType =>
            {
                parsedHashlinkType.Setup(bytecode, this);

                if (parsedHashlinkType.GetType().IsAssignableTo(typeof(ParsedHashlinkObj)))
                    log?.Invoke($"Setting up obj {((ParsedHashlinkObj)parsedHashlinkType).Name}");
                else log?.Invoke($"Setting up type {parsedHashlinkType}");
            });

            Perpendicular.ForEach(Types.Where(x => x.GetType().IsAssignableTo(typeof(ParsedHashlinkObj))).Cast<ParsedHashlinkObj>(), parsedHashlinkObj =>
            {
                log?.Invoke($"Setting up bindings for {parsedHashlinkObj.Name}");
                parsedHashlinkObj.SetupBindings(this);
            });

            Perpendicular.ForEach(Functions.Where(x => x.GetType() == typeof(ParsedHashlinkMethod)).Cast<ParsedHashlinkMethod>(), parsedHashlinkMethod =>
            {
                log?.Invoke($"Parsing opcodes for {parsedHashlinkMethod.FullName}");
                parsedHashlinkMethod.ParseOpcodes(bytecode, this);
            });

            EntryPoint = GetFunctionReference(bytecode.EntryPointIndex);

            sw.Stop();

            log?.Invoke($"Finished in {sw.Elapsed.TotalSeconds:.00}s");
        }

        public ParsedHashlinkType GetTypeReference(int index) => Types.First(x => x.HashlinkType == bytecode.TypesPool[index]);
        public ParsedHashlinkFunction GetFunctionReference(int index) => Functions.First(x => x.HashlinkFunction.Index == index);
        public ParsedHashlinkGlobal GetGlobalReference(int index) => Globals[index - 1];
    }
}
