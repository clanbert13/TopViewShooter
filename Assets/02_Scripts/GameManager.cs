using UnityEngine;
using UnityEngine.SceneManagement; // 씬 관리를 위해 추가
using TMPro; // TextMeshPro 사용을 위해 추가 (점수 표시)

public class GameManager : MonoBehaviour
{
    // 싱글톤 패턴
    public static GameManager Instance { get; private set; }

    [Header("Game Over UI")]
    [SerializeField] private GameObject gameOverPanel; // 게임 오버 패널 (UI)
    [SerializeField] private TextMeshProUGUI finalScoreText; // 최종 점수 표시 TMP 텍스트
    [SerializeField] private string nextSceneName = "GameOverScene"; // 이동할 다음 씬 이름

    private float playerScore = 0; // 플레이어의 최종 점수를 저장할 변수

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            // 씬이 바뀌어도 GameManager가 유지
            //DontDestroyOnLoad(gameObject);
        }

        // 게임 오버 패널이 비활성화 상태로 시작하도록
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("GameManager: Game Over Panel is not assigned in the Inspector!", this);
        }

        if (finalScoreText == null)
        {
            Debug.LogWarning("GameManager: Final Score Text (TMP) is not assigned in the Inspector!", this);
        }
    }

    // 플레이어의 현재 점수를 업데이트하는 함수 (UI_Manager나 Player_Script에서 호출)
    public void SetPlayerScore(float score)
    {
        playerScore = score;
    }

    // 캐릭터가 죽었을 때 호출될 함수
    public void OnPlayerDied()
    {
        Debug.Log("Game Over! Player Died.");

        // 1. 게임 시간 멈추기
        Time.timeScale = 0f; // 모든 게임 로직이 멈춥니다. (Update, FixedUpdate 등)

        // 2. 게임 오버 UI 활성화 및 점수 표기
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if (finalScoreText != null)
        {
            finalScoreText.text = "Final Score: " + playerScore.ToString();
            // 또는 더 꾸밈: finalScoreText.text = $"최종 점수: {playerScore:N0}";
        }

    }

    // 게임 오버 UI 버튼에 연결할 함수 (예: "다시 시작" 또는 "메인 메뉴")
    public void LoadNextScene()
    {
        Time.timeScale = 1f; // 다음 씬으로 이동하기 전에 시간을 다시 정상화 (매우 중요!)
        SceneManager.LoadScene(nextSceneName);
    }
}