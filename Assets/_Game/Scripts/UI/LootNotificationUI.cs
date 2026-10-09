using System.Collections.Generic;
using LootboundIsles.Inventory;
using LootboundIsles.Loot;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LootboundIsles.UI
{
    public sealed class LootNotificationUI : MonoBehaviour
    {
        private const int MaximumVisibleNotifications = 4;
        private const float NotificationLifetime = 3.5f;
        private static readonly Color PanelColor = new(0.055f, 0.075f, 0.09f, 0.92f);
        private static readonly Color TextColor = new(0.94f, 0.95f, 0.92f, 1f);

        private readonly List<Notification> notifications = new();
        private LootRewardService rewardService;
        private RectTransform listRoot;

        private sealed class Notification
        {
            public GameObject Root;
            public TMP_Text Text;
            public float Remaining;
        }

        private void Start()
        {
            Canvas canvas = GetComponent<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("LootNotificationUI must be attached to the existing GameUI Canvas.", this);
                enabled = false;
                return;
            }

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            rewardService = player != null ? player.GetComponent<LootRewardService>() : null;
            if (rewardService == null)
            {
                Debug.LogError("LootNotificationUI could not find LootRewardService on Player.", this);
                enabled = false;
                return;
            }

            BuildList(canvas.transform as RectTransform);
            rewardService.LootGranted += Show;
        }

        private void OnDestroy()
        {
            if (rewardService != null)
                rewardService.LootGranted -= Show;
        }

        private void Update()
        {
            for (int i = notifications.Count - 1; i >= 0; i--)
            {
                Notification notification = notifications[i];
                notification.Remaining -= Time.unscaledDeltaTime;
                if (notification.Remaining <= 0f)
                {
                    Destroy(notification.Root);
                    notifications.RemoveAt(i);
                }
            }
        }

        private void BuildList(RectTransform canvasRoot)
        {
            GameObject root = new("LootNotifications");
            root.transform.SetParent(canvasRoot, false);
            listRoot = root.AddComponent<RectTransform>();
            listRoot.anchorMin = new Vector2(1f, 1f);
            listRoot.anchorMax = new Vector2(1f, 1f);
            listRoot.pivot = new Vector2(1f, 1f);
            listRoot.anchoredPosition = new Vector2(-28f, -96f);
            listRoot.sizeDelta = new Vector2(360f, 210f);
            VerticalLayoutGroup layout = root.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.UpperRight;
            layout.spacing = 6f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
        }

        private void Show(string label, ItemRarity rarity, bool isItem)
        {
            if (notifications.Count >= MaximumVisibleNotifications)
            {
                Notification oldest = notifications[0];
                Destroy(oldest.Root);
                notifications.RemoveAt(0);
            }

            GameObject row = new("LootNotification");
            row.transform.SetParent(listRoot, false);
            RectTransform rect = row.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(360f, 42f);
            LayoutElement element = row.AddComponent<LayoutElement>();
            element.preferredHeight = 42f;
            Image background = row.AddComponent<Image>();
            background.color = PanelColor;
            background.raycastTarget = false;

            GameObject textObject = new("Text");
            textObject.transform.SetParent(row.transform, false);
            RectTransform textRect = textObject.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(14f, 2f);
            textRect.offsetMax = new Vector2(-12f, -2f);
            TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
            text.text = label;
            text.fontSize = rarity == ItemRarity.Legendary ? 22f : 18f;
            text.fontStyle = isItem && rarity >= ItemRarity.Rare ? FontStyles.Bold : FontStyles.Normal;
            text.alignment = TextAlignmentOptions.MidlineRight;
            text.color = RarityColor(rarity);
            text.raycastTarget = false;

            notifications.Add(new Notification { Root = row, Text = text, Remaining = NotificationLifetime });
        }

        private static Color RarityColor(Inventory.ItemRarity rarity)
        {
            return rarity switch
            {
                ItemRarity.Uncommon => new Color(0.52f, 0.9f, 0.55f),
                ItemRarity.Rare => new Color(0.4f, 0.7f, 1f),
                ItemRarity.Epic => new Color(0.78f, 0.52f, 1f),
                ItemRarity.Legendary => new Color(1f, 0.72f, 0.25f),
                _ => TextColor
            };
        }
    }
}
