using System;
using _Scripts.Dices;

namespace _Scripts.Managers
{
    public class GameStateManager
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        public event Action<GameState> StateChanged;
        
        public GameState CurrentState { get; private set; } = GameState.MainMenu;
        public GameEndType LastGameResult { get; private set; }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        
        public void EnterMenuState() => ChangeState(GameState.MainMenu);
        public void EnterGameState() => ChangeState(GameState.Game);

        public void EnterGameResultState(GameEndType resultType)
        {
            LastGameResult = resultType;
            ChangeState(GameState.GameResult);
        }

        public void ChangeState(GameState newState, GameEndType? resultType = null)
        {
            if (resultType.HasValue)
                LastGameResult = resultType.Value;

            if (CurrentState == newState) return;
            
            CurrentState = newState;
            StateChanged?.Invoke(newState);
        }
    }
}