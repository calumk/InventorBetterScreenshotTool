---
title: "I remade an abandoned Inventor add-in from scratch"
published: false
tags: cad, autodesk, inventor, csharp
cover_image: https://raw.githubusercontent.com/calumk/InventorBetterScreenshotTool/main/screenshots/ui.png
---

I needed clean white-background screenshots out of Autodesk Inventor — for docs, quotes, and presentations. There *was* a tool for exactly that. So instead of starting from zero, I started by asking: why not just use it?

**The result, anyway:** https://github.com/calumk/InventorBetterScreenshotTool

![Better Screenshots ribbon tab](https://raw.githubusercontent.com/calumk/InventorBetterScreenshotTool/main/screenshots/ribbon.png)

![Better Screenshot Tool dialog](https://raw.githubusercontent.com/calumk/InventorBetterScreenshotTool/main/screenshots/ui.png)

### The tool that was

[Avant.CAD's Screenshot Tool](https://avantcad.wordpress.com/inventor-tools/advanced-screenshot-tool-for-autodesk-inventor/) (on the [Autodesk App Store](https://apps.autodesk.com/INVNTOR/en/Detail/HelpDoc?appId=9103249803369100603&appLang=en&os=Win64#inunininfo)) was genuinely good. One button under the Tools tab, white-background captures, a screenshot list, JPG-to-PDF, even attach-to-Outlook. For its era, it nailed the workflow.

### Why I remade it

Three reasons, all visible in public:

1. **It's abandonware.** The last update added Inventor 2020 compatibility (v1.3, ~2019). The publisher blog has been quiet since. Nothing for 2021–2026.
2. **It was always a bit buggy.** Its own App Store changelog tells the story: white-background crashes, Inventor's background flipping to gradient/one-colour after capture, save-path bugs, screenshots bound to the wrong document. And on the Autodesk forums, users report it stopped working properly from Inventor 2022 on ("Until installing version 2022 I was able to use Screenshot Tool…").
3. **The scope crept.** JPG lists, PDF conversion, Outlook integration, Paint hooks — useful in 2015, but each one is a new way to break on a modern setup.

So I remade the core idea from scratch for Inventor 2025: one dialog, white PNGs, exact sizes, preview, sensible filenames. Nothing else.

### What mine does

- Adds a **Better Screenshots → Capture → Better Screenshot** button to Inventor 2025
- Exact output size (e.g. 1920×1080) with a centred live preview
- Auto-numbered files that never overwrite: `Bracket_0001.png`, `Bracket_0002.png`
- Saves to a `Screenshots` folder beside your `.ipt` / `.iam`, or a fixed folder
- Always restores your Inventor background mode after capture — the bug class that haunted the original gets a dedicated code path

Deliberately out of scope: transparent exports, PDF, email. The original's extras are exactly what I cut.

### Get it

1. Close Inventor
2. Download `InventorBetterScreenshotTool-Setup-v0.3.1.exe` from the [Releases page](https://github.com/calumk/InventorBetterScreenshotTool/releases)
3. Run it (per-user, no admin needed), restart Inventor, enable under **Tools → Add-ins** if prompted

Note: it's unsigned, so SmartScreen will warn. That's expected for now.

### Dev notes

- C# / .NET 8 WinForms, captures via the active Inventor camera
- Preview renders to a temp PNG, loads it into a centred `PreviewCanvas`, then deletes it
- Fun gotcha in v0.3.1: Inventor's dark ribbon hates transparent PNGs. I ship opaque RGB 16/32px icons as 24bpp bitmaps to kill the alpha halo
- Inno Setup installer, per-user, upgrades in place with a stable add-in GUID
- CI is split: GitHub-hosted runners do static checks, a self-hosted `inventor-2025` runner does the real build (GitHub runners don't have Autodesk's interop DLL, and I don't check it in)

MIT licensed. Independent community tool, not an Autodesk product — and not a fork; no code from the original, just the lesson of what to keep and what to leave behind.

If you lived through the old Screenshot Tool, I'd love to know what you miss most — and what you don't.
