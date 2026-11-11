using _Scripts.Interfaces;
using UnityEngine;

namespace _Scripts.Dices
{
    public class DiceModelDisplayView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [SerializeField] private DiceFaceView[] _faceViews;
        
        private IReadOnlyDiceData _data;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        public void Bind(IReadOnlyDiceData data)
        {
            _data = data;
            _data.OnDataChanged += UpdateFaces;
            UpdateFaces();
        }

        private void OnDestroy()
        {
            if (_data != null)
                _data.OnDataChanged -= UpdateFaces;
        }

        private void UpdateFaces()
        {
            for (var i = 0; i < _faceViews.Length; i++)
            {
                var value = _data.GetFaceValue(i);
                _faceViews[i].SetValue(value);
                _faceViews[i].SetCracksLevel(_data.CurrentDurability, _data.MaxDurability);
            }
        }
    }
}