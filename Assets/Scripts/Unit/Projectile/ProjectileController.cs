using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    private Rigidbody2D _rb;
    private Unit _unit;
    public float force = 5.0f;
    // Start is called before the first frame update
    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetDirection(Vector3 direction, Vector3 rotation, Unit unit)
    {
        if (_rb != null)
        {
            _unit = unit;
            _rb.velocity = new Vector2(direction.x, direction.y).normalized * force;
            float rot = Mathf.Atan2(rotation.y, rotation.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, rot + 180);
            Destroy(this.gameObject, 5f);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {

        if(other.TryGetComponent<Unit>(out Unit otherUnit))
        {
            if (_unit.tag != otherUnit.tag && !otherUnit.unitController.isUnitDie)
            {
                otherUnit.unitController.ReceiveDamage(_unit.unitController.unitDamage);
                Destroy(this.gameObject);
            }
        }
    }
}
