using UnityEngine;

public class PanelSwitcher : MonoBehaviour
{
    [SerializeField] private GameObject panelToHide;
    [SerializeField] private GameObject panelToShow;

    public void Switch()
    {
        panelToHide.SetActive(false);
        panelToShow.SetActive(true);
    }
}