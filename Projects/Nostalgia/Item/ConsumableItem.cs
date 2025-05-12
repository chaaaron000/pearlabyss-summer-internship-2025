using Fusion;
using Nostal.Interfaces;
using UnityEngine;

namespace Item
{
    [RequireComponent(typeof(NetworkObject), typeof(NetworkTransform), typeof(BoxCollider))]
    public class ConsumableItem : NetworkBehaviour, IInteractable
    {
        [Header("Item Scriptable Object")]
        [SerializeField] private ConsumableItemSO m_itemSO;

        [Header("Interactable Prompt Data")] 
        [SerializeField] private InteractPromptData m_interactPromptData;

        public void OnInteract(NetworkObject playerObject)
        {
            Player player = playerObject.GetComponent<Player>();

            Debug.LogWarning("Consumable Item OnInteract");

            //m_itemSO.Use(player);
           
            // TODO: 승욱이 PlayerInventory 클래스 완성 후 인벤토리에 추가 로직 작성할 것
            // 플레이어의 현재 인벤토리 용량 확인
            // 추가 불가능하면 return
            // 가능하면 AddItem
            PlayerInventory playerInventory = player.GetComponent<PlayerInventory>();
            if(playerInventory != null) {
                if(playerInventory.CanAddItem(m_itemSO)) {
                    playerInventory.AddItem(m_itemSO);
                    DespawnRpc();
                }
                else {
                    Debug.Log("아이템을 추가할 수 없습니다.");
                    return;
                }
            }
 
        }

        public InteractPromptData GetInteractPromptData()
        {
            return m_interactPromptData;
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void DespawnRpc()
        {
            Runner.Despawn(GetComponent<NetworkObject>());
        }
    }
}