using System;
using UnityEngine;

//It could also be the base of agent as it is
interface ISquadLeader: IDamageable
{
    public event Action<GameObject> OnDamageTaken;
    public event Action<int> OnCriticalHP;
    public event Action<Vector3> OnMoving;
    public event Action<Vector3> OnShooting;
}
