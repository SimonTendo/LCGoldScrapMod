using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "LCGoldScrapMod/ArtOfGoldMaterials", order = 1)]
public class ArtOfGoldMaterials : ScriptableObject
{
    public List<Material> allArtwork;
    public List<Material> allSillyArtwork;
}
