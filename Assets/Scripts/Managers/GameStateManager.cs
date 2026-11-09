using System;
using VContainer.Unity;

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
    
    public void EnterMainMenu()
    {
        if (CurrentState == GameState.MainMenu)
            return;
        CurrentState = GameState.MainMenu;
        StateChanged?.Invoke(CurrentState);
    }

    public void StartGame()
    {
        if (CurrentState == GameState.Game)
            return;
        CurrentState = GameState.Game;
        StateChanged?.Invoke(CurrentState);
    }
}