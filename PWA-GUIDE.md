# TailorBlend PWA Implementation Guide

Complete guide for the Progressive Web App (PWA) features in TailorBlend.

## Overview

TailorBlend is now a fully-featured Progressive Web App with:
- ✅ **Installable** on mobile and desktop devices
- ✅ **Offline support** with intelligent caching
- ✅ **Mobile-optimized** UI with touch gestures
- ✅ **iOS and Android** platform support
- ✅ **Service worker** for background sync
- ✅ **Network status** monitoring
- ✅ **App-like experience** when installed

## What's New

### PWA Features Added

1. **Web App Manifest** (`manifest.json`)
   - App name, description, and branding
   - Icon definitions for all platforms
   - Theme colors and display settings
   - App shortcuts for quick actions
   - Share target integration

2. **Service Worker** (`service-worker.js`)
   - Offline caching strategy
   - Background sync support
   - Push notification handlers
   - Automatic updates

3. **Install Prompt**
   - Custom installation banner
   - Smart dismissal (7-day cooldown)
   - Native install button integration

4. **Network Monitoring**
   - Offline/online status detection
   - Connection quality indicators
   - Auto-retry on reconnection
   - Sync pending changes

5. **Mobile Optimizations**
   - iOS safe area support (notches, home indicators)
   - Touch-optimized UI (44px minimum targets)
   - Prevent zoom on input focus
   - GPU-accelerated animations
   - Smooth scrolling with momentum

## Installation

### For Users

#### On Mobile (iOS)
1. Open https://your-domain.com in Safari
2. Tap the **Share** button (square with arrow)
3. Scroll and tap **Add to Home Screen**
4. Tap **Add** in the top right

#### On Mobile (Android)
1. Open the app in Chrome
2. Tap the **install prompt** banner at the bottom
3. Or tap menu (⋮) → **Install app**
4. Tap **Install** in the dialog

#### On Desktop (Chrome/Edge)
1. Open the app in Chrome or Edge
2. Click the **install icon** in the address bar
3. Or click menu → **Install TailorBlend**
4. Click **Install** in the dialog

### For Developers

All PWA files are in place. To enable PWA features in production:

1. **Generate Icons** (see `wwwroot/ICONS-README.md`)
   ```bash
   npm install -g pwa-asset-generator
   pwa-asset-generator source-icon.png wwwroot/icons --icon-only
   ```

2. **Configure HTTPS**
   - PWA requires HTTPS (except localhost)
   - Ensure SSL certificate is valid
   - Service worker will only register over HTTPS

3. **Deploy**
   ```bash
   dotnet publish -c Release
   # Deploy to your hosting platform
   ```

4. **Verify**
   - Open Chrome DevTools → Application
   - Check Manifest is loaded
   - Verify Service Worker is registered
   - Run Lighthouse PWA audit

## File Structure

```
wwwroot/
├── manifest.json                 # PWA manifest
├── service-worker.js            # Service worker
├── offline.html                 # Offline fallback page
├── browserconfig.xml            # Windows tiles
├── js/
│   ├── pwa-install.js          # Installation logic
│   └── network-status.js       # Network monitoring
├── icons/                       # App icons (to be generated)
│   ├── icon-16x16.png
│   ├── icon-32x32.png
│   ├── icon-192x192.png
│   ├── icon-512x512.png
│   └── ... (see ICONS-README.md)
└── splash/                      # iOS splash screens (to be generated)
    └── ... (see ICONS-README.md)
```

## Features in Detail

### 1. Offline Support

**What works offline:**
- Previously viewed pages
- Cached static assets (CSS, JS)
- App shell (navigation, layout)

**What doesn't work offline:**
- New chat messages (requires backend)
- Session stats updates
- File uploads

**User Experience:**
- Automatic offline page when disconnected
- Offline banner notification
- Auto-retry when connection restored
- Pending changes sync when back online

### 2. Installation

**Benefits of installing:**
- Standalone window (no browser UI)
- Quick access from home screen/desktop
- Better performance (pre-cached assets)
- Background sync support
- Push notifications (future)

**Installation prompts:**
- Shows after ~30 seconds on first visit
- Only on HTTPS
- Only if not already installed
- Can be dismissed for 7 days

### 3. Caching Strategy

**Cache-first for:**
- Static assets (CSS, JS, fonts)
- Icons and images
- Offline fallback page

**Network-first for:**
- API requests
- SignalR connections
- Dynamic content

**Never cached:**
- POST requests
- API calls
- Real-time chat streams

