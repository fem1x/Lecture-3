using UnityEngine;

public interface ISfxPlayer
{
    void Initialize();
    void Play(string name, Vector3 spawnPos, float pitchMultiplier = 1f);
    void Play(string name, float pitchMultiplier = 1f);
}
