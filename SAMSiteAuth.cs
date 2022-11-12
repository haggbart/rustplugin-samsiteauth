using System.Collections.Generic;
using static BaseVehicle;

namespace Oxide.Plugins
{
    [Info("SAMSiteAuth", "haggbart", "2.4.3")]
    [Description("Makes SAM Sites act in a similar fashion to shotgun traps and flame turrets.")]
    internal class SAMSiteAuth : RustPlugin
    {
        private readonly object True = true;

        private object OnSamSiteTarget(SamSite samSite, BaseCombatEntity target)
        {
            var mountPoints = (target as BaseVehicle)?.mountPoints;
            if (!IsOccupied(target, mountPoints))
                return True;

            if (samSite.staticRespawn)
                return null;

            var cupboard = samSite.GetBuildingPrivilege(samSite.WorldSpaceBounds());
            if ((object)cupboard == null)
                return null;

            if (mountPoints != null)
            {
                foreach (var mountPoint in mountPoints)
                {
                    var player = mountPoint.mountable.GetMounted();
                    if ((object)player != null && IsAuthed(cupboard, player.userID))
                        return True;
                }
            }

            foreach (var child in target.children)
            {
                var player = child as BasePlayer;
                if ((object)player != null)
                {
                    if (IsAuthed(cupboard, player.userID))
                        return True;
                }
            }

            return null;
        }

        private static bool IsOccupied(BaseCombatEntity entity, List<MountPointInfo> mountPoints)
        {
            if (mountPoints != null)
            {
                foreach (var mountPoint in mountPoints)
                {
                    var player = mountPoint.mountable.GetMounted();
                    if ((object)player != null)
                        return true;
                }
            }

            foreach (var child in entity.children)
            {
                if (child is BasePlayer)
                    return true;
            }

            return false;
        }

        private static bool IsAuthed(BuildingPrivlidge cupboard, ulong userId)
        {
            foreach (var entry in cupboard.authorizedPlayers)
            {
                if (entry.userid == userId)
                    return true;
            }

            return false;
        }
    }
}