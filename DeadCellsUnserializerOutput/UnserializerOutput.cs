using Haxelink;
using Haxelink.Cli;

namespace DeadCellsUnserializerOutput
{
    [BytecodePatch("Dead Cells Unserializer Output", "Lebi", "1.0")]
    public class UnserializerOutput : Patch
    {
        public override void PatchParsedBytecode(Bytecode bytecode, ParsedBytecode parsedBytecode, Action<string> log, Action<string> logError)
        {
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
        }
    }
}
