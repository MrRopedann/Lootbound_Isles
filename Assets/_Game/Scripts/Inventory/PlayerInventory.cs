using System;
using System.Collections.Generic;
using LootboundIsles.Save;
using UnityEngine;

namespace LootboundIsles.Inventory
{
    [Serializable]
    public sealed class InventoryStack
    {
        [SerializeField] private ItemDefinition definition;
        [SerializeField, Min(1)] private int quantity = 1;

        public ItemDefinition Definition => definition;
        public int Quantity => quantity;

        internal InventoryStack(ItemDefinition definition, int quantity)
        {
            this.definition = definition;
            this.quantity = quantity;
        }

        internal void Add(int amount)
        {
            quantity += amount;
        }

        internal void Remove(int amount)
        {
            quantity -= amount;
        }
    }

    [Serializable]
    public sealed class InventoryItemInstance
    {
        [SerializeField] private string instanceId;
        [SerializeField] private ItemDefinition definition;

        public string InstanceId => instanceId;
        public ItemDefinition Definition => definition;

        internal InventoryItemInstance(ItemDefinition definition, string restoredId = null)
        {
            instanceId = string.IsNullOrWhiteSpace(restoredId) ? Guid.NewGuid().ToString("N") : restoredId;
            this.definition = definition;
        }
    }

    [Serializable]
    public sealed class EquipmentInstance
    {
        public const int SocketCount = 3;

        [SerializeField] private string instanceId;
        [SerializeField] private ItemDefinition definition;
        [SerializeField, Range(1, 10)] private int level = 1;
        [SerializeField, Range(100f, 120f)] private float efficiency = 100f;
        [SerializeField] private bool isLocked;
        [SerializeField] private GemInstance[] socketedGems = new GemInstance[SocketCount];
        [SerializeField] private GemSaveData[] unresolvedSocketedGems = new GemSaveData[SocketCount];

        public string InstanceId => instanceId;
        public ItemDefinition Definition => definition;
        public int Level => level;
        public float Efficiency => efficiency;
        public bool IsLocked => isLocked;
        public IReadOnlyList<GemInstance> SocketedGems => socketedGems;

        public bool HasSocketContent(int index)
        {
            return index >= 0 && index < SocketCount &&
                   ((socketedGems != null && index < socketedGems.Length && socketedGems[index] != null) ||
                    (unresolvedSocketedGems != null && index < unresolvedSocketedGems.Length && unresolvedSocketedGems[index] != null));
        }

        internal EquipmentInstance(ItemDefinition definition, string restoredId = null, int restoredLevel = 1, float restoredEfficiency = 100f, bool restoredLocked = false)
        {
            instanceId = string.IsNullOrWhiteSpace(restoredId) ? Guid.NewGuid().ToString("N") : restoredId;
            this.definition = definition;
            level = Mathf.Clamp(restoredLevel, 1, 10);
            efficiency = Mathf.Clamp(restoredEfficiency, 100f, 120f);
            isLocked = restoredLocked;
            socketedGems = new GemInstance[SocketCount];
            unresolvedSocketedGems = new GemSaveData[SocketCount];
        }

        internal void SetLocked(bool locked)
        {
            isLocked = locked;
        }

        internal void SetLevel(int value)
        {
            level = Mathf.Clamp(value, 1, 10);
        }

        internal void RestoreSocket(int index, GemInstance gem)
        {
            if (index < 0 || index >= SocketCount) return;
            socketedGems[index] = gem;
            unresolvedSocketedGems[index] = null;
        }

        internal bool CanInstallGem(int index, GemInstance gem)
        {
            if (index < 0 || index >= SocketCount || gem?.Definition == null ||
                Definition == null || HasSocketContent(index) ||
                !gem.Definition.IsCompatibleWith(Definition))
                return false;

            for (int i = 0; i < SocketCount; i++)
            {
                GemInstance socketedGem = socketedGems != null && i < socketedGems.Length ? socketedGems[i] : null;
                if (socketedGem?.Definition == gem.Definition)
                    return false;
                GemSaveData unresolved = unresolvedSocketedGems != null && i < unresolvedSocketedGems.Length
                    ? unresolvedSocketedGems[i]
                    : null;
                if (unresolved != null && unresolved.itemId == gem.Definition.ItemId)
                    return false;
            }

            return true;
        }

