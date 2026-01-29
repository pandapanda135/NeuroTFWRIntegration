# <img src="Assets/ColourGymbag.svg" width="32" style="vertical-align:middle;"> Neuro The Farmer Was Replaced Integration

This is an integration for the game [The Farmer Was Replaced](https://store.steampowered.com/app/2060160?snr=5000_5100__), currently this is intended for someone else to play with Neuro to help her code.

Any issues observed with running or installing this should either be noted in an issue created in this repo or the respective [project thread](https://discord.com/channels/574720535888396288/1451269145588142090) in the Neurosama discord.

## Running Dependencies

- This was developed with [BepinEx 5.4.23.4](https://github.com/BepInEx/BepInEx/releases/tag/v5.4.23.4).
- You shouldn't have to download anything else.

## Installing

You can get released builds from the project's Releases tab. Extract the ZIP and place the plugin folder into the `BepInEx/plugins` folder inside the game's installation. Do not remove the AssetBundles folder as the plugin requires those bundles and the files they contain.

The plugin configuration file is located in the game's `BepInEx/config` folder and should be named `com.pandapanda135.NeuroTFWRIntegration.cfg`. If the file is missing, start the game once to generate it.

## Building

After pulling from this repo, you will need to install Unity Editor V6000.0.43f1, this is the same version the game runs on. This is needed as the asset bundles are not distributed in the repo and must be built yourself. This can be done by, launching the Unity project, under Assets click Build AssetBundles and then they should be in an AssetBundles folder to be copied to the game when NeuroTFWRIntegration is built.

Reminder: set the GamePath (This should be the folder containing the exe) and restore the dependencies of the project before building.

You may also have to change the SdkSetup file in the sdk to use Internal.ResourceManager as both the game and the sdk have a resource manager and I don't know how to make fix the sdk using the game's instead of it's own. Please help if you know how, I have wasted more hours than I would like to admit trying to fix it.

While this should work, I have not tested this on a fresh install so sorry for any issues in this guide. You can ask for help in the project thread that is mentioned above.