using UnityEngine;

public interface ISfxPlayer
{
    void Initialize();
    void Play(string name, Vector3 spawnPos);
    void Play(string name);
}
