using UnityEngine;
using UnityEngine.UI;

public class UI_Manager : MonoBehaviour
{
    public static UI_Manager Instance { get; private set; }

    [Header("Player Elements")]
    [SerializeField] private Player_Script playerScript; 
    [SerializeField] private Slider playerHealthSlider; // 인스펙터에서 Slider 컴포넌트를 직접 연결

    private void Awake()
    {
        // 이미 인스턴스가 존재하고 현재 인스턴스가 아니라면, 이 오브젝트를 파괴합니다.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject);
        }

    }



    // 체력 바 업데이트
    public void UpdatePlayerHealthUI() // 인자를 직접 받지 않고 playerScript에서 가져오도록 변경
    {
        if (playerHealthSlider != null && playerScript != null)
        {
            playerHealthSlider.maxValue = playerScript.GetMAXHealth();
            playerHealthSlider.value = playerScript.GetCurHealth();
        }
        else
        {
            Debug.LogError("UI_Manager: Health Bar Slider 또는 Player Script가 초기화되지 않았습니다!", this);
        }
    }
}