using Bolt;
using UnityEngine;


public class BombController : EntityBehaviour<IBombState>
{
    static BombController instance = null;
    private static float _MAX_DISTANCE = 2f;
    private BoltEntity _difuser = null;
    public static bool _IS_DIFUSED = false;

    public override void Attached()
    {
        state.SetTransforms(state.Transform, transform);
        instance = this;
    }

    public static bool CheckDifuse(Vector3 player)
    {
        if (!instance)
            return false;
        if (instance._difuser != null || _IS_DIFUSED)
            return false;
        return Vector3.Distance(player, instance.transform.position) < _MAX_DISTANCE;
    }
    public static void SetDifuser(BoltEntity be)
    {
        if (!instance)
        {
            instance._difuser = be;
        }
    }
}
