using UnityEngine;

public class GoldStoreItem : MonoBehaviour
{
    private void Start()
    {
        if (StartOfRound.Instance.inShipPhase)
        {
            return;
        }
        GrabbableObject item = GetComponent<GrabbableObject>();
        if (item != null && item.itemProperties != null && item.itemProperties.isConductiveMetal)
        {
            General.AddMetalObjectsRuntime(item);
        }
    }
}
