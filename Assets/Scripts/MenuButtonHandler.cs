using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButtonHandler : MonoBehaviour
{
    [Header("게임 설명서 UI")]
    [SerializeField] private GameObject manualPopup;
    
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
    
    public void ToggleManualPopup()
    {
        if (manualPopup != null)
        {
            manualPopup.SetActive(!manualPopup.activeSelf);
        }
        else
        {
            Debug.LogWarning("manualPopup이 할당되지 않았습니다.");
        }
    }
    
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // 빌드된 게임에서는 실제 종료
        Application.Quit();
#endif
    }
}