        internal bool TryInstallGem(int index, GemInstance gem)
        {
            if (!CanInstallGem(index, gem)) return false;
            socketedGems[index] = gem;
            unresolvedSocketedGems[index] = null;
            return true;
        }

        internal void RemoveInstalledGem(int index, GemInstance gem)
        {
            if (index < 0 || index >= SocketCount || socketedGems[index] != gem) return;
            socketedGems[index] = null;
        }

        internal GemInstance ExtractInstalledGem(int index)
        {
            if (index < 0 || index >= SocketCount ||
                socketedGems == null || index >= socketedGems.Length ||
                unresolvedSocketedGems == null || index >= unresolvedSocketedGems.Length ||
                unresolvedSocketedGems[index] != null)
                return null;

            GemInstance gem = socketedGems[index];
            socketedGems[index] = null;
            return gem;
        }

        internal void RestoreExtractedGem(int index, GemInstance gem)
        {
            if (index < 0 || index >= SocketCount || gem == null) return;
            socketedGems[index] = gem;
            unresolvedSocketedGems[index] = null;
        }

        internal void RestoreUnresolvedSocket(int index, GemSaveData gem)
        {
            if (index < 0 || index >= SocketCount) return;
            socketedGems[index] = null;
            unresolvedSocketedGems[index] = gem;
        }

        internal GemSaveData[] CreateSocketSaveData()
        {
            GemSaveData[] result = new GemSaveData[SocketCount];
            for (int i = 0; i < SocketCount; i++)
            {
                GemInstance gem = socketedGems != null && i < socketedGems.Length ? socketedGems[i] : null;
                if (gem?.Definition != null)
                    result[i] = new GemSaveData { instanceId = gem.InstanceId, itemId = gem.Definition.ItemId, level = gem.Level };
                else if (unresolvedSocketedGems != null && i < unresolvedSocketedGems.Length && unresolvedSocketedGems[i] != null)
                {
                    GemSaveData unresolved = unresolvedSocketedGems[i];
                    result[i] = new GemSaveData { instanceId = unresolved.instanceId, itemId = unresolved.itemId, level = unresolved.level };
                }
            }
            return result;
        }
    }

    [Serializable]
    public sealed class GemInstance
    {
        [SerializeField] private string instanceId;
        [SerializeField] private GemDefinition definition;
        [SerializeField, Range(1, 9)] private int level = 1;

        public string InstanceId => instanceId;
        public GemDefinition Definition => definition;
        public int Level => level;

        internal GemInstance(GemDefinition definition, string restoredId = null, int restoredLevel = 1)
        {
            instanceId = string.IsNullOrWhiteSpace(restoredId) ? Guid.NewGuid().ToString("N") : restoredId;
            this.definition = definition;
            level = Mathf.Clamp(restoredLevel, 1, 9);
        }

        internal void SetLevel(int value)
        {
            level = Mathf.Clamp(value, 1, 9);
        }
    }

    public sealed class PlayerInventory : MonoBehaviour
    {
        [SerializeField] private List<InventoryStack> stacks = new();
        [SerializeField] private List<InventoryItemInstance> uniqueItems = new();
        [SerializeField] private List<EquipmentInstance> equipment = new();
        [SerializeField] private List<GemInstance> gems = new();
        [SerializeField] private ItemDefinitionCatalog catalog;
        [SerializeField] private List<InventoryStackSaveData> unresolvedStacks = new();
        [SerializeField] private List<InventoryItemSaveData> unresolvedUniqueItems = new();
        [SerializeField] private List<EquipmentSaveData> unresolvedEquipment = new();
        [SerializeField] private List<GemSaveData> unresolvedGems = new();

        private LocalSaveSystem saveSystem;

        public IReadOnlyList<InventoryStack> Stacks => stacks;
        public IReadOnlyList<InventoryItemInstance> UniqueItems => uniqueItems;
        public IReadOnlyList<EquipmentInstance> Equipment => equipment;
        public IReadOnlyList<GemInstance> Gems => gems;

        public event Action Changed;

        private void Awake()
        {
            saveSystem = GetComponent<LocalSaveSystem>();
        }

