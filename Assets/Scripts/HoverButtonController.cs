using UnityEngine;
using UnityEngine.EventSystems;

public class HoverButtonController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject buttonObject; // Arrastra aquí tu botón

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Se activa al entrar el mouse
        buttonObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Se desactiva al salir el mouse
        buttonObject.SetActive(false);
    }
}