using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UnityEngine.UI;

public class PlayerController : NetworkBehaviour
{
    [SerializeField]
    private NetworkCharacterControllerPrototype networkCharacterController = null;

    [SerializeField]
    private Bullet bulletPrefab;

    [SerializeField]
    private Image hpBar = null;

    [SerializeField]
    private float moveSpeed = 15f;

    [SerializeField]
    private int maxHp = 100;

    [Networked(OnChanged = nameof(OnHpChanged))]
    public int Hp { get; set; }

    [Networked]
    public NetworkButtons ButtonsPrevious { get; set; }

    public override void Spawned()
    {
        if(Object.HasStateAuthority)
            Hp = maxHp;
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {
            NetworkButtons buttons = data.buttons;
            var pressed = buttons.GetPressed(ButtonsPrevious);
            ButtonsPrevious = buttons;

            Vector3 moveVector = data.movementInput.normalized;
            networkCharacterController.Move(moveSpeed * moveVector * Runner.DeltaTime);

            if (pressed.IsSet(InputButtons.JUMP))
            {
                networkCharacterController.Jump();
            }

            if (pressed.IsSet(InputButtons.FIRE))
            {
                Runner.Spawn(
                    bulletPrefab, 
                    transform.position + transform.TransformDirection(Vector3.forward),
                    Quaternion.LookRotation(transform.TransformDirection(Vector3.forward)),
                    Object.InputAuthority);
            }
        }

        if(Hp <= 0 || networkCharacterController.transform.position.y <= -5f)
        {
            Respawn();
        }
    }

    private void Respawn()
    {
        networkCharacterController.transform.position = Vector3.up * 2;
        Hp = maxHp;
    }

    public void TakeDamage(int damage)
    {
        if (Object.HasStateAuthority)
        {
            Hp -= damage;
        }
    }

    private static void OnHpChanged(Changed<PlayerController> changed)
    {
        changed.Behaviour.hpBar.fillAmount = (float)changed.Behaviour.Hp / changed.Behaviour.maxHp;
    }
}
