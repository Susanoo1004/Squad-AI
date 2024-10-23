using System;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using Squad;

public class SimpleController : MonoBehaviour
{
    [SerializeField]
    float moveSpeed = 6f;

    PlayerAgent Player;
    [SerializeField]
    SquadController Allies;

    Camera viewCamera;
    Vector3 velocity;

    public Action<Vector3> OnMouseLeftClicked;
    public Action<Vector3> OnMouseRightClicked;
    public Action<Vector3> OnMouseLeftHold;
    public Action<Vector3> OnMouseRightHold;

    public bool IsBarrageMode { get; private set; } = false;

    void Start()
    {
        Player = GetComponent<PlayerAgent>();
        viewCamera = Camera.main;

        OnMouseLeftClicked += Player.ShootToPosition;
        //OnMouseLeftClicked += Allies.OrderToShoot;
        OnMouseLeftHold += Player.ShootToPosition;
        //OnMouseLeftHold += Allies.OrderToShoot;


        //OnMouseRightClicked += Player.NPCShootToPosition;
        //OnMouseRightClicked += Allies.OrderToShoot ;

    }
    void Update()
    {
        int floorLayer = 1 << LayerMask.NameToLayer("Floor");
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit raycastInfo;
        Vector3 targetPos = Vector3.zero;
        if (Physics.Raycast(ray, out raycastInfo, Mathf.Infinity, floorLayer))
        {
            Vector3 newPos = raycastInfo.point;
            targetPos = newPos;
            targetPos.y += 0.1f;

            Player.AimAtPosition(targetPos);
        }

        velocity = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized * moveSpeed;

        if (Input.GetMouseButtonDown(0))
        {
            OnMouseLeftClicked(targetPos);
        }
        else if (Input.GetMouseButton(0))
        {
            OnMouseLeftHold(targetPos);
        }
        if (Input.GetMouseButtonDown(1))
        {
            IsBarrageMode = !IsBarrageMode;
            OnMouseRightClicked(targetPos);
            if (!IsBarrageMode)
                Player.RemoveNPCTarget();
            else
                Player.NPCShootToPosition(targetPos);
        }
        //else if (Input.GetMouseButton(1))
        //{
        //    OnMouseRightHold(targetPos);
        //}
    }
    void FixedUpdate()
    {
        Player.MoveToward(velocity);
    }
}