using UnityEngine;

public class Bullet : MonoBehaviour
{
    private float timer;                //탄이 발사된 후의 시간
    [SerializeField] private string targetTag;           //탄이 피해를 입힐 적의 tag
    [SerializeField] private float endTime = 10.0f;        //탄의 소멸 시간
    [SerializeField] private float startAngle = 0.0f;    //탄의 시작 각도
    [SerializeField] protected float speed = 5f;         //탄의 속도
    [SerializeField] protected float frequency = 0f;     //탄의 흔들림 주기
    [SerializeField] protected float amplitude = 0f;     //탄의 흔들림 진폭
    [SerializeField] protected int moveType = 0;      //탄의 이동 패턴

    public float damage = 1f;           //탄의 피해량


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        timer = 0;
        this.transform.rotation = Quaternion.Euler(0, startAngle, 0); //탄의 시작 각도 설정
        this.gameObject.SetActive(true); //탄을 활성화
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= endTime)
        {
            this.gameObject.SetActive(false);
            timer = 0;
        }
        MovePattern(moveType);
    }

    public Bullet()
    {
        Debug.Log("Bullet 기본 생성자 호출됨");
    }
    public Bullet(float speed, float frequency, float amplitude, string targetTag, float endTime, float startAngle, int moveType)
    {
        Debug.Log("Bullet 생성자 호출됨");
        this.speed = speed;
        this.frequency = frequency;
        this.amplitude = amplitude;
        this.targetTag = targetTag;
        this.endTime = endTime;
        this.startAngle = startAngle;
        this.moveType = moveType;
    }


    public void ActiveTimer()
    {
        
    }

    public void MovePattern(int type)
    {
        switch (type)
        {
            case 0:
                //직선으로 이동
                Debug.Log("직선 이동 패턴입니다.");
                transform.position += transform.forward * speed * Time.deltaTime;
                break;
            case 1:
                //탄이 흔들리며 이동
                Debug.Log("곡선 이동 패턴입니다.");
                transform.position += new Vector3(Mathf.Cos(timer * frequency) * amplitude, 0, speed) * Time.deltaTime;
                break;
            default:
                Debug.Log("잘못된 이동 패턴입니다.");
                break;
        }

    }
}
