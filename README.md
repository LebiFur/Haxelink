# Haxelink

Hashlink bytecode parser and patcher

## CLI - hlpatch

### Installation

[Download for windows](https://github.com/LebiFur/Haxelink/releases/tag/v0.1.0-alpha), requires [.NET 8.0 runtime](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-8.0.10-windows-x64-installer).

It also contains 2 example patches for dead cells:
- InfiniteJumps - gives you infinite jumps, requires -r flag
- UnserializerOutput - adds logging to the unserializer, -r flag must not be present

> Dead Cells if patched without -r flag will crash soon after steam init due to an unknown deserialization issue, UnserializerOutput patch is aimed to help fix it

---

### Usage

```
Usage: hlpatch [-r | --raw] [-p | --patch <file>]... [-s | --sequential] [-v | --verbose] <input> <output>

-r | --raw            Patch unparsed bytecode, will only invoke PatchBytecode and not PatchParsedBytecode in patches

-p | --patch <file>   Patch file to be applied, can define multiple, file is the path to the dll

-s | --sequential     Do not enable parallelization, will run much slower

-v | --verbose        Log every step

<input>               Path to the bytecode file to be patched

<output>              Path where the resulting patched bytecode will be saved

Example: hlpatch -r --patch patch1.dll -p "./patches here/patch2.dll" bytecode.hl ./output/output.bin
```

---

### Preparing Dead Cells

To extract the bytecode from the game automatically you can use [N3rdL0rd's script](https://github.com/N3rdL0rd/alivecells) or manually open the game's executable with 7zip and look for `HLBOOT.DAT`.

If you chose the manual way you'll also need to replace `steam_api.dll` with the one from [Goldberg Emulator](https://mr_goldberg.gitlab.io/goldberg_emulator/) and add `hl.exe` from [HashLink 1.10](https://github.com/HaxeFoundation/hashlink/releases/tag/1.10) (other versions might work as well but I'm sure with this one).

Now you can run the game from command line like this `hl.exe bytecode.bin`

> File extensions don't matter

Now to patch the game with infinite jumps:

1. `hlpatch.exe -r -p DeadCellsInfiniteJumps.dll "path/to/deadcells/bytecode.dat" patched.dat`
2. `hl.exe patched.dat`
