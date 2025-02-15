using ArcCreate.SceneTransition;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Gameplay.Audio
{
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField] private Button pauseButton;
        [SerializeField] private RectTransform pauseButtonRect;
        [SerializeField] private Camera uiCamera;
        [SerializeField] private GameObject pauseScreen;
        [SerializeField] private Button playButton;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button returnButton;

        private void Awake()
        {
            pauseButton.onClick.AddListener(OnPauseButton);
            playButton.onClick.AddListener(OnPlayButton);
            retryButton.onClick.AddListener(OnRetryButton);
            returnButton.onClick.AddListener(OnReturnButton);
            Application.focusChanged += OnFocusChange;
        }

        private void OnDestroy()
        {
            pauseButton.onClick.RemoveListener(OnPauseButton);
            playButton.onClick.RemoveListener(OnPlayButton);
            retryButton.onClick.RemoveListener(OnRetryButton);
            returnButton.onClick.RemoveListener(OnReturnButton);
            Application.focusChanged -= OnFocusChange;
        }

        private void OnFocusChange(bool focused)
        {
            if (!focused)
            {
                OnPauseButton();
            }
        }

        private void OnPauseButton()
        {
            // Hacky but whatever
            if (Values.EnablePauseMenu
            && (Services.Audio.IsPlayingAndNotStationary || (Services.Audio.AudioTiming >= Services.Audio.AudioLength - 1000)))
            {
                int touchCount = Input.touchCount;
                for (int i = 0; i < touchCount; i++)
                {
                    var touch = Input.GetTouch(i);
                    if (!RectTransformUtility.RectangleContainsScreenPoint(pauseButtonRect, touch.position, uiCamera))
                    {
                        return;
                    }
                }

                pauseScreen.SetActive(true);
                Services.Audio.Pause();
            }
        }

        private void OnPlayButton()
        {
            pauseScreen.SetActive(false);
            Services.Audio.ResumeWithDelay(Values.DelayBeforeAudioResume, false);
            Services.Judgement.RefreshInputHandler();
            DisablePauseButton().Forget();
        }

        private void OnRetryButton()
        {
            pauseScreen.SetActive(false);
            Services.Judgement.RefreshInputHandler();
            StartRetry().Forget();
        }

        private async UniTask StartRetry()
        {
            ITransition transition = new ShutterTransition(500);
            transition.EnableGameObject();
            await transition.StartTransition();
            Services.Audio.AudioTiming = -Values.DelayBeforeAudioStart;
            await transition.EndTransition();
            if (!pauseScreen.activeInHierarchy)
            {
                Services.Audio.PlayWithDelay(0, Values.DelayBeforeAudioStart);
            }

            await DisablePauseButton();
        }

        private async UniTask DisablePauseButton()
        {
            pauseButton.interactable = false;
            await UniTask.Delay(1000);
            pauseButton.interactable = true;
        }

        private void OnReturnButton()
        {
            SceneTransitionManager.Instance.SetTransition(new ShutterTransition());
            SceneTransitionManager.Instance.SwitchScene(SceneNames.SelectScene).Forget();
        }
    }
}