using System.Collections;
using UnityEngine;

public class BoltDestroy : MonoBehaviour
{
    [SerializeField]
    private float _delay = 60f;
    private IEnumerator Start()
    {
        yield return new WaitForSeconds(_delay);
        if (GetComponent<BoltEntity>().IsOwner)
            BoltNetwork.Destroy(gameObject);
    }
}
