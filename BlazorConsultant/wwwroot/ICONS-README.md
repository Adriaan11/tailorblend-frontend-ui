# TailorBlend PWA Icons & Assets

This document explains how to generate the required PWA icons and splash screens for TailorBlend.

## Required Icons

The following icon sizes need to be created in the `/wwwroot/icons/` directory:

### Standard Icons (PNG)
- `icon-16x16.png` - Browser favicon
- `icon-32x32.png` - Browser favicon
- `icon-57x57.png` - iOS legacy
- `icon-60x60.png` - iOS
- `icon-72x72.png` - iPad
- `icon-76x76.png` - iPad
- `icon-96x96.png` - Android
- `icon-114x114.png` - iOS retina
- `icon-120x120.png` - iOS retina
- `icon-128x128.png` - Chrome Web Store
- `icon-144x144.png` - Windows tile
- `icon-152x152.png` - iPad retina
- `icon-180x180.png` - iOS retina
- `icon-192x192.png` - Android (recommended size)
- `icon-384x384.png` - Android
- `icon-512x512.png` - Android (recommended size)

### Maskable Icons
For better integration with Android adaptive icons:
- Use the same 192x192 and 512x512 images
- Ensure important content is within the safe zone (80% of canvas)
- Avoid text or logos near edges

## Icon Design Guidelines

### Brand Identity
- **Primary Color**: `#70d1c7` (Teal)
- **Secondary Color**: `#5bbfb5` (Darker Teal)
- **Background**: Gradient from `#70d1c7` to `#5bbfb5` (135deg)
- **Symbol**: Flower/leaf icon (🌿 or similar botanical element)

### Design Specifications
1. **Margins**: 10% safe area on all sides
2. **Shape**: Rounded corners (16px radius for 512x512)
3. **Style**: Modern, flat design with subtle gradient
4. **Icon**: White or light-colored botanical symbol centered

### Example Design (SVG Template)
```svg
<svg width="512" height="512" xmlns="http://www.w3.org/2000/svg">
  <defs>
    <linearGradient id="bg" x1="0%" y1="0%" x2="100%" y2="100%">
      <stop offset="0%" style="stop-color:#70d1c7;stop-opacity:1" />
      <stop offset="100%" style="stop-color:#5bbfb5;stop-opacity:1" />
    </linearGradient>
  </defs>
  <!-- Background with rounded corners -->
  <rect width="512" height="512" rx="64" fill="url(#bg)"/>
  <!-- Icon placeholder - replace with botanical symbol -->
  <circle cx="256" cy="256" r="120" fill="white" opacity="0.9"/>
</svg>
```

## How to Generate Icons

### Option 1: Using PWA Asset Generator (Recommended)
```bash
npm install -g pwa-asset-generator

# Generate all icons and splash screens from a single 512x512 source
pwa-asset-generator source-icon.png wwwroot/icons \
  --icon-only \
  --favicon \
  --opaque false \
  --padding "10%" \
  --background "#70d1c7"
```

### Option 2: Using ImageMagick
```bash
# Install ImageMagick
# macOS: brew install imagemagick
# Linux: sudo apt-get install imagemagick

# Generate icons from source
for size in 16 32 57 60 72 76 96 114 120 128 144 152 180 192 384 512; do
  convert source-icon.png -resize ${size}x${size} wwwroot/icons/icon-${size}x${size}.png
done
```

### Option 3: Online Tools
- **RealFaviconGenerator**: https://realfavicongenerator.net/
- **PWA Builder**: https://www.pwabuilder.com/imageGenerator
- **Favicon.io**: https://favicon.io/

## iOS Splash Screens

Create splash screens in `/wwwroot/splash/` directory:

### Required Sizes
- `apple-splash-2048-2732.jpg` - iPad Pro 12.9" (2048x2732)
- `apple-splash-1668-2388.jpg` - iPad Pro 11" (1668x2388)
- `apple-splash-1536-2048.jpg` - iPad (1536x2048)
- `apple-splash-1125-2436.jpg` - iPhone X/XS (1125x2436)
- `apple-splash-1242-2688.jpg` - iPhone XS Max (1242x2688)
- `apple-splash-750-1334.jpg` - iPhone 8 (750x1334)
- `apple-splash-640-1136.jpg` - iPhone SE (640x1136)

### Splash Screen Design
1. **Background**: Dark gradient (`#0b1220` to `#1f2937`)
2. **Logo**: Centered TailorBlend icon + wordmark
3. **Theme**: Match app's dark theme for consistency
4. **Status Bar**: Account for safe areas

### Generate Splash Screens
```bash
pwa-asset-generator source-icon.png wwwroot/splash \
  --splash-only \
  --background "#0b1220" \
  --quality 90 \
  --type jpg
```

## Screenshots for App Stores

Create screenshots in `/wwwroot/screenshots/`:

### Mobile Screenshot (540x720)
- `mobile-1.png` - Chat interface on mobile
- Show: Chat conversation with AI responses
- Theme: Use current theme (light or dark)

### Desktop Screenshot (1280x720)
- `desktop-1.png` - Full app view on desktop
- Show: Sidebar + chat + settings panel
- Theme: Use current theme (light or dark)

## Quick Setup Script

Create all placeholder icons (temporary until real icons are designed):

```bash
#!/bin/bash
# create-placeholder-icons.sh

mkdir -p wwwroot/icons wwwroot/splash wwwroot/screenshots

# Create simple colored placeholder (requires ImageMagick)
convert -size 512x512 gradient:"#70d1c7-#5bbfb5" \
  -gravity center \
  -pointsize 200 \
  -fill white \
  -annotate +0+0 "TB" \
  source-icon.png

# Generate all icon sizes
for size in 16 32 57 60 72 76 96 114 120 128 144 152 180 192 384 512; do
  convert source-icon.png -resize ${size}x${size} wwwroot/icons/icon-${size}x${size}.png
done

echo "✅ Icons generated successfully!"
```

## Favicon

The `icon-32x32.png` and `icon-16x16.png` will be used as the browser favicon automatically.

## Testing Icons

### Test Locally
1. Build and run the app: `dotnet run`
2. Open Chrome DevTools → Application → Manifest
3. Verify all icons are loaded correctly

### Test PWA Installation
1. Open app in Chrome (mobile or desktop)
2. Click install prompt
3. Verify icon appears correctly on home screen/desktop

### Test on iOS
1. Open app in Safari on iPhone/iPad
2. Tap Share → Add to Home Screen
3. Verify icon and splash screen appear correctly

## Icon Validation

Use these tools to validate your icons:
- **Lighthouse**: Chrome DevTools → Lighthouse → PWA audit
- **PWA Builder**: https://www.pwabuilder.com/
- **Favicon Checker**: https://realfavicongenerator.net/favicon_checker

## Notes

- Icons should be **opaque** (no transparency) for iOS
- Use **maskable** icons for better Android integration
- Keep **safe zone** margins (10-20% on all sides)
- Test on both **light and dark** backgrounds
- Compress images to reduce load time (use **TinyPNG** or **ImageOptim**)

## Current Status

⚠️ **Action Required**: Icons need to be generated using the instructions above.

Until then, the app will use default browser icons. The PWA functionality will still work, but the app icon will not display correctly when installed.
