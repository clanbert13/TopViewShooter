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

    protected void Move(Vector3 direction)      // 방향을 받아 이동
    {
        // 이동 방향을 정규화하여 속도에 곱해줍니다.
        direction.Normalize();
        transform.position += direction * characterSpeed * Time.deltaTime;
    }
    public virtual void GetHit(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            // 게임 오버
            Debug.Log(this.name.ToString() + "의 체력이 0이 되었습니다.");
        }
    }


    public virtual void Attack(Vector3 targetPosition)
    {
        // 공격 로직
    }

    public virtual void Die()
    {
        // 죽음 처리 로직
    }
    
}