        private void Start()
        {
            if (saveSystem == null)
            {
                Debug.LogError("PlayerInventory requires LocalSaveSystem on the same Player object.", this);
                enabled = false;
                return;
            }
            if (catalog == null)
            {
                Debug.LogError("PlayerInventory requires an ItemDefinitionCatalog.", this);
                enabled = false;
                return;
            }
            if (saveSystem.TryLoadProgression(out PlayerProgressSaveData data))
                Restore(data.inventory);
        }

        public bool TryAdd(ItemDefinition definition, int quantity = 1)
        {
            if (definition == null || quantity <= 0)
                return false;

            if (string.IsNullOrWhiteSpace(definition.ItemId))
            {
                Debug.LogError(
                    $"ItemDefinition '{definition.name}' has no Item ID.",
                    definition
                );
                return false;
            }

            if (catalog == null || catalog.Find(definition.ItemId) != definition)
            {
                Debug.LogError(
                    $"ItemDefinition '{definition.name}' is not registered in the PlayerInventory catalog.",
                    this
                );
                return false;
            }

            EnsureCollections();

            if (definition.Category == ItemCategory.Equipment)
            {
                for (int i = 0; i < quantity; i++)
                    equipment.Add(new EquipmentInstance(definition));
            }
            else if (definition.Category == ItemCategory.Gems)
            {
                if (definition is not GemDefinition gemDefinition)
                    return false;
                for (int i = 0; i < quantity; i++)
                    gems.Add(new GemInstance(gemDefinition));
            }
            else if (definition.IsStackable)
            {
                InventoryStack stack = FindStack(definition);
                if (stack != null)
                {
                    if (stack.Quantity > int.MaxValue - quantity)
                        return false;

                    stack.Add(quantity);
                }
                else
                {
                    stacks.Add(new InventoryStack(definition, quantity));
                }
            }
            else
            {
                for (int i = 0; i < quantity; i++)
                    uniqueItems.Add(new InventoryItemInstance(definition));
            }

            if (!Save())
            {
                if (definition.Category == ItemCategory.Equipment)
                {
                    equipment.RemoveRange(equipment.Count - quantity, quantity);
                }
                else if (definition.Category == ItemCategory.Gems)
                {
                    gems.RemoveRange(gems.Count - quantity, quantity);
                }
                else if (definition.IsStackable)
                {
                    InventoryStack stack = FindStack(definition);
                    stack.Remove(quantity);
                    if (stack.Quantity == 0) stacks.Remove(stack);
                }
                else
                {
                    uniqueItems.RemoveRange(uniqueItems.Count - quantity, quantity);
                }
                return false;
            }
            Changed?.Invoke();
            return true;
        }

        public int GetQuantity(ItemDefinition definition)
        {
            if (definition == null)
                return 0;

            EnsureCollections();
            int quantity = 0;

            foreach (InventoryStack stack in stacks)
            {
                if (stack != null && stack.Definition == definition)
                    quantity += stack.Quantity;
            }

            foreach (InventoryItemInstance item in uniqueItems)
            {
                if (item != null && item.Definition == definition)
                    quantity++;
            }

            foreach (EquipmentInstance item in equipment)
            {
                if (item != null && item.Definition == definition)
                    quantity++;
            }

            foreach (GemInstance gem in gems)
            {
                if (gem != null && gem.Definition == definition)
                    quantity++;
            }

            return quantity;
        }

        public bool TryRemoveStack(ItemDefinition definition, int quantity)
        {
            if (definition == null || !definition.IsStackable || quantity <= 0)
                return false;

            EnsureCollections();
            InventoryStack stack = FindStack(definition);
            if (stack == null || stack.Quantity < quantity)
                return false;

            stack.Remove(quantity);
            if (stack.Quantity == 0)
                stacks.Remove(stack);

            if (!Save())
            {
                if (!stacks.Contains(stack)) stacks.Add(stack);
                stack.Add(quantity);
                return false;
            }
            Changed?.Invoke();
            return true;
        }

        public bool TryRemoveUniqueItem(string instanceId)
        {
            EnsureCollections();
            InventoryItemInstance item = uniqueItems.Find(
                candidate => candidate != null &&
                             candidate.InstanceId == instanceId
            );

            if (item == null)
                return false;

            int index = uniqueItems.IndexOf(item);
            uniqueItems.RemoveAt(index);
            if (!Save())
            {
                uniqueItems.Insert(index, item);
                return false;
            }
            Changed?.Invoke();
            return true;
        }

