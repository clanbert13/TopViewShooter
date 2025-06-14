using Unity.VisualScripting;
using UnityEngine;

public class Bullet_Script : MonoBehaviour
{
    private float timer;                //탄이 발사된 후의 시간
    [SerializeField] private string targetTag;           //탄이 피해를 입힐 적의 tag
    [SerializeField] private float endTime = 10.0f;        //탄의 소멸 시간
    private Vector3 startAngle;    //탄의 시작 각도
    [SerializeField] protected float speed = 5f;         //탄의 속도
    [SerializeField] protected float frequency = 0f;     //탄의 흔들림 주기
    [SerializeField] protected float amplitude = 0f;     //탄의 흔들림 진폭
    [SerializeField] protected int moveType = 0;      //탄의 이동 패턴

    public float damage = 1f;           //탄의 피해량


    void Start()
    {
        timer = 0.0f;
        this.gameObject.SetActive(true); //탄을 활성화
    }

    void Update()
    {
        timer += Time.deltaTime;
        ActiveTimer(); //탄의 활성화 시간 체크
        MovePattern(moveType);
    }

    public Bullet_Script()
    {
        //Debug.Log("Bullet 기본 생성자 호출됨");
    }

    // 생성할때 해당 정보들 입력하시면 됩니다.
    public Bullet_Script(float speed, float frequency, float amplitude,
        string targetTag, float endTime, Vector3 startAngle, Vector3 startPos, int moveType)
    {
        //Debug.Log("Bullet 생성자 호출됨");
        SetBullet(speed, frequency, amplitude,
        targetTag, endTime, startAngle, startPos, moveType);

    }

    public void SetBullet(float speed, float frequency, float amplitude,
            string targetTag, float endTime, Vector3 startAngle, Vector3 startPos, int moveType)
    {
        this.speed = speed;
        this.frequency = frequency;
        this.amplitude = amplitude;
        this.targetTag = targetTag;
        this.endTime = endTime;
        this.startAngle = startAngle;
        this.moveType = moveType;
        this.transform.position = startPos; //탄의 시작 위치 설정
        this.transform.rotation = Quaternion.LookRotation(startAngle); //탄의 시작 각도 설정
    }
    public void ActiveTimer()
    {
        if (timer >= endTime)
        {
            //복제(institate) & 삭제보다 활성화 & 비활성화가 더 빠릅니다.
            this.gameObject.SetActive(false);       //탄을 비활성화
            timer = 0;
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
                transform.position += new Vector3(Mathf.Cos(timer * frequency)
                                        * amplitude, 0, speed) * Time.deltaTime;
                break;
            default:
                Debug.Log("잘못된 이동 패턴입니다.");
                break;
        }
    }
    

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(targetTag))
        {
            Debug.Log(other.gameObject.tag + "대상과 충돌했습니다.");
            //충돌한 대상에게 피해
            other.gameObject.GetComponent<Character_Script>().GetHit(damage);

            this.gameObject.SetActive(false); //탄을 비활성화
        }
    }
}
