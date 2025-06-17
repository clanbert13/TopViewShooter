using UnityEngine;
using UnityEngine.UI;

public class UI_Manager : MonoBehaviour
{
    // 싱글톤 인스턴스 (모든 곳에서 접근 가능하도록 static)
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
            // 씬이 변경되어도 이 오브젝트를 파괴하지 않도록 합니다 (선택 사항).
            // UI_Manager가 게임의 전반적인 UI를 관리하고 싶다면 Don't Destroy On Load를 사용하는 것이 좋습니다.

            // DontDestroyOnLoad(gameObject);
        }

        // Awake에서 컴포넌트 초기화: Start보다 먼저 호출되므로, 다른 스크립트들이 Start에서 UI_Manager.Instance를 참조할 때 NullPointerException을 방지할 수 있습니다.
        InitializeUI();
    }

    // InitializeUI 메서드를 별도로 분리하여 Awake에서 호출하도록 합니다.
    private void InitializeUI()
    {
        // playerScript가 인스펙터에서 직접 할당되었는지 확인
        if (playerScript == null)
        {
            // 만약 인스펙터에서 할당되지 않았다면, "Player" 태그를 가진 오브젝트에서 찾아봅니다.
            // (권장되지는 않지만, 임시방편으로 사용 가능)
            GameObject foundPlayer = GameObject.FindWithTag("Player");
            if (foundPlayer != null)
            {
                playerScript = foundPlayer.GetComponent<Player_Script>();
            }

            if (playerScript == null)
            {
                Debug.LogError("UI_Manager: Player_Script 참조를 찾을 수 없습니다! 인스펙터에 할당하거나 태그로 찾을 수 있는지 확인하세요.", this);
                return; 
            }
        }

        // playerHealthSlider가 인스펙터에서 직접 할당되었는지 확인
        if (playerHealthSlider == null)
        {
            Debug.LogError("UI_Manager: Player Health Slider가 인스펙터에 할당되지 않았습니다!", this);
            return; 
        }

        // 초기 체력 바 설정
        if (playerScript != null && playerHealthSlider != null)
        {
            playerHealthSlider.maxValue = playerScript.GetMAXHealth();
            playerHealthSlider.value = playerScript.GetCurHealth();
        }
    }


    // 체력 바 업데이트 메서드를 Public으로 유지하여 다른 스크립트에서 호출할 수 있도록 합니다.
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