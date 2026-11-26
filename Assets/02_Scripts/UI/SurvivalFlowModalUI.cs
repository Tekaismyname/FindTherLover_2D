using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using FindTheLover.Augments;
using FindTheLover.Combat;
using FindTheLover.Core;
using FindTheLover.Environment;
using FindTheLover.Player;

namespace FindTheLover.UI
{
    /// <summary>
    /// Presentation Layer: Singleplayer v1.0 Survival Flow Controller managing:
    /// 1. Pause Menu toggled via ESC (Resume, Save JSON, Load JSON, Quit to Desktop).
    /// 2. Game Over / Campfire Respawn modal triggered upon player death (if Immortal Bond did not revive).
    /// 100% Standalone Windows 64-bit (.exe) compatible.
    /// </summary>
    public class SurvivalFlowModalUI : MonoBehaviour
    {
        [Header("Pause Menu References")]
        [SerializeField] private GameObject _pauseBackdrop;
        [SerializeField] private TextMeshProUGUI _pauseStatsText;
        [SerializeField] private TextMeshProUGUI _pauseFeedbackText;
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _saveGameButton;
        [SerializeField] private Button _loadGameButton;
        [SerializeField] private Button _pauseQuitButton;

        [Header("Game Over / Respawn References")]
        [SerializeField] private GameObject _gameOverBackdrop;
        [SerializeField] private TextMeshProUGUI _gameOverStatsText;
        [SerializeField] private Button _respawnCampfireButton;
        [SerializeField] private Button _gameOverLoadButton;
        [SerializeField] private Button _gameOverQuitButton;

        private HealthSystem _playerHealth;
        private PlayerRespawnController _respawnController;
        private AugmentSelectionModalUI _augmentModal;
        private CraftingModalUI _craftingModal;

        private bool _isPaused;
        private bool _isGameOverOpen;
        private CursorLockMode _prevCursorLock;
        private bool _prevCursorVisible;

        public bool IsPaused => _isPaused;
        public bool IsGameOverOpen => _isGameOverOpen;

        private void Start()
        {
            if (_pauseBackdrop != null) _pauseBackdrop.SetActive(false);
            if (_gameOverBackdrop != null) _gameOverBackdrop.SetActive(false);
            if (_pauseFeedbackText != null) _pauseFeedbackText.text = string.Empty;

            BindButtons();
            LocatePlayerSystems();
        }

        private void OnDestroy()
        {
            UnbindButtons();
            if (_playerHealth != null) _playerHealth.OnDeath -= HandlePlayerDeath;
            if (_isPaused || _isGameOverOpen) Time.timeScale = 1f;
        }

        private void Update()
        {
            if (_isGameOverOpen) return;
            if (Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame) return;

            // Do not open Pause Menu on top of Crafting Forge or TFT Augment Selection
            if (_augmentModal == null) _augmentModal = Object.FindAnyObjectByType<AugmentSelectionModalUI>(FindObjectsInactive.Include);
            if (_craftingModal == null) _craftingModal = Object.FindAnyObjectByType<CraftingModalUI>(FindObjectsInactive.Include);

            if ((_augmentModal != null && _augmentModal.IsOpen) || (_craftingModal != null && _craftingModal.IsOpen))
            {
                return;
            }

            if (_isPaused) ResumeGame();
            else OpenPauseMenu();
        }

        public void OpenPauseMenu()
        {
            if (_pauseBackdrop == null || _isGameOverOpen) return;

            _isPaused = true;
            _pauseBackdrop.SetActive(true);
            RefreshSummaryText(_pauseStatsText);
            if (_pauseFeedbackText != null) _pauseFeedbackText.text = string.Empty;

            _prevCursorLock = Cursor.lockState;
            _prevCursorVisible = Cursor.visible;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 0f;
        }

        public void ResumeGame()
        {
            _isPaused = false;
            if (_pauseBackdrop != null) _pauseBackdrop.SetActive(false);

            Time.timeScale = 1f;
            Cursor.lockState = _prevCursorLock;
            Cursor.visible = _prevCursorVisible;
        }

        public void OpenGameOverScreen()
        {
            if (_gameOverBackdrop == null) return;

            _isPaused = false;
            if (_pauseBackdrop != null) _pauseBackdrop.SetActive(false);

            _isGameOverOpen = true;
            _gameOverBackdrop.SetActive(true);
            RefreshSummaryText(_gameOverStatsText);

            _prevCursorLock = Cursor.lockState;
            _prevCursorVisible = Cursor.visible;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 0f;
        }

        public void CloseGameOverScreen()
        {
            _isGameOverOpen = false;
            if (_gameOverBackdrop != null) _gameOverBackdrop.SetActive(false);
            Time.timeScale = 1f;
            Cursor.lockState = _prevCursorLock;
            Cursor.visible = _prevCursorVisible;
        }

