using UnityEngine;
using UnityEngine.SceneManagement;

public class ManagerEscenas : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Menu()
    {
        SceneManager.LoadScene("Menu", LoadSceneMode.Single);
    }

    // Update is called once per frame
    public void Juego()
    {
        SceneManager.LoadScene("Juego", LoadSceneMode.Single);
    }

    public void Salir()
    {
        Application.Quit();
    }
}
