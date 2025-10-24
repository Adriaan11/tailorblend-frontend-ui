#!/usr/bin/env node

/**
 * TailorBlend PWA Icon Generator
 * Generates all required PWA icons with TB monogram and teal gradient
 */

const fs = require('fs');
const path = require('path');

// Check if we're running in Node.js with canvas support
let Canvas;
try {
    Canvas = require('canvas');
} catch (err) {
    console.error('❌ canvas package not found. Installing...');
    console.error('Run: npm install canvas');
    process.exit(1);
}

const { createCanvas } = Canvas;

// Icon configurations
const iconConfigs = [
    { size: 16, name: 'favicon-16.png' },
    { size: 32, name: 'favicon-32.png' },
    { size: 180, name: 'apple-touch-icon.png' },
    { size: 192, name: 'icon-192.png' },
    { size: 512, name: 'icon-512.png' },
];

// Output directory
const iconsDir = path.join(__dirname, '..', 'icons');

// Ensure icons directory exists
if (!fs.existsSync(iconsDir)) {
    fs.mkdirSync(iconsDir, { recursive: true });
    console.log('✅ Created icons directory');
}

function drawIcon(canvas, size) {
    const ctx = canvas.getContext('2d');

    // Create gradient background (TailorBlend teal)
    const gradient = ctx.createLinearGradient(0, 0, size, size);
    gradient.addColorStop(0, '#70D1C7');
    gradient.addColorStop(1, '#5bbfb5');

    // Draw background with rounded corners (20% radius)
    const radius = size * 0.2;
    ctx.fillStyle = gradient;
    ctx.beginPath();
    ctx.moveTo(radius, 0);
    ctx.lineTo(size - radius, 0);
    ctx.quadraticCurveTo(size, 0, size, radius);
    ctx.lineTo(size, size - radius);
    ctx.quadraticCurveTo(size, size, size - radius, size);
    ctx.lineTo(radius, size);
    ctx.quadraticCurveTo(0, size, 0, size - radius);
    ctx.lineTo(0, radius);
    ctx.quadraticCurveTo(0, 0, radius, 0);
    ctx.closePath();
    ctx.fill();

    // Draw "TB" text
    ctx.fillStyle = '#FFFFFF';
    ctx.font = `bold ${size * 0.45}px Arial, sans-serif`;
    ctx.textAlign = 'center';
    ctx.textBaseline = 'middle';
    ctx.fillText('TB', size / 2, size / 2);
}

function generateIcon(config) {
    const canvas = createCanvas(config.size, config.size);
    drawIcon(canvas, config.size);

    const outputPath = path.join(iconsDir, config.name);
    const buffer = canvas.toBuffer('image/png');
    fs.writeFileSync(outputPath, buffer);

    console.log(`✅ Generated ${config.name} (${config.size}x${config.size})`);
}

// Generate all icons
console.log('🎨 Generating TailorBlend PWA icons...\n');

iconConfigs.forEach(config => {
    generateIcon(config);
});

console.log(`\n✅ All icons generated successfully!`);
console.log(`📁 Location: ${iconsDir}`);
