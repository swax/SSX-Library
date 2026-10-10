# SSX-Library
C#/.NET Library for Extracting, Creating, and Modifying files for SSX games. 

> ## This is a modified fork
>
> This repository is a fork of [GlitcherOG/SSX-Library](https://github.com/GlitcherOG/SSX-Library),
> **modified by swax** and maintained as the build input for
> another tool. It is not the upstream project.
> Modifications are Copyright (C) 2026 swax and, like the upstream work, are
> licensed **GPL-3.0** — see [LICENSE.txt](LICENSE.txt).
>
> ### Changes from upstream
>
> Compared with `upstream/main` at `1bfdf18` (PR #55), the remaining changes are:
>
> - **Tricky texture reuse:** `SeedImageFiles` preserves original texture slot
>   ordering when rebuilding a level against an existing SSH. The PBD texture
>   count matches that SSH, and the caller's seed list is kept intact.
> - **SSF string reading:** bound HUD strings to their node payload and consume
>   alignment padding so the next effect is read from the correct offset.
>   HUD types and JSON use upstream's grouped API;
>   OpenSlope handles its portable HUD fields in its own effects adapter.
>
> Regression tests cover texture slots and HUD binary round trips. Earlier
> audio, lightmap, format, and dependency changes
> from this fork are now included upstream. Particle instances use
> `ParticleModelIndex` in C# and JSON.
> Existing formatting and helper placement are retained to keep the merge diff focused.
>
> Use `git diff upstream/main` to inspect the current differences. Fixes are
> offered back upstream where they are generally applicable; please report
> library bugs to upstream first unless they are specific to the changes above.
>
> ### Third-party notices
>
> - **EA MicroTalk decoder:** `UtkCodec.cs` is a port of vgmstream's `utkdec.c`
>   and carries vgmstream's ISC copyright and permission notice in its file header.
> - **Vendored SharpTriStrip:** `SSX-Library/SharpTriStrip/` is MaxHwoy's C# port
>   of NVidia's NvTriStrip algorithm and remains under its BSD-3-Clause license,
>   not this fork's general GPL terms. The complete required notice is retained
>   in [`SSX-Library/SharpTriStrip/LICENSE.txt`](SSX-Library/SharpTriStrip/LICENSE.txt)
>   and copied beside binary build output as `licenses/SharpTriStrip-LICENSE.txt`.

The library serves as a framework for GUI/CLI modding tools, but it can also be used as a standalone tool for non tool developers. The library comes with abstraction classes and json formats for simplifying game data parsing. For example, its own Model class that unifies character model types from multiple games into one. And a level extraction feature that extracts level data into readable json files, that includes terrain, props, triggers, textures, lighting and more.

The library was made to isolate the backend from the Windows only [SSX Collection Multitool](https://github.com/GlitcherOG/SSX-Collection-Multitool). Currently we're refactoring every part of the library to make it maintainable and cross-platform. We have a general checklist of the things we need to do, though many will be combined or removed. 
## Refactor Checklist

## Special Thanks
https://github.com/Erickson400/SSXTrickyModelExporter <br>
https://github.com/WouterBaeyens/Ssx3SshConverter <br>
https://github.com/SSXModding/bigfile <br>
https://github.com/SSXModding/ <br>
https://github.com/gibbed/Gibbed.RefPack <br>