        public bool TryRemoveGem(string instanceId)
        {
            EnsureCollections();
            GemInstance gem = gems.Find(candidate => candidate != null && candidate.InstanceId == instanceId);
            if (gem == null) return false;
            int index = gems.IndexOf(gem);
            gems.RemoveAt(index);
            if (!Save())
            {
                gems.Insert(index, gem);
                return false;
            }
            Changed?.Invoke();
            return true;
        }

        public bool TryInstallGem(string equipmentInstanceId, int socketIndex, string gemInstanceId,
            int goldCost, LootboundIsles.Economy.PlayerWallet wallet)
        {
            EnsureCollections();
            EquipmentInstance target = FindEquipment(equipmentInstanceId);
            GemInstance gem = gems.Find(candidate => candidate != null && candidate.InstanceId == gemInstanceId);
            if (target == null || gem == null || !target.CanInstallGem(socketIndex, gem) ||
                goldCost <= 0 || wallet == null || wallet.Gold < goldCost)
                return false;

            int gemIndex = gems.IndexOf(gem);
            if (!target.TryInstallGem(socketIndex, gem)) return false;
            gems.RemoveAt(gemIndex);

            if (!wallet.TrySpendGoldForInventory(goldCost, CreateSaveData()))
            {
                target.RemoveInstalledGem(socketIndex, gem);
                gems.Insert(gemIndex, gem);
                return false;
            }

            Changed?.Invoke();
            return true;
        }

        public bool TryExtractGems(string equipmentInstanceId, IReadOnlyList<int> socketIndexes,
            IReadOnlyList<GemExtractionOutcome> outcomes, int goldCost, ItemDefinition catalyst,
            LootboundIsles.Economy.PlayerWallet wallet)
        {
            EnsureCollections();
            EquipmentInstance target = FindEquipment(equipmentInstanceId);
            if (target == null || socketIndexes == null || outcomes == null || socketIndexes.Count == 0 ||
                socketIndexes.Count != outcomes.Count || goldCost <= 0 || wallet == null || wallet.Gold < goldCost ||
                !IsValidGemExtractionMaterial(catalyst))
                return false;

            InventoryStack catalystStack = catalyst == null ? null : FindStack(catalyst);
            if (catalyst != null && (catalystStack == null || catalystStack.Quantity < 1)) return false;

            HashSet<int> uniqueSockets = new();
            List<GemInstance> removedGems = new(socketIndexes.Count);
            List<int> originalLevels = new(socketIndexes.Count);
            for (int i = 0; i < socketIndexes.Count; i++)
            {
                int socketIndex = socketIndexes[i];
                if (socketIndex < 0 || socketIndex >= EquipmentInstance.SocketCount || !uniqueSockets.Add(socketIndex) ||
                    !target.HasSocketContent(socketIndex) || target.SocketedGems[socketIndex] == null ||
                    outcomes[i] == null || !outcomes[i].IsValid ||
                    outcomes[i].SocketIndex != socketIndex || outcomes[i].OriginalGem != target.SocketedGems[socketIndex])
                    return false;
                removedGems.Add(target.SocketedGems[socketIndex]);
                originalLevels.Add(target.SocketedGems[socketIndex].Level);
            }

            StackState catalystState = RemoveOneFromStack(catalystStack);
            List<GemInstance> extracted = new(socketIndexes.Count);
            for (int i = 0; i < socketIndexes.Count; i++)
            {
                GemInstance gem = target.ExtractInstalledGem(socketIndexes[i]);
                if (gem == null)
                {
                    for (int rollbackIndex = 0; rollbackIndex < i; rollbackIndex++)
                    {
                        removedGems[rollbackIndex].SetLevel(originalLevels[rollbackIndex]);
                        target.RestoreExtractedGem(socketIndexes[rollbackIndex], removedGems[rollbackIndex]);
                    }
                    RestoreStack(catalystState);
                    return false;
                }

                GemExtractionOutcome outcome = outcomes[i];
                gem.SetLevel(outcome.ReturnedLevel > 0 ? outcome.ReturnedLevel : originalLevels[i]);
                if (outcome.ReturnsGem)
                {
                    gems.Add(gem);
                    extracted.Add(gem);
                }
                else extracted.Add(null);
            }

            if (!wallet.TrySpendGoldForInventory(goldCost, CreateSaveData()))
            {
                for (int i = 0; i < socketIndexes.Count; i++)
                {
                    if (extracted[i] != null) gems.Remove(extracted[i]);
                    removedGems[i].SetLevel(originalLevels[i]);
                    target.RestoreExtractedGem(socketIndexes[i], removedGems[i]);
                }
                RestoreStack(catalystState);
                return false;
            }

            Changed?.Invoke();
            return true;
        }

