#!/usr/bin/env python3
"""
TailorBlend PWA Icon Generator
Generates all required PWA icons with TB monogram and teal gradient
"""

import os
from pathlib import Path

try:
    from PIL import Image, ImageDraw, ImageFont
except ImportError:
    print("❌ Pillow package not found. Installing...")
    print("Run: pip install Pillow")
    exit(1)

# Icon configurations
ICON_CONFIGS = [
    {"size": 16, "name": "favicon-16.png"},
    {"size": 32, "name": "favicon-32.png"},
    {"size": 180, "name": "apple-touch-icon.png"},
    {"size": 192, "name": "icon-192.png"},
    {"size": 512, "name": "icon-512.png"},
]

# Colors
COLOR_START = "#70D1C7"
COLOR_END = "#5bbfb5"
COLOR_TEXT = "#FFFFFF"

# Convert hex to RGB
def hex_to_rgb(hex_color):
    hex_color = hex_color.lstrip('#')
    return tuple(int(hex_color[i:i+2], 16) for i in (0, 2, 4))

def interpolate_color(color1, color2, factor):
    """Interpolate between two colors"""
    r1, g1, b1 = hex_to_rgb(color1)
    r2, g2, b2 = hex_to_rgb(color2)

    r = int(r1 + (r2 - r1) * factor)
    g = int(g1 + (g2 - g1) * factor)
    b = int(b1 + (b2 - b1) * factor)

    return (r, g, b)

def create_gradient_background(size):
    """Create a diagonal gradient background"""
    img = Image.new('RGBA', (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    # Draw gradient line by line
    for y in range(size):
        factor = y / size
        color = interpolate_color(COLOR_START, COLOR_END, factor)
        draw.line([(0, y), (size, y)], fill=color)

    return img

def add_rounded_corners(img, radius):
    """Add rounded corners to an image"""
    size = img.size[0]

    # Create a mask for rounded corners
    mask = Image.new('L', (size, size), 0)
    draw = ImageDraw.Draw(mask)

    # Draw rounded rectangle on mask
    draw.rounded_rectangle([(0, 0), (size, size)], radius=radius, fill=255)

    # Apply mask
    img.putalpha(mask)
    return img

def draw_text(img, text, size):
    """Draw centered text on image"""
    draw = ImageDraw.Draw(img)

    # Calculate font size (45% of image size)
    font_size = int(size * 0.45)

    # Try to use a system font, fallback to default
    try:
        # macOS
        font = ImageFont.truetype("/System/Library/Fonts/Supplemental/Arial Bold.ttf", font_size)
    except:
        try:
            # Linux
            font = ImageFont.truetype("/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf", font_size)
        except:
            try:
                # Windows
                font = ImageFont.truetype("C:\\Windows\\Fonts\\arialbd.ttf", font_size)
            except:
                # Fallback to default
                font = ImageFont.load_default()

    # Get text bounding box
    bbox = draw.textbbox((0, 0), text, font=font)
    text_width = bbox[2] - bbox[0]
    text_height = bbox[3] - bbox[1]

    # Calculate centered position
    x = (size - text_width) // 2
    y = (size - text_height) // 2 - bbox[1]  # Adjust for baseline

    # Draw white text
    draw.text((x, y), text, fill=hex_to_rgb(COLOR_TEXT), font=font)

def generate_icon(config, output_dir):
    """Generate a single icon"""
    size = config["size"]
    name = config["name"]

    # Create gradient background
    img = create_gradient_background(size)

    # Add rounded corners (20% radius)
    radius = int(size * 0.2)
    img = add_rounded_corners(img, radius)

    # Draw "TB" text
    draw_text(img, "TB", size)

    # Save icon
    output_path = output_dir / name
    img.save(output_path, "PNG")

    print(f"✅ Generated {name} ({size}x{size})")

def main():
    # Get output directory
    script_dir = Path(__file__).parent
    icons_dir = script_dir.parent / "icons"

    # Create icons directory if it doesn't exist
    icons_dir.mkdir(exist_ok=True)
    print(f"✅ Icons directory: {icons_dir}\n")

    print("🎨 Generating TailorBlend PWA icons...\n")

    # Generate all icons
    for config in ICON_CONFIGS:
        generate_icon(config, icons_dir)

    print(f"\n✅ All icons generated successfully!")
    print(f"📁 Location: {icons_dir}")

if __name__ == "__main__":
    main()
