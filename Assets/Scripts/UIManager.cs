using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    private GameObject winScreen;
    [SerializeField]
    private GameObject loseScreen;

    void Awake()
    {
        winScreen.SetActive(false);
        loseScreen.SetActive(false);
    }
    
    public void ShowWinScreen()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        winScreen.SetActive(true);
    }

    public void ShowLoseScreen()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        loseScreen.SetActive(true);
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
