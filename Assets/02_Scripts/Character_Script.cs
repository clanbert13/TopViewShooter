using UnityEngine;

// 모든 캐릭터 오브젝트들의 기본 틀입니다. 상속받아 사용
public class Character_Script : MonoBehaviour
{
    [SerializeField] protected float characterSpeed = 5f; // 캐릭터 이동 속도
    protected float health;
    [SerializeField] protected float maxHealth;

    void Start()
    {

    }

    void Update()
    {

    }

    protected virtual void Move(Vector3 direction)      // 방향을 받아 이동
    {
        // 이동 방향을 정규화하여 속도에 곱해줍니다.
        direction.Normalize();
        //this.transform.position += direction * characterSpeed * Time.deltaTime;
        this.transform.Translate(direction * characterSpeed * Time.deltaTime, Space.World);
    }
    public virtual void GetHit(float damage)
    {
        health -= damage;
        Debug.Log("캐릭터 체력 감소 : " + health);
        if (health <= 0)
        {
            // 게임 오버
            Debug.Log(this.name.ToString() + "의 체력이 0이 되었습니다.");
            Die(); // 죽음 처리 메소드 호출
        }
    }


    public virtual void Attack(Vector3 targetPosition, string targetTag)
    {
        // 공격 로직
    }

    public virtual void Die()
    {
        // 죽음 처리 로직
        Debug.Log(this.name.ToString() + "이(가) 죽었습니다.");
        // 예를 들어, 오브젝트를 비활성화하거나 파괴할 수 있습니다.
        this.gameObject.SetActive(false);
    }

    public float GetMAXHealth()
    {
        return maxHealth;
    }
    
    public float GetCurHealth()
    {
        return health;
    }
}
