# Neuro The Farmer Was Replaced Integration

This is an integration for the game [The Farmer Was Replaced](https://store.steampowered.com/app/2060160?snr=5000_5100__), currently this is intended for someone else to play with Neuro to help her code.

## Running Dependencies

- This was developed with [BepinEx 5.4.23.4](https://github.com/BepInEx/BepInEx/releases/tag/v5.4.23.4).
- You shouldn't have to download anything else.

## Installing

You can get the released versions from the releases tab, all you should need to do is extract the zip and add the folder to the plugins folder in the BepInEx folder where the game is installed.

The config can be modified in the config folder of the same BepInEx installation, the file should be called com.pandapanda135.NeuroTFWRIntegration.cfg if it is not present you may need to start the game.

## Building

After pulling from this repo, the only steps you should need to do are to restore the project's dependencies and to set GamePath in NeuroTFWRIntegration to the game's folder. (The folder containing the exe file to run the game.)

You may also have to change the SdkSetup file in the sdk to use Internal.ResourceManager as both the game and the sdk have a resource manager and I don't know how to make fix the sdk using the game's instead of it's own. Please help if you know how, I have wasted more hours than I would like to admit trying to fix it.

Sorry if there is more you need to do, I'm a bit too lazy to set it up again to check if I'm right.

## Other stuff

As of right now this is considered finished, however updates and improvements will be made if needed. I may also add support for just Neuro gameplay if I ever feel like it.

This is mostly made in about 4 days so sorry for any edge cases or issues with the mod or the parser for the patch format.
