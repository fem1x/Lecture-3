using _Scripts.UI;
using Cysharp.Threading.Tasks;

public class TransitionService
{
    // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
    private readonly GameStateManager _gameStateManager;
    private readonly ScreenFadeView _screenFadeView;
    
    private int _transitionId;
    // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
    
    public TransitionService(GameStateManager gameStateManager, ScreenFadeView screenFadeView)
    {
        _gameStateManager = gameStateManager;
        _screenFadeView = screenFadeView;
    }
    
    public async UniTaskVoid TransitionToStateAsync(GameState newState)
    {
        if (_gameStateManager.CurrentState == newState) return;

        _transitionId++;
        var currentId = _transitionId;

        await _screenFadeView.FadeInAsync();
        
        if (_transitionId != currentId) return;

        _gameStateManager.ChangeState(newState);
        
        await _screenFadeView.FadeOutAsync();
    }
}