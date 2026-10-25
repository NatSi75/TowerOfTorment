using UnityEngine;

public static class MobileLayout
{
    public const float HandCardScale = 1.2f;
    public const float HoverCardScale = 1.6f;
    private const float SmallScreenHeightInches = 5f;
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
