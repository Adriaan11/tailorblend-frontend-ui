# PWA Setup Guide

TailorBlend is now configured as a **Progressive Web App (PWA)** and can be installed on desktop and mobile devices.

## What's Included

✅ Web App Manifest (`wwwroot/manifest.json`)
✅ Minimal Service Worker (`wwwroot/service-worker.js`)
✅ PWA meta tags in `_Host.cshtml`
✅ Service worker registration
✅ Icon generation tool

## Important Notes

- **No offline functionality**: Blazor Server requires an active SignalR connection
- **Installation only**: Users can install the app for easy access
- **HTTPS required**: PWA features only work over HTTPS (production)

## Generate Icons

### Option 1: Using Browser (Recommended)

1. Start the app: `dotnet run --project BlazorConsultant`
2. Open: http://localhost:8080/generate-icons.html
3. Click "Download 192x192 Icon" and "Download 512x512 Icon"
4. Save files as:
   - `BlazorConsultant/wwwroot/icon-192.png`
   - `BlazorConsultant/wwwroot/icon-512.png`

### Option 2: Using Design Tool

Create two PNG icons with the TailorBlend logo:
- **192x192 pixels** (required for Android)
- **512x512 pixels** (required for splash screen)

Place them in `BlazorConsultant/wwwroot/` as:
- `icon-192.png`
- `icon-512.png`

### Option 3: Use SVG (Temporary)

A placeholder SVG is available at `wwwroot/icon.svg`. Convert it to PNG using:

```bash
# Using ImageMagick (if available)
convert -background none -size 192x192 wwwroot/icon.svg wwwroot/icon-192.png
convert -background none -size 512x512 wwwroot/icon.svg wwwroot/icon-512.png

# Using Inkscape (if available)
inkscape icon.svg --export-filename=icon-192.png --export-width=192
inkscape icon.svg --export-filename=icon-512.png --export-width=512
```

## Testing PWA Installation

### Desktop (Chrome/Edge)

1. Run the app: `dotnet run --project BlazorConsultant`
2. Open http://localhost:8080
3. Look for the install button (⊕) in the address bar
4. Click to install
5. App opens in standalone window

**Note**: Some browsers require HTTPS for installation. For local testing:

```bash
# Use HTTPS locally
dotnet run --project BlazorConsultant --launch-profile https
```

### Mobile (iOS Safari)

1. Open http://localhost:8080 (or production URL)
2. Tap the Share button
3. Scroll down and tap "Add to Home Screen"
4. Tap "Add"

### Mobile (Android Chrome)

1. Open http://localhost:8080 (or production URL)
2. Tap the menu (⋮)
3. Tap "Install app" or "Add to Home Screen"
4. Tap "Install"

## Verify Service Worker

1. Open DevTools (F12)
2. Go to "Application" tab
3. Click "Service Workers" in sidebar
4. Should see: `service-worker.js` with status "activated"

## Manifest Validation

Check your manifest configuration:

1. DevTools → Application → Manifest
2. Should show:
   - Name: TailorBlend AI Consultant
   - Short name: TailorBlend
   - Start URL: /
   - Display: standalone
   - Theme color: #594ae2
   - Icons: 192x192 and 512x512

## Production Deployment

### Requirements

- **HTTPS required** (PWA won't work over HTTP in production)
- Icons must be accessible at `/icon-192.png` and `/icon-512.png`
- Manifest must be accessible at `/manifest.json`
- Service worker must be accessible at `/service-worker.js`

### Deploy to fly.io

```bash
fly deploy
```

The Dockerfile already serves static files from wwwroot, so PWA files will be included automatically.

### Testing Production Install

1. Visit your production URL (https://your-app.fly.dev)
2. Look for browser install prompt
3. Install and test

## Customization

### Change App Colors

Edit `wwwroot/manifest.json`:

```json
{
  "theme_color": "#your-primary-color",
  "background_color": "#your-background-color"
}
```

Also update `_Host.cshtml`:

```html
<meta name="theme-color" content="#your-primary-color" />
```

### Change App Name

Edit `wwwroot/manifest.json`:

```json
{
  "name": "Your Full App Name",
  "short_name": "Short Name",
  "description": "Your app description"
}
```

### Add Offline Support (Advanced)

If you want basic offline capabilities:

1. Edit `wwwroot/service-worker.js`
2. Add cache strategies for static assets
3. Note: Blazor Server SignalR will still require connection

## Troubleshooting

### "Service Worker registration failed"

- Check browser console for errors
- Ensure service-worker.js is accessible at `/service-worker.js`
- HTTPS required in production

### "Install prompt doesn't appear"

- Verify manifest.json is accessible
- Check icons exist (192x192 and 512x512)
- Clear browser cache and reload
- Some browsers require HTTPS even for localhost

### "Icons not showing"

- Generate icons using `generate-icons.html`
- Place in `BlazorConsultant/wwwroot/`
- Clear cache and reinstall

### "App doesn't work offline"

- Expected behavior for Blazor Server
- Requires active connection for SignalR
- Only installation is supported, not offline functionality

## Further Reading

- [MDN: Progressive Web Apps](https://developer.mozilla.org/en-US/docs/Web/Progressive_web_apps)
- [Web App Manifest Spec](https://w3c.github.io/manifest/)
- [Service Worker API](https://developer.mozilla.org/en-US/docs/Web/API/Service_Worker_API)
- [Blazor Server Hosting](https://learn.microsoft.com/en-us/aspnet/core/blazor/host-and-deploy/server)
