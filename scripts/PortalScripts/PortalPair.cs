using UnityEngine;

[System.Serializable]
public class PortalPair
{
    public int pairId;
    public Portal portalA;
    public Portal portalB;

    public void SetActive(bool active)
    {
        if (portalA != null)
            if (active) portalA.ActivatePortal(); else portalA.DeactivatePortal();
        if (portalB != null)
            if (active) portalB.ActivatePortal(); else portalB.DeactivatePortal();
    }
}