        private void HandlePlayerDeath()
        {
            StartCoroutine(DelayedGameOverCheckRoutine());
        }

        private IEnumerator DelayedGameOverCheckRoutine()
        {
            // Wait 1 frame so Immortal Lover Bond augment can intercept and revive Quene first
            yield return null;
            if (_playerHealth != null && !_playerHealth.IsDead)
            {
                yield break;
            }
            OpenGameOverScreen();
        }

        private void OnSaveClicked()
        {
            if (GameDataManager.Instance != null)
            {
                bool ok = GameDataManager.Instance.SaveToDisk();
                if (_pauseFeedbackText != null)
                {
                    _pauseFeedbackText.text = ok
                        ? "<color=#55FF99>GAME SAVED TO DISK!</color>"
                        : "<color=#FF6666>SAVE FAILED!</color>";
                }
            }
        }

        private void OnLoadClicked()
        {
            if (GameDataManager.Instance != null)
            {
                Time.timeScale = 1f;
                bool ok = GameDataManager.Instance.LoadFromDisk();
                if (ok)
                {
                    CloseGameOverScreen();
                    ResumeGame();
                }
                else if (_pauseFeedbackText != null)
                {
                    _pauseFeedbackText.text = "<color=#FFAA44>NO SAVE FILE FOUND YET.</color>";
                }
            }
        }

        private void OnRespawnCampfireClicked()
        {
            LocatePlayerSystems();
            CloseGameOverScreen();
            if (_respawnController != null)
            {
                _respawnController.RespawnNowAtCampfire();
            }
            else if (_playerHealth != null)
            {
                _playerHealth.SetMaxHealth(_playerHealth.MaxHealth, resetCurrentHealth: true);
            }
        }

        private void OnQuitClicked()
        {
            Time.timeScale = 1f;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void RefreshSummaryText(TextMeshProUGUI targetText)
        {
            if (targetText == null) return;

            int day = DayNightCycleManager.Instance != null ? DayNightCycleManager.Instance.CurrentDay : 1;
            int seed = SeededWorldTerrainManager.Instance != null ? SeededWorldTerrainManager.Instance.WorldSeed : 20261119;
            int augCount = AugmentManager.Instance != null ? AugmentManager.Instance.TotalActiveAugmentCount : 0;

            targetText.text = $"DAY {day} / 20   |   WORLD SEED: {seed}   |   AUGMENTS: {augCount}";
        }

        private void LocatePlayerSystems()
        {
            if (_playerHealth == null)
            {
                GameObject player = GameObject.Find("Quene_Player") ?? GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    _playerHealth = player.GetComponentInChildren<HealthSystem>() ?? player.GetComponent<HealthSystem>();
                    _respawnController = player.GetComponentInChildren<PlayerRespawnController>() ?? player.GetComponent<PlayerRespawnController>();
                    if (_playerHealth != null)
                    {
                        _playerHealth.OnDeath -= HandlePlayerDeath;
                        _playerHealth.OnDeath += HandlePlayerDeath;
                    }
                }
            }
        }

        private void BindButtons()
        {
            if (_resumeButton != null) _resumeButton.onClick.AddListener(ResumeGame);
            if (_saveGameButton != null) _saveGameButton.onClick.AddListener(OnSaveClicked);
            if (_loadGameButton != null) _loadGameButton.onClick.AddListener(OnLoadClicked);
            if (_pauseQuitButton != null) _pauseQuitButton.onClick.AddListener(OnQuitClicked);
            if (_respawnCampfireButton != null) _respawnCampfireButton.onClick.AddListener(OnRespawnCampfireClicked);
            if (_gameOverLoadButton != null) _gameOverLoadButton.onClick.AddListener(OnLoadClicked);
            if (_gameOverQuitButton != null) _gameOverQuitButton.onClick.AddListener(OnQuitClicked);
        }

        private void UnbindButtons()
        {
            if (_resumeButton != null) _resumeButton.onClick.RemoveListener(ResumeGame);
            if (_saveGameButton != null) _saveGameButton.onClick.RemoveListener(OnSaveClicked);
            if (_loadGameButton != null) _loadGameButton.onClick.RemoveListener(OnLoadClicked);
            if (_pauseQuitButton != null) _pauseQuitButton.onClick.RemoveListener(OnQuitClicked);
            if (_respawnCampfireButton != null) _respawnCampfireButton.onClick.RemoveListener(OnRespawnCampfireClicked);
            if (_gameOverLoadButton != null) _gameOverLoadButton.onClick.RemoveListener(OnLoadClicked);
            if (_gameOverQuitButton != null) _gameOverQuitButton.onClick.RemoveListener(OnQuitClicked);
        }
    }
}
