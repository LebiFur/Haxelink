using Haxelink;
using Haxelink.Cli;

namespace DeadCellsInfiniteJumps
{
    [BytecodePatch("Dead Cells Infinite Jumps", "Lebi", "1.0")]
    public class InfiniteJumps : Patch
    {
        public override void PatchBytecode(Bytecode bytecode, ParsedBytecode parsedBytecode, Action<string> log, Action<string> logError)
        {
            //find the get_airJumps original function index
            //you could also use a fixed index
            //but this way it's a lot clearer and foolproof
            int functionIndex = parsedBytecode.Functions.First(x => x.Name == "get_airJumps").HashlinkFunction.Index;

            //get the unparsed function, since natives and functions share the same index space
            //we need to find the first function that matches the index
            HashlinkMethod function = bytecode.FunctionsPool.First(x => x.Index == functionIndex);

            //replace opcode 2 which is a field opcode with an int opcode with args 1 and 1
            //the first arg is the destinaton register and the second one is an index to the int pool
            //int at index 1 in the int pool is 0 so this function will always return 0 allowing for infinite jumps
            function.Opcodes[2] = new Opcode(OpcodeType.Int, [1, 1]);
        }
    }
}
