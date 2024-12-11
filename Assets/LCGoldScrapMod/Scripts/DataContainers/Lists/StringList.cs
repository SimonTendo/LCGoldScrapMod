using UnityEngine;

[CreateAssetMenu(menuName = "LCGoldScrapMod/List/StringList")]
public class StringList : ScriptableObject
{
    [TextArea(1, 20)]
    public string[] allStrings;
}
