using UnityEngine;

namespace Item
{
    [CreateAssetMenu(fileName = "SO_AdrenalineItem", menuName = "Scriptable Object/Items/Adrenaline", order = 1)]
    public class AdrenalineItemSO : ConsumableItemSO
    {
        [Header("Adrenaline Item Config")]
        [SerializeField] private float m_boostRate = 1.5f;
        [SerializeField] private float m_boostDuration = 20f;
        
        public override void Use(Player usingPlayer)
        {
            PlayerMovement movement = usingPlayer.GetComponent<PlayerMovement>();
            
            movement.BoostSpeed(m_boostRate, m_boostDuration);
        }
    }
}