### 4. Mobile Optimizations

**Touch Gestures:**
- Minimum 44px touch targets
- Active state feedback
- Prevent accidental zooms
- Smooth momentum scrolling

**iOS Specific:**
- Safe area insets for notches
- Status bar theming
- Home indicator spacing
- Splash screens

**Android Specific:**
- Adaptive icons (maskable)
- Theme color in status bar
- Install banner
- Share target

**Performance:**
- GPU acceleration on animations
- Lazy loading images
- Code splitting
- Compressed assets

### 5. Network Monitoring

**Features:**
- Online/offline detection
- Connection quality indicators
- Slow connection warnings
- Auto-sync on reconnect

**User Feedback:**
- Persistent offline banner
- Connection status notifications
- Pending changes indicator

## Configuration

### Manifest Settings

Edit `wwwroot/manifest.json`:

```json
{
  "name": "TailorBlend AI Consultant",
  "short_name": "TailorBlend",
  "theme_color": "#70d1c7",
  "background_color": "#0b1220",
  "display": "standalone",
  "start_url": "/"
}
```

**Key properties:**
- `name`: Full app name (45 chars max recommended)
- `short_name`: Home screen name (12 chars max)
- `theme_color`: Browser UI color
- `background_color`: Splash screen background
- `display`: "standalone" (app-like) or "browser"
- `start_url`: Opens when launched

### Service Worker Cache

Edit `wwwroot/service-worker.js`:

```javascript
const CACHE_NAME = 'tailorblend-v1'; // Increment to force update

const STATIC_CACHE_URLS = [
    '/',
    '/chat',
    '/css/tailorblend.css',
    // Add more URLs to cache
];
```

**Cache versioning:**
- Increment `CACHE_NAME` to deploy updates
- Old caches are automatically cleared
- Users get update notification

## Testing

### Local Testing

1. **Start development server:**
   ```bash
   dotnet run --project BlazorConsultant
   ```

2. **Open Chrome DevTools:**
   - Application → Manifest
   - Application → Service Workers
   - Lighthouse → PWA Audit

3. **Test offline:**
   - DevTools → Network → Offline
   - Verify offline page appears
   - Check cached assets load

### Production Testing

1. **Lighthouse PWA Audit:**
   ```bash
   # In Chrome DevTools
   Lighthouse → PWA → Generate Report
   ```

   **Target scores:**
   - Installable: ✓
   - PWA Optimized: ✓
   - Fast and reliable: 90+
   - Works offline: ✓

2. **Real Device Testing:**
   - Test on actual iOS device (Safari)
   - Test on actual Android device (Chrome)
   - Verify install process
   - Check offline functionality

### Common Issues

**Service Worker not registering:**
- Check HTTPS is enabled (except localhost)
- Verify `/service-worker.js` is accessible
- Check browser console for errors
- Clear browser cache and reload

**Icons not showing:**
- Ensure icons exist at specified paths
- Check icon sizes match manifest
- Verify MIME types (image/png)
- Use absolute paths in manifest

**Install prompt not showing:**
- Wait ~30 seconds after page load
- Only works on HTTPS
- Won't show if already installed
- Check `beforeinstallprompt` event fires

**Offline page not working:**
- Verify `/offline.html` exists
- Check it's in cache list
- Test with DevTools offline mode
- Check service worker status

## Performance Optimizations

### Already Implemented

✅ **GPU Acceleration**
- `transform: translateZ(0)` on animated elements
- Hardware-accelerated CSS properties

✅ **Layout Optimization**
- CSS `contain` property on stable elements
- Prevent layout shifts

✅ **Touch Optimization**
- 44px minimum touch targets
- Disable text selection on UI elements
- Prevent iOS zoom on inputs

✅ **Accessibility**
- Respects prefers-reduced-motion
- High contrast mode support
- Keyboard navigation focus states

✅ **Network Optimization**
- Intelligent caching strategy
- Lazy load images
- Compress assets

### Future Optimizations

🔄 **Code Splitting**
```csharp
// Lazy load pages
@page "/practitioner-blend"
@attribute [Authorize]
@* Load only when needed *@
```

🔄 **Image Optimization**
```html
<!-- WebP with fallback -->
<picture>
  <source srcset="image.webp" type="image/webp">
  <img src="image.jpg" alt="...">
</picture>
```

🔄 **Resource Hints**
```html
<link rel="preconnect" href="https://api.tailorblend.com">
<link rel="dns-prefetch" href="https://fonts.googleapis.com">
```

