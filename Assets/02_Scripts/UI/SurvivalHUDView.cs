using TMPro;
using UnityEngine;
using UnityEngine.UI;
using FindTheLover.Environment;
using FindTheLover.Enemies;
using FindTheLover.Augments;
using FindTheLover.Augments.Data;

namespace FindTheLover.UI
{
    /// <summary>
    /// Presentation Layer: Displays survival marathon progress (DAY X / 20), phase countdown timer,
    /// active TFT augment counter, and a pulsing red warning banner during Night Raids / Calamity Nights.
    /// </summary>
    public class SurvivalHUDView : MonoBehaviour
    {
        [Header("Day & Timer Display")]
        [Tooltip("Text displaying current survival day against target marathon goal (e.g., DAY 1 / 20).")]
        [SerializeField] private TextMeshProUGUI _dayCounterText;

        [Tooltip("Text displaying phase name and remaining countdown seconds until Dusk or Dawn.")]
        [SerializeField] private TextMeshProUGUI _phaseCountdownText;

        [Tooltip("Progress fill bar tracking time within the current phase.")]
        [SerializeField] private Image _phaseProgressBar;

        [Header("Night Raid Warning Banner")]
        [Tooltip("Root container toggled visible when a Night Raid or Calamity Night is active.")]
        [SerializeField] private GameObject _raidWarningBanner;

        [Tooltip("Warning headline text pulsing red during nighttime raids.")]
        [SerializeField] private TextMeshProUGUI _raidWarningText;

        [Tooltip("Banner background image tinted crimson during raids.")]
        [SerializeField] private Image _raidBannerBackground;

        [Header("Augments Summary")]
        [Tooltip("Displays total active TFT augments collected by the player.")]
        [SerializeField] private TextMeshProUGUI _augmentSummaryText;

        [Header("Visual Palette")]
        [SerializeField] private Color _dayBarColor = new Color(1.0f, 0.82f, 0.25f, 1f);
        [SerializeField] private Color _nightBarColor = new Color(0.75f, 0.22f, 0.95f, 1f);
        [SerializeField] private Color _normalRaidColor = new Color(0.78f, 0.14f, 0.14f, 0.85f);
        [SerializeField] private Color _calamityRaidColor = new Color(0.55f, 0.04f, 0.32f, 0.92f);

        private bool _isRaidAlertVisible;
        private bool _isCalamityAlert;

        private void Start()
        {
            SubscribeEvents();
            RefreshStaticCounters();
            HideRaidBanner();
        }

        private void OnDestroy()
        {
            UnsubscribeEvents();
        }

        private void Update()
        {
            UpdateDayAndCountdown();
            AnimateRaidBannerPulse();
        }

        private void SubscribeEvents()
        {
            if (NightRaidDirector.Instance != null)
            {
                NightRaidDirector.Instance.OnRaidStarted += HandleRaidStarted;
                NightRaidDirector.Instance.OnRaidEnded += HandleRaidEnded;
            }

            if (AugmentManager.Instance != null)
            {
                AugmentManager.Instance.OnAugmentSelected += HandleAugmentSelected;
            }
        }

        private void UnsubscribeEvents()
        {
            if (NightRaidDirector.Instance != null)
            {
                NightRaidDirector.Instance.OnRaidStarted -= HandleRaidStarted;
                NightRaidDirector.Instance.OnRaidEnded -= HandleRaidEnded;
            }

            if (AugmentManager.Instance != null)
            {
                AugmentManager.Instance.OnAugmentSelected -= HandleAugmentSelected;
            }
        }

        private void UpdateDayAndCountdown()
        {
            DayNightCycleManager cycle = DayNightCycleManager.Instance;
            if (cycle == null) return;

            int targetDays = NightRaidDirector.Instance != null ? NightRaidDirector.Instance.TargetSurvivalDays : 20;

            if (_dayCounterText != null)
            {
                _dayCounterText.text = $"DAY {cycle.CurrentDay} / {targetDays}";
            }

            float secondsLeft = cycle.GetSecondsUntilNextPhase();
            int mins = Mathf.FloorToInt(secondsLeft / 60f);
            int secs = Mathf.FloorToInt(secondsLeft % 60f);

            if (_phaseCountdownText != null)
            {
                string phaseLabel = cycle.IsNight ? "DAWN IN" : "NIGHT IN";
                _phaseCountdownText.text = $"{phaseLabel} {mins:00}:{secs:00}";
            }

            if (_phaseProgressBar != null)
            {
                _phaseProgressBar.fillAmount = cycle.NormalizedTime;
                _phaseProgressBar.color = cycle.IsNight ? _nightBarColor : _dayBarColor;
            }
        }

        private void AnimateRaidBannerPulse()
        {
            if (!_isRaidAlertVisible || _raidBannerBackground == null) return;

            Color baseColor = _isCalamityAlert ? _calamityRaidColor : _normalRaidColor;
            float pulse = 0.72f + 0.28f * Mathf.Sin(Time.time * (_isCalamityAlert ? 6.5f : 3.5f));
            _raidBannerBackground.color = new Color(baseColor.r, baseColor.g, baseColor.b, baseColor.a * pulse);
        }

        private void HandleRaidStarted(int day, int totalBudget, bool isCalamity)
        {
            _isRaidAlertVisible = true;
            _isCalamityAlert = isCalamity;

            if (_raidWarningBanner != null)
            {
                _raidWarningBanner.SetActive(true);
            }

            if (_raidWarningText != null)
            {
                _raidWarningText.text = isCalamity
                    ? $"CALAMITY NIGHT {day}! BOSS RAID INCOMING!"
                    : $"NIGHT {day} RAID! DEFEND THE CAMP!";
            }
        }

        private void HandleRaidEnded(int survivedDay)
        {
            HideRaidBanner();
        }

        private void HideRaidBanner()
        {
            _isRaidAlertVisible = false;
            _isCalamityAlert = false;
            if (_raidWarningBanner != null)
            {
                _raidWarningBanner.SetActive(false);
            }
        }

        private void HandleAugmentSelected(AugmentDataSO augment, int newStackCount)
        {
            RefreshStaticCounters();
        }

        private void RefreshStaticCounters()
        {
            if (_augmentSummaryText == null) return;
            int total = AugmentManager.Instance != null ? AugmentManager.Instance.TotalActiveAugmentCount : 0;
            _augmentSummaryText.text = $"AUGMENTS: {total}";
        }
    }
}
