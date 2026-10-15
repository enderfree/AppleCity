using UnityEngine;

public class HurtBox : MonoBehaviour
{
    // _contactDamage was originally in enemy, but we might be able to do cooler things if I put it in hurtbox instead... // to test
    [SerializeField] private float _contactDamage;

    /// <summary>
    /// Deal damage on contact
    /// </summary>
    /// <param name="other">the object hit</param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent<IHitable>(out IHitable hitable))
        {
            hitable.OnHit(_contactDamage);
        }
    }
}
