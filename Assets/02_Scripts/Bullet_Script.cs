using UnityEngine;

public class Bullet_Script : MonoBehaviour
{
    private float timer;                //탄이 발사된 후의 시간
    [SerializeField] private string targetTag;           //탄이 피해를 입힐 적의 tag
    [SerializeField] private float endTime = 10.0f;        //탄의 소멸 시간
    private Vector3 startAngle;    //탄의 시작 각도
    [SerializeField] private float speed = 5f;         //탄의 속도
    [SerializeField] private float frequency = 0f;     //탄의 흔들림 주기
    [SerializeField] private float amplitude = 0f;     //탄의 흔들림 진폭
    [SerializeField] private int moveType = 0;      //탄의 이동 패턴

    public float damage = 1f;           //탄의 피해량


    void Start()
    {

    }

    void Update()
    {
        timer += Time.deltaTime;
        ActiveTimer(); //탄의 활성화 시간 체크
        MovePattern(moveType);
    }

    // SetBullet 메서드는 탄의 속성들을 설정합니다.
    public void SetBullet(float speed, float frequency, float amplitude, float damage,
            string targetTag, float endTime, Vector3 startAngle, Vector3 startPos, int moveType)
    {
        timer = 0.0f;
        this.speed = speed;
        this.frequency = frequency;
        this.amplitude = amplitude;
        this.damage = damage;
        this.targetTag = targetTag;
        this.endTime = endTime;
        this.startAngle = startAngle;
        this.moveType = moveType;
        this.transform.position = startPos; //탄의 시작 위치 설정
        this.transform.rotation = Quaternion.LookRotation(startAngle); //탄의 시작 각도 설정
        this.gameObject.SetActive(true); //탄을 활성화
    }
    public void ActiveTimer()
    {
        if (timer >= endTime)
        {
            //복제(institate) & 삭제보다 활성화 & 비활성화가 더 빠릅니다.
            this.gameObject.SetActive(false);       //탄을 비활성화
        }
    }

    public void MovePattern(int type)
    {
        switch (type)
        {
            case 0:
                //직선으로 이동
                //Debug.Log("직선 이동 패턴입니다.");
                transform.position += transform.forward * speed * Time.deltaTime;
                break;
            case 1:
                //탄이 흔들리며 이동
                //Debug.Log("곡선 이동 패턴입니다.");
                Vector3 perpendicularDirection = Vector3.Cross(transform.forward, Vector3.up).normalized;

                // 흔들림 계산
                float sway = Mathf.Cos(timer * frequency) * amplitude;
                Vector3 swayOffset = perpendicularDirection * sway;

                // 최종 계산 = 전진 + 흔들림
                Vector3 finalMovement = (transform.forward * speed) + swayOffset;
                transform.position += finalMovement * Time.deltaTime;
                break;
            default:
                Debug.Log("잘못된 이동 패턴입니다.");
                break;
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Wall")){
            this.gameObject.SetActive(false);
        }

        if (other.gameObject.CompareTag(targetTag))
        {
            Debug.Log($"Bullet hit {other.gameObject.tag} ({other.gameObject.name})");

            // 탄이 적에게 맞았을 때, 적의 Character_Script를 찾아서 GetHit 메서드를 호출 (상속받은 스크립트들도 가능)
            Character_Script character = other.gameObject.GetComponent<Character_Script>();

            if (character != null)
            {
                character.GetHit(damage);
            }
            else
            {
                Debug.LogWarning($"Bullet hit {other.gameObject.name} (Tag: {targetTag}), but no Character_Script found.", other.gameObject);
            }

            // 탄이 적에게 맞았을 때, 탄을 비활성화
            this.gameObject.SetActive(false);
        }
    }
}
