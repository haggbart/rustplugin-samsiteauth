using System.Collections.Generic;
using System.Linq;

namespace Oxide.Plugins
{
    [Info("SAMSiteAuth", "haggbart", "2.3.3")]
    [Description("Makes SAM Sites act in a similar fashion to shotgun traps and flame turrets.")]
    internal class SAMSiteAuth : RustPlugin
    {
        private static readonly List<BasePlayer> players = new List<BasePlayer>();
        private static Dictionary<uint, int> vehicles;
        private static BuildingPrivlidge buildingPrivlidge;
        private static BasePlayer driver;

        private const string ALLTARGET = "samsite.alltarget";
        private const string TARGET_HELI = "Target heli (requires alltarget)";

        protected override void LoadDefaultConfig()
        {
            Config[ALLTARGET] = false;
            Config[TARGET_HELI] = true;
        }

        private void Init()
        {
            vehicles = new Dictionary<uint, int>
            {
                {2278499844, 1},     // minicopter
                {1675349834, 1},     // ch47
                {350141265, 1},      // sedan
                {3484163637, 1},     // scrapheli​
                {3111236903, 2}      // balloon
            };
            if (!(bool)Config[ALLTARGET]) return;
            SamSite.alltarget = true;
            if (!(bool)Config[TARGET_HELI]) vehicles.Add(3029415845, 0); // attack heli
        }

        private void Unload() => SamSite.alltarget = false;

        private object OnSamSiteTarget(SamSite samSite, BaseCombatEntity target)
        {
            if (SamSite.alltarget)
            {
                if (samSite.OwnerID == 0) // stop monument samsites from shooting attack heli or ch47
                {
                    if (target.prefabID == 1514383717 || target.prefabID == 3029415845)
                    {
                        return false;
                    }
                }
                if (target.prefabID == 209286362) // stop hackable crate being shot
                {
                    return false;
                }
            }
            int kind;
            if (!vehicles.TryGetValue(target.prefabID, out kind)) return null;
            if (!IsAuthed(samSite, target, kind)) return null;
            return false;
        }

        private static bool IsAuthed(SamSite samSite, BaseCombatEntity target, int kind)
        {
            switch (kind)
            {
                case 0: return true;
                case 1: return IsPilot(samSite, target);
                case 2: return IsVicinity(samSite, target);
                default: return false;
            }
        }

        private static bool IsAuthed(BasePlayer player, BaseEntity entity)
        {
            buildingPrivlidge = entity.GetBuildingPrivilege();
            return buildingPrivlidge != null &&
                   buildingPrivlidge.authorizedPlayers.Any(x => x.userid == player.userID);
        }

        private static bool IsPilot(SamSite entity, BaseCombatEntity target)
        {
            driver = (target as BaseVehicle).GetDriver();
            return driver == null || IsAuthed(driver, entity);
        }

        private static bool IsVicinity(SamSite entity, BaseCombatEntity target)
        {
            players.Clear();
            Vis.Entities(target.transform.position, 2, players, Rust.Layers.Mask.Player_Server);
            return players.Count == 0 || players.Any(player => IsAuthed(player, entity));
        }
    }
}