        private static bool IsValidGemExtractionMaterial(ItemDefinition definition)
        {
            return definition == null ||
                   (definition.Category == ItemCategory.Materials && definition.IsStackable);
        }

        public bool TryUpgradeGem(string targetId, string donorId, int goldCost,
            ItemDefinition protectionStone, ItemDefinition catalyst, bool succeeded,
            LootboundIsles.Economy.PlayerWallet wallet, out bool wasProtected)
        {
            wasProtected = false;
            EnsureCollections();
            GemInstance target = gems.Find(candidate => candidate != null && candidate.InstanceId == targetId);
            GemInstance donor = gems.Find(candidate => candidate != null && candidate.InstanceId == donorId);
            if (target == null || donor == null || target == donor || target.Definition == null ||
                target.Definition != donor.Definition || donor.Level != 1 || target.Level >= 9 ||
                goldCost <= 0 || wallet == null || wallet.Gold < goldCost ||
                !IsValidGemUpgradeMaterial(protectionStone) || !IsValidGemUpgradeMaterial(catalyst) ||
                (protectionStone != null && protectionStone == catalyst))
                return false;

            InventoryStack protectionStack = protectionStone == null ? null : FindStack(protectionStone);
            InventoryStack catalystStack = catalyst == null ? null : FindStack(catalyst);
            if ((protectionStone != null && (target.Level == 1 || protectionStack == null || protectionStack.Quantity < 1)) ||
                (catalyst != null && (catalystStack == null || catalystStack.Quantity < 1)))
                return false;

            int targetLevel = target.Level;
            int donorIndex = gems.IndexOf(donor);
            bool consumeProtection = !succeeded && protectionStone != null;
            StackState protectionState = RemoveOneFromStack(consumeProtection ? protectionStack : null);
            StackState catalystState = RemoveOneFromStack(catalystStack);
            gems.RemoveAt(donorIndex);
            wasProtected = consumeProtection;
            target.SetLevel(succeeded ? targetLevel + 1 : wasProtected ? targetLevel : targetLevel - 1);

            if (!wallet.TrySpendGoldForInventory(goldCost, CreateSaveData()))
            {
                target.SetLevel(targetLevel);
                gems.Insert(donorIndex, donor);
                RestoreStack(protectionState);
                RestoreStack(catalystState);
                wasProtected = false;
                return false;
            }

            Changed?.Invoke();
            return true;
        }

        private static bool IsValidGemUpgradeMaterial(ItemDefinition definition)
        {
            return definition == null ||
                   (definition.Category == ItemCategory.Materials && definition.IsStackable);
        }

        private StackState RemoveOneFromStack(InventoryStack stack)
        {
            if (stack == null)
                return default;
            int index = stacks.IndexOf(stack);
            int previousQuantity = stack.Quantity;
            stack.Remove(1);
            if (stack.Quantity == 0)
                stacks.RemoveAt(index);
            return new StackState(stack, index, previousQuantity);
        }

        private void RestoreStack(StackState state)
        {
            if (state.Stack == null)
                return;
            if (!stacks.Contains(state.Stack))
                stacks.Insert(Mathf.Clamp(state.Index, 0, stacks.Count), state.Stack);
            state.Stack.Add(state.PreviousQuantity - state.Stack.Quantity);
        }

        private readonly struct StackState
        {
            public readonly InventoryStack Stack;
            public readonly int Index;
            public readonly int PreviousQuantity;

            public StackState(InventoryStack stack, int index, int previousQuantity)
            {
                Stack = stack;
                Index = index;
                PreviousQuantity = previousQuantity;
            }
        }

