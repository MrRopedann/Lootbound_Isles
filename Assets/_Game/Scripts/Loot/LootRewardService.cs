using System;
using LootboundIsles.Economy;
using LootboundIsles.Enemies;
using LootboundIsles.Inventory;
using UnityEngine;

namespace LootboundIsles.Loot
{
    public sealed class LootRewardService : MonoBehaviour
    {
        [Header("DEBUG")]
        [SerializeField] private bool isDebug;

        private PlayerWallet wallet;
        private PlayerInventory inventory;

        public event Action<string, ItemRarity, bool> LootGranted;

        private void Awake()
        {
            wallet = GetComponent<PlayerWallet>();
            inventory = GetComponent<PlayerInventory>();
        }

        public bool GiveLoot(EnemyDefinition definition)
        {
            if (definition == null || definition.LootTable == null)
                return false;
            if (wallet == null || inventory == null)
            {
                Debug.LogError("LootRewardService requires PlayerWallet and PlayerInventory on the same Player.", this);
                return false;
            }

            bool allGranted = true;
            foreach (LootDrop drop in definition.LootTable.Roll())
            {
                if (drop.Quantity <= 0)
                    continue;

                if (drop.Group == LootGroupType.Currency)
                {
                    if (drop.Currency == LootCurrency.Gold)
                        wallet.AddGold(drop.Quantity);
                    else
                        wallet.AddPiastres(drop.Quantity);

                    string currencyName = drop.Currency == LootCurrency.Gold ? "Gold" : "Piastres";
                    LootGranted?.Invoke($"{currencyName} +{drop.Quantity}", ItemRarity.Common, false);
                    if (isDebug)
                        Debug.Log($"Loot: {currencyName} +{drop.Quantity}", this);
                    continue;
                }

                if (drop.Item == null || !inventory.TryAdd(drop.Item, drop.Quantity))
                {
                    allGranted = false;
                    Debug.LogError($"LootRewardService could not grant a rolled item from {definition.name}.", this);
                    continue;
                }

                string itemName = string.IsNullOrWhiteSpace(drop.Item.DisplayName)
                    ? drop.Item.name
                    : drop.Item.DisplayName;
                LootGranted?.Invoke($"{itemName} +{drop.Quantity}", drop.Item.Rarity, true);
                if (isDebug)
                    Debug.Log($"Loot: {itemName} +{drop.Quantity}", this);
            }

            return allGranted;
        }
    }
}
