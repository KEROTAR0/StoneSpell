using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    public Vector2 movementInput {get; private set;}
    public bool DashInput { get; private set; }
    public bool NormalAttackInput { get; private set; }
    public bool Spell1Input { get; private set; }
    public bool Spell2Input { get; private set; }
    public bool Spell3Input { get; private set; }
    public bool Spell4Input { get; private set; }
    public bool Spell5Input { get; private set; }
    public Vector2 MousePosition { get; private set; }
    public void OnMoveInput(InputAction.CallbackContext ctx)
    {
        movementInput = ctx.ReadValue<Vector2>();
    }
    public void OnDashInput(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {//button push
            DashInput = true;
        }

        if(ctx.performed)
        {//button hold
            
        }
        if(ctx.canceled)
        {//button release
            DashInput = false;
        }
    }
    public void OnNormalAttackInput(InputAction.CallbackContext ctx)
    {
        if (ctx.started) NormalAttackInput = true;
    }
    public void OnSpell1Input(InputAction.CallbackContext ctx)
    {
        if (ctx.started) Spell1Input = true;
    }
    public void OnSpell2Input(InputAction.CallbackContext ctx)
    {
        if (ctx.started) Spell2Input = true;
    }
    public void OnSpell3Input(InputAction.CallbackContext ctx)
    {
        if (ctx.started) Spell3Input = true;
    }
    public void OnSpell4Input(InputAction.CallbackContext ctx)
    {
        if (ctx.started) Spell4Input = true;
    }
    public void OnSpell5Input(InputAction.CallbackContext ctx)
    {
        if (ctx.started) Spell5Input = true;
    }

    //=========================================================
    // END INPUT
    //=========================================================
    public void UseAttackInput() => NormalAttackInput = false;
    public void UsedDashInput() => DashInput = false;
    public void UseSpell1Input() => Spell1Input = false;
    public void UseSpell2Input() => Spell2Input = false;
    public void UseSpell3Input() => Spell3Input = false;
    public void UseSpell4Input() => Spell4Input = false;
    public void UseSpell5Input() => Spell5Input = false;
    public void UseSpellInputByIndex(int index)
    {
        if (index == 0) UseSpell1Input();
        else if (index == 1) UseSpell2Input();
        else if (index == 2) UseSpell3Input();
        else if (index == 3) UseSpell4Input();
        else if (index == 4) UseSpell5Input();
    }
    public void OnMousePositionInput(InputAction.CallbackContext ctx)
    {
        MousePosition = ctx.ReadValue<Vector2>();
    }
}
