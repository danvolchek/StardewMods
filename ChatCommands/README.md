# Chat Commands


See [This link](http://www.nexusmods.com/stardewvalley/mods/2092) for the NexusMods mod page, which has a description, screenshots, and a download of the built mod.

## How it works

The mod
 - Replaces the default chat box with one that passes input to my mod before it is proccessed.
 - Replaces the default `Console.Out` stream with one that passes input to my mod before it is written to the original `Console.Out`.
 - Detects when the input is a valid SMAPI command.
 - If it is valid, it runs the command, copies the output the command made, and writes that back to the chat box.
 - Otherwise it lets the game handle the input.