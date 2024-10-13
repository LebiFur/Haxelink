/*
this csproj alters the unparsed bytecode
which is not the intended use but due to
issues with constructing dead cells' bytecode
it's the only viable way to work with it
look at Haxelink.DeadCellsTest
*/

namespace Haxelink.DeadCellsInfiniteJumps
{
    internal class Program
    {
        //change these paths, file extension doesn't matter
        private const string originalBytecode = "D:\\steam\\steamapps\\common\\Dead Cells MOD\\LATEST.dat";
        private const string outputBytecode = "D:\\steam\\steamapps\\common\\Dead Cells MOD\\OUTPUT.dat";

        private static void Log(string message)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.ff}] {message}");
        }

        private static void Title(string message)
        {
            Log(message);
            Thread.Sleep(500);
        }

        static void Main(string[] args)
        {
            //enable parallelization to speed up the parsing and constructing process
            Perpendicular.Parallel = true;

            Title("Reading bytecode");
            Bytecode bytecode = new(originalBytecode);

            Title("Parsing bytecode");
            ParsedBytecode parsedBytecode = new(bytecode, Log);

            //find the get_airJumps original function index
            //you could also use a fixed index which is 34961
            //but this way it's a lot clearer and foolproof
            int functionIndex = parsedBytecode.Functions.First(x => x.Name == "get_airJumps").HashlinkFunction.Index;

            //get the unparsed function, since natives and functions share the same index space
            //we need to find the first function that matches the index
            HashlinkMethod function = bytecode.FunctionsPool.First(x => x.Index == functionIndex);

            //replace opcode 2 which is a field opcode with an int opcode with args 1 and 1
            //the first arg is the destinaton register and the second one is an index to the int pool
            //int at index 1 in the int pool is 0 so this function will always return 0 allowing for infinite jumps
            function.Opcodes[2] = new Opcode(OpcodeType.Int, [1, 1]);

            bytecode.WriteToFile(outputBytecode);

            Log($"Done");
        }
    }
}
