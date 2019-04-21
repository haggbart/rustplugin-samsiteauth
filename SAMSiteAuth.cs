using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("SAMSiteAuth", "Kektus", "1.1.1")]
    [Description("Makes SAM Sites act in a similar fashion to shotgun traps and flame turrets.")]
    public class SAMSiteAuth : RustPlugin
    {
        private static List<BasePlayer> Players;
        private static BaseVehicleSeat Seat;
        private static readonly Dictionary<uint, Delegate> _IsAuthed = new Dictionary<uint, Delegate>()
        {
            { 2278499844,  new Func<SamSite, bool>(IsPilot) },
            { 1675349834, new Func<SamSite, bool>(IsPilot) },
            { 350141265, new Func<SamSite, bool>(IsPilot) },
            { 3111236903, new Func<SamSite, bool>(IsVicinity) }
        };

        private void OnServerInitialized()
        {
            NextTick(InitSAMSites);
        }
        
        private void Unload()
        {
            var objects = UnityEngine.Object.FindObjectsOfType<SamController>();
            if (objects == null) return;
            foreach (var obj in objects)
            {
                UnityEngine.Object.Destroy(obj);
            }   
        }

        private static void InitSAMSites()
        {
            var entities = BaseNetworkable.serverEntities.Where(p => p is SamSite).ToList();
            foreach (var entity in entities)
            {
                var samsite = entity as SamSite;
                if (!samsite.GetBuildingPrivilege().IsValid()) continue;
                entity.gameObject.AddComponent<SamController>();
            }
        }

        private void OnEntityBuilt(Planner plan, GameObject go)
        {
            var entity = go.GetComponent<BaseEntity>();
            if (entity is SamSite)
            {
                entity.gameObject.AddComponent<SamController>();
            }
        }

        private static bool IsAuthed(BasePlayer player, BaseEntity entity)
        {
            return entity.GetBuildingPrivilege().authorizedPlayers.Any(x => x.userid == player.userID);
        }

        private static bool IsPilot(SamSite entity)
        {
            Seat = entity.currentTarget.GetComponentsInChildren<BaseVehicleSeat>()[0];
            return Seat._mounted != null && IsAuthed(Seat._mounted, entity);
        }

        private static bool IsVicinity(SamSite entity)
        {
            Players = new List<BasePlayer>();
            Vis.Entities(entity.currentTarget.transform.position, 2, Players);
            foreach (var player in Players)
            {
                if (IsAuthed(player, entity)) return true;
            }
            return false;
        }

        private class SamController : MonoBehaviour
        {
            private SamSite entity;
            
            private void Awake()
            {
                entity = GetComponent<SamSite>();
            }

            private void FixedUpdate()
            {
                if (entity.currentTarget == null) return;
                if (!_IsAuthed.ContainsKey(entity.currentTarget.prefabID)) return;
                if (!(bool) _IsAuthed[entity.currentTarget.prefabID].DynamicInvoke(entity)) return;
                entity.currentTarget = null;
                entity.CancelInvoke(entity.WeaponTick);
            }
        }
    }
}