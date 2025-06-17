using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private string sceneToLoad = "GamePlayScene"; // 로드할 씬의 이름 (Inspector에서 설정)

    // 이 함수는 버튼의 OnClick() 이벤트에 연결
    public void LoadGameScene()
    {
        Time.timeScale = 1f; 

        // 지정된 씬을 로드합니다.
        SceneManager.LoadScene(sceneToLoad);
        Debug.Log($"Loading scene: {sceneToLoad}");
    }

    // 게임 종료 버튼
    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit(); // 게임 종료
        // 에디터에서 Application.Quit() 동작 확인
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

}