---
title: "I remade an abandoned Inventor add-in for better screenshots"
published: false
tags: cad, autodesk, inventor, csharp
cover_image: https://raw.githubusercontent.com/calumk/InventorBetterScreenshotTool/main/screenshots/ui.png
---

I needed clean white-background screenshots out of Autodesk Inventor — for docs, quotes, and presentations. There *was* a tool for exactly that - I have used it for a year or so, but it was abandoned and buggy. So I remade it.


**The result, anyway:** https://github.com/calumk/InventorBetterScreenshotTool

![Better Screenshots ribbon tab](https://raw.githubusercontent.com/calumk/InventorBetterScreenshotTool/main/screenshots/ribbon.png)

![Better Screenshot Tool dialog](https://raw.githubusercontent.com/calumk/InventorBetterScreenshotTool/main/screenshots/ui.png)

### The tool that was

[Avant.CAD's Screenshot Tool](https://avantcad.wordpress.com/inventor-tools/advanced-screenshot-tool-for-autodesk-inventor/) (on the [Autodesk App Store](https://apps.autodesk.com/INVNTOR/en/Detail/HelpDoc?appId=9103249803369100603&appLang=en&os=Win64#inunininfo)) was genuinely good. One button under the Tools tab, white-background captures, a screenshot list, JPG-to-PDF, even attach-to-Outlook. For its era, it nailed the workflow.

### Why I remade it


1. **It's seemingly abandonware.** The last update added Inventor 2020 compatibility (v1.3, ~2019). The publisher blog has been quiet since. Nothing for 2021–2026.
2. **Its buggy.** The original tool has some install issues, and wasnt open-source, so I couldn't fix them. - Hopefully the new one can be maintained and improved by others.

So (A)I rewrote it in C# / .NET 8 WinForms, I made it open-source, and stripped out the extras that I didn't need. The result is a small, focused tool that does one thing well: clean screenshots.

### What mine does

- Adds a **Better Screenshots → Capture → Better Screenshot** button to Inventor 2025+
- Exact output size (e.g. 1920×1080) with a centred live preview
- Auto-numbered files that never overwrite: `Bracket_0001.png`, `Bracket_0002.png`
- Saves to a `Screenshots` folder beside your `.ipt` / `.iam`, or a fixed folder

### Get it
1. Close Inventor
2. Download `InventorBetterScreenshotTool-Setup-v0.3.1.exe` from the [Releases page](https://github.com/calumk/InventorBetterScreenshotTool/releases)
3. Run it (per-user, no admin needed), restart Inventor, enable under **Tools → Add-ins** if prompted

Note: it's unsigned, so SmartScreen will warn. That's expected for now - You can rebuild it yourself, or run the powershell installer if you like.

### Dev notes

- C# / .NET 8 WinForms, captures via the active Inventor camera
- Preview renders to a temp PNG, loads it into a centred `PreviewCanvas`, then deletes it
- Fun gotcha in v0.3.1: Inventor's dark ribbon hates transparent PNGs. I ship opaque RGB 16/32px icons as 24bpp bitmaps to kill the alpha halo
- Inno Setup installer, per-user, upgrades in place with a stable add-in GUID
- CI is split: GitHub-hosted runners do static checks, a self-hosted `inventor-2025` runner does the real build (GitHub runners don't have Autodesk's interop DLL, and I don't check it in)

MIT licensed. 
not an Autodesk product 
