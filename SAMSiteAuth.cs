using System.Collections.Generic;
using System.Linq;

namespace Oxide.Plugins
{
    [Info("SAMSiteAuth", "haggbart", "2.2.3")]
    [Description("Makes SAM Sites act in a similar fashion to shotgun traps and flame turrets.")]
    class SAMSiteAuth : RustPlugin
    {
        private static readonly List<BasePlayer> players = new List<BasePlayer>();
        private static Dictionary<uint, int> vehicles;
        private static BaseVehicleSeat seat;
        private static BuildingPrivlidge buildingPrivlidge;
        
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
                { 2278499844,  1 },   // minicopter 
                { 1675349834, 1 },    // ch47
                { 350141265, 1 },     // sedan
                { 3484163637, 1 },    // scrapheli​
                { 3111236903, 2 }     // balloon
            };
            if (!(bool)Config[ALLTARGET]) return;
            SamSite.alltarget = true;
            if (!(bool)Config[TARGET_HELI]) vehicles.Add(3029415845, 0); // attack heli
        }

        private void Unload() => SamSite.alltarget = false;

        
        
        private void OnSamSiteTarget(SamSite samSite, BaseCombatEntity target) 
        {
            if (samSite.OwnerID == 0) return; 
            if (!vehicles.ContainsKey(target.prefabID)) return;
            if (!isAuthed(samSite, vehicles[target.prefabID])) return;
            samSite.currentTarget = null;
        }

        private static bool isAuthed(SamSite samSite, int kind)
        {
            switch (kind)
            {
                case 0: return true;
                case 1: return IsPilot(samSite);
                case 2: return IsVicinity(samSite);
                default: return false;
            }
        }
        
        private static bool IsPilot(SamSite entity)
        {
            seat = entity.currentTarget.GetComponentsInChildren<BaseVehicleSeat>()[0];
            return seat._mounted == null || IsAuthed(seat._mounted, entity);
        }

        private static bool IsVicinity(SamSite entity)
        {
            players.Clear();
            Vis.Entities(entity.currentTarget.transform.position, 2, players);
            return players.Count == 0 || players.Any(player => IsAuthed(player, entity));
        }
        
        private static bool IsAuthed(BasePlayer player, BaseEntity entity)
        {
            buildingPrivlidge = entity.GetBuildingPrivilege();
            return buildingPrivlidge != null && entity.GetBuildingPrivilege().authorizedPlayers.Any(x => x.userid == player.userID);
        }
    }
}