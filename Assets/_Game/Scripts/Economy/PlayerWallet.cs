using System;
using LootboundIsles.Inventory;
using LootboundIsles.Save;
using UnityEngine;

namespace LootboundIsles.Economy
{
    [RequireComponent(typeof(LocalSaveSystem))]
    public class PlayerWallet : MonoBehaviour
    {
        [Header("DEBUG")]
        [SerializeField]
        private bool isDebug;

        private LocalSaveSystem saveSystem;

        public int Gold { get; private set; }
        public int Piastres { get; private set; }

        public event Action<int, int> BalanceChanged;

        private void Awake()
        {
            saveSystem = GetComponent<LocalSaveSystem>();

            if (saveSystem.TryLoadProgression(
                    out PlayerProgressSaveData saveData))
            {
                Gold = saveData.gold;
                Piastres = saveData.piastres;
            }

            if (isDebug)
            {
                Debug.Log(
                    $"PlayerWallet: загружено Gold={Gold}, " +
                    $"Piastres={Piastres}.",
                    this
                );
            }
        }

        public void AddGold(int amount)
        {
            if (amount <= 0)
                return;

            Gold = AddWithoutOverflow(
                Gold,
                amount
            );

            SaveAndNotify();
        }

        public bool TrySpendGold(int amount)
        {
            if (amount <= 0 || amount > Gold)
                return false;

            int previousGold = Gold;
            Gold -= amount;

            if (!saveSystem.SaveWallet(Gold, Piastres))
            {
                Gold = previousGold;
                return false;
            }

            NotifyBalanceChanged();
            return true;
        }

        public bool TrySpendGoldForInventory(int amount, InventorySaveData inventory)
        {
            if (amount <= 0 || amount > Gold || inventory == null)
                return false;
            int nextGold = Gold - amount;
            if (!saveSystem.SaveWalletAndInventory(nextGold, Piastres, inventory))
                return false;
            Gold = nextGold;
            NotifyBalanceChanged();
            return true;
        }

        public bool TryAddGoldForInventory(int amount, InventorySaveData inventory)
        {
            if (amount <= 0 || inventory == null)
                return false;
            int nextGold = AddWithoutOverflow(Gold, amount);
            if (nextGold == Gold || !saveSystem.SaveWalletAndInventory(nextGold, Piastres, inventory))
                return false;
            Gold = nextGold;
            NotifyBalanceChanged();
            return true;
        }

        public void AddPiastres(int amount)
        {
            if (amount <= 0)
                return;

            Piastres = AddWithoutOverflow(
                Piastres,
                amount
            );

            SaveAndNotify();
        }

        public bool TrySpendPiastres(int amount)
        {
            if (amount <= 0 || amount > Piastres)
                return false;

            int previousPiastres = Piastres;
            Piastres -= amount;

            if (!saveSystem.SaveWallet(Gold, Piastres))
            {
                Piastres = previousPiastres;
                return false;
            }

            NotifyBalanceChanged();
            return true;
        }

        private void SaveAndNotify()
        {
            saveSystem.SaveWallet(
                Gold,
                Piastres
            );

            NotifyBalanceChanged();
        }

        private void NotifyBalanceChanged()
        {
            BalanceChanged?.Invoke(
                Gold,
                Piastres
            );

            if (isDebug)
            {
                Debug.Log(
                    $"PlayerWallet: Gold={Gold}, " +
                    $"Piastres={Piastres}.",
                    this
                );
            }
        }

        private static int AddWithoutOverflow(
            int current,
            int amount)
        {
            long result =
                (long)current + amount;

            return result > int.MaxValue
                ? int.MaxValue
                : (int)result;
        }

        [ContextMenu("DEBUG/Add 100 Gold")]
        private void DebugAddGold()
        {
            if (isDebug && Application.isPlaying)
                AddGold(100);
        }

        [ContextMenu("DEBUG/Spend 25 Gold")]
        private void DebugSpendGold()
        {
            if (isDebug && Application.isPlaying)
                TrySpendGold(25);
        }

        [ContextMenu("DEBUG/Add 1 Piastre")]
        private void DebugAddPiastre()
        {
            if (isDebug && Application.isPlaying)
                AddPiastres(1);
        }

        [ContextMenu("DEBUG/Spend 1 Piastre")]
        private void DebugSpendPiastre()
        {
            if (isDebug && Application.isPlaying)
                TrySpendPiastres(1);
        }
    }
}
