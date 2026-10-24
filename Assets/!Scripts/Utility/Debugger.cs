using System.Collections.Generic;
using System.Linq;
using System.Text;
using _Scripts.Dices;
using _Scripts.Managers;
using UnityEngine;
using VContainer;

public class Debugger : MonoBehaviour
{
    [SerializeField] private bool _enableDebug = true;

    private ResourceManager _resourceManager;
    private ScoreManager _scoreManager;
    private DiceRoller _diceRoller;
    private TurnFlowController _turnFlowController;

    private readonly StringBuilder _sessionMarkdown = new();
    private int _turnIndex;
    private int _subActionIndex;
    private bool _hasActiveSession;

    #region DI
    [Inject]
    public void Construct(
        ResourceManager resourceManager, 
        ScoreManager scoreManager, 
        DiceRoller diceRoller,
        TurnFlowController turnFlowController)
    {
        _resourceManager = resourceManager;
        _scoreManager = scoreManager;
        _diceRoller = diceRoller;
        _turnFlowController = turnFlowController;

        _diceRoller.OnRollCompleted += HandleRollCompleted;
        _turnFlowController.OnScoreCalculated += HandleScoreCalculated;
    }
    #endregion
    // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

    private void Start() => StartNewSession();

    private void OnDestroy()
    {
        if (_diceRoller != null)
            _diceRoller.OnRollCompleted -= HandleRollCompleted;

        if (_turnFlowController != null)
            _turnFlowController.OnScoreCalculated -= HandleScoreCalculated;
    }
    
    private void OnApplicationQuit()
    {
        if (!_enableDebug) return;
        CopyFullSessionToClipboard();
        ClearSession();
    }

    [ContextMenu("Start New Session Log")]
    public void StartNewSession()
    {
        _turnIndex = 1;
        _subActionIndex = 0;
        _hasActiveSession = true;

        _sessionMarkdown.Clear();
        _sessionMarkdown.AppendLine("### Game Session Log\n");
        _sessionMarkdown.AppendLine("| Turn | Action | Dices | Hand / Score | Total / Quota | Dices / Rerolls |");
        _sessionMarkdown.AppendLine("| :-: | :--- | :---: | :--- | :---: | :---: |");

        if (_enableDebug)
            Debug.Log("[Debugger] New Markdown session started.");
    }

    private void HandleRollCompleted(bool isFirstThrow, List<Dice> dices)
    {
        if (!_enableDebug) return;

        if (!_hasActiveSession)
            StartNewSession();

        _subActionIndex++;
        string actionName = isFirstThrow ? "Throw" : "Reroll";
        string diceValues = string.Join(", ", dices.Where(d => d != null).Select(FormatDice));

        _sessionMarkdown.AppendLine(
            $"| #{_turnIndex}.{_subActionIndex} | {actionName} | `[{diceValues}]` | — | — | {_resourceManager.DiceCount} / {_resourceManager.RerollsCount} |");

        Debug.Log($"[Debugger] Turn #{_turnIndex}.{_subActionIndex} ({actionName}): [{diceValues}]");
    }

    private void HandleScoreCalculated(ScoreSequencePlan scoreSequence, List<Dice> playedDices)
    {
        if (!_enableDebug) return;

        if (!_hasActiveSession)
            StartNewSession();

        int projectedScore = scoreSequence.TotalScore + _scoreManager.CurrentScore;
        int quota = _scoreManager.ScoreQuota;

        string combinationName = scoreSequence.Combination != null 
            ? scoreSequence.Combination.DisplayName.GetLocalizedString() 
            : "None";

        string playedValues = string.Join(", ", playedDices.Where(d => d != null).Select(FormatDice));
    
        string formula = $"{scoreSequence.TotalPoints} × {scoreSequence.TotalMultiplier:0.#}";

        _sessionMarkdown.AppendLine(
            $"| #{_turnIndex} | **SCORE** | Played: `[{playedValues}]` | **{combinationName}** (+{scoreSequence.TotalScore}: {formula}) | {projectedScore}/{quota} | {_resourceManager.DiceCount} / {_resourceManager.RerollsCount} |");
        _sessionMarkdown.AppendLine("| --- | --- | --- | --- | --- | --- |");

        Debug.Log($"[Debugger] Turn #{_turnIndex} SCORED: {combinationName} (+{scoreSequence.TotalScore}: {formula}) | Played: [{playedValues}]");

        _turnIndex++;
        _subActionIndex = 0;
    }

    private static string FormatDice(Dice dice)
    {
        var data = dice.Data;
        int val = data.RolledValue;

        if (data.DamageLevel > 0)
            return $"{val}(x{data.DamageLevel})";

        if (data.CurrentDurability < data.MaxDurability)
            return $"{val}({data.CurrentDurability}hp)";

        return val.ToString();
    }

    [ContextMenu("Copy Full Session to Clipboard")]
    public void CopyFullSessionToClipboard()
    {
        string result = _sessionMarkdown.ToString();
        GUIUtility.systemCopyBuffer = result;
        Debug.Log($"[Debugger] Session log copied to clipboard!\n{result}");
    }

    [ContextMenu("Clear Session Log")]
    public void ClearSession()
    {
        _sessionMarkdown.Clear();
        _turnIndex = 1;
        _subActionIndex = 0;
        _hasActiveSession = false;
        Debug.Log("[Debugger] Session log cleared.");
    }
}