        public bool TryRemoveEquipment(string instanceId)
        {
            EnsureCollections();
            EquipmentInstance item = FindEquipment(instanceId);
            if (item == null || item.IsLocked)
                return false;

            int index = equipment.IndexOf(item);
            equipment.RemoveAt(index);
            if (!Save())
            {
                equipment.Insert(index, item);
                return false;
            }
            Changed?.Invoke();
            return true;
        }

        public bool SetEquipmentLocked(string instanceId, bool locked)
        {
            EnsureCollections();
            EquipmentInstance item = FindEquipment(instanceId);
            if (item == null)
                return false;

            if (item.IsLocked == locked)
                return true;

            item.SetLocked(locked);
            if (!Save())
            {
                item.SetLocked(!locked);
                return false;
            }
            Changed?.Invoke();
            return true;
        }

        public bool TryUpgradeEquipment(string targetId, string donorId, ItemDefinition fragment,
            int fragmentCount, int goldCost, LootboundIsles.Economy.PlayerWallet wallet)
        {
            EnsureCollections();
            EquipmentInstance target = FindEquipment(targetId);
            EquipmentInstance donor = FindEquipment(donorId);
            InventoryStack fragmentStack = FindStack(fragment);
            if (target == null || donor == null || target == donor || donor.IsLocked ||
                target.Definition == null || target.Definition != donor.Definition || target.Level >= 10 ||
                fragment == null || fragment.Category != ItemCategory.Materials ||
                fragment.Rarity != target.Definition.Rarity || !fragment.IsStackable ||
                fragmentCount <= 0 || goldCost <= 0 || wallet == null ||
                fragmentStack == null || fragmentStack.Quantity < fragmentCount)
                return false;

            int targetLevel = target.Level;
            int donorIndex = equipment.IndexOf(donor);
            int stackIndex = stacks.IndexOf(fragmentStack);
            equipment.RemoveAt(donorIndex);
            fragmentStack.Remove(fragmentCount);
            if (fragmentStack.Quantity == 0) stacks.Remove(fragmentStack);
            target.SetLevel(targetLevel + 1);

            if (!wallet.TrySpendGoldForInventory(goldCost, CreateSaveData()))
            {
                target.SetLevel(targetLevel);
                equipment.Insert(donorIndex, donor);
                if (!stacks.Contains(fragmentStack)) stacks.Insert(stackIndex, fragmentStack);
                fragmentStack.Add(fragmentCount);
                return false;
            }
            Changed?.Invoke();
            return true;
        }

        public bool TrySellEquipment(string instanceId, int goldValue, LootboundIsles.Economy.PlayerWallet wallet)
        {
            EnsureCollections();
            EquipmentInstance item = FindEquipment(instanceId);
            if (item == null || item.IsLocked || goldValue <= 0 || wallet == null) return false;
            int index = equipment.IndexOf(item);
            equipment.RemoveAt(index);
            if (!wallet.TryAddGoldForInventory(goldValue, CreateSaveData()))
            {
                equipment.Insert(index, item);
                return false;
            }
            Changed?.Invoke();
            return true;
        }

        public bool TryDismantleEquipment(string instanceId, ItemDefinition fragment, int yield)
        {
            EnsureCollections();
            EquipmentInstance item = FindEquipment(instanceId);
            if (item == null || item.IsLocked || fragment == null ||
                fragment.Category != ItemCategory.Materials || !fragment.IsStackable ||
                fragment.Rarity != item.Definition.Rarity || yield <= 0) return false;

            InventoryStack fragmentStack = FindStack(fragment);
            if (fragmentStack != null && fragmentStack.Quantity > int.MaxValue - yield) return false;
            int itemIndex = equipment.IndexOf(item);
            equipment.RemoveAt(itemIndex);
            if (fragmentStack == null)
            {
                fragmentStack = new InventoryStack(fragment, yield);
                stacks.Add(fragmentStack);
            }
            else fragmentStack.Add(yield);

            if (!Save())
            {
                equipment.Insert(itemIndex, item);
                fragmentStack.Remove(yield);
                if (fragmentStack.Quantity == 0) stacks.Remove(fragmentStack);
                return false;
            }
            Changed?.Invoke();
            return true;
        }

