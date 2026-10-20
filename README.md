# Autodesk Inventor Better Screenshot Tool — By CalumK

**Version 0.3.1** · **Autodesk Inventor 2025.2.1+** · **C# / .NET 8 (Windows x64)** · [calumk.com](https://calumk.com)

A lightweight Inventor ribbon add-in for clean **white-background PNG screenshots** of saved `.ipt` and `.iam` files. Supports exact output dimensions, a centred render-based preview, automatic sequential filenames, and saving beside the active model or in a fixed folder.

This is an **independent community tool**, not an official Autodesk product. Uses the Phosphor Icons `cube-focus` duotone icon with the accompanying MIT license in `src/InventorScreenshotTool/Assets/`.

## Screenshot behaviour

When capturing `D:\Jobs\Widget\Bracket.ipt`, the default output is:

```text
D:\Jobs\Widget\Screenshots\Bracket_0001.png
D:\Jobs\Widget\Screenshots\Bracket_0002.png
```

Existing screenshot files are not overwritten. A preview is rendered to a temporary PNG and deleted after loading. The full export uses the active Inventor camera and restores the previous display background mode on completion or error. Transparent exports, PDF and email are out of scope for this release.

## What's new in v0.3.1

- Ribbon icons now have **solid white backgrounds** rather than transparency. The 16px and 32px variants are stored as opaque RGB and are passed to Inventor as 24bpp bitmaps, preventing alpha artifacts against Inventor's dark toolbar. Dialog header and Windows window icon are unchanged.
- Everything else from v0.3.1 is retained, including installer, preview, and upgrade-in-place behavior.

## Changes originally introduced in v0.3.1

- **Header and control clipping fixed.** Replaced potentially overlapping docked panels with a three-row root layout (header/content/footer).
- **Preview centred consistently.** A new `PreviewCanvas` centres and sizes the actual image on *every paint*, including the first one, rather than relying on resize-event timing.
- **White image boundary visible.** Removed the right-hand white card, replaced it with a neutral grey preview canvas, and added a subtle border and shadow around the PNG.
- Reworked preview header, button placement, bottom status, and resizable settings section.
- Added source validation, release scripts, GitHub Actions, and an **Inno Setup per-user installer**.
- Installer and developer scripts use the original add-in ID and directory, so older installations are upgraded in place.

## Install: end users

After a release is published on GitHub:

1. Close Autodesk Inventor and save your work.
2. Open the repository **Releases** page, download `InventorBetterScreenshotTool-Setup-vX.Y.Z.exe` and run it.
3. Setup installs for the current Windows user, without administrator rights.
4. Restart Inventor 2025; enable the add-in if prompted under **Tools → Add-ins**.
5. Open a saved part/assembly; use **Better Screenshots → Capture → Better Screenshot**.

The installer uses `%APPDATA%\Autodesk\Inventor 2025\Addins\InventorScreenshotTool`. It overwrites the same add-in files from v0.1.x/v0.2.x without duplicating the add-in. User settings remain at `%APPDATA%\InventorScreenshotTool\settings.json`. Screenshots and preferences are not deleted or migrated.

**Unsigned installer warning:** Until a trusted code-signing certificate is configured, Windows SmartScreen may display an unrecognized publisher warning. Releases are not currently digitally signed.

## Build/install locally — developer

This is still the simplest way to test the v0.3.1 icon fix against the user's Inventor 2025.2.1 installation.

Prerequisites: Windows x64, Autodesk Inventor 2025 installed, .NET 8 SDK (Visual Studio 2022 is optional).

```powershell
# From the repository root, with Inventor CLOSED:
.\scripts\install.ps1
```

`install.ps1` calls `package.ps1`, compiles the DLL with the API reference from the local Inventor installation, and copies only **our application files** and `.addin` manifest into the version-specific add-ins directory. It does not copy/redistribute `Autodesk.Inventor.Interop.dll`. If Inventor is installed in a nonstandard directory:

```powershell
.\scripts\install.ps1 -InventorBin 'D:\Autodesk\Inventor 2025\Bin'
```

For **developer-script installs only**, remove the add-in but leave screenshots and user settings. If you used `Setup.exe`, uninstall through **Windows Settings → Installed apps** instead:

```powershell
.\scripts\uninstall.ps1
```

For a local distributable payload without installing:

```powershell
.\scripts\package.ps1
```

The staged payload appears under `artifacts/plugin/`. To create the installer manually, first install **Inno Setup 6**, and run:

```powershell
& 'C:\Program Files (x86)\Inno Setup 6\ISCC.exe' 'installer\BetterScreenshot.iss'
```

