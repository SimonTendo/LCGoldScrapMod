using UnityEngine;

public class GoldStoreItem : MonoBehaviour
{
    private void Start()
    {
        GrabbableObject item = GetComponent<GrabbableObject>();
        if (item != null && item.itemProperties.isConductiveMetal)
        {
            General.AddMetalObjectsRuntime(item);
        }
    }
}
