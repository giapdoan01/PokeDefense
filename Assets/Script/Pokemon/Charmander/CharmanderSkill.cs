using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharmanderSkill : MonoBehaviour, ISkill
{
    private static Dictionary<int, CharmanderSkill> activeSkills = new Dictionary<int, CharmanderSkill>();
    
    [Header("Damage Settings")]
    [SerializeField] private float damageInterval = 0.5f;
    [SerializeField] private float targetCheckInterval = 0.2f;
    
    [Header("Deactivation Settings")]
    [SerializeField] private int maxNoTargetChecks = 5;
    
    private float damage;
    private float range;
    private float nextTargetCheckTime;
    private EnemyHealth target;
    private ParticleSystem fireEffect;
    private Transform portalEnd;
    private GameObject pokemonOwner;
    private bool isActive;

    private void Awake()
    {
        fireEffect = GetComponentInChildren<ParticleSystem>();
        pokemonOwner = FindPokemonOwner();
        
        if (pokemonOwner == null) 
        { 
            Destroy(gameObject); 
            return; 
        }
        
        int id = pokemonOwner.GetInstanceID();
        if (activeSkills.TryGetValue(id, out var existing) && existing != null && existing != this)
        {
            Destroy(gameObject);
            return;
        }
        activeSkills[id] = this;
    }

    private void Update()
    {
        if (!isActive || pokemonOwner == null) return;
        
        if (Time.time >= nextTargetCheckTime)
        {
            // Kiểm tra target hiện tại
            if (target != null)
            {
                if (!target.gameObject.activeInHierarchy || 
                    Vector3.Distance(transform.position, target.transform.position) > range)
                {
                    target = null;
                }
            }
            
            // Tìm target mới nếu cần
            if (target == null)
            {
                target = FindBestTarget();
            }
            
            nextTargetCheckTime = Time.time + targetCheckInterval;
        }
        
        // Xoay hiệu ứng về phía target
        if (target != null && fireEffect != null)
        {
            Vector3 dir = target.transform.position - transform.position;
            dir.y = 0;
            if (dir != Vector3.zero) 
            {
                fireEffect.transform.rotation = Quaternion.LookRotation(dir);
            }
        }
    }

    private void OnDestroy()
    {
        if (pokemonOwner != null)
        {
            int id = pokemonOwner.GetInstanceID();
            if (activeSkills.ContainsKey(id) && activeSkills[id] == this)
            {
                activeSkills.Remove(id);
            }
        }
    }

    public void Initialize(float damage, float range, EnemyHealth target = null, Animator pokemonAnimator = null)
    {
        if (pokemonOwner == null) 
        { 
            Destroy(gameObject); 
            return; 
        }
        
        this.damage = damage;
        this.range = range;
        this.target = target;
        this.isActive = true;
        
        transform.SetParent(pokemonOwner.transform);
        transform.localPosition = Vector3.zero;
        
        portalEnd = GameObject.FindGameObjectWithTag("PE")?.transform;
        
        if (fireEffect != null) 
        { 
            fireEffect.Clear(); 
            fireEffect.Play(); 
        }
        
        StartCoroutine(DamageLoop());
    }

    public void DeactivateSkill()
    {
        if (!isActive) return;
        
        isActive = false;
        
        if (fireEffect != null) 
        { 
            fireEffect.Stop(); 
            fireEffect.Clear(); 
        }
        
        Destroy(gameObject);
    }

    private EnemyHealth FindBestTarget()
    {
        if (portalEnd == null) return null;
        
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        EnemyHealth best = null;
        float closestToPortal = float.MaxValue;
        
        foreach (var enemy in enemies)
        {
            if (!enemy.activeInHierarchy) continue;
            
            var health = enemy.GetComponent<EnemyHealth>();
            if (health == null) continue;
            
            float distToSkill = Vector3.Distance(transform.position, enemy.transform.position);
            if (distToSkill > range) continue;
            
            float distToPortal = Vector3.Distance(enemy.transform.position, portalEnd.position);
            if (distToPortal < closestToPortal)
            {
                closestToPortal = distToPortal;
                best = health;
            }
        }
        
        return best;
    }

    private IEnumerator DamageLoop()
    {
        var wait = new WaitForSeconds(damageInterval);
        int consecutiveNoTargetChecks = 0;
        
        while (isActive)
        {
            bool targetIsValid = target != null && 
                                target.gameObject.activeInHierarchy && 
                                Vector3.Distance(transform.position, target.transform.position) <= range;
            
            if (targetIsValid)
            {
                consecutiveNoTargetChecks = 0;
                target.TakeDamage(damage);
            }
            else
            {
                target = FindBestTarget();
                
                if (target != null)
                {
                    consecutiveNoTargetChecks = 0;
                }
                else
                {
                    consecutiveNoTargetChecks++;
                    
                    if (consecutiveNoTargetChecks >= maxNoTargetChecks)
                    {
                        DeactivateSkill();
                        yield break;
                    }
                }
            }
            
            yield return wait;
        }
    }

    private GameObject FindPokemonOwner()
    {
        // Tìm trong phạm vi gần trước
        Collider[] colliders = Physics.OverlapSphere(transform.position, 2f);
        GameObject nearest = null;
        float minDist = float.MaxValue;
        
        foreach (var col in colliders)
        {
            if (col.GetComponent<SkillController>() != null)
            {
                float dist = Vector3.Distance(transform.position, col.transform.position);
                if (dist < minDist) 
                { 
                    minDist = dist; 
                    nearest = col.gameObject; 
                }
            }
        }
        
        if (nearest != null) return nearest;
        
        // Tìm tất cả nếu không tìm thấy gần
        GameObject[] pokemons = GameObject.FindGameObjectsWithTag("Player");
        foreach (var pokemon in pokemons)
        {
            if (pokemon.GetComponent<SkillController>() != null)
            {
                float dist = Vector3.Distance(transform.position, pokemon.transform.position);
                if (dist < minDist) 
                { 
                    minDist = dist; 
                    nearest = pokemon; 
                }
            }
        }
        
        return nearest;
    }

    private void OnDrawGizmos()
    {
        if (!isActive) return;
        
        Gizmos.color = target != null ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, range);
        
        if (target != null && target.gameObject.activeInHierarchy)
        {
            float dist = Vector3.Distance(transform.position, target.transform.position);
            
            Gizmos.color = dist <= range ? Color.green : Color.red;
            Gizmos.DrawLine(transform.position, target.transform.position);
            Gizmos.DrawWireSphere(target.transform.position, 0.5f);
        }
    }
}