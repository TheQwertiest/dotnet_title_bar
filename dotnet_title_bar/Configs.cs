using fooTitle.Config;
using System;

namespace fooTitle
{
    internal class Configs
    {
        public static ConfString Base_SkinsDir = new("base/dataDir", "dotnet_title_bar\\skins\\");

        /// <summary>
        /// The name of the currently used skin. Can be changed
        /// </summary>
        public static ConfString Base_CurrentSkinName = new("base/skinName", null);
        public static ConfInt Display_ArtLoadRetryFrequency = new("display/artLoadEvery", 10, 1, int.MaxValue);
        public static ConfInt Display_ArtLoadMaxRetries = new("display/artLoadMaxTimes", 2, -1, int.MaxValue);
        public static ConfEnum<EnableDragging> Display_IsDraggingEnabled = new("display/enableDragging", EnableDragging.Always);

        public static ConfBool Display_IsDpiScalingEnabled = new("display/dpiScale", true);

        /// <summary>
        ///     The opacity when the mouse is over foo_title
        /// </summary>
        public static ConfInt Display_MouseOverOpacity = new("display/overOpacity", 255, 5, 255);

        /// <summary>
        ///     The opacity in normal state
        /// </summary>
        public static ConfInt Display_NormalOpacity = new("display/normalOpacity", 255, 5, 255);

        public static ConfInt Display_PositionX = new("display/positionX", 0);
        public static ConfInt Display_PositionY = new("display/positionY", 0);

        /// <summary>
        /// How often the display should be redrawn (in FPS)
        /// </summary>
        public static ConfInt Display_RefreshRate = new("display/refreshRate", 30, 1, 250);

        public static ConfBool Display_ShouldRefreshOnTop = new("display/reShowOnTop", true);

        /// <summary>
        /// Indicates the need to draw anchor
        /// </summary>
        public static ConfBool Display_ShouldDrawAnchor = new("display/drawAnchor", false);

        public static ConfBool Display_ShouldEdgeSnap = new("display/edgeSnap", true);

        /// <summary>
        /// The z position of the window - either always on top or on the bottom.
        /// </summary>
        public static ConfEnum<Win32.WindowPosition> Display_WindowPosition = new("display/windowPosition", Win32.WindowPosition.Topmost);

        public static ConfEnum<EnableWhenEnum> ShowControl_EnableWhen = new("showControl/enableWhen", EnableWhenEnum.Always);
        public static ConfEnum<ShowWhenEnum> ShowControl_ShowPopupWhen = new("showControl/popupShowing", ShowWhenEnum.Always);
        public static ConfBool ShowControl_ShouldShow_OnSongStart = new("showControl/onSongStart", true);
        public static ConfBool ShowControl_ShouldShow_OnSongEnd = new("showControl/beforeSongEnds", true);
        public static ConfBool ShowControl_ShouldShow_WhenNotPlaying = new("showControl/showWhenNotPlaying", false);
        public static ConfInt ShowControl_StayDelay_OnPeek = new("showControl/timeBeforeFade", 2, 0, int.MaxValue);
        public static ConfInt ShowControl_StayDelay_OnSongStart = new("showControl/onSongStartStay", 5, 0, int.MaxValue);
        public static ConfInt ShowControl_StayDelay_OnSongEnd = new("showControl/beforeSongEndsStay", 5, 0, int.MaxValue);
    }
}