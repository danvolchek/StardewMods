# Chat Commands


See [This link](http://www.nexusmods.com/stardewvalley/mods/2092) for the NexusMods mod page, which has a description, screenshots, and a download of the built mod.

## How it works

The mod
 - Replaces the default chat box with one that passes input to my mod before it is proccessed.
 - Replaces the default `Console.Out` stream with one that passes input to my mod before it is written to the original `Console.Out`.
 - Detects when the input is a valid SMAPI command.
 - If it is valid, it runs the command, copies the output the command made, and writes that back to the chat box.
 - Otherwise it lets the game handle the input.

It also
 - Adds the ability to scroll by not actually deleting messages over the limit, and instead only displaying a subset of messages.
 - Piggybacks off the existing history system to add history for messages the user sent.
 - Dynamically changes the message limit based on the user's window size, so larger screens see more messages.
 - Rewrites how text entry works so players can use the left/right arrow keys how they'd expect to edit any part of their message.
 - Emulates a monospace font by drawing characters in fixed width slots for console output.