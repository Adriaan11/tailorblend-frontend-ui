using MudBlazor;

namespace BlazorConsultant;

/// <summary>
/// TailorBlend custom MudBlazor theme configuration.
/// Brand colors: Purple (primary) and Green (secondary) for health/wellness.
/// </summary>
public static class TailorBlendTheme
{
    public static MudTheme Theme => new()
    {
        PaletteLight = new PaletteLight
        {
            // Primary palette — TailorBlend teal (brand color)
            Primary = "#70D1C7",
            PrimaryDarken = "#5BBFB5",
            PrimaryLighten = "#8ADDD5",

            // Secondary palette — modern indigo accent
            Secondary = "#6366F1",
            SecondaryDarken = "#4F46E5",
            SecondaryLighten = "#A5B4FC",

            Tertiary = "#14B8A6",
            TertiaryDarken = "#0D9488",
            TertiaryLighten = "#2DD4BF",

            // Semantic colors aligned with refreshed UI
            Success = "#70D1C7",
            Info = "#3B82F6",
            Warning = "#F59E0B",
            Error = "#EF4444",

            // Surface system
            Background = "#F5F5F5",
            Surface = "#FFFFFF",
            AppbarBackground = "#FFFFFF",
            AppbarText = "#111827",
            DrawerBackground = "#FFFFFF",
            DrawerText = "#111827",
            DrawerIcon = "#6B7280",

            // Typography colors
            TextPrimary = "#111827",
            TextSecondary = "#4B5563",
            TextDisabled = "#CBD5F5",

            // Action states & dividers
            ActionDefault = "#9CA3AF",
            ActionDisabled = "#E5E7EB",
            ActionDisabledBackground = "#E5E7EB",
            Divider = "rgba(17, 24, 39, 0.08)",
            DividerLight = "rgba(17, 24, 39, 0.04)",

            TableLines = "rgba(17, 24, 39, 0.06)",
            TableStriped = "#F9FAFB",
            TableHover = "rgba(112, 209, 199, 0.06)",

            HoverOpacity = 0.1,
        },

        PaletteDark = new PaletteDark
        {
            Primary = "#75D5CA",
            PrimaryDarken = "#70D1C7",
            PrimaryLighten = "#8ADDD5",

            Secondary = "#818CF8",
            SecondaryDarken = "#6366F1",
            SecondaryLighten = "#C7D2FE",

            Background = "#0F172A",
            Surface = "#111827",
            AppbarBackground = "#111827",
            AppbarText = "#F8FAFC",
            DrawerBackground = "#111827",
            DrawerText = "#E2E8F0",
            DrawerIcon = "#CBD5F5",

            TextPrimary = "#F8FAFC",
            TextSecondary = "#E2E8F0",
            TextDisabled = "#64748B",

            Divider = "rgba(148, 163, 184, 0.14)",
            DividerLight = "rgba(148, 163, 184, 0.08)",
        },

        Shadows = new Shadow
        {
            // Softer, more refined shadows - MudBlazor requires 25 elevation levels (0-24)
            Elevation = new[]
            {
                "none",                                                                         // 0
                "0 2px 1px -1px rgba(0,0,0,0.2),0 1px 1px 0 rgba(0,0,0,0.14),0 1px 3px 0 rgba(0,0,0,0.12)",      // 1
                "0 3px 1px -2px rgba(0,0,0,0.2),0 2px 2px 0 rgba(0,0,0,0.14),0 1px 5px 0 rgba(0,0,0,0.12)",      // 2
                "0 3px 3px -2px rgba(0,0,0,0.2),0 3px 4px 0 rgba(0,0,0,0.14),0 1px 8px 0 rgba(0,0,0,0.12)",      // 3
                "0 2px 4px -1px rgba(0,0,0,0.2),0 4px 5px 0 rgba(0,0,0,0.14),0 1px 10px 0 rgba(0,0,0,0.12)",     // 4
                "0 3px 5px -1px rgba(0,0,0,0.2),0 5px 8px 0 rgba(0,0,0,0.14),0 1px 14px 0 rgba(0,0,0,0.12)",     // 5
                "0 3px 5px -1px rgba(0,0,0,0.2),0 6px 10px 0 rgba(0,0,0,0.14),0 1px 18px 0 rgba(0,0,0,0.12)",    // 6
                "0 4px 5px -2px rgba(0,0,0,0.2),0 7px 10px 1px rgba(0,0,0,0.14),0 2px 16px 1px rgba(0,0,0,0.12)", // 7
                "0 5px 5px -3px rgba(0,0,0,0.2),0 8px 10px 1px rgba(0,0,0,0.14),0 3px 14px 2px rgba(0,0,0,0.12)", // 8
                "0 5px 6px -3px rgba(0,0,0,0.2),0 9px 12px 1px rgba(0,0,0,0.14),0 3px 16px 2px rgba(0,0,0,0.12)", // 9
                "0 6px 6px -3px rgba(0,0,0,0.2),0 10px 14px 1px rgba(0,0,0,0.14),0 4px 18px 3px rgba(0,0,0,0.12)", // 10
                "0 6px 7px -4px rgba(0,0,0,0.2),0 11px 15px 1px rgba(0,0,0,0.14),0 4px 20px 3px rgba(0,0,0,0.12)", // 11
                "0 7px 8px -4px rgba(0,0,0,0.2),0 12px 17px 2px rgba(0,0,0,0.14),0 5px 22px 4px rgba(0,0,0,0.12)", // 12
                "0 7px 8px -4px rgba(0,0,0,0.2),0 13px 19px 2px rgba(0,0,0,0.14),0 5px 24px 4px rgba(0,0,0,0.12)", // 13
                "0 7px 9px -4px rgba(0,0,0,0.2),0 14px 21px 2px rgba(0,0,0,0.14),0 5px 26px 4px rgba(0,0,0,0.12)", // 14
                "0 8px 9px -5px rgba(0,0,0,0.2),0 15px 22px 2px rgba(0,0,0,0.14),0 6px 28px 5px rgba(0,0,0,0.12)", // 15
                "0 8px 10px -5px rgba(0,0,0,0.2),0 16px 24px 2px rgba(0,0,0,0.14),0 6px 30px 5px rgba(0,0,0,0.12)", // 16
                "0 8px 11px -5px rgba(0,0,0,0.2),0 17px 26px 2px rgba(0,0,0,0.14),0 6px 32px 5px rgba(0,0,0,0.12)", // 17
                "0 9px 11px -5px rgba(0,0,0,0.2),0 18px 28px 2px rgba(0,0,0,0.14),0 7px 34px 6px rgba(0,0,0,0.12)", // 18
                "0 9px 12px -6px rgba(0,0,0,0.2),0 19px 29px 2px rgba(0,0,0,0.14),0 7px 36px 6px rgba(0,0,0,0.12)", // 19
                "0 10px 13px -6px rgba(0,0,0,0.2),0 20px 31px 3px rgba(0,0,0,0.14),0 8px 38px 7px rgba(0,0,0,0.12)", // 20
                "0 10px 13px -6px rgba(0,0,0,0.2),0 21px 33px 3px rgba(0,0,0,0.14),0 8px 40px 7px rgba(0,0,0,0.12)", // 21
                "0 10px 14px -6px rgba(0,0,0,0.2),0 22px 35px 3px rgba(0,0,0,0.14),0 8px 42px 7px rgba(0,0,0,0.12)", // 22
                "0 11px 14px -7px rgba(0,0,0,0.2),0 23px 36px 3px rgba(0,0,0,0.14),0 9px 44px 8px rgba(0,0,0,0.12)", // 23
                "0 11px 15px -7px rgba(0,0,0,0.2),0 24px 38px 3px rgba(0,0,0,0.14),0 9px 46px 8px rgba(0,0,0,0.12)", // 24
                "0 12px 16px -8px rgba(0,0,0,0.2),0 25px 40px 3px rgba(0,0,0,0.14),0 10px 48px 9px rgba(0,0,0,0.12)"  // 25
            }
        },

        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "8px",      // Rounded corners
            DrawerWidthLeft = "260px",        // Wider drawer
            DrawerWidthRight = "260px",
            AppbarHeight = "64px",            // Standard height
        },

        ZIndex = new ZIndex
        {
            Drawer = 1200,
            AppBar = 1100,
            Dialog = 1300,
            Snackbar = 1400,
            Tooltip = 1500,
        }
    };
}
