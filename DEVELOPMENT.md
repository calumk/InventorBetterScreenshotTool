# Development guide — Better Screenshot Tool

Developer and maintainer notes for building, packaging, and releasing. End users should start with `README.md`.

## Prerequisites

- Windows x64 with Autodesk Inventor 2025 installed.
- .NET 8 SDK (Visual Studio 2022 is optional).
- [Inno Setup 6](https://jrsoftware.org/isinfo.php) — only needed to build the end-user installer.

## Build and install locally

The simplest way to test against a local Inventor 2025.2.1 installation. Run from the repository root, with Inventor **closed**:

```powershell
.\scripts\install.ps1
```

`install.ps1` calls `package.ps1`, compiles the DLL with the API reference from the local Inventor installation, and copies only **our application files** and the `.addin` manifest into the version-specific add-ins directory. It does not copy or redistribute `Autodesk.Inventor.Interop.dll`.

If Inventor is installed in a nonstandard directory:

```powershell
.\scripts\install.ps1 -InventorBin 'D:\Autodesk\Inventor 2025\Bin'
```

For **developer-script installs only**, this removes the add-in but leaves screenshots and user settings. If you installed via `Setup.exe`, uninstall through **Windows Settings → Installed apps** instead:

```powershell
.\scripts\uninstall.ps1
```

For a local distributable payload without installing:

```powershell
.\scripts\package.ps1
```

The staged payload appears under `artifacts/plugin/`. To create the installer manually, first install **Inno Setup 6**, then run:

```powershell
& 'C:\Program Files (x86)\Inno Setup 6\ISCC.exe' 'installer\BetterScreenshot.iss'
```

Installer output: `artifacts/installer/InventorBetterScreenshotTool-Setup-v0.3.1.exe`.

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

## Validation and tests

Portable structural check (runs anywhere, does not compile C#):

```powershell
python3 scripts/validate_repository.py
```

It validates XML/configuration consistency and version/manifest alignment — not C# type checking. Full acceptance tests are in `docs/TEST_PLAN.md`; run them on a real Inventor 2025.2.1 installation before recommending a build to users.

## GitHub workflows and automated releases

The repository includes two workflows:

- `.github/workflows/checks.yml` — portable source/configuration checks on pushes and PRs. **Does not compile Inventor code**.
- `.github/workflows/release.yml` — on `v*` tags, build on your **own Windows x64 Inventor 2025 machine** via a self-hosted runner, upload the app-only payload to a GitHub-hosted Windows job, compile an Inno Setup `.exe`, and attach the installer to the GitHub Release.

### One-time runner setup

GitHub's Windows-hosted runners do **not** include Autodesk Inventor and its interop DLL. This setup **does not** use third-party copies of Autodesk binaries or check any into the repository.

1. Set up a dedicated/isolated Windows build machine or VM with Inventor 2025 and the .NET 8 SDK installed.
2. In the GitHub repository: **Settings → Actions → Runners → New self-hosted runner → Windows x64**; follow GitHub's setup steps.
3. Assign the custom label **`inventor-2025`** to this runner. Default labels `self-hosted`, `Windows`, and `X64` are used too.
4. Confirm the machine has `C:\Program Files\Autodesk\Inventor 2025\Bin\Autodesk.Inventor.Interop.dll` and can run `dotnet build`.
5. Keep the runner online when creating release tags.
6. In the repository's Settings → Actions → General, permit GitHub Actions to create releases with the workflow's `contents: write` permission if your organization has restricted workflow permissions.

**Security:** GitHub cautions against self-hosted runners for public repositories. Use a separate, minimally privileged machine/VM, protect tag creation, review commits before tagging, and **never** trigger self-hosted builds for untrusted pull requests. This workflow intentionally runs self-hosted compilation only for explicit release tags or manual `workflow_dispatch`; PR checks run on GitHub-hosted runners. A personal workstation with important private data is not the recommended public repository runner. [GitHub security guidance](https://docs.github.com/en/actions/reference/security/secure-use).

### Release a version

Commit/push the files, then create a tag matching the `.csproj` `Version`:

```powershell
git add .
git commit -m 'Prepare Better Screenshot Tool v0.3.1'
git push origin main
git tag v0.3.1
git push origin v0.3.1
```

Check **Actions → Build and publish installer**. After both jobs complete, the `v0.3.1` GitHub Release contains `InventorBetterScreenshotTool-Setup-v0.3.1.exe`. For subsequent versions, increment the version in the `.csproj`, optionally update the Inno default `AppVersion`, and push a matching tag; the CI build checks tags for version mismatches.

The workflow can also be run manually from the **Actions** tab to test an installer; a manual run creates a downloadable **workflow artifact**, not a GitHub Release.

## Checklist before publishing a release

- Review the code and README to confirm the branding and ownership information are correct.
- Don't commit `Autodesk.Inventor.Interop.dll`, personal screenshots, CAD documents, screenshots from customers, secrets or Autodesk product binaries.
- Test the packaged installer and uninstaller on an actual Inventor 2025 installation before recommending it to users.
- Consider code-signing the `.exe` when distributing more broadly (the provided workflow does not sign binaries).

## Change history

See `CHANGELOG.md` for version history. v0.3.1 technical notes:

- Ribbon icons are opaque RGB 16px/32px variants passed to Inventor as 24bpp bitmaps, preventing alpha artifacts against the dark toolbar. Dialog header and window icon unchanged.
- Three-row root layout (header/content/footer) fixes header and control clipping.
- `PreviewCanvas` centres and sizes the image on every paint, including the first one.
- Neutral grey preview canvas with border and shadow makes the white PNG boundary visible.
- Installer and scripts keep the original add-in ID and directory, so older installations upgrade in place.

## Status / limitations

Structural validation only can run outside Windows. The DLL and installer must be compiled and tested with Inventor before release: run `install.ps1` locally, verify UI and capture per `docs/TEST_PLAN.md`, then tag. `scripts/validate_repository.py` validates XML/configuration consistency, not C# type checking.