## Security Considerations

### Service Worker Scope

Service worker runs at `/` scope:
- Can intercept all requests
- Use HTTPS to prevent man-in-the-middle
- Validate cached responses

### Content Security Policy

Add to `_Host.cshtml`:
```html
<meta http-equiv="Content-Security-Policy"
      content="default-src 'self';
               script-src 'self' 'unsafe-inline' 'unsafe-eval';
               style-src 'self' 'unsafe-inline';">
```

### Sensitive Data

❌ **Never cache:**
- User credentials
- API keys
- Personal health data
- Session tokens

✅ **Safe to cache:**
- Static assets (CSS, JS)
- Public pages
- App icons
- Offline fallback

## Monitoring & Analytics

### Track PWA Events

```javascript
// Track installation
window.addEventListener('appinstalled', () => {
    gtag('event', 'pwa_installed');
});

// Track standalone mode
if (window.matchMedia('(display-mode: standalone)').matches) {
    gtag('event', 'pwa_standalone');
}
```

### Service Worker Updates

```javascript
// Track service worker updates
registration.addEventListener('updatefound', () => {
    console.log('New service worker available');
    // Show update notification to user
});
```

## Deployment Checklist

Before deploying PWA features:

- [ ] Generate all required icons
- [ ] Create iOS splash screens
- [ ] Test on real iOS device
- [ ] Test on real Android device
- [ ] Enable HTTPS in production
- [ ] Configure correct domain in manifest
- [ ] Run Lighthouse PWA audit (score 90+)
- [ ] Test offline functionality
- [ ] Test install/uninstall process
- [ ] Verify service worker updates
- [ ] Check console for errors
- [ ] Test on slow 3G connection
- [ ] Verify safe areas on notched devices

## Browser Support

### Fully Supported
- ✅ Chrome 90+ (Android, Desktop)
- ✅ Edge 90+ (Desktop)
- ✅ Safari 15+ (iOS, macOS)
- ✅ Samsung Internet 14+

### Partial Support
- ⚠️ Firefox (no install prompt, but works)
- ⚠️ Opera (limited manifest support)

### Not Supported
- ❌ Internet Explorer (use Edge)

## Troubleshooting

### Clear PWA Cache

**Chrome:**
1. DevTools → Application → Storage
2. Click "Clear site data"
3. Reload page

**iOS Safari:**
1. Settings → Safari → Clear History and Website Data
2. Re-open app

**Force Service Worker Update:**
```javascript
// In browser console
navigator.serviceWorker.getRegistrations()
    .then(registrations => {
        registrations.forEach(registration => {
            registration.unregister();
        });
    });
```

### Debug Service Worker

```javascript
// Check service worker status
navigator.serviceWorker.getRegistrations()
    .then(registrations => {
        console.log('Registered:', registrations.length);
        registrations.forEach(reg => {
            console.log('Scope:', reg.scope);
            console.log('Active:', reg.active?.state);
        });
    });
```

## Resources

### Documentation
- [MDN: Progressive Web Apps](https://developer.mozilla.org/en-US/docs/Web/Progressive_web_apps)
- [web.dev: PWA](https://web.dev/progressive-web-apps/)
- [Apple: PWA Guidelines](https://developer.apple.com/library/archive/documentation/AppleApplications/Reference/SafariWebContent/ConfiguringWebApplications/ConfiguringWebApplications.html)

### Tools
- [Lighthouse](https://developers.google.com/web/tools/lighthouse)
- [PWA Builder](https://www.pwabuilder.com/)
- [Workbox](https://developers.google.com/web/tools/workbox) (future enhancement)

### Testing
- [Chrome DevTools: PWA](https://developers.google.com/web/tools/chrome-devtools/progressive-web-apps)
- [BrowserStack](https://www.browserstack.com/) (real device testing)
- [LambdaTest](https://www.lambdatest.com/) (cross-browser testing)

## Support

For issues or questions about PWA implementation:

1. Check browser console for errors
2. Review this guide's troubleshooting section
3. Test on different devices/browsers
4. Check service worker registration status
5. Verify HTTPS is enabled

## Next Steps

After completing PWA setup:

1. ✅ Generate production icons
2. ✅ Test on real devices
3. ✅ Deploy with HTTPS
4. 🔄 Set up push notifications
5. 🔄 Add background sync for messages
6. 🔄 Implement app shortcuts
7. 🔄 Add share target functionality

---

**Last Updated:** 2025-10-24
**PWA Version:** 1.0
**Blazor Version:** .NET 8
