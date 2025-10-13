using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class OnClickImage : MonoBehaviour
{
    [SerializeField] private GameObject narrative1;
    [SerializeField] private GameObject narrative2;
    [SerializeField] private GameObject narrative3;
    [SerializeField] private GameObject narrative4;

    [SerializeField] private GameObject narrative1Text;
    [SerializeField] private GameObject narrative2Text;
    [SerializeField] private GameObject narrative3Text;
    [SerializeField] private GameObject narrative4Text;

    public void Start()
    {
        narrative1.SetActive(true);
        narrative1Text.SetActive(true);

        narrative2.SetActive(false);
        narrative3.SetActive(false);
        narrative4.SetActive(false);

        narrative2Text.SetActive(false);
        narrative3Text.SetActive(false);
        narrative4Text.SetActive(false);
    }

    public void OnNarrative1()
    {
        narrative2.SetActive(true);
        narrative2Text.SetActive(true);

        narrative1.SetActive(false);
        narrative1Text.SetActive(false);
    }

    public void OnNarrative2()
    {
        narrative3.SetActive(true);
        narrative3Text.SetActive(true);

        narrative2.SetActive(false);
        narrative2Text.SetActive(false);
    }

    public void OnNarrative3()
    {
        narrative4.SetActive(true);
        narrative4Text.SetActive(true);

        narrative3.SetActive(false);
        narrative3Text.SetActive(false);
    }

    public void OnNarrative4()
    {
        narrative4.SetActive(false);
        narrative4Text.SetActive(false);

        SceneManager.LoadScene("SampleScene 1");
    }
}
