using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Peddler : MonoBehaviour, IStatusAble, INeutrality
{
    public event Action<GameObject> OnDestroyed;
    
    [SerializeField] private float maxHealth;
    private float currentHealth;
    private bool isDie;
    [SerializeField] private float speed;

    [SerializeField, Range(0f, 1f)] private float[] stateThreshold = new float[1];
    [SerializeField] List<SpawnUnitsSO> waveUnits;
    private SpawnUnit spawnUnit;

    private int currentState = 0;
    private Animator animator;
    [HideInInspector] public NavMeshAgent agent;


    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        spawnUnit = GetComponent<SpawnUnit>();
        isDie = false;
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.speed = speed;
        agent.isStopped = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void ReceiveDamage(DamageData damage)
    {
        currentHealth -= damage.damage;
        ApplyEffect(damage);
        if(currentHealth <= 0)
        {
            Die();
        }
        ChangeState(currentHealth);
    }

    public void ApplyEffect(DamageData damage)
    {
        throw new System.NotImplementedException();
    }

    private void ChangeState(float health)
    {
        if (currentState >= stateThreshold.Length) return;
        float normalizedHealth = health / maxHealth;
        
        while (currentState < stateThreshold.Length && normalizedHealth <= stateThreshold[currentState])
        {
            animator.Play("AttackedState");
            spawnUnit.SpawnUnits(waveUnits[currentState], this.transform.position, "Mercenary");
            currentState++;
        }
    }

    public void MoveToDestination(Vector3 destination)
    {
        agent.ResetPath();
        agent.SetDestination(destination);
    }

    private void Die()
    {
        OnDestroyed?.Invoke(this.gameObject);
        animator.Play("DownState");
    }

    public bool IsDestroyed()
    {
        return isDie;
    }
}
