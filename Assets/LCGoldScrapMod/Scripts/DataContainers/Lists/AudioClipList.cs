using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "LCGoldScrapMod/List/AudioClipList")]
public class AudioClipList : ScriptableObject
{
    public List<AudioClip> allClips = new List<AudioClip>();
}
