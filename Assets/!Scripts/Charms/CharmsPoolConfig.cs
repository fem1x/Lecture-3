using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Charms
{
    [CreateAssetMenu(fileName = "CharmsPoolConfig", menuName = "Configs/CharmsPoolConfig")]
    public class CharmsPoolConfig : ScriptableObject
    {
        [field: SerializeField] public List<CharmConfigBase> CharmsPool { get; private set; }= new();
    }
}