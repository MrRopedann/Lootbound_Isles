using System.Collections;
using System.Collections.Generic;
using LootboundIsles.Player;
using LootboundIsles.Upgrades;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LootboundIsles.UI
{
    public class LevelUpUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private UpgradeSystem upgradeSystem;

        [SerializeField]
        private GameObject panel;

        [Header("Respawn")]
        [SerializeField, Min(0f)]
        private float reopenAfterRespawnDelay = 0.25f;

        [Header("Buttons")]
        [SerializeField]
        private Button[] upgradeButtons;

        [Header("Texts")]
        [SerializeField]
        private TMP_Text[] upgradeTexts;

        [Header("DEBUG")]
        [SerializeField]
        private bool isDebug = true;

        private PlayerMovement playerMovement;
        private PlayerDeath playerDeath;
        private Coroutine reopenAfterRespawnCoroutine;

        private void Awake()
        {
            if (upgradeSystem == null)
            {
                Debug.LogError(
                    "LevelUpUI: UpgradeSystem не назначен.",
                    this
                );

                enabled = false;
                return;
            }

            if (panel == null)
            {
                Debug.LogError(
                    "LevelUpUI: Panel не назначен.",
                    this
                );

                enabled = false;
                return;
            }

            playerMovement =
                upgradeSystem.GetComponent<PlayerMovement>();

            playerDeath =
                upgradeSystem.GetComponent<PlayerDeath>();

            if (playerMovement == null)
            {
                Debug.LogError(
                    "LevelUpUI: на объекте UpgradeSystem отсутствует PlayerMovement.",
                    upgradeSystem
                );

                enabled = false;
                return;
            }

            if (playerDeath == null)
            {
                Debug.LogError(
                    "LevelUpUI: на объекте UpgradeSystem отсутствует PlayerDeath.",
                    upgradeSystem
                );

                enabled = false;
                return;
            }

            if (upgradeButtons == null ||
                upgradeButtons.Length < 3)
            {
                Debug.LogError(
                    "LevelUpUI: необходимо назначить 3 кнопки.",
                    this
                );

                enabled = false;
                return;
            }

            if (upgradeTexts == null ||
                upgradeTexts.Length < 3)
            {
                Debug.LogError(
                    "LevelUpUI: необходимо назначить 3 текста.",
                    this
                );

                enabled = false;
                return;
            }

            panel.SetActive(false);

            upgradeButtons[0].onClick.AddListener(
                () => SelectUpgrade(0)
            );

            upgradeButtons[1].onClick.AddListener(
                () => SelectUpgrade(1)
            );

            upgradeButtons[2].onClick.AddListener(
                () => SelectUpgrade(2)
            );
        }

        private void OnEnable()
        {
            if (upgradeSystem == null)
                return;

            upgradeSystem.ChoicesGenerated +=
                ShowChoices;

            playerDeath.DeathStarted +=
                HandlePlayerDeath;

            playerDeath.Respawned +=
                HandlePlayerRespawned;
        }

        private void OnDisable()
        {
            if (upgradeSystem == null)
                return;

            upgradeSystem.ChoicesGenerated -=
                ShowChoices;

            if (playerDeath != null)
            {
                playerDeath.DeathStarted -=
                    HandlePlayerDeath;

                playerDeath.Respawned -=
                    HandlePlayerRespawned;
            }

            if (reopenAfterRespawnCoroutine != null)
            {
                StopCoroutine(reopenAfterRespawnCoroutine);
                reopenAfterRespawnCoroutine = null;
            }
        }

        private void ShowChoices(
            IReadOnlyList<UpgradeDefinition> choices)
        {
            if (choices == null ||
                choices.Count == 0)
            {
                return;
            }

            for (int i = 0;
                 i < upgradeButtons.Length;
                 i++)
            {
                bool hasChoice =
                    i < choices.Count;

                upgradeButtons[i]
                    .gameObject
                    .SetActive(hasChoice);

                if (!hasChoice)
                    continue;

                UpgradeDefinition upgrade =
                    choices[i];

                upgradeTexts[i].text =
                    $"{upgrade.DisplayName}\n" +
                    $"<size=70%>" +
                    $"{upgrade.Description}" +
                    $"</size>";
            }

            panel.SetActive(true);

            playerMovement.enabled = false;

            if (isDebug)
            {
                Debug.Log(
                    "LevelUpUI: окно улучшений открыто. " +
                    $"Ожидающих выборов: " +
                    $"{upgradeSystem.PendingChoices}. " +
                    "Движение Player временно отключено.",
                    this
                );
            }
        }

        private void SelectUpgrade(int index)
        {
            if (!upgradeSystem.IsWaitingForChoice)
                return;

            upgradeSystem.SelectUpgrade(index);

            // UpgradeSystem мог сразу сформировать следующий
            // выбор из очереди.
            if (upgradeSystem.IsWaitingForChoice)
            {
                panel.SetActive(true);

                playerMovement.enabled = false;

                if (isDebug)
                {
                    Debug.Log(
                        "LevelUpUI: есть следующий выбор. " +
                        $"Осталось выборов: " +
                        $"{upgradeSystem.PendingChoices}.",
                        this
                    );
                }

                return;
            }

            panel.SetActive(false);

            playerMovement.enabled = true;

            if (isDebug)
            {
                Debug.Log(
                    "LevelUpUI: все улучшения выбраны. " +
                    "Движение Player снова включено.",
                    this
                );
            }
        }

        private void HandlePlayerDeath()
        {
            if (reopenAfterRespawnCoroutine != null)
            {
                StopCoroutine(reopenAfterRespawnCoroutine);
                reopenAfterRespawnCoroutine = null;
            }

            panel.SetActive(false);
        }

        private void HandlePlayerRespawned()
        {
            if (!upgradeSystem.IsWaitingForChoice)
                return;

            reopenAfterRespawnCoroutine =
                StartCoroutine(ReopenAfterRespawnRoutine());
        }

        private IEnumerator ReopenAfterRespawnRoutine()
        {
            if (reopenAfterRespawnDelay > 0f)
            {
                yield return new WaitForSeconds(
                    reopenAfterRespawnDelay
                );
            }

            reopenAfterRespawnCoroutine = null;

            if (!upgradeSystem.IsWaitingForChoice)
                yield break;

            ShowChoices(upgradeSystem.CurrentChoices);
        }
    }
}
