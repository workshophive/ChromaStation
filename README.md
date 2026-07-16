# Einstein Engines

<p align="center"><img src="[https://raw.githubusercontent.com/Simple-Station/Einstein-Engines/master/Resources/Textures/Logo/splashlogo.png](https://raw.githubusercontent.com/workshophive/ChromaStation/refs/heads/master/Resources/Textures/Logo/splashlogo.png)" width="512px" /></p>

---

ChromaStation is a fork of [Einstein Engines](https://github.com/Simple-Station/Einstein-Engines) built around EE's built-in lore, as well as personal expansions by the Workshop Hive upon it. PJB's auth is not used and will not be used, refer to the new one instead.

**No ChromaStation content will appeal to the opinions of bigots.** This includes sysmedicalists, transmedicalists, TERFs and TIRFs, ableists, et cetera. Mald PRs about anything "woke" will be closed as soon as they are noticed. Disrespectfully, fuck off.

Space Station 14 is inspired heavily by Space Station 13 and runs on [Robust Toolbox](https://github.com/space-wizards/RobustToolbox), a homegrown engine written in C#.

As a hard fork, any code sourced from a different upstream cannot ever be merged directly here, and must instead be ported.
All code present in this repository is subject to change as desired by the council of maintainers.

## Official Server Policy..?

**No official server currently exists for ChromaStation, and likely will not for a while**.

What it says on the tin.
We currently do not have the ability to host a server of our own, nor can we pay for hosting. It sucks actual ass, however this honestly makes it easier on us in the long run!
Any servers claiming to be the official ChromaStation before this is removed is not endorsed and we WILL complain about it directly to the people hosting it.

## Links

(website & discord coming eventually!) [Steam(SSMV Launcher)](https://store.steampowered.com/app/2585480/Space_Station_Multiverse/) | [Steam(WizDen Launcher)](https://store.steampowered.com/app/1255460/Space_Station_14/) | [Standalone](https://spacestationmultiverse.com/downloads/)

## Contributing

[will be rewritten soon!!!]
We are happy to accept contributions from anybody, come join our Discord if you want to help.
We've got a [list of issues](https://github.com/Simple-Station/Einstein-Engines/issues) that need to be done and anybody can pick them up. Don't be afraid to ask for help in Discord either!

We are currently accepting translations of the game on our main repository.
If you would like to translate the game into another language check the #contributor-general channel in our Discord.

## Building

Refer to [the Space Wizards' guide](https://docs.spacestation14.com/en/general-development/setup/setting-up-a-development-environment.html) on setting up a development environment for general information, but keep in mind that Einstein Engines is not the same and many things may not apply.
We provide some scripts shown below to make the job easier.

### Build dependencies

> - Git
> - .NET SDK 9.0.101


### Windows

> 1. Clone this repository
> 2. Run `git submodule update --init --recursive` in a terminal to download the engine
> 3. Run `Scripts/bat/buildAllDebug.bat` after making any changes to the source
> 4. Run `Scripts/bat/runQuickAll.bat` to launch the client and the server
> 5. Connect to localhost in the client and play

### Linux

> 1. Clone this repository
> 2. Run `git submodule update --init --recursive` in a terminal to download the engine
> 3. Run `Scripts/sh/buildAllDebug.sh` after making any changes to the source
> 4. Run `Scripts/sh/runQuickAll.sh` to launch the client and the server
> 5. Connect to localhost in the client and play

### MacOS

> I don't know anybody using MacOS to test this, but it's probably roughly the same steps as Linux

## License

Please read the [LEGAL.md](./LEGAL.md) file for information on the licenses of the code and assets in this repository.
