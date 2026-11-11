using _Scripts.Dices;
using _Scripts.Managers;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

namespace _Scripts.DebugTools
{
    public class GameStateDebugHotkeys : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        private GameStateManager _gameStateManager;

        #region DI
        [Inject]
        public void Construct(GameStateManager gameStateManager)
        {
            _gameStateManager = gameStateManager;
        }
        #endregion
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

#if UNITY_EDITOR
        private void Update()
        {
            if (Keyboard.current == null) return;

            if (Keyboard.current.f9Key.wasPressedThisFrame)
                _gameStateManager.ChangeState(GameState.GameResult, GameEndType.Victory);

            if (Keyboard.current.f8Key.wasPressedThisFrame)
                _gameStateManager.ChangeState(GameState.Game);
        }

        [ContextMenu("Debug / Trigger Victory")]
        private void TriggerVictory() => _gameStateManager.ChangeState(GameState.GameResult, GameEndType.Victory);

        [ContextMenu("Debug / Trigger Game")]
        private void TriggerGame() => _gameStateManager.ChangeState(GameState.Game);
#endif
    }
}