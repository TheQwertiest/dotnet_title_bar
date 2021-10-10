using System.IO;
using System.Reflection;

namespace fooTitle
{
    class Directories
    {
        public static readonly string Component = Assembly.GetExecutingAssembly().Location;
        public static readonly string Skins_Sample = Path.Combine(Component, "skins");
        public static string Fb2kProfile
        {
            get
            {
                if (_fb2kProfile == null)
                { // Fb2k profile can only be retrieved after initialization
                    _fb2kProfile = Main.Get().Fb2kUtils.ProfilePath();
                }

                return _fb2kProfile;
            }
        }
        public static string Skins_Profile
        {
            get
            {
                return Path.Combine(Fb2kProfile, Constants.ComponentNameUnderscored, "skins");
            }
        }
        public static string Skins_ProfileLegacy
        {
            get
            {
                return Path.Combine(Fb2kProfile, "foo_title");
            }
        }

        public static string Temp
        {
            get
            {
                return Path.Combine(Fb2kProfile, Constants.ComponentNameUnderscored, "temp");
            }
        }

        public static string Temp_SkinUnpack
        {
            get
            {
                return Path.Combine(Temp, "unpacked_skin");
            }
        }

        private static string _fb2kProfile;
    }
}
