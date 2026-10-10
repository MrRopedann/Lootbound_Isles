using System.Collections.Generic;
using LootboundIsles.Economy;
using LootboundIsles.Inventory;
using LootboundIsles.Player;
using LootboundIsles.Upgrades;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace LootboundIsles.UI
{
    /// <summary>Lightweight uGUI inventory presentation. It never pauses gameplay.</summary>
    public sealed class InventoryUI : MonoBehaviour
    {
        private static readonly Color[] RarityColors =
        {
            new(0.88f, 0.88f, 0.88f), new(0.35f, 0.85f, 0.45f),
            new(0.3f, 0.6f, 1f), new(0.75f, 0.4f, 1f), new(1f, 0.67f, 0.18f)
        };

        private readonly List<Button> categoryButtons = new();
        private readonly List<Button> gemSocketButtons = new();
        private PlayerInventory inventory;
        private PlayerWallet wallet;
        private UpgradeSystem upgrades;
        private PlayerDeath death;
        private GameObject panel;
        private RectTransform rows;
        private TMP_Text headerText;
        private TMP_Text detailsText;
        private TMP_Text balanceText;
        private TMP_Text selectionText;
        private Button lockButton;
        private Button upgradeButton;
        private Button sellButton;
        private Button dismantleButton;
        private Button installGemButton;
        private EquipmentUpgradeService equipmentUpgrades;
        private GemUpgradeService gemUpgrades;
        private GemInstallationService gemInstallation;
        private InventoryAction pendingAction;
        private enum InventoryAction { None, Upgrade, Sell, Dismantle, Exchange, GemUpgrade, InstallGem }
        private ItemCategory selectedCategory;
        private string selectedInstanceId;
        private string installationEquipmentInstanceId;
        private int installationSocketIndex = -1;
        private ItemDefinition selectedDefinition;
        private bool selectedIsEquipment;
        private bool selectedIsGem;
        private bool useGemProtection;
        private int selectedCatalystIndex = -1;
        private string gemUpgradeMessage;
        private bool previousCursorVisible;
        private CursorLockMode previousCursorLock;

        private void Awake()
        {
            PlayerInventory[] inventories = FindObjectsByType<PlayerInventory>();
            if (inventories.Length != 1)
            {
                Debug.LogError($"InventoryUI expects exactly one PlayerInventory; found {inventories.Length}.", this);
                enabled = false;
                return;
            }
            inventory = inventories[0];
            wallet = inventory.GetComponent<PlayerWallet>();
            upgrades = inventory.GetComponent<UpgradeSystem>();
            equipmentUpgrades = inventory.GetComponent<EquipmentUpgradeService>();
            gemUpgrades = inventory.GetComponent<GemUpgradeService>();
            gemInstallation = inventory.GetComponent<GemInstallationService>();
            death = inventory.GetComponent<PlayerDeath>();
            BuildUI();
        }

        private void OnEnable()
        {
            if (inventory != null) inventory.Changed += Refresh;
            if (wallet != null) wallet.BalanceChanged += HandleBalanceChanged;
            if (upgrades != null) upgrades.ChoicesGenerated += HandleLevelUp;
            if (death != null) death.DeathStarted += Close;
        }

        private void Start()
        {
            if (panel != null) panel.SetActive(false);
            Refresh();
            RefreshBalance();
        }

        private void OnDisable()
        {
            if (inventory != null) inventory.Changed -= Refresh;
            if (wallet != null) wallet.BalanceChanged -= HandleBalanceChanged;
            if (upgrades != null) upgrades.ChoicesGenerated -= HandleLevelUp;
            if (death != null) death.DeathStarted -= Close;
            RestoreCursor();
        }

        private void Update()
        {
            if (panel == null || Keyboard.current == null) return;
            if (Keyboard.current.escapeKey.wasPressedThisFrame) Close();
            else if (Keyboard.current.iKey.wasPressedThisFrame)
            {
                if (panel.activeSelf) Close(); else Open();
            }
        }

        private void BuildUI()
        {
            TMP_FontAsset font = TMP_Settings.defaultFontAsset;
            if (font == null) Debug.LogError("InventoryUI: TMP default font is missing.", this);

            Button openButton = MakeButton("InventoryOpenButton", transform, "Inventory [I]", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-105f, -38f), new Vector2(180f, 48f), font, 19);
            openButton.onClick.AddListener(Open);
            panel = MakeRect("InventoryPanel", transform, new Vector2(.5f, .5f), new Vector2(.5f, .5f), Vector2.zero, new Vector2(1000f, 650f)).gameObject;
            Image background = panel.AddComponent<Image>(); background.color = new Color(.055f, .075f, .09f, .97f);
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.SetAsLastSibling();

            headerText = MakeText("Header", panel.transform, "Inventory", new Vector2(.04f, .88f), new Vector2(.55f, .98f), font, 29, TextAlignmentOptions.Left);
            balanceText = MakeText("Balances", panel.transform, "", new Vector2(.55f, .9f), new Vector2(.86f, .98f), font, 18, TextAlignmentOptions.Right);
            Button close = MakeButton("CloseButton", panel.transform, "Close", new Vector2(.87f, .9f), new Vector2(.97f, .98f), Vector2.zero, Vector2.zero, font, 17);
            close.onClick.AddListener(Close);

            string[] names = { "Equipment", "Materials", "Gems", "Consumables", "Blueprints" };
            for (int i = 0; i < names.Length; i++)
            {
                int index = i;
                float x0 = .04f + i * .184f;
                Button tab = MakeButton("Category_" + names[i], panel.transform, names[i], new Vector2(x0, .8f), new Vector2(x0 + .17f, .87f), Vector2.zero, Vector2.zero, font, 16);
                tab.onClick.AddListener(() => SelectCategory((ItemCategory)index));
                categoryButtons.Add(tab);
            }

            RectTransform listFrame = MakeRect("ItemListFrame", panel.transform, new Vector2(.04f, .08f), new Vector2(.56f, .78f), Vector2.zero, Vector2.zero);
            Image listImage = listFrame.gameObject.AddComponent<Image>(); listImage.color = new Color(.1f, .13f, .15f, 1f);
            ScrollRect scroll = listFrame.gameObject.AddComponent<ScrollRect>(); scroll.horizontal = false; scroll.movementType = ScrollRect.MovementType.Clamped;
            RectTransform viewport = MakeRect("Viewport", listFrame, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            viewport.gameObject.AddComponent<RectMask2D>(); scroll.viewport = viewport;
            rows = MakeRect("Rows", viewport, new Vector2(0f, 1f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            VerticalLayoutGroup layout = rows.gameObject.AddComponent<VerticalLayoutGroup>(); layout.padding = new RectOffset(8, 8, 8, 8); layout.spacing = 5; layout.childControlHeight = true; layout.childForceExpandHeight = false; layout.childControlWidth = true;
            ContentSizeFitter fitter = rows.gameObject.AddComponent<ContentSizeFitter>(); fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.content = rows;

            RectTransform detail = MakeRect("Details", panel.transform, new Vector2(.6f, .08f), new Vector2(.96f, .78f), Vector2.zero, Vector2.zero);
            Image detailImage = detail.gameObject.AddComponent<Image>(); detailImage.color = new Color(.1f, .13f, .15f, 1f);
            selectionText = MakeText("Selection", detail, "Select an item", new Vector2(.07f, .83f), new Vector2(.93f, .98f), font, 23, TextAlignmentOptions.Left);
            detailsText = MakeText("ItemDetails", detail, "", new Vector2(.07f, .3f), new Vector2(.93f, .8f), font, 17, TextAlignmentOptions.TopLeft);
            for (int i = 0; i < EquipmentInstance.SocketCount; i++)
            {
                int socketIndex = i;
                float x0 = .07f + i * .3f;
                Button socket = MakeButton($"GemSocket{i + 1}", detail, $"Socket {i + 1}: Empty",
                    new Vector2(x0, .31f), new Vector2(x0 + .28f, .42f), Vector2.zero, Vector2.zero, font, 12);
                socket.onClick.AddListener(() => SelectInstallationSocket(socketIndex));
                socket.gameObject.SetActive(false);
                gemSocketButtons.Add(socket);
            }
            lockButton = MakeButton("LockButton", detail, "Lock", new Vector2(.53f, .07f), new Vector2(.93f, .18f), Vector2.zero, Vector2.zero, font, 16);
            lockButton.onClick.AddListener(ToggleLock);
            upgradeButton = MakeButton("UpgradeButton", detail, "Upgrade", new Vector2(.07f, .07f), new Vector2(.47f, .18f), Vector2.zero, Vector2.zero, font, 16);
            upgradeButton.onClick.AddListener(() => RequestAction(selectedIsGem ? InventoryAction.GemUpgrade : selectedIsEquipment ? InventoryAction.Upgrade : InventoryAction.Exchange));
            sellButton = MakeButton("SellButton", detail, "Sell", new Vector2(.07f, .19f), new Vector2(.47f, .3f), Vector2.zero, Vector2.zero, font, 16);
            sellButton.onClick.AddListener(() => { if (selectedIsGem) ToggleGemProtection(); else RequestAction(InventoryAction.Sell); });
            dismantleButton = MakeButton("DismantleButton", detail, "Dismantle", new Vector2(.53f, .19f), new Vector2(.93f, .3f), Vector2.zero, Vector2.zero, font, 16);
            dismantleButton.onClick.AddListener(() => { if (selectedIsGem) CycleGemCatalyst(); else RequestAction(InventoryAction.Dismantle); });
            installGemButton = MakeButton("InstallGemButton", detail, "Install Gem", new Vector2(.53f, .07f), new Vector2(.93f, .18f), Vector2.zero, Vector2.zero, font, 16);
            installGemButton.onClick.AddListener(() => RequestAction(InventoryAction.InstallGem));
            installGemButton.gameObject.SetActive(false);
            panel.SetActive(false);
        }

        private void Open()
        {
            if (death != null && death.IsDead) return;
            if (upgrades != null && upgrades.IsWaitingForChoice) return;
            pendingAction = InventoryAction.None;
            previousCursorVisible = Cursor.visible; previousCursorLock = Cursor.lockState;
            Cursor.visible = true; Cursor.lockState = CursorLockMode.None;
            panel.SetActive(true); Refresh(); RefreshBalance();
        }

        private void Close()
        {
            if (panel == null || !panel.activeSelf) return;
            pendingAction = InventoryAction.None;
            panel.SetActive(false); RestoreCursor();
        }

        private void RestoreCursor()
        {
            if (panel == null || !panel.activeSelf)
            {
                Cursor.visible = previousCursorVisible;
                Cursor.lockState = previousCursorLock;
            }
        }

        private void HandleLevelUp(System.Collections.Generic.IReadOnlyList<LootboundIsles.Upgrades.UpgradeDefinition> _) => Close();
        private void HandleBalanceChanged(int _, int __) => RefreshBalance();
        private void RefreshBalance() => balanceText?.SetText(wallet == null ? "" : $"Gold  {wallet.Gold:N0}     Piastres  {wallet.Piastres:N0}");

        private void SelectCategory(ItemCategory category)
        {
            pendingAction = InventoryAction.None;
            selectedCategory = category; selectedInstanceId = null; selectedDefinition = null;
            selectedIsEquipment = false; selectedIsGem = false;
            if (category != ItemCategory.Equipment && category != ItemCategory.Gems)
                ClearInstallationSelection();
            ResetGemUpgradeOptions();
            Refresh();
        }

        private void Refresh()
        {
            if (rows == null || inventory == null) return;
            pendingAction = InventoryAction.None;
            // Hide first so multiple refreshes in one frame cannot leave
            // scheduled-for-destruction rows visible or clickable.
            for (int i = rows.childCount - 1; i >= 0; i--)
            {
                GameObject oldRow = rows.GetChild(i).gameObject;
                oldRow.SetActive(false);
                Destroy(oldRow);
            }
            for (int i = 0; i < categoryButtons.Count; i++)
                categoryButtons[i].GetComponent<Image>().color = (int)selectedCategory == i ? new Color(.35f, .49f, .47f) : new Color(.18f, .24f, .26f);

            int count = 0;
            if (selectedCategory == ItemCategory.Equipment)
            {
                foreach (EquipmentInstance item in inventory.Equipment)
                {
                    if (item?.Definition == null) continue;
                    AddRow(item.Definition, $"{item.Definition.DisplayName}  ·  Lv {item.Level}", item.InstanceId, true, item.IsLocked);
                    count++;
                }
            }
            else if (selectedCategory == ItemCategory.Gems)
            {
                foreach (GemInstance gem in inventory.Gems)
                {
                    if (gem?.Definition == null) continue;
                    AddRow(gem.Definition, $"{gem.Definition.DisplayName}  ·  Lv {gem.Level}", gem.InstanceId, false, false, true);
                    count++;
                }
            }
            else
            {
                foreach (InventoryStack stack in inventory.Stacks)
                {
                    if (stack?.Definition == null || stack.Definition.Category != selectedCategory) continue;
                    AddRow(stack.Definition, $"{stack.Definition.DisplayName}  × {stack.Quantity:N0}", null, false, false); count++;
                }
                foreach (InventoryItemInstance item in inventory.UniqueItems)
                {
                    if (item?.Definition == null || item.Definition.Category != selectedCategory) continue;
                    AddRow(item.Definition, item.Definition.DisplayName, item.InstanceId, false, false); count++;
                }
            }
            headerText.SetText($"Inventory  <size=65%>({count} {(count == 1 ? "entry" : "entries")})</size>");
            if (count == 0) AddRow(null, "This category is empty", null, false, false);
            RefreshDetails();
        }

        private void AddRow(ItemDefinition definition, string label, string instanceId, bool isEquipment, bool isLocked, bool isGem = false)
        {
            TMP_FontAsset font = TMP_Settings.defaultFontAsset;
            Button row = MakeButton("ItemRow", rows, label, new Vector2(0, 1), new Vector2(1, 1), Vector2.zero, new Vector2(0, 48), font, 16);
            LayoutElement element = row.gameObject.AddComponent<LayoutElement>(); element.minHeight = 48; element.preferredHeight = 48;
            if (definition != null) row.GetComponentInChildren<TMP_Text>().color = RarityColors[(int)definition.Rarity];
            string id = instanceId;
            row.onClick.AddListener(() =>
            {
                pendingAction = InventoryAction.None;
                selectedDefinition = definition;
                selectedInstanceId = id;
                selectedIsEquipment = isEquipment;
                selectedIsGem = isGem;
                if (isEquipment)
                {
                    installationEquipmentInstanceId = id;
                    installationSocketIndex = -1;
                }
                else if (!isGem)
                    ClearInstallationSelection();
                ResetGemUpgradeOptions();
                RefreshDetails();
            });
            if (isEquipment && isLocked) row.GetComponent<Image>().color = new Color(.24f, .21f, .29f);
        }

        private void RefreshDetails()
        {
            foreach (Button socket in gemSocketButtons) socket.gameObject.SetActive(false);
            installGemButton.gameObject.SetActive(false);
            detailsText.rectTransform.anchorMin = new Vector2(.07f, .3f);
            detailsText.rectTransform.anchorMax = new Vector2(.93f, .8f);
            if (selectedDefinition == null)
            {
                if (!string.IsNullOrEmpty(installationEquipmentInstanceId) && installationSocketIndex >= 0)
                {
                    selectionText.SetText("Choose a Gem");
                    detailsText.SetText($"Select one compatible Gem from your inventory for socket {installationSocketIndex + 1}.\n\nInstallation is guaranteed and costs {gemInstallation?.Rules?.GoldCost ?? 0} Gold.");
                }
                else
                {
                    selectionText.SetText("Select an item");
                    detailsText.SetText("Items received from loot go directly into inventory.\n\nInventory stays open during combat and does not pause the world.");
                }
                lockButton.gameObject.SetActive(false); upgradeButton.gameObject.SetActive(false);
                sellButton.gameObject.SetActive(false); dismantleButton.gameObject.SetActive(false); return;
            }
            selectionText.SetText($"<color=#{ColorUtility.ToHtmlStringRGB(RarityColors[(int)selectedDefinition.Rarity])}>{selectedDefinition.DisplayName}</color>");
            detailsText.SetText($"Category: {selectedDefinition.Category}\nRarity: {selectedDefinition.Rarity}\nQuantity: {inventory.GetQuantity(selectedDefinition):N0}\nItem ID: {selectedDefinition.ItemId}");
            if (selectedIsGem)
            {
                GemInstance gem = FindGem(selectedInstanceId);
                if (gem != null)
                {
                    string family = gem.Definition is GemDefinition definition ? $"\nStat family: {definition.StatFamily}" : string.Empty;
                    detailsText.SetText($"Category: Gem\nRarity: {selectedDefinition.Rarity}\nLevel: {gem.Level}{family}\nItem ID: {selectedDefinition.ItemId}");
                }
            }
            lockButton.gameObject.SetActive(selectedIsEquipment);
            upgradeButton.gameObject.SetActive(selectedIsEquipment || selectedIsGem);
            sellButton.gameObject.SetActive(selectedIsEquipment || selectedIsGem);
            dismantleButton.gameObject.SetActive(selectedIsEquipment || selectedIsGem);
            if (selectedIsEquipment)
            {
                EquipmentInstance item = FindEquipment(selectedInstanceId);
                EquipmentInstance donor = null;
                ItemDefinition fragment = null;
                int goldCost = 0;
                int fragmentCost = 0;
                bool canUpgrade = equipmentUpgrades != null && equipmentUpgrades.TryGetUpgradeInfo(
                    selectedInstanceId, out _, out donor, out fragment, out goldCost, out fragmentCost);
                bool affordable = canUpgrade && inventory.GetQuantity(fragment) >= fragmentCost && wallet != null && wallet.Gold >= goldCost;
                if (item != null)
                {
                    string costs = canUpgrade
                        ? $"Upgrade consumes: {donor.Definition.DisplayName} (Lv {donor.Level}), {fragmentCost} {fragment.DisplayName}, {goldCost:N0} Gold"
                        : item.Level >= 10 ? "Maximum Level reached." : "Upgrade unavailable: missing donor, fragments, Gold, or setup.";
                    int sale = equipmentUpgrades == null ? 0 : equipmentUpgrades.GetSellValue(item.InstanceId);
                    int yield = equipmentUpgrades == null ? 0 : equipmentUpgrades.GetDismantleYield(item.InstanceId);
                    detailsText.rectTransform.anchorMin = new Vector2(.07f, .44f);
                    detailsText.rectTransform.anchorMax = new Vector2(.93f, .8f);
                    detailsText.SetText($"Category: Equipment\nRarity: {selectedDefinition.Rarity}\nLevel: {item.Level}\nEfficiency: {item.Efficiency:F0}%\nStatus: {(item.IsLocked ? "Locked" : "Unlocked")}\n\n{costs}\nSell: {sale:N0} Gold · Dismantle: {yield} {selectedDefinition.Rarity} Fragment(s)\nSlot ID: {item.Definition.EquipmentSlotId ?? "Not configured"}");
                    for (int i = 0; i < gemSocketButtons.Count; i++)
                    {
                        GemInstance socketedGem = item.SocketedGems[i];
                        Button socket = gemSocketButtons[i];
                        socket.gameObject.SetActive(true);
                        bool hasSocketContent = item.HasSocketContent(i);
                        socket.GetComponentInChildren<TMP_Text>().SetText(socketedGem == null
                            ? hasSocketContent ? $"Socket {i + 1}: Unavailable" : $"Socket {i + 1}: Empty"
                            : $"Socket {i + 1}: {socketedGem.Definition.DisplayName} Lv{socketedGem.Level}");
                        socket.interactable = !hasSocketContent && !string.IsNullOrWhiteSpace(item.Definition.EquipmentSlotId);
                        socket.GetComponent<Image>().color = installationEquipmentInstanceId == item.InstanceId && installationSocketIndex == i
                            ? new Color(.35f, .49f, .47f)
                            : new Color(.18f, .24f, .26f, .98f);
                    }
                }
                lockButton.GetComponentInChildren<TMP_Text>().SetText(item != null && item.IsLocked ? "Unlock item" : "Lock item");
                upgradeButton.interactable = affordable || pendingAction == InventoryAction.Upgrade;
                sellButton.interactable = equipmentUpgrades != null && equipmentUpgrades.GetSellValue(selectedInstanceId) > 0 || pendingAction == InventoryAction.Sell;
                dismantleButton.interactable = equipmentUpgrades != null && equipmentUpgrades.GetDismantleYield(selectedInstanceId) > 0 || pendingAction == InventoryAction.Dismantle;
                upgradeButton.GetComponentInChildren<TMP_Text>().SetText(pendingAction == InventoryAction.Upgrade ? "Confirm" : "Upgrade");
                sellButton.GetComponentInChildren<TMP_Text>().SetText(pendingAction == InventoryAction.Sell ? "Confirm" : "Sell");
                dismantleButton.GetComponentInChildren<TMP_Text>().SetText(pendingAction == InventoryAction.Dismantle ? "Confirm" : "Dismantle");
            }
            else if (selectedIsGem)
            {
                lockButton.gameObject.SetActive(false);
                GemInstance gem = FindGem(selectedInstanceId);
                GemInstance donor = null;
                int goldCost = 0;
                bool canUpgrade = gem != null && gemUpgrades != null && gemUpgrades.TryGetUpgradeInfo(
                    selectedInstanceId, out _, out donor, out goldCost, out _);
                ItemDefinition catalyst = gem == null || gem.Level <= 1 ? null : GetSelectedCatalyst();
                float successChance = gem != null && gemUpgrades?.Rules != null
                    ? gemUpgrades.Rules.GetSuccessChance(gem.Level, catalyst)
                    : 0f;
                bool protectionAvailable = gem != null && gem.Level > 1 && gemUpgrades != null && gemUpgrades.CanUseProtectionStone();
                bool catalystAvailable = catalyst != null && inventory.GetQuantity(catalyst) > 0;
                bool affordable = canUpgrade && wallet != null && wallet.Gold >= goldCost &&
                                  (!useGemProtection || protectionAvailable) &&
                                  (catalyst == null || catalystAvailable);
                string donorText = canUpgrade ? $"Donor: {donor.Definition.DisplayName} Lv1 (exact variant)" : "Requires another exact same Gem at Lv1.";
                string optionText = gem != null && gem.Level == 1
                    ? "Lv1 → Lv2 is guaranteed; materials do not affect this attempt."
                    : $"Success chance: {successChance:P0}\nFailure: donor and Gold are consumed; main Gem loses 1 level{(useGemProtection ? " (protected)" : "")}.";
                detailsText.SetText($"Category: Gem\nRarity: {selectedDefinition.Rarity}\nLevel: {gem?.Level ?? 1}\nStat family: {(gem == null ? "Unknown" : gem.Definition.StatFamily.ToString())}\n\n{donorText}\nGold cost: {(canUpgrade ? goldCost.ToString("N0") : "—")} (you have {(wallet == null ? 0 : wallet.Gold):N0})\n{optionText}{(string.IsNullOrEmpty(gemUpgradeMessage) ? "" : "\n\n" + gemUpgradeMessage)}");

                if (!string.IsNullOrEmpty(installationEquipmentInstanceId) && installationSocketIndex >= 0 && gemInstallation != null)
                {
                    bool canInstall = gemInstallation.TryGetInstallationInfo(installationEquipmentInstanceId,
                        installationSocketIndex, selectedInstanceId, out EquipmentInstance targetEquipment,
                        out _, out int installationCost, out string reason);
                    detailsText.SetText($"{detailsText.text}\n\nInstall into: {targetEquipment?.Definition.DisplayName ?? "selected Equipment"}, socket {installationSocketIndex + 1}\nInstall cost: {installationCost:N0} Gold\n{(canInstall ? "Guaranteed installation." : reason)}");
                    installGemButton.gameObject.SetActive(true);
                    installGemButton.interactable = canInstall || pendingAction == InventoryAction.InstallGem;
                    installGemButton.GetComponentInChildren<TMP_Text>().SetText(pendingAction == InventoryAction.InstallGem ? "Confirm" : "Install Gem");
                }

                upgradeButton.interactable = affordable || pendingAction == InventoryAction.GemUpgrade;
                upgradeButton.GetComponentInChildren<TMP_Text>().SetText(pendingAction == InventoryAction.GemUpgrade ? "Confirm" : "Upgrade Gem");
                sellButton.GetComponentInChildren<TMP_Text>().SetText(
                    gem != null && gem.Level == 1 ? "Protection unused at Lv1" :
                    $"Protection: {(useGemProtection ? "On" : "Off")} ({(protectionAvailable ? "available" : "none")})");
                sellButton.interactable = gem != null && gem.Level > 1 && (protectionAvailable || useGemProtection);
                dismantleButton.GetComponentInChildren<TMP_Text>().SetText(
                    catalyst == null ? "Catalyst: None" : $"Catalyst: {catalyst.DisplayName} ({inventory.GetQuantity(catalyst)})");
                dismantleButton.interactable = gem != null && gem.Level > 1 && gemUpgrades != null && gemUpgrades.GetCatalystCount() > 0;
            }
            else
            {
                lockButton.gameObject.SetActive(false);
                sellButton.gameObject.SetActive(false);
                dismantleButton.gameObject.SetActive(false);
                string preview = null;
                bool exchangeAvailable = equipmentUpgrades != null && equipmentUpgrades.TryGetExchangeInfo(selectedDefinition, out preview);
                upgradeButton.gameObject.SetActive(exchangeAvailable);
                upgradeButton.interactable = exchangeAvailable || pendingAction == InventoryAction.Exchange;
                upgradeButton.GetComponentInChildren<TMP_Text>().SetText(pendingAction == InventoryAction.Exchange ? "Confirm" : "Exchange");
                if (exchangeAvailable)
                    detailsText.SetText($"Category: Materials\nRarity: {selectedDefinition.Rarity}\nQuantity: {inventory.GetQuantity(selectedDefinition):N0}\n\nFree upward exchange (auto-chain):\n{preview}");
            }
        }

        private void RequestAction(InventoryAction action)
        {
            if (action == InventoryAction.GemUpgrade && gemUpgrades == null) return;
            if (action == InventoryAction.InstallGem && gemInstallation == null) return;
            if (action != InventoryAction.GemUpgrade && action != InventoryAction.InstallGem && equipmentUpgrades == null) return;
            if (pendingAction != action)
            {
                pendingAction = action;
                RefreshDetails();
                return;
            }
            bool success;
            if (action == InventoryAction.GemUpgrade)
            {
                success = gemUpgrades.TryUpgrade(selectedInstanceId, useGemProtection,
                    GetSelectedCatalyst(), out GemUpgradeResult result, out float chance);
                gemUpgradeMessage = success
                    ? result switch
                    {
                        GemUpgradeResult.Success => "Upgrade succeeded.",
                        GemUpgradeResult.FailedProtected => "Upgrade failed; the protection stone prevented level loss.",
                        GemUpgradeResult.FailedDowngraded => "Upgrade failed; the main Gem lost one level.",
                        _ => "Upgrade failed."
                    }
                    : result == GemUpgradeResult.SaveFailed ? "Upgrade could not be saved; no costs were kept." :
                        $"Upgrade unavailable. Chance: {chance:P0}; check the donor, materials and Gold.";
            }
            else if (action == InventoryAction.InstallGem)
            {
                success = gemInstallation.TryInstall(installationEquipmentInstanceId, installationSocketIndex, selectedInstanceId);
                if (success)
                {
                    selectedCategory = ItemCategory.Equipment;
                    selectedDefinition = null;
                    selectedInstanceId = null;
                    selectedIsEquipment = false;
                    selectedIsGem = false;
                    ClearInstallationSelection();
                }
            }
            else success = action switch
            {
                InventoryAction.Upgrade => equipmentUpgrades.TryUpgrade(selectedInstanceId),
                InventoryAction.Sell => equipmentUpgrades.TrySell(selectedInstanceId),
                InventoryAction.Dismantle => equipmentUpgrades.TryDismantle(selectedInstanceId),
                InventoryAction.Exchange => equipmentUpgrades.TryExchange(selectedDefinition),
                _ => false
            };
            pendingAction = InventoryAction.None;
            if (success) Refresh(); else RefreshDetails();
            if (selectedIsGem) RefreshDetails();
        }

        private void ToggleGemProtection()
        {
            GemInstance gem = FindGem(selectedInstanceId);
            if (gem == null || gem.Level <= 1 || gemUpgrades == null ||
                (!useGemProtection && !gemUpgrades.CanUseProtectionStone())) return;
            useGemProtection = !useGemProtection;
            gemUpgradeMessage = null;
            pendingAction = InventoryAction.None;
            RefreshDetails();
        }

        private void CycleGemCatalyst()
        {
            GemInstance gem = FindGem(selectedInstanceId);
            if (gem == null || gem.Level <= 1 || gemUpgrades == null) return;
            int count = gemUpgrades.GetCatalystCount();
            int nextIndex = -1;
            for (int step = 1; step <= count; step++)
            {
                int candidateIndex = (selectedCatalystIndex + step + count) % count;
                ItemDefinition candidate = gemUpgrades.GetCatalyst(candidateIndex);
                if (candidate != null && inventory.GetQuantity(candidate) > 0)
                {
                    nextIndex = candidateIndex;
                    break;
                }
            }
            selectedCatalystIndex = nextIndex;
            gemUpgradeMessage = null;
            pendingAction = InventoryAction.None;
            RefreshDetails();
        }

        private ItemDefinition GetSelectedCatalyst()
        {
            return gemUpgrades == null ? null : gemUpgrades.GetCatalyst(selectedCatalystIndex);
        }

        private void ResetGemUpgradeOptions()
        {
            useGemProtection = false;
            selectedCatalystIndex = -1;
            gemUpgradeMessage = null;
        }

        private void SelectInstallationSocket(int socketIndex)
        {
            EquipmentInstance equipment = FindEquipment(installationEquipmentInstanceId);
            if (equipment?.Definition == null || socketIndex < 0 || socketIndex >= EquipmentInstance.SocketCount ||
                equipment.HasSocketContent(socketIndex) || string.IsNullOrWhiteSpace(equipment.Definition.EquipmentSlotId))
                return;

            installationSocketIndex = socketIndex;
            pendingAction = InventoryAction.None;
            RefreshDetails();
        }

        private void ClearInstallationSelection()
        {
            installationEquipmentInstanceId = null;
            installationSocketIndex = -1;
        }

        private EquipmentInstance FindEquipment(string id)
        {
            foreach (EquipmentInstance item in inventory.Equipment) if (item != null && item.InstanceId == id) return item;
            return null;
        }

        private GemInstance FindGem(string id)
        {
            foreach (GemInstance item in inventory.Gems) if (item != null && item.InstanceId == id) return item;
            return null;
        }

        private void ToggleLock()
        {
            EquipmentInstance item = FindEquipment(selectedInstanceId);
            if (item == null) return;
            pendingAction = InventoryAction.None;
            inventory.SetEquipmentLocked(item.InstanceId, !item.IsLocked);
        }

        private static TMP_Text MakeText(string name, Transform parent, string value, Vector2 min, Vector2 max, TMP_FontAsset font, float size, TextAlignmentOptions alignment)
        {
            RectTransform rect = MakeRect(name, parent, min, max, Vector2.zero, Vector2.zero);
            TMP_Text text = rect.gameObject.AddComponent<TextMeshProUGUI>(); text.font = font; text.text = value; text.fontSize = size; text.color = Color.white; text.alignment = alignment; return text;
        }

        private static Button MakeButton(string name, Transform parent, string value, Vector2 min, Vector2 max, Vector2 anchored, Vector2 size, TMP_FontAsset font, float fontSize)
        {
            RectTransform rect = MakeRect(name, parent, min, max, anchored, size);
            Image image = rect.gameObject.AddComponent<Image>(); image.color = new Color(.18f, .24f, .26f, .98f);
            Button button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = image;
            ColorBlock colors = button.colors; colors.highlightedColor = new Color(.32f, .42f, .42f); colors.pressedColor = new Color(.4f, .52f, .48f); button.colors = colors;
            RectTransform labelRect = MakeRect("Label", rect, new Vector2(.04f, .05f), new Vector2(.96f, .95f), Vector2.zero, Vector2.zero);
            TMP_Text label = labelRect.gameObject.AddComponent<TextMeshProUGUI>(); label.font = font; label.text = value; label.fontSize = fontSize; label.color = Color.white; label.alignment = TextAlignmentOptions.Center; label.raycastTarget = false;
            return button;
        }

        private static RectTransform MakeRect(string name, Transform parent, Vector2 min, Vector2 max, Vector2 anchored, Vector2 size)
        {
            GameObject go = new(name, typeof(RectTransform)); go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>(); rect.anchorMin = min; rect.anchorMax = max; rect.anchoredPosition = anchored; rect.sizeDelta = size; return rect;
        }
    }
}
