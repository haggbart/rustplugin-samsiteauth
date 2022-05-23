namespace Oxide.Plugins
{
    [Info("SAMSiteAuth", "haggbart", "2.4.0")]
    [Description("Makes SAM Sites act in a similar fashion to shotgun traps and flame turrets.")]
    internal class SAMSiteAuth : RustPlugin
    {
        private readonly object False = false;

        private object OnSamSiteTarget(SamSite samSite, BaseCombatEntity target)
        {
            if (samSite.staticRespawn)
                return null;

            var cupboard = samSite.GetBuildingPrivilege();
            if ((object)cupboard == null)
                return null;

            var vehicle = target as BaseVehicle;
            if ((object)vehicle != null)
            {
                if (vehicle.mountPoints != null)
                {
                    foreach (var mountPoint in vehicle.mountPoints)
                    {
                        var player = mountPoint.mountable.GetMounted();
                        if ((object)player != null && IsAuthed(cupboard, player.userID))
                            return False;
                    }
                }
            }

            foreach (var child in target.children)
            {
                var player = child as BasePlayer;
                if ((object)player != null)
                {
                    if (IsAuthed(cupboard, player.userID))
                        return False;
                }
            }

            return null;
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