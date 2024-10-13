namespace Haxelink.Example
{
    internal class Program
    {
        private static void Log(string message)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.ff}] {message}");
        }

        private static void Title(string message)
        {
            Log(message);
            Thread.Sleep(500);
        }

        private static void PatchExample(string exampleName, Action<ParsedBytecode> patchBody)
        {
            Log($"------ {exampleName} ------");

            Title("Reading bytecode");
            Bytecode bytecode = new(Path.Combine("Examples", exampleName));

            Title("Parsing bytecode");
            ParsedBytecode parsedBytecode = new(bytecode, Log);

            Title("Patching bytecode");
            patchBody(parsedBytecode);

            Title("Constructing bytecode");
            Bytecode constructedBytecode = new(parsedBytecode, Log);

            constructedBytecode.WriteToFile(Path.Combine("Output", exampleName));

            Log($"------ {exampleName} ------");
        }

        private static void Main(string[] args)
        {
            //enable parallelization to speed up the parsing and constructing process
            Perpendicular.Parallel = true;

            Directory.CreateDirectory("Output");

            PatchExample("HelloWorld.bin", parsedBytecode =>
            {
                //find the constant that has the value of "Hello world!" and replace it
                parsedBytecode.Constants.First(x => x.Bytes == "Hello world!").Bytes = "Bread amonger";
            });

            PatchExample("MoreComplex.bin", parsedBytecode =>
            {
                //find the main method
                ParsedHashlinkMethod main = (ParsedHashlinkMethod)parsedBytecode.Functions.First(x => x.Name == "main");

                //convert the opcode array to a list to add new opcodes
                List<ParsedOpcode> opcodes = [.. main.Opcodes];

                //opcode 4 calls Math.random and stores the returned value in register 3
                //we insert a new opcode just after that which sets register 3 to 0
                opcodes.Insert(5, new ParsedOpcodeFloat(new(3), new(0)));

                //convert the opcode list to an array and assign it to the method
                main.Opcodes = [.. opcodes];

                //now the x coordinate of the point will always be 0
            });
        }
    }
}