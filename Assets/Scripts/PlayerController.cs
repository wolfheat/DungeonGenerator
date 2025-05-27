using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Wolfheat.Inputs;
using static UnityEngine.InputSystem.InputAction;

public enum MoveActionType { Step, SideStep, Rotate }
public class MoveAction
{
    public MoveActionType moveType;
    public int dir = 0;
    public Vector2Int move;
    public MoveAction(MoveActionType t, int d)
    {
        moveType = t;
        dir = d;
    }
    public MoveAction(MoveActionType t, Vector2Int m)
    {
        moveType = t;
        move = m;
    }
}
public class PlayerController : MonoBehaviour
{
    [SerializeField] Mock playerMock;
    //[SerializeField] PlayerAnimationController playerAnimationController;
    public PickUpController pickupController;
    public bool DoingAction { get; set; } = false;
    public MoveActionType ActiveMoveActionType { get; set; } = MoveActionType.Step;


    private MoveAction savedAction = null;
    private MoveAction lastAction = null;

    private float timer = 0;
    private const float MoveTime = 0.2f;

    public Action PlayerReachedNewTile;
    public Action<Vector2Int> MovedToNewSquare;
    public static PlayerController Instance { get; private set; }
   // public bool IsDead { get { return Stats.Instance.IsDead; } }

    private Vector3 offset = new Vector3(0.5f,0.5f,0.5f);


    private void Awake()
    {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        ResetPlayerPosition();
    }

    private void OnEnable()
    {
        // set up input actions
        //Inputs.Instance.Controls.Player.Move.performed += NewMoveInput;
        Inputs.Instance.PlayerControls.Player.Step.performed += Step;
        Inputs.Instance.PlayerControls.Player.SideStep.performed += SideStep;
        Inputs.Instance.PlayerControls.Player.Turn.performed += TurnPerformed;
        Inputs.Instance.PlayerControls.Player.Click.performed += InterractWith;
        Inputs.Instance.PlayerControls.Player.Space.performed += InterractWith;
        Inputs.Instance.PlayerControls.UI.Enter.performed += InterractWith;
        Inputs.Instance.PlayerControls.Player.RightClick.performed += RightClick;
        Inputs.Instance.PlayerControls.Player.RightClickKeysSubstitute.performed += RightClick;
        //Inputs.Instance.Controls.Player.Y.performed += InstantDeath;
        //TakeFireDamage.PlayerTakeFireDamage += FireDamage;
        //playerAnimationController.HitComplete += HitWithTool;
    }

    private void OnDisable()
    {
        //Inputs.Instance.Controls.Player.Move.performed -= NewMoveInput;
        Inputs.Instance.PlayerControls.Player.Step.performed -= Step;
        Inputs.Instance.PlayerControls.Player.SideStep.performed -= SideStep;
        Inputs.Instance.PlayerControls.Player.Turn.performed -= TurnPerformed;
        Inputs.Instance.PlayerControls.Player.Click.performed -= InterractWith;
        Inputs.Instance.PlayerControls.Player.Space.performed -= InterractWith;
        Inputs.Instance.PlayerControls.UI.Enter.performed -= InterractWith;
        Inputs.Instance.PlayerControls.Player.RightClick.performed -= RightClick;
        Inputs.Instance.PlayerControls.Player.RightClickKeysSubstitute.performed -= RightClick;
        //playerAnimationController.HitComplete -= HitWithTool;
        //TakeFireDamage.PlayerTakeFireDamage -= FireDamage;
    }


    public void InstantDeath(CallbackContext context)
    {
        Debug.Log("Instant Death");
        //TakeDamage(10);
    }
    public void RightClick(CallbackContext context)
    {
        //if (Stats.Instance.IsDead || GameState.IsPaused)
        //    return;
        Debug.Log("Right Click ");
        //PlaceBomb();
    }

    public void InterractWith(CallbackContext context)
    {
        Debug.Log("Interract");
        InterractWith(true);
    }


    public void InterractWith(bool mouseSource = false)
    {

        //if (Stats.Instance.IsDead || GameState.IsPaused) return;
        //pickupController.UpdateColliders();

        // Check if item exists to pick up
        if (EventSystem.current.IsPointerOverGameObject() && Mouse.current.leftButton.IsPressed()) {
            // Only check for this if not in shop - prob should only do if interact was performed by mouse
            Debug.Log("Interacting over UI element");
            return;
        }

        // When shop is open try to buy when interacting
        //if (Shop.Instance.ShopSpecificIsOpen) {
        //    if (Mouse.current.leftButton.IsPressed())
        //        return;
        //    Debug.Log("Shop is open try buy");
        //    Shop.Instance.BuyShopItem();
        //    return;
        //}

        //toolHolder.ChangeTool(DestructType.Breakable);


        

    }

