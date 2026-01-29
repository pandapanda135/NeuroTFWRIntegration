# V2.0.0 Release Notes

This version primarily includes large behine the scene changes that allow for custom content created in Unity to be added to the game. Due to that, those are the only changes that will be mentioned here.

These are large fundamental changes, so issues are to be expected. I will try to fix them as quickly as possible.

## Unity Additions

### Toasts

In previous versions it was hard to know what actions Neuro was running. Toasts, similar to notifications, now appear after an action's validation completes. They show whether the action succeeded, and if not, why.

There is also another type of toast, that Neuro can create herself. This is primarily so she can insult people and theres not many other uses to them.

These can be disabled in the config under the `Toasts` section.

### Neuro Chat

Previously there was no way to send Neuro the contents of specific windows. With "Neuro Chat" (the internal name, that is not super accurate) you can send her the contents of one or multiple windows along with a prompt.

Currently she can either write a patch or deny the request specified in the prompt.

This can be disabled in the config under `Chat`.

### Version Checker

It exists on the main menu and shows what version you have installed.

Information about the latest version is stored in `version.json` in this repo. The local version is referenced from BepInEx.

This can be disabled in the config under `VersionChecking`.

### Swarm Drone

You can make the drone look like the swarm drone, or at least how it is usually depicted. This is unlocked early, along with most of the other hats in the game.

This cannot be disabled in the config, however it is never equipped automatically and must be enabled the same way hats are in the base game. More specifically, it is under `Hats.Swarm`.