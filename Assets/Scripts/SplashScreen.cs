using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SplashScreen : MonoBehaviour
{
    [SerializeField] private GameObject panelSplash;
    [SerializeField] private Image barraLoading;  // Image con Image Type = Filled
    [SerializeField] private float duracion = 3f;

    private void Start()
    {
        panelSplash.SetActive(true);
        StartCoroutine(Loading());
    }

    private IEnumerator Loading()
    {
        float tiempo = 0f;
        barraLoading.fillAmount = 0f;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            barraLoading.fillAmount = tiempo / duracion;
            yield return null;
        }

        barraLoading.fillAmount = 1f;
        panelSplash.SetActive(false);
    }
}