    private void Update()
    {
        if (DoingAction) return;

        if (savedAction != null) {
            if (savedAction.moveType == MoveActionType.Step || savedAction.moveType == MoveActionType.SideStep) {

                Vector3 target = EndPositionForMotion(savedAction);

                //Debug.Log("Executing step movement = " + savedAction.moveType+" target is "+target);

                if (Mocks.Instance.IsTileFree(Convert.V3ToV2Int(target)) && pickupController.IsTileFree(this.transform.position,target)) {
                    lastAction = savedAction;
                    // Check for stair?
                    float movementTimeMultiplier = 1f;

                    if (pickupController.IsStair(target, out Vector3 newTarget)) {
                        Debug.Log("Moving Into a stair");
                        Debug.Log("Exit point moves to "+newTarget);
                        target = newTarget;
                        movementTimeMultiplier = 3.16f;
                    }

                    //Debug.Log("Storing saved Movement as Last movement and start new movement");
                    //Debug.Log("Moving to " + target+" "+savedAction.moveType);
                    StartCoroutine(Move(target, movementTimeMultiplier));
                }
                else {
                    CenterPlayerPosition();
                    //Debug.Log("Walls or Enemies ahead");

                }

            }
            else if (savedAction.moveType == MoveActionType.Rotate) {
                lastAction = savedAction;
                StartCoroutine(Rotate(EndRotationForMotion(savedAction)));
            }

            // Remove last attempted motion
            savedAction = null;
        }
    }

    private void TurnPerformed(InputAction.CallbackContext obj)
    {
        TurnPerformed();
    }
    private bool TurnPerformed()
    {
        //if (GameState.state == GameStates.Paused || Stats.Instance.IsDead) return false; // No input while paused

        float movement = Inputs.Instance.PlayerControls.Player.Turn.ReadValue<float>();
        if (movement == 0) return false;

        MoveAction moveAction = new MoveAction(MoveActionType.Rotate, (int)movement);
        savedAction = moveAction;
        return true;
    }

    private void SideStep(CallbackContext obj)
    {
        SideStep();
    }

    private void Step(CallbackContext obj)
    {
        Step();
    }

    private bool SideStep()
    {
        //if (GameState.state == GameStates.Paused || Stats.Instance.IsDead) return false; // No input while paused

        // Return if no movement input currently held 
        float movement = Inputs.Instance.PlayerControls.Player.SideStep.ReadValue<float>();
        if (movement == 0) return false;

        // Special case Do not overwrite with SideStep if last time player moved it was a SideStep, and there is a Step stored
        if (lastAction != null && lastAction.moveType == MoveActionType.SideStep && savedAction != null && savedAction.moveType == MoveActionType.Step)
            return false;


        // Write or overwrite next action
        MoveAction moveAction;
        moveAction = new MoveAction(MoveActionType.SideStep, Mathf.RoundToInt(movement));
        savedAction = moveAction;

        //Debug.Log("New Moveaction: Strafe Moving dir " + savedAction.dir + " " + savedAction.moveType);

        return true;
    }


    private bool Step()
    {
        //Debug.Log(" Player position " + transform.position);

        //if (GameState.state == GameStates.Paused || Stats.Instance.IsDead) return false; // No input while paused


        // Return if no movement input currently held 
        float movement = Inputs.Instance.PlayerControls.Player.Step.ReadValue<float>();
        if (movement == 0) return false;

        // Special case Do not overwrite with Step if last time player moved it was a Step, and there is a Sidestep stored
        if (lastAction != null && lastAction.moveType == MoveActionType.Step && savedAction != null && savedAction.moveType == MoveActionType.SideStep)
            return false;

        // Write or overwrite next action
        MoveAction moveAction;
        moveAction = new MoveAction(MoveActionType.Step, Mathf.RoundToInt(movement));
        savedAction = moveAction;

        //Debug.Log("New Moveaction: Step Moving dir " + savedAction.dir + " " + savedAction.moveType);

        return true;
    }


    private void HeldMovementInput()
    {
        // Check if player is holding a movement button
        if (Step()) {
            // If held button is not same as last input center player
            if (savedAction != null && savedAction.moveType != lastAction.moveType)
                CenterPlayerPosition();
            return;
        }
        if (SideStep()) {
            // If held button is not same as last input center player
            if (savedAction != null && savedAction.moveType != lastAction.moveType)
                CenterPlayerPosition();
            return;
        }
        CenterPlayerPosition();
        // Check for interact


        return;
    }

    private void CenterPlayerPosition()
    {
        //Debug.Log("Center player "+transform.position);
        transform.position = Convert.Align(transform.position);
        //transform.position = new Vector3(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y), Mathf.RoundToInt(transform.position.z));
        //Debug.Log("Centered player " + transform.position);

    }

