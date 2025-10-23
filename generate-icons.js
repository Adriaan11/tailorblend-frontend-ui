#!/usr/bin/env node

/**
 * Generate PWA icons for TailorBlend
 * Creates 192x192 and 512x512 PNG icons
 */

const fs = require('fs');
const path = require('path');

// Try to use canvas package if available
let Canvas;
try {
    Canvas = require('canvas');
    console.log('Using canvas package');
} catch (e) {
    console.log('canvas package not available, creating SVG icons instead');
}

const outputDir = path.join(__dirname, 'BlazorConsultant', 'wwwroot');

function generateSVGIcon(size) {
    const svg = `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 ${size} ${size}">
  <rect width="${size}" height="${size}" fill="#594ae2" rx="${size * 0.2}"/>
  <text x="${size / 2}" y="${size / 2 + size * 0.15}" font-family="Arial, sans-serif" font-size="${size * 0.55}" font-weight="bold" fill="white" text-anchor="middle">TB</text>
</svg>`;

    const filename = `icon-${size}.svg`;
    const filepath = path.join(outputDir, filename);
    fs.writeFileSync(filepath, svg);
    console.log(`✓ Created ${filename}`);
}

function generateCanvasIcon(size) {
    if (!Canvas) {
        throw new Error('Canvas not available');
    }

    const canvas = Canvas.createCanvas(size, size);
    const ctx = canvas.getContext('2d');

    // Draw rounded rectangle background
    const radius = size * 0.2;
    ctx.fillStyle = '#594ae2';
    ctx.beginPath();
    ctx.moveTo(radius, 0);
    ctx.lineTo(size - radius, 0);
    ctx.arcTo(size, 0, size, radius, radius);
    ctx.lineTo(size, size - radius);
    ctx.arcTo(size, size, size - radius, size, radius);
    ctx.lineTo(radius, size);
    ctx.arcTo(0, size, 0, size - radius, radius);
    ctx.lineTo(0, radius);
    ctx.arcTo(0, 0, radius, 0, radius);
    ctx.closePath();
    ctx.fill();

    // Draw text
    ctx.fillStyle = 'white';
    ctx.font = `bold ${size * 0.55}px Arial`;
    ctx.textAlign = 'center';
    ctx.textBaseline = 'middle';
    ctx.fillText('TB', size / 2, size / 2);

    // Save as PNG
    const filename = `icon-${size}.png`;
    const filepath = path.join(outputDir, filename);
    const buffer = canvas.toBuffer('image/png');
    fs.writeFileSync(filepath, buffer);
    console.log(`✓ Created ${filename}`);
}

// Generate icons
console.log('Generating PWA icons for TailorBlend...\n');

const sizes = [192, 512];

if (Canvas) {
    // Generate PNG icons using canvas
    sizes.forEach(size => {
        try {
            generateCanvasIcon(size);
        } catch (e) {
            console.error(`✗ Failed to create icon-${size}.png:`, e.message);
            console.log(`  Falling back to SVG for ${size}x${size}`);
            generateSVGIcon(size);
        }
    });
} else {
    // Generate SVG icons as fallback
    console.log('Note: Install "canvas" package for PNG generation');
    console.log('  npm install canvas\n');
    console.log('Generating SVG icons instead...\n');
    sizes.forEach(generateSVGIcon);
    console.log('\nTo convert SVG to PNG:');
    console.log('1. Open BlazorConsultant/wwwroot/generate-icons.html in browser');
    console.log('2. Click download buttons');
    console.log('3. Save files to BlazorConsultant/wwwroot/');
}

console.log('\nDone! PWA icons generated in', outputDir);
console.log('\nNext steps:');
console.log('1. Verify icons exist: ls -lh BlazorConsultant/wwwroot/icon-*');
console.log('2. Run app: dotnet run --project BlazorConsultant');
console.log('3. Open http://localhost:8080');
console.log('4. Look for install button in browser address bar');
