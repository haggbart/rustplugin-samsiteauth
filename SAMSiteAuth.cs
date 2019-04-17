using System;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("SAMSiteAuth", "Kektus", "1.0.1")]
    [Description("Makes SAM Sites act in a similar fashion to shotgun traps and flame turrets.")]
    public class SAMSiteAuth : RustPlugin
    {
        private void Init()
        {
            var entities = BaseNetworkable.serverEntities.Where(p => p is SamSite).ToList();
            foreach (var entity in entities)
            {
                var samsite = entity as SamSite;
                if (!samsite.GetBuildingPrivilege().IsValid()) continue;
                entity.gameObject.AddComponent<SamController>();
            }
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

        private void OnEntityBuilt(Planner plan, GameObject go)
        {
            var entity = go.GetComponent<BaseEntity>();
            if (entity is SamSite)
                entity.gameObject.AddComponent<SamController>();
        }

        private static bool IsAuthed(BasePlayer player, BaseEntity entity)
        {
            return entity.GetBuildingPrivilege().authorizedPlayers.Any(x => x.userid == player.userID);
        }
    
        public class SamController : MonoBehaviour
        {
            public SamSite entity;
            private readonly List<BasePlayer> targets = new List<BasePlayer>();
           
            private void Awake()
            {
                entity = GetComponent<SamSite>();
                entity.enabled = true;
            }

            public void FixedUpdate()
            {
                if (entity.currentTarget == null) return;
                Vis.Entities(entity.currentTarget.transform.position, 1, targets);
                foreach (var target in targets)
                {
                    if (!IsAuthed(target, entity)) continue;
                    entity.currentTarget = null;
                    entity.CancelInvoke((entity.WeaponTick));
                }
            }
        }
    }
}