        public bool TryExchangeFragments(ItemDefinition[] fragments, int[] exchangeRates)
        {
            EnsureCollections();
            if (fragments == null || fragments.Length != 5 || exchangeRates == null || exchangeRates.Length < 4)
                return false;

            int[] previousCounts = new int[5];
            InventoryStack[] previousStacks = new InventoryStack[5];
            long[] nextCounts = new long[5];
            long carry = 0;
            for (int i = 0; i < fragments.Length; i++)
            {
                ItemDefinition fragment = fragments[i];
                if (fragment == null || fragment.Category != ItemCategory.Materials ||
                    fragment.Rarity != (ItemRarity)i || !fragment.IsStackable) return false;
                previousStacks[i] = FindStack(fragment);
                previousCounts[i] = previousStacks[i] == null ? 0 : previousStacks[i].Quantity;
                if (i < 4)
                {
                    int rate = exchangeRates[i];
                    if (rate < 2) return false;
                    long total = previousCounts[i] + carry;
                    nextCounts[i] = total % rate;
                    carry = total / rate;
                }
                else nextCounts[i] = previousCounts[i] + carry;
                if (nextCounts[i] > int.MaxValue) return false;
            }
            bool changed = false;
            for (int i = 0; i < fragments.Length; i++)
                if (nextCounts[i] != previousCounts[i]) { changed = true; break; }
            if (!changed) return false;

            for (int i = 0; i < fragments.Length; i++)
            {
                int quantity = (int)nextCounts[i];
                InventoryStack stack = previousStacks[i];
                if (stack == null)
                {
                    if (quantity > 0) stacks.Add(new InventoryStack(fragments[i], quantity));
                }
                else
                {
                    int difference = quantity - previousCounts[i];
                    if (difference > 0) stack.Add(difference);
                    else if (difference < 0) stack.Remove(-difference);
                    if (quantity == 0) stacks.Remove(stack);
                }
            }

            if (!Save())
            {
                for (int i = 0; i < fragments.Length; i++)
                {
                    InventoryStack current = FindStack(fragments[i]);
                    if (previousStacks[i] == null)
                    {
                        if (current != null) stacks.Remove(current);
                    }
                    else
                    {
                        if (!stacks.Contains(previousStacks[i])) stacks.Add(previousStacks[i]);
                        int difference = previousCounts[i] - previousStacks[i].Quantity;
                        if (difference > 0) previousStacks[i].Add(difference);
                        else if (difference < 0) previousStacks[i].Remove(-difference);
                    }
                }
                return false;
            }
            Changed?.Invoke();
            return true;
        }

