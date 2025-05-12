using Fusion;
using UnityEngine;

namespace Item
{
    [CreateAssetMenu(fileName = "SO_TeleportItem", menuName = "Scriptable Object/Items/Teleport", order = 2)]
    public class TeleportItemSO : ConsumableItemSO
    {
        public override void Use(Player usingPlayer)
        {
            MobSpawner mobSpawner = FindObjectOfType<MobSpawner>();
            if (mobSpawner == null)
            {
                Debug.LogError(mobSpawner + "가 null입니다. Teleport Item을 사용할 수 없습니다.");
                return;
            }

            Transform[] mobSpawnPositions = mobSpawner.MobPositions;
            Vector3 teleportTarget = default;
            
            if (GameManager.Instance._deathFlag)  // 플레이어가 한명 죽어있으면 가장 먼 Mob 스폰 포지션으로
            {
                Vector3 playerPosition = usingPlayer.gameObject.transform.position;
                float maxDist = 0f;
                
                foreach (Transform mobSpawnPosition in mobSpawnPositions)
                {
                    float dist = Vector3.Distance(playerPosition, mobSpawnPosition.position);
                    Debug.Log("dist = " + dist);
                    if (dist > maxDist)
                    {
                        maxDist = dist;
                        teleportTarget = mobSpawnPosition.position;
                    }
                }
            }
            else  // 플레이어가 살아있으면 가장 가까운 Mob 스폰 포지션으로
            {
                NetworkObject otherPlayerObject =
                    GameManager.Instance.GetOtherPlayer(usingPlayer.GetComponent<NetworkObject>());
                if (otherPlayerObject == null)
                {
                    Debug.LogError(otherPlayerObject + "가 null입니다. Teleport Item을 사용할 수 없습니다.");
                    return;
                }
                
                Vector3 otherPlayerPosition = otherPlayerObject.gameObject.transform.position;
                float minDist = float.MaxValue;

                foreach (Transform mobSpawnPosition in mobSpawnPositions)
                {
                    float dist = Vector3.Distance(otherPlayerPosition, mobSpawnPosition.position);
                    Debug.Log("dist = " + dist);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        teleportTarget = mobSpawnPosition.position;
                    }
                }
            }

            Debug.Log("teleportTarget = " + teleportTarget);
            usingPlayer.Movement.TeleportRpc(teleportTarget);
        }
    }
}
