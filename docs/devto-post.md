---
title: "Inventor Better Screenshot Tool – clean white PNGs from Autodesk Inventor"
published: false
tags: cad, autodesk, inventor, csharp
cover_image: https://raw.githubusercontent.com/calumk/InventorBetterScreenshotTool/main/screenshots/ui.png
---

Inventor's built-in screenshots always annoyed me — dark background, wrong size, extra cropping. So I built a tiny ribbon add-in that does one thing well: clean **white-background PNGs** at exact dimensions.

**GitHub + download:** https://github.com/calumk/InventorBetterScreenshotTool

![Better Screenshots ribbon tab](https://raw.githubusercontent.com/calumk/InventorBetterScreenshotTool/main/screenshots/ribbon.png)

![Better Screenshot Tool dialog](https://raw.githubusercontent.com/calumk/InventorBetterScreenshotTool/main/screenshots/ui.png)

### What it does

- Adds a **Better Screenshots → Capture → Better Screenshot** button to Inventor 2025
- Exact output size (e.g. 1920×1080), centred live preview
- Auto-numbered files that never overwrite, e.g:
  - `Bracket_0001.png`, `Bracket_0002.png`
- Saves to a `Screenshots` folder beside your `.ipt` / `.iam`, or a fixed folder
- Restores your Inventor background mode after every capture

### Get it

1. Close Inventor
2. Download `InventorBetterScreenshotTool-Setup-v0.3.1.exe` from the [Releases page](https://github.com/calumk/InventorBetterScreenshotTool/releases)
3. Run it (per-user, no admin needed), restart Inventor, enable under **Tools → Add-ins** if prompted

Note: it's unsigned, so SmartScreen will warn. That's expected for now.

### Dev notes

- C# / .NET 8 WinForms, uses the active Inventor camera for capture
- Preview renders to a temp PNG, loads it into a centred `PreviewCanvas`, then deletes it
- Fun gotcha in v0.3.1: Inventor's dark ribbon hates transparent PNGs. I had to ship opaque RGB 16/32px icons as 24bpp bitmaps to kill the alpha halo
- Installer is Inno Setup, per-user, upgrades in place with a stable add-in GUID
- CI is split: GitHub-hosted runners do static checks, a self-hosted `inventor-2025` runner does the real build (GitHub runners don't have Autodesk's interop DLL, and I don't check it in)

MIT licensed. Independent community tool, not an Autodesk product.

If you live in Inventor and need doc-ready screenshots, give it a try — feedback welcome.
