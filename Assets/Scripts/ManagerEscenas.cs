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
        SceneManager.LoadScene("Juego Pueblo", LoadSceneMode.Single);
    }

    public void Salir()
    {
        Application.Quit();
    }

    public void Bosque()
    {
        SceneManager.LoadScene("Bosque", LoadSceneMode.Single);
    }

    public void Mañana()
    {
        SceneManager.LoadScene("Pueblo Mañana", LoadSceneMode.Single);
    }

    public void PuebloAbandonado()
    {
        Time.timeScale = 1f;
        Debug.Log("ManagerEscenas: Cargando escena 'Pueblo abandonado'...");
        SceneManager.LoadScene("Pueblo abandonado", LoadSceneMode.Single);
    }

    public void Final()
    {
        Time.timeScale = 1f;
        Debug.Log("ManagerEscenas: Cargando escena 'Final'...");
        SceneManager.LoadScene("Final", LoadSceneMode.Single);
    }

    public void PeleaFinal()
    {
        Time.timeScale = 1f;
        Debug.Log("ManagerEscenas: Cargando escena 'Pelea Final'...");
        SceneManager.LoadScene("Pelea Final", LoadSceneMode.Single);
    }
}
