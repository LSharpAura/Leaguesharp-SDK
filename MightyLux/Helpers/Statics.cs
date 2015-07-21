using System.Media;
using LeagueSharp;
using LeagueSharp.SDK.Core.UI.INotifications;
using LeagueSharp.SDK.Core.Wrappers;
using Menu = LeagueSharp.SDK.Core.UI.IMenu.Menu;

namespace MightyLux.Helpers
{
    public class Statics
    {
        public static Notification KillableR;
        public static Notification Loaded;
        public static Spell Q, W, E, R;
        public static Menu Config;
        public static readonly Obj_AI_Hero Player = ObjectManager.Player;
        public static GameObject LuxE;
        public static SpellSlot Ignite;
        public static int Exist;
        public static SoundPlayer welcome = new SoundPlayer(Properties.Resources.welcome1);
    }
}
