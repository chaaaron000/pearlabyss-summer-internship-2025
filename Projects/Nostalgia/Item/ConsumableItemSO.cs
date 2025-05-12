using UnityEngine;
using UnityEngine.Localization;

public abstract class ConsumableItemSO : ScriptableObject
{
    [Header("Default Item Config")]
    [SerializeField] protected LocalizedString itemName;
    public string ItemName => itemName.GetLocalizedString();

    [SerializeField] protected LocalizedString description;
    public string Description => description.GetLocalizedString();

    [SerializeField] protected Sprite icon;
    public Sprite Icon => icon;

    public abstract void Use(Player usingPlayer);
}