using System;
using System.Collections.Generic;
using System.Linq;

namespace Oxide.Plugins
{
    [Info("SAMSiteAuth", "haggbart", "2.0.0")]
    [Description("Makes SAM Sites act in a similar fashion to shotgun traps and flame turrets.")]
    class SAMSiteAuth : RustPlugin
    {
        private static readonly List<BasePlayer> Players = new List<BasePlayer>();
        private static BaseVehicleSeat Seat;
        private readonly Dictionary<uint, Delegate> _IsAuthed = new Dictionary<uint, Delegate>()
        {
            { 2278499844,  new Func<SamSite, bool>(IsPilot) },   // minicopter 
            { 1675349834, new Func<SamSite, bool>(IsPilot) },    // ch47
            { 350141265, new Func<SamSite, bool>(IsPilot) },     // sedan
            { 3111236903, new Func<SamSite, bool>(IsVicinity) }  // balloon
        };
        
        private object CanSamSiteShoot(SamSite samSite)
        {
            if (samSite.OwnerID == 0 || samSite.currentTarget == null) return null; // currentTarget is null in some cases
            if (!_IsAuthed.ContainsKey(samSite.currentTarget.prefabID)) return null;
            if (!(bool) _IsAuthed[samSite.currentTarget.prefabID].DynamicInvoke(samSite)) return null;
            samSite.currentTarget = null;
            samSite.CancelInvoke(samSite.WeaponTick);
            return false;
        }

        private static bool IsPilot(SamSite entity)
        {
            Seat = entity.currentTarget.GetComponentsInChildren<BaseVehicleSeat>()[0];
            return Seat._mounted == null || IsAuthed(Seat._mounted, entity);
        }

        private static bool IsVicinity(SamSite entity)
        {
            Players.Clear();
            Vis.Entities(entity.currentTarget.transform.position, 2, Players);
            if (Players.Count == 0) return true;
            foreach (var player in Players)
            {
                if (IsAuthed(player, entity)) return true;
            }
            return false;
        }
        
        private static bool IsAuthed(BasePlayer player, BaseEntity entity)
        {
            return entity.GetBuildingPrivilege().authorizedPlayers.Any(x => x.userid == player.userID);
        }
    }
}