        public bool Restore(InventorySaveData data)
        {
            if (data == null || catalog == null) return false;
            EnsureCollections();
            stacks.Clear(); uniqueItems.Clear(); equipment.Clear(); gems.Clear();
            unresolvedStacks.Clear(); unresolvedUniqueItems.Clear(); unresolvedEquipment.Clear(); unresolvedGems.Clear();
            foreach (InventoryStackSaveData entry in data.stacks)
            {
                if (entry == null || entry.quantity <= 0) continue;
                ItemDefinition definition = catalog.Find(entry.itemId);
                if (definition == null) { unresolvedStacks.Add(entry); continue; }
                if (definition.Category == ItemCategory.Gems && definition is GemDefinition legacyGem)
                {
                    for (int i = 0; i < entry.quantity; i++) gems.Add(new GemInstance(legacyGem));
                    continue;
                }
                if (definition.Category == ItemCategory.Equipment || definition.Category == ItemCategory.Gems || !definition.IsStackable) { unresolvedStacks.Add(entry); continue; }
                stacks.Add(new InventoryStack(definition, entry.quantity));
            }
            foreach (InventoryItemSaveData entry in data.uniqueItems)
            {
                if (entry == null) continue;
                ItemDefinition definition = catalog.Find(entry.itemId);
                if (definition == null) { unresolvedUniqueItems.Add(entry); continue; }
                if (definition.Category == ItemCategory.Gems && definition is GemDefinition legacyGem)
                {
                    gems.Add(new GemInstance(legacyGem, entry.instanceId));
                    continue;
                }
                if (definition.Category == ItemCategory.Equipment || definition.IsStackable) { unresolvedUniqueItems.Add(entry); continue; }
                uniqueItems.Add(new InventoryItemInstance(definition, entry.instanceId));
            }
            foreach (EquipmentSaveData entry in data.equipment)
            {
                if (entry == null) continue;
                ItemDefinition definition = catalog.Find(entry.itemId);
                if (definition == null || definition.Category != ItemCategory.Equipment) { unresolvedEquipment.Add(entry); continue; }
                EquipmentInstance restoredEquipment = new(definition, entry.instanceId, entry.level, entry.efficiency, entry.isLocked);
                for (int socketIndex = 0; socketIndex < EquipmentInstance.SocketCount; socketIndex++)
                {
                    GemSaveData socketData = entry.socketedGems != null && socketIndex < entry.socketedGems.Length
                        ? entry.socketedGems[socketIndex]
                        : null;
                    if (socketData == null) continue;

                    ItemDefinition socketDefinition = catalog.Find(socketData.itemId);
                    if (socketDefinition is GemDefinition socketGemDefinition)
                        restoredEquipment.RestoreSocket(socketIndex,
                            new GemInstance(socketGemDefinition, socketData.instanceId, socketData.level));
                    else
                        restoredEquipment.RestoreUnresolvedSocket(socketIndex, socketData);
                }
                equipment.Add(restoredEquipment);
            }
            foreach (GemSaveData entry in data.gems ?? new List<GemSaveData>())
            {
                if (entry == null) continue;
                ItemDefinition definition = catalog.Find(entry.itemId);
                if (definition is not GemDefinition gemDefinition) { unresolvedGems.Add(entry); continue; }
                gems.Add(new GemInstance(gemDefinition, entry.instanceId, entry.level));
            }
            Changed?.Invoke();
            return true;
        }

        public InventorySaveData CreateSaveData()
        {
            InventorySaveData data = new();
            foreach (InventoryStackSaveData entry in unresolvedStacks) if (entry != null) data.stacks.Add(entry);
            foreach (InventoryItemSaveData entry in unresolvedUniqueItems) if (entry != null) data.uniqueItems.Add(entry);
            foreach (EquipmentSaveData entry in unresolvedEquipment) if (entry != null) data.equipment.Add(entry);
            foreach (GemSaveData entry in unresolvedGems) if (entry != null) data.gems.Add(entry);
            foreach (InventoryStack stack in stacks)
                if (stack?.Definition != null) data.stacks.Add(new InventoryStackSaveData { itemId = stack.Definition.ItemId, quantity = stack.Quantity });
            foreach (InventoryItemInstance item in uniqueItems)
                if (item?.Definition != null) data.uniqueItems.Add(new InventoryItemSaveData { instanceId = item.InstanceId, itemId = item.Definition.ItemId });
            foreach (EquipmentInstance item in equipment)
                if (item?.Definition != null) data.equipment.Add(new EquipmentSaveData { instanceId = item.InstanceId, itemId = item.Definition.ItemId, level = item.Level, efficiency = item.Efficiency, isLocked = item.IsLocked, socketedGems = item.CreateSocketSaveData() });
            foreach (GemInstance item in gems)
                if (item?.Definition != null) data.gems.Add(new GemSaveData { instanceId = item.InstanceId, itemId = item.Definition.ItemId, level = item.Level });
            return data;
        }

        private bool Save()
        {
            if (saveSystem == null) return true;
            if (saveSystem.SaveInventory(CreateSaveData())) return true;
            Debug.LogError("PlayerInventory: inventory save failed; operation was reverted.", this);
            return false;
        }

        private InventoryStack FindStack(ItemDefinition definition)
        {
            return stacks.Find(
                stack => stack != null && stack.Definition == definition
            );
        }

        private EquipmentInstance FindEquipment(string instanceId)
        {
            if (string.IsNullOrEmpty(instanceId))
                return null;

            return equipment.Find(
                item => item != null && item.InstanceId == instanceId
            );
        }

        private void EnsureCollections()
        {
            stacks ??= new List<InventoryStack>();
            uniqueItems ??= new List<InventoryItemInstance>();
            equipment ??= new List<EquipmentInstance>();
            gems ??= new List<GemInstance>();
        }
    }
}
