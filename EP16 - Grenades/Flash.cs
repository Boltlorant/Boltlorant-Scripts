using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flash : Grenade
{
    private float _radius = 75f;

    public override void Detached()
    {
        if (entity.IsOwner)
            _Flash();
    }

    private void _Flash()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, _radius);

        foreach (Collider c in colliders)
        {
            if (c.GetComponent<PlayerMotor>())
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position, c.transform.position - transform.position, out hit))
                {
                    if (hit.transform.GetComponent<PlayerMotor>())
                    {
                        if (Vector3.Dot(hit.transform.forward, hit.transform.position - transform.position) < 0)
                            hit.transform.GetComponent<PlayerCallback>().RaiseFlashEvent();
                    }
                }

            }
        }
    }
}
