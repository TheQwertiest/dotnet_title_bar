using System;
using System.IO;

namespace fooTitle
{
    class SkinUtils
    {
        public static string SkinEnumToRootPath(SkinDirType skinDirType)
        {
            return skinDirType switch
            {
                SkinDirType.Component => Main.ComponentSkinsDir,
                SkinDirType.Profile => Main.ProfileSkinsDir,
                SkinDirType.ProfileOld => Main.ProfileSkinsDirOld,
                _ => throw new Exception("Internal error: unexpected skin dir type path `{skinDirType}`")
            };
        }
        public static bool IsCurrentSkin(SkinDirType dirType, string skinDir)
        {
            return (dirType == Configs.Base_SkinDirType.Value && skinDir == Configs.Base_CurrentSkinName.Value);
        }
        public static string GenerateSkinPath(SkinDirType dirType, string skinDir)
        {
            return Path.Combine(SkinEnumToRootPath(dirType), skinDir);
        }
    }
}
