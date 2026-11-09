using System;

public enum GameState
{
    None,
    MainMenu,
    Game
}

public class GameStateManager
{
    // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
    public event Action<GameState> StateChanged;
    public GameState CurrentState { get; private set; }
    // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
    
    public void ChangeState(GameState newState)
    {
        if (CurrentState == newState) return;
        
        CurrentState = newState;
        StateChanged?.Invoke(CurrentState);
    }
    
    public void EnterMenuState() => ChangeState(GameState.MainMenu);
    public void EnterGameState() => ChangeState(GameState.Game);
}