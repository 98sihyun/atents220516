using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Player : MonoBehaviour, IHealth, IBattle
{
    GameObject weapon;
    GameObject sheild;

    ParticleSystem ps;
    Animator anim;

    // IHealth ------------------------------------------------------------------------------------
    public float hp = 100.0f;
    float maxHP = 100.0f;

    public float HP
    {
        get => hp;
        set
        {
            if(hp != value)
            {
                hp = value;
                onHealthChange?.Invoke();
            }
        }
    }

    public float MaxHP
    {
        get => maxHP;
    }

    public System.Action onHealthChange { get; set; }

    // IBattle ------------------------------------------------------------------------------------
    public float attackPower = 30.0f;
    public float defencePower = 10.0f;
    public float criticalRate = 0.3f;
    public float AttackPower { get => attackPower; }

    public float DefencePower { get => defencePower; }

    // 락온 용 ---------------------------------------------------------------------------------------
    public GameObject lockOnEffect;
    Transform lockOnTarget;
    float lockOnRange = 5.0f;
    public Transform LockOnTarget { get => lockOnTarget; }


    private void Awake()
    {
        anim = GetComponent<Animator>();

        weapon = GetComponentInChildren<FindWeapon>().gameObject;
        sheild = GetComponentInChildren<FindShield>().gameObject;

        ps = weapon.GetComponentInChildren<ParticleSystem>();
    }

    public void ShowWeapons(bool isShow)
    {
        weapon.SetActive(isShow);
        sheild.SetActive(isShow);
    }

    public void TurnOnAura(bool turnOn)
    {
        if (turnOn)
        {
            ps.Play();
        }
        else
        {
            ps.Stop();
        }
    }

    public void Attack(IBattle target)
    {
        if (target != null)
        {
            float damage = AttackPower;
            if (Random.Range(0.0f, 1.0f) < criticalRate)
            {
                damage *= 2.0f;
            }
            target.TakeDamage(damage);
        }
    }

    public void TakeDamage(float damage)
    {
        float finalDamage = damage - defencePower;
        if (finalDamage < 1.0f)
        {
            finalDamage = 1.0f;
        }
        HP -= finalDamage;

        if (HP > 0.0f)
        {
            //살아있다.
            anim.SetTrigger("Hit");
        }
        else
        {
            //죽었다.
            //Die();
        }
    }

    public void LockOnToggle()
    {
        if(lockOnTarget == null)
        {
            // 락온 시도
            LockOn();
        }
        else
        {
            // 락온 풀기
            LockOff();
        }
    }

    void LockOn()
    {
        // transform.position지점에서 반경 lockOnRange 범위 안에 있는 Enemy레이어를 가진 컬라이더를 전부 찾기
        Collider[] cols = Physics.OverlapSphere(transform.position, lockOnRange, LayerMask.GetMask("Enemy"));

        // 가장 가까운 컬라이더를 찾기
        Collider nearest = null;
        float nearestDistance = float.MaxValue;
        foreach(Collider col in cols)
        {
            float distanceSqr = (col.transform.position - transform.position).sqrMagnitude;
            if( distanceSqr < nearestDistance )
            {
                nearestDistance = distanceSqr;
                nearest = col;
            }
        }

        lockOnTarget = nearest.transform;
        Debug.Log($"Lock on : {lockOnTarget.name}");

        lockOnEffect.transform.position = lockOnTarget.position;
        lockOnEffect.transform.parent = lockOnTarget;
        lockOnEffect.SetActive(true);

    }

    void LockOff()
    {
        lockOnTarget = null;
        lockOnEffect.transform.parent = null;
        lockOnEffect.SetActive(false);
    }

    private void OnDrawGizmos()
    {
        Handles.color = Color.white;
        Handles.DrawWireDisc(transform.position, transform.up, lockOnRange);
    }
}
