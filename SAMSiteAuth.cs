using System.Collections.Generic;
using static BaseVehicle;

namespace Oxide.Plugins
{
    [Info("SAMSiteAuth", "haggbart", "2.4.4")]
    [Description("Makes SAM Sites act in a similar fashion to shotgun traps and flame turrets.")]
    internal class SAMSiteAuth : RustPlugin
    {
        private readonly object True = true;

        private object OnSamSiteTarget(SamSite samSite, BaseCombatEntity target)
        {
            if (target is Drone drone)
                return HandleDroneTarget(samSite, drone);

            if (target is BaseVehicle vehicle)
                return HandleVehicleTarget(samSite, vehicle);

            return null;
        }

        // Mimic auto turrets, which check the auth of the drone owner (not the drone controller)
        private object HandleDroneTarget(SamSite samSite, Drone drone)
        {
            // Don't interfere with static sam sites because players cannot be authorized to them
            if (samSite.staticRespawn)
                return null;

            // Don't interfere with drones that have no owner (we don't know whose auth to check)
            if (drone.OwnerID == 0)
                return null;

            // Don't interfere with sam sites that don't have a cupboard in range
            var cupboard = samSite.GetBuildingPrivilege(samSite.WorldSpaceBounds());
            if (cupboard is null)
                return null;

            // Block targeting if the drone owner is authed
            if (cupboard.IsAuthed(drone.OwnerID))
                return True;

            // Don't interfere with targeting in this case
            return null;
        }

        private object HandleVehicleTarget(SamSite samSite, BaseVehicle vehicle)
        {
            // Don't target empty vehicles (intentionally applies even to static sam sites)
            var mountPoints = vehicle.mountPoints;
            if (!IsOccupied(vehicle, mountPoints))
                return True;

            // Don't interfere with static sam sites because players cannot be authorized to them
            if (samSite.staticRespawn)
                return null;

            // Don't interfere with sam sites that don't have a cupboard in range
            var cupboard = samSite.GetBuildingPrivilege(samSite.WorldSpaceBounds());
            if (cupboard is null)
                return null;

            // Block targeting if any mounted player is authed
            if (mountPoints != null)
            {
                foreach (var mountPoint in mountPoints)
                {
                    var player = mountPoint.mountable.GetMounted();
                    if (player is not null && cupboard.IsAuthed(player.userID))
                        return True;
                }
            }

            // Block targeting if any passenger is authed (e.g., in the back of a Scrap Heli)
            foreach (var child in vehicle.children)
            {
                if (child is BasePlayer player)
                {
                    if (cupboard.IsAuthed(player.userID))
                        return True;
                }
            }

            // Don't interfere with targeting in this case
            return null;
        }

        private static bool IsOccupied(BaseCombatEntity entity, List<MountPointInfo> mountPoints)
        {
            if (mountPoints != null)
            {
                foreach (var mountPoint in mountPoints)
                {
                    var player = mountPoint.mountable.GetMounted();
                    if (player is not null)
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
    }
}