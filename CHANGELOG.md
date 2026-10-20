# Changelog

## 0.3.1 — solid white ribbon icon

- Replaced the embedded 16px and 32px transparent ribbon PNGs with opaque RGB white-backed variants of Phosphor Cube Focus Duotone.
- Explicitly create opaque 24bpp bitmaps before conversion to Inventor IPictureDisp, avoiding alpha-edge artifacts on the dark ribbon.
- Kept the 48px dialog banner graphic and ICO unchanged.
- Retained add-in GUID, update path, screenshot outputs, preferences and features.

## 0.3.0 — source release candidate

- Prevented export controls from hiding under the top banner by using fixed header, content and footer table rows.
- Moved preview into a paint-centred panel that does not require a resize to centre PNGs.
- Replaced white preview card with grey canvas, bordered image and subtle shadow.
- Improved preview-button layout, spacing and footer.
- Added installer (Inno Setup), GitHub Actions release workflow, static checks, staged build script, and docs.
- Kept the add-in GUID, output folders, naming scheme and settings format unchanged.

## 0.2.0

- Branded UI, preview, Phosphor icon, website link.

## 0.1.2

- Fixed ambiguous Inventor/System.IO references.
