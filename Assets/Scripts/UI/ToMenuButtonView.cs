using _Scripts.Dices;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace _Scripts.UI
{
    public class ToMenuButtonView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [SerializeField] private Button _button;

        #region DI
        private TransitionService _transitionService;

        [Inject]
        public void Construct(TransitionService transitionService) => _transitionService = transitionService;
        #endregion
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        private void OnEnable() => _button.onClick.AddListener(HandleClick);
        private void OnDisable() => _button.onClick.RemoveListener(HandleClick);

        private void HandleClick() => _transitionService.TransitionToStateAsync(GameState.MainMenu).Forget();
    }
}