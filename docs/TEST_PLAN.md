# Inventor 2025.2.1 v0.3.1 test plan

The release is not validated until checked manually on Inventor 2025.2.1 (Build 293).

## UI acceptance

- [ ] Run `scripts/install.ps1` with Inventor closed. Build succeeds.
- [ ] In a saved `.ipt`, open **Better Screenshots > Capture > Better Screenshot**.
- [ ] Header fully visible; nothing overlaps it at first render.
- [ ] Preview header's Refresh button appears at top right without resizing.
- [ ] Preview is visible and geometrically centred immediately on open.
- [ ] Grey background and thin border make the white PNG edges distinguishable.
- [ ] Resize wide/tall and verify centring, header and footer alignment.
- [ ] Test Windows display scaling at 100%, 125%, 150%; settings remain reachable by scrolling.
- [ ] Confirm footer text and calumk.com link are visible and functional.

## Functional acceptance

- [ ] Preview does **not** create a numbered PNG.
- [ ] 1920x1080 and 2048x2048 renders respect selected aspect ratio.
- [ ] Refresh works after dimension changes; Save PNG produces full output.
- [ ] Default folder is `Screenshots` adjacent to active `.ipt` / `.iam`.
- [ ] Sequential filenames increment, no existing PNG overwritten.
- [ ] Fixed-folder option still works; Open Folder opens last save.
- [ ] Original background mode is restored after successful and failed exports.
- [ ] An unsaved model results in a clear error without a leftover temporary PNG.
- [ ] Preferences survive restarting Inventor and upgrading from v0.2.0.

## Installer and GitHub

- [ ] Build `scripts/package.ps1`, check output contains no Autodesk DLLs.
- [ ] Build Inno Setup `.exe`, install over existing v0.2.0, verify single add-in entry.
- [ ] Settings and prior screenshots preserved after install, upgrade and uninstall.
- [ ] Installer refuses to proceed while Inventor is running, without force-closing it.
- [ ] New install with no old source installation works for a non-admin Windows user.
- [ ] GitHub-hosted static workflow completes.
- [ ] Self-hosted runner builds tagged release, GitHub-hosted packaging produces executable.
- [ ] Installer downloaded from GitHub Release works on another Inventor 2025 machine.

## v0.3.1 ribbon icon

- [ ] Inventor Part and Assembly ribbon buttons show blue icon on a solid white square (no transparent background).
- [ ] No checkerboard, grey alpha halo, or jagged transparent edges on the dark ribbon.
- [ ] The navy dialog header icon still has its original transparent background.
