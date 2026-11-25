# Code Obfuscator

Copy this folder into any project's `Assets`. On every player build it renames private methods and
fields in your game's code, making a decompiled build harder to read. Tested on Unity 6.

## Setup
1. Copy this folder into `Assets`.
2. Install the `com.unity.nuget.mono-cecil` package (Window > Package Manager).
3. Build normally — renaming happens automatically. Check `Logs/Obfuscation_NameMap.txt` for the list of renames.

## Keeping a name
Add `[Nikson.DontObfuscate]` to a class, method or field to keep its name.

## Notes
- Only touches your own game code, not Unity or package code.
- Never renames Unity message methods (`Awake`, `Update`, ...), `[SerializeField]`/`[SerializeReference]` fields, or any name also used as a string elsewhere in your code.
- For stronger protection, build with IL2CPP instead of Mono.