Installer output: `artifacts/installer/InventorBetterScreenshotTool-Setup-v0.3.1.exe`.

## GitHub publication and automated releases

The repository includes two workflows:

- `.github/workflows/checks.yml` — portable source/configuration checks on pushes and PRs. **Does not compile Inventor code**.
- `.github/workflows/release.yml` — on `v*` tags, build on your **own Windows x64 Inventor 2025 machine** via a self-hosted runner, upload the app-only payload to a GitHub-hosted Windows job, compile an Inno Setup `.exe`, and attach the installer to the GitHub Release.

### One-time runner setup

GitHub's Windows-hosted runners do **not** include Autodesk Inventor and its interop DLL. This template **does not** use third-party copies of Autodesk binaries or check any into your repository.

1. Set up a dedicated/isolated Windows build machine or VM with Inventor 2025 and the .NET 8 SDK installed.
2. In your GitHub repository: **Settings → Actions → Runners → New self-hosted runner → Windows x64**; follow GitHub's setup steps.
3. Assign the custom label **`inventor-2025`** to this runner. Default labels `self-hosted`, `Windows`, and `X64` are used too.
4. Confirm the machine has `C:\Program Files\Autodesk\Inventor 2025\Bin\Autodesk.Inventor.Interop.dll` and can run `dotnet build`.
5. Keep the runner online when creating release tags.
6. In the repository's Settings → Actions → General, permit GitHub Actions to create releases with the workflow's `contents: write` permission if your organization has restricted workflow permissions.

**Security:** GitHub cautions against self-hosted runners for public repositories. Use a separate, minimally privileged machine/VM, protect tag creation, review commits before tagging, and **never** trigger self-hosted builds for untrusted pull requests. This workflow intentionally runs self-hosted compilation only for explicit release tags or manual `workflow_dispatch`; PR checks run on GitHub-hosted runners. A personal workstation with important private data is not the recommended public repository runner. [GitHub security guidance](https://docs.github.com/en/actions/reference/security/secure-use).

### Release a version

Commit/push the files to a repository you own, then create a tag matching the `.csproj` `Version`:

```powershell
git add .
git commit -m 'Prepare Better Screenshot Tool v0.3.1'
git push origin main
git tag v0.3.1
git push origin v0.3.1
```

Check **Actions → Build and publish installer**. After both jobs complete, the `v0.3.1` GitHub Release contains `InventorBetterScreenshotTool-Setup-v0.3.1.exe`. For subsequent versions, increment the version in the `.csproj`, optionally update the Inno default `AppVersion`, and push a matching tag; the CI build checks tags for version mismatches.

The workflow can also be run manually from the **Actions** tab to test an installer; a manual run creates a downloadable **workflow artifact**, not a GitHub Release.

### Before making the repository public

- Decide on **your own project's source-code license**. No license is implied just by publishing publicly; the Phosphor third-party license in `Assets/` applies to Phosphor files only.
- Review the code and README to confirm the branding and ownership information are correct.
- Don't commit `Autodesk.Inventor.Interop.dll`, personal screenshots, CAD documents, screenshots from customers, secrets or Autodesk product binaries.
- Test the packaged installer and uninstaller on an actual Inventor 2025 installation before recommending it to users.
- Consider code-signing the `.exe` when distributing more broadly (the provided workflow does not sign binaries).

## Source map

| File | Role |
|---|---|
| `src/InventorScreenshotTool/ScreenshotDialog.cs` | WinForms controls, persistent settings, status |
| `src/InventorScreenshotTool/PreviewCanvas.cs` | Centred grey preview surface with white PNG boundary |
| `src/InventorScreenshotTool/ScreenshotExporter.cs` | Camera capture, temporary preview, background restoration |
| `src/InventorScreenshotTool/StandardAddInServer.cs` | Inventor add-in ribbon and stable GUID |
| `src/InventorScreenshotTool/Assets/` | Phosphor CubeFocus icon variants and MIT notice |
| `scripts/package.ps1` | Windows build; restrict payload to our files |
| `scripts/install.ps1` | Local developer installation / in-place update |
| `installer/BetterScreenshot.iss` | Per-user end-user installer |
| `.github/workflows/` | Static PR checks, tag-based build and release |

## Status / limitations

This v0.3.1 source package has undergone **structural validation only** in a non-Windows environment. The updated DLL and installer **have not yet been compiled or tested with Inventor**. First run `install.ps1` locally, verify UI and capture, then enable automated public releases. `scripts/validate_repository.py` validates XML/configuration consistency, not C# type checking.

See `docs/TEST_PLAN.md` for acceptance tests. Autodesk and Inventor are trademarks of Autodesk, Inc. This tool is independent and not endorsed by Autodesk.
