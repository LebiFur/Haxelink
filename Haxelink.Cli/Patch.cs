namespace Haxelink.Cli
{
    [AttributeUsage(AttributeTargets.Class)]
    public class BytecodePatchAttribute(string name, string author, string version) : Attribute
    {
        public string Name = name;
        public string Author = author;
        public string Version = version;
    }

    public abstract class Patch
    {
        public virtual void PatchBytecode(Bytecode bytecode, ParsedBytecode parsedBytecode, Action<string> log, Action<string> logError)
        {
            logError("This patch cannot be applied to unparsed bytecode");
        }

        public virtual void PatchParsedBytecode(Bytecode bytecode, ParsedBytecode parsedBytecode, Action<string> log, Action<string> logError)
        {
            logError("This patch cannot be applied to parsed bytecode");
        }
    }
}
