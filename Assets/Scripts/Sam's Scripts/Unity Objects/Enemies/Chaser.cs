using UnityEngine;

public class Chaser: NPC
{
    [Header("Chasser attributes")]
    [SerializeField] private float _contactDamage;
    [SerializeField] private float _pursuitRange;

    //Unity
    private void Update()
    {
        // NB. Teacher doesn't want us to work directly in there so get that out asap
        if (Target == null || Vector3.Distance(transform.position, Target.position) > PursuitRange)
        {
            Transform newTarget = null;
            
            foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
            {
                if (newTarget == null || 
                    Vector3.Distance(transform.position, player.transform.position) < Vector3.Distance(transform.position, newTarget.position))
                {
                    newTarget = player.transform;
                }
            }

            Target = newTarget;
        }

        if (Target != null)
        {
            Agent.SetDestination(Target.position);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (collision.gameObject.TryGetComponent<IHitable>(out IHitable hitable))
            {
                hitable.OnHit(ContactDamage);
            }
        }
    }

    // Getters and Setters
    public virtual float ContactDamage
    {
        get
        {
            return _contactDamage;
        }
        set
        {
            _contactDamage = value;
        }
    }

    public virtual float PursuitRange
    {
        get
        {
            return _pursuitRange;
        }
        set
        {
            _pursuitRange = value;
        }
    }
}
