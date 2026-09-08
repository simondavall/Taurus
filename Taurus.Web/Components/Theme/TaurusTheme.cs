using MudBlazor;

namespace Taurus.Components.Theme;

public static class TaurusTheme
{
    public static MudTheme Default { get; } = new() {
        PaletteLight = new PaletteLight {
            Primary = "#1E5AA8",
            PrimaryContrastText = "#FFFFFF",
            Secondary = "#007C83",
            SecondaryContrastText = "#FFFFFF",

            Background = "#F5F7FA",
            Surface = "#FFFFFF",
            AppbarBackground = "#1B2635",
            AppbarText = "#FFFFFF",
            DrawerBackground = "#FFFFFF",
            DrawerText = "#263238",

            TextPrimary = "#202B33",
            TextSecondary = "#53636F",
            TextDisabled = "#8B969E",

            ActionDefault = "#53636F",
            ActionDisabled = "#AEB6BC",
            ActionDisabledBackground = "#E5E9EC",

            Divider = "#D9E0E5",
            DividerLight = "#E8ECEF",

            Info = "#1976D2",
            Success = "#2E7D32",
            Warning = "#ED6C02",
            Error = "#D32F2F"
        },
        PaletteDark = new PaletteDark {
            Primary = "#79B2FF",
            PrimaryContrastText = "#0D2035",
            Secondary = "#67D5DA",
            SecondaryContrastText = "#082629",

            Background = "#121820",
            Surface = "#1B2430",
            AppbarBackground = "#0D131A",
            AppbarText = "#F5F7FA",
            DrawerBackground = "#161E27",
            DrawerText = "#E8EDF1",

            TextPrimary = "#F1F4F6",
            TextSecondary = "#B6C1C9",
            TextDisabled = "#788691",

            ActionDefault = "#C1CBD2",
            ActionDisabled = "#64727D",
            ActionDisabledBackground = "#28333D",

            Divider = "#34414C",
            DividerLight = "#28343E",

            Info = "#64B5F6",
            Success = "#81C784",
            Warning = "#FFB74D",
            Error = "#EF9A9A"
        }
    };
}