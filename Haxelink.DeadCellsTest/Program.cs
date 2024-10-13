/*
keep in mind that in the current state of haxelink
dead cells bytecode when parsed and reconstucted
when technically working will crash soon after start
due to a deserialization issue which I have no idea what causes
*/

namespace Haxelink.DeadCellsTest
{
    internal class Program
    {
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

            //find the print method
            ParsedHashlinkMethod print = (ParsedHashlinkMethod)parsedBytecode.Functions.First(x => x.Name == "println");

            ParsedHashlinkMethod run = (ParsedHashlinkMethod)parsedBytecode.Functions.First(x => x.FullName == "haxe.$Unserializer.run");

            //convert the opcode array to a list to add new opcodes
            List<ParsedOpcode> opcodes = [.. run.Opcodes];

            //register 3 is void, register 0 is the serialized string
            opcodes.Insert(0, new ParsedOpcodeCall(new(3), new(print), new ArgumentRegister(0)));

            //convert the opcode list to an array and assign it to the method
            run.Opcodes = [.. opcodes];

            //now everytime the unserializer runs it will print out whatever it tries to deserialize

            Title("Constructing bytecode");
            Bytecode constructedBytecode = new(parsedBytecode, Log);

            constructedBytecode.WriteToFile(outputBytecode);

            Log($"Done");
        }
    }
}
