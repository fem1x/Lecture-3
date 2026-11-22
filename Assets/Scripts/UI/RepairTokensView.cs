using _Scripts.Managers;
using _Scripts.Utility.Extensions;
using TMPro;
using UnityEngine;
using VContainer;

namespace _Scripts.UI
{
    public class RepairTokensView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _tokensCountText;

        private RepairTokensManager _repairTokensManager;

        [Inject]
        public void Construct(RepairTokensManager repairTokensManager)
        {
            _repairTokensManager = repairTokensManager;
        }
        

        private void OnEnable()
        {
            _repairTokensManager.OnTokensCountChange += HandleTokensCountChanged;
            _tokensCountText.text = _repairTokensManager.CurrentTokens.ToString();
        }

        private void OnDisable()
        {
            _repairTokensManager.OnTokensCountChange -= HandleTokensCountChanged;
        }

        private void HandleTokensCountChanged(int count)
        {
            _tokensCountText.text = count.ToString();
            _tokensCountText.transform.DoScalePop(scaleMult: 1.2f, outTime: 0.18f);
        }
    }
}