    private IEnumerator Move(Vector3 target, float timeMultiplier = 1f)
    {
        // Used to define what player moves above
        //int stepSoundFromTerrain = TerrainChecker.ProminentTerrainType(transform.position,LevelCreator.Instance.ActiveTerrain);

        // 2 = STONE STEPs
        //SoundMaster.Instance.PlayStepSound(2);

        //Shop.Instance.CloseIfOpen();

        // Place mock
        PlaceMock(target);

        DoingAction = true;
        Vector3 start = transform.position;
        Vector3 end = target;
        timer = 0;
        while (timer < MoveTime * timeMultiplier) {
            yield return null;
            transform.position = Vector3.LerpUnclamped(start, end, timer / (MoveTime * timeMultiplier));
            timer += Time.deltaTime;
        }
        //Debug.Log("Moving player "+(transform.position-target).magnitude);
        //transform.position = target;
        DoingAction = false;

        MotionActionCompleted();

    }

    private Vector3 EndPositionForMotion(MoveAction motion)
    {
        // Round the answer
        Vector3 target = transform.position + motion.dir * (motion.moveType == MoveActionType.Step ? transform.forward : transform.right);
        target = new Vector3(Mathf.RoundToInt(target.x), Mathf.RoundToInt(target.y), Mathf.RoundToInt(target.z));
        return target;
    }

    private Quaternion EndRotationForMotion(MoveAction motion)
    {
        return Quaternion.LookRotation(transform.right * motion.dir, Vector3.up);
    }

    private IEnumerator Rotate(Quaternion target)
    {
        //Shop.Instance.CloseIfOpen();
        DoingAction = true;
        Quaternion start = transform.rotation;
        Quaternion end = target;
        timer = 0;
        while (timer < MoveTime) {
            yield return null;
            transform.rotation = Quaternion.Lerp(start, end, timer / MoveTime);
            timer += Time.deltaTime;
        }
        transform.rotation = target;
        DoingAction = false;
        MotionActionCompleted();
    }

    public void UpdateInputDelayed()
    {
        StartCoroutine(UpdatePlayerInputCO());
    }

    public IEnumerator UpdatePlayerInputCO()
    {
        yield return null;
        UpdatePlayerInput();
    }

    public void UpdatePlayerInput()
    {
        //if (Stats.Instance.IsDead) return;

        // Check to align player
        if (savedAction != null && savedAction.moveType != lastAction.moveType)
            CenterPlayerPosition();

        // Player has no movement saved check if button is held
        if (savedAction == null)
            HeldMovementInput();
        //pickupController.UpdateColliders();

        if (Inputs.Instance.PlayerControls.Player.Click.IsPressed() || Inputs.Instance.PlayerControls.Player.Space.IsPressed()) {
            CenterPlayerPosition();
            Debug.Log("Mouse is held, interact");
            InterractWith(true);
        }

    }
    public void MotionActionCompleted()
    {
        //if (Stats.Instance.IsDead) return;

        //Debug.Log("Motion completed, has stored action: "+savedAction);
        PlayerReachedNewTile?.Invoke();
        MovedToNewSquare?.Invoke(Convert.V3ToV2Int(transform.position));
        UpdatePlayerInput();
    }

    public void Reset()
    {
        Debug.Log("Reset Player");

        ResetPlayerPosition();

        //Stats.Instance.Revive();
        PlaceMock(transform.position);
    }

    private void ResetPlayerPosition()
    {
        Debug.Log("RESET PLAYER ");
        // Setting PLayer to init position with forward rotation

        // Make sure the coroutines are stopped
        StopAllCoroutines();

        //transform.position = new Vector3(Stats.Instance.SavedStartPosition.x, 0, Stats.Instance.SavedStartPosition.z);
        

        transform.position = FindFirstObjectByType<StartPositioner>().transform.position+GameController.Instance.GridOffset;
        
        Debug.Log("Player moved to " + transform.position);

        transform.rotation = Quaternion.identity;

        Debug.Log(" Player position " + transform.position);

        savedAction = null;
        lastAction = null;
        DoingAction = false;

        //takeFireDamage.StopFire();

        MovedToNewSquare?.Invoke(Convert.V3ToV2Int(transform.position));
    }

    private void PlaceMock(Vector3 position)
    {
        playerMock.pos = Convert.V3ToV2Int(position);
        playerMock.transform.position = position;
    }

    internal void GotoStartPosition()
    {
        //Stats.Instance.SetSpecificPosition(0);
        ResetPlayerPosition();
    }
    internal void GotoStartPosition(int leadsTo)
    {
        Debug.Log("Specific position");
        //Stats.Instance.SetSpecificPosition(leadsTo);
        ResetPlayerPosition();
        //Stats.Instance.DeActivateMap();
    }
}
