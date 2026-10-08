using UnityEngine;

// Small-screen tweaks: on phones and tablets the cards are drawn larger so their text stays readable.
public static class MobileLayout
{
    // Hand cards on a touch device, relative to their normal size.
    public const float HandCardScale = 1.2f;
    // The enlarged preview card on a touch device, relative to its normal (desktop) size.
    public const float HoverCardScale = 1.6f;

    // Screens shorter than this (in inches) count as small even when the platform is not reported as mobile.
    private const float SmallScreenHeightInches = 5f;

    // Set to true/false to test the small-screen layout in the Editor; null = detect.
    public static bool? Override;

    public static bool IsSmallScreen
    {
        get
        {
            if (Override.HasValue) return Override.Value;
            if (Application.isMobilePlatform) return true;
            return Input.touchSupported && Screen.dpi > 0f && Screen.height / Screen.dpi < SmallScreenHeightInches;
        }
    }

    public static float HandScale => IsSmallScreen ? HandCardScale : 1f;
    public static float HoverScale => IsSmallScreen ? HoverCardScale : 1f;
}
