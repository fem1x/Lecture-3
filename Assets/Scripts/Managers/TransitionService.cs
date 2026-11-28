using _Scripts.Dices;
using _Scripts.Managers;
using _Scripts.UI;
using Cysharp.Threading.Tasks;

public class TransitionService
{
    // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
    private readonly GameStateManager _gameStateManager;
    private readonly ScreenFadeView _screenFadeView;
    private readonly MusicPlayer _musicPlayer;
    
    private int _transitionId;
    // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
    
    public TransitionService(GameStateManager gameStateManager, ScreenFadeView screenFadeView, MusicPlayer musicPlayer)
    {
        _gameStateManager = gameStateManager;
        _screenFadeView = screenFadeView;
        _musicPlayer = musicPlayer;
    }
    
    public async UniTaskVoid TransitionToStateAsync(GameState newState)
    {
        if (_gameStateManager.CurrentState == newState) return;

        _transitionId++;
        var currentId = _transitionId;

        if (newState == GameState.MainMenu)
            _musicPlayer.FadeOutGameplay(_screenFadeView.FadeInDuration);

        await _screenFadeView.FadeInAsync();
        
        if (_transitionId != currentId) return;

        _gameStateManager.ChangeState(newState);
        
        if (newState != GameState.MainMenu)
            await _screenFadeView.FadeOutAsync();
    }
}
