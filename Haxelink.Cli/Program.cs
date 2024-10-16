using System.Reflection;

namespace Haxelink.Cli
{
    internal class Program
    {
        private static readonly string[] optionalArgs = ["-p", "--patch", "-r", "--raw", "-s", "--sequential", "-v", "--verbose"];

        private static void Log(string message)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.ff}] {message}");
        }

        private static void LogError(string message)
        {
            Console.BackgroundColor = ConsoleColor.Red;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.ff}] [Error] {message}");
            Console.ResetColor();
        }

        private static void Title(string message)
        {
            Log(message);
            Thread.Sleep(500);
        }

        private static int GetPathFromArgument(string[] args, int index, out string path)
        {
            if (index >= args.Length) throw new Exception("End of command");

            string val = args[index];

            if (val.Length <= 0) throw new Exception("Path is empty");
            if (optionalArgs.Contains(val)) throw new Exception("Invalid path");

            path = "";
            int skip = 0;

            if (val[0] == '"')
            {
                bool ended = false;

                for (int j = index; j < args.Length && !ended; j++)
                {
                    string valToAdd = args[j][1..];

                    if (args[j][^1] == '"')
                    {
                        valToAdd = args[j][..^1];
                        ended = true;
                    }

                    path += $" {valToAdd}";
                    skip++;
                }

                if (!ended) throw new Exception("Path in quotation marks has not been closed");
            }
            else
            {
                if (val[^1] == '"') throw new Exception("Path in quotation marks has not been opened");

                path = val;
                skip = 1;
            }

            return skip;
        }

        static void Main(string[] args)
        {
            args = [.. args.Where(x => x.Length > 0 && !char.IsWhiteSpace(x[0]))];

            try
            {
                bool raw = false;
                bool verbose = false;
                bool sequential = false;
                List<string> patches = [];
                string? input = null;
                string? output = null;

                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i].Length > 0 && args[i][0] == '-')
                    {
                        if (args[i] == "-r" || args[i] == "--raw")
                        {
                            if (raw) throw new Exception("Multiple [-r | --raw] parameters");
                            raw = true;
                        }
                        else if (args[i] == "-p" || args[i] == "--patch")
                        {
                            int skip;
                            string path;

                            try
                            {
                                skip = GetPathFromArgument(args, i + 1, out path);
                            }
                            catch (Exception e)
                            {
                                throw new Exception($"[-p | --patch] : {e.Message}");
                            }

                            i += skip;
                            patches.Add(path);
                        }
                        else if (args[i] == "-s" || args[i] == "--sequential")
                        {
                            if (sequential) throw new Exception("Multiple [-s | --sequential] parameters");
                            sequential = true;
                        }
                        else if (args[i] == "-v" || args[i] == "--verbose")
                        {
                            if (verbose) throw new Exception("Multiple [-v | --verbose] parameters");
                            verbose = true;
                        }
                        else throw new Exception($"Invalid parameter {args[i]}");
                    }
                    else
                    {
                        int skip;
                        string path;

                        try
                        {
                            skip = GetPathFromArgument(args, i, out path) - 1;
                        }
                        catch (Exception e)
                        {
                            if (input == null) throw new Exception($"<input> : {e.Message}");
                            else if (output == null) throw new Exception($"<output> : {e.Message}");
                            else throw new Exception($"Invalid argument {args[i + 1]} : {e.Message}");
                        }

                        i += skip;

                        if (input == null) input = path;
                        else if (output == null) output = path;
                        else throw new Exception($"Invalid argument {args[i]}");
                    }
                }

                if (input == null) throw new Exception("<input> is not defined");
                if (output == null) throw new Exception("<output> is not defined");

                try
                {
                    Perpendicular.Parallel = !sequential;

                    Title("Reading bytecode");
                    Bytecode bytecode = new(input);

                    Title("Parsing bytecode");
                    ParsedBytecode parsedBytecode = new(bytecode, verbose ? Log : null);

                    foreach (string patchPath in patches)
                    {
                        Title($"Applying patch {patchPath}");

                        void log(string log) => Log($"[{patchPath}] {log}");
                        void logError(string log) => LogError($"[{patchPath}] {log}");

                        try
                        {
                            Assembly assembly = Assembly.LoadFile(Path.GetFullPath(patchPath));

                            Type type = assembly.ExportedTypes.FirstOrDefault(x =>
                                x.IsAssignableTo(typeof(Patch)) &&
                                x.GetCustomAttribute<BytecodePatchAttribute>() != null
                            ) ?? throw new Exception("Assembly does not contain any patches");

                            Patch patch = (Patch?)Activator.CreateInstance(type) ?? throw new Exception("Couldn't create patch instance");

                            BytecodePatchAttribute attribute = type.GetCustomAttribute<BytecodePatchAttribute>()!;

                            log($"{attribute.Name} {attribute.Version} by {attribute.Author}");

                            try
                            {
                                if (raw) patch.PatchBytecode(bytecode, parsedBytecode, log, logError);
                                else patch.PatchParsedBytecode(bytecode, parsedBytecode, log, logError);
                            }
                            catch (Exception ex)
                            {
                                logError(ex.Message);
                                logError("This patch crashed, resulting bytecode might be corrupted");
                            }
                        }
                        catch (Exception ex)
                        {
                            logError(ex.Message);
                            log("Patch has not been applied");
                        }
                    }

                    Bytecode bytecodeToWrite = bytecode;

                    if (!raw)
                    {
                        Title("Constructing patched bytecode");
                        bytecodeToWrite = new(parsedBytecode, verbose ? Log : null);
                    }

                    bytecodeToWrite.WriteToFile(output);
                    Title($"Patched bytecode written to {output}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("\nUsage: hlpatch [-r | --raw] [-p | --patch <file>]... [-s | --sequential] [-v | --verbose] <input> <output>\n");
                Console.WriteLine(" -r | --raw            Patch unparsed bytecode, will only invoke PatchBytecode and not PatchParsedBytecode in patches");
                Console.WriteLine(" -p | --patch <file>   Patch file to be applied, can define multiple, file is the path to the dll");
                Console.WriteLine(" -s | --sequential     Do not enable parallelization, will run much slower");
                Console.WriteLine(" -v | --verbose        Log every step");
                Console.WriteLine(" <input>               Path to the bytecode file to be patched");
                Console.WriteLine(" <output>              Path where the resulting patched bytecode will be saved");
                Console.WriteLine("\nExample: hlpatch -r --patch patch1.dll -p \"./patches here/patch2.dll\" bytecode.hl ./output/output.bin");
            }
        }
    }
}
