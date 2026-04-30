using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using FMOD.Studio;
using TMPro;
using UnityEngine.AI;
using UnityEngine.SceneManagement;


public class Player : MonoBehaviour, IDataPersistence
{
    [SerializeField] private TestingInputSystem testingInputSystem;

    [SerializeField] private LayerMask interactablesLayerMask;
    [SerializeField] private PlayerInputActions playerInputActions;
    [SerializeField] private PlayerInput playerInput;


    private EventInstance Arms;
    private EventInstance Grabbing;
    private EventInstance PlayerMovement;


    public SceneInfo sceneInfo;
    public TextMeshProUGUI Daytext;
    public TextMeshProUGUI noCreditsText;

    public Animator PlayerAnim;
    public AnimatorStateInfo playeranimStateInfo;
    public float Ntime;
    public bool animationFinished;
    public bool animStarted;
    public float armsCooldownTime;
    private float armsCooldownTimer;
    private bool armIsCooldown = false;


    public float Speed = 5f;
    public float OgSpeed = 2f;
    public float DashForce = 25f;
    public float FlipForceRot = 5f;
    public float RotSpeed = 50f;
    public float MaxSpeed = 7f;
    public float Acceleration = 2f;
    public float dashCooldownTime;
    private float dashCooldownTimer;
    private bool dashIsCooldown = false;
    public bool Dashing = false;

    public Rigidbody rb;
    public Transform RayZone;
    public BoxCollider boxCollider;
    public MoneyTracker moneytracker;
    public TextMeshProUGUI MoneyText;

    public ParticleSystem pickupEffect;
    public ParticleSystem dropEffect;


    public bool Holding;
    public bool HoldingNPC = false;
    public bool isMoving = false;

    public Vector3 boxSize;
    public float maxDistance;
    public LayerMask layermask;
    public bool ArmsRaised = false;

    public Storage storage;
    public Storage storage2;
    public Storage storage3;
    public Interactable interactable;

    public ThrowBar throwBar;
    
    public float ThrowForce = 2f;
    public float MaxThrowForce = 10f;
    public float ThrowChargeSpeed = 5f;
    public GameObject ObjectHeld;
    public GameObject HighlightRing;

    private WaitForEndOfFrame waitForEndOfFrame;

    public GameObject Skin1;
    public GameObject Skin2;
    public GameObject Skin3;
    public GameObject Skin4;
    public GameObject Skin5;
    public GameObject Skin6;

    public bool Highlighted = false;
    public GameObject boxUI;

    public GameObject GumballMachine;
    public GameObject Mop;
    public GameObject MopChild1;
    public GameObject MopChild2;
    public bool EPressed;

    public DayTimer dayTimer;

    public PauseMenu pauseMenu;

    public Scene currentScene;

    private void Awake()
    {
        Time.timeScale = 1;
        playerInput = GetComponent<PlayerInput>();

        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();
        rb = GetComponent<Rigidbody>();
        playerInputActions.Player.Flip.performed += Dash;
        playerInputActions.Player.ArmRaise.performed += ArmRaise;
        playerInputActions.Player.Throw.started += Throw;
        playerInputActions.Player.Throw.canceled += Throw;
        playerInputActions.Player.Storing.performed += Storing;
        playerInputActions.Player.ActualFlip.performed += SecretFlip;




        waitForEndOfFrame = new WaitForEndOfFrame();

        switch (sceneInfo.SkinCounter)
        {
            case 1:
                 Skin1.SetActive(true);
                 break;
            case 2:
                Skin2.SetActive(true);
                break;
            case 3:
                Skin3.SetActive(true);
                break;
            case 4:
                Skin4.SetActive(true);
                break;
            case 5:
                Skin5.SetActive(true);
                break;
            case 6:
                Skin6.SetActive(true);
                break;

        }


        playeranimStateInfo = PlayerAnim.GetCurrentAnimatorStateInfo(0);
        Ntime = playeranimStateInfo.normalizedTime;

    }


    void Start()
    {
        currentScene = SceneManager.GetActiveScene();
        testingInputSystem.OnInteractAction += TestingInputSystem_OnInteractAction;

        Grabbing = AudioManager.instance.CreateInstance(FMODEvents.instance.grabSound);
        Arms = AudioManager.instance.CreateInstance(FMODEvents.instance.armRisingSound);
        PlayerMovement = AudioManager.instance.CreateInstance(FMODEvents.instance.Drive);

        Daytext.text = "Day:" + sceneInfo.dayCount.ToString("0");

        if(sceneInfo.gumballMachineUnlocked)
        {
            GumballMachine.gameObject.SetActive(true);
        }

        if(sceneInfo.roboMopUnlocked)
        {
            Mop.GetComponent<NavMeshAgent>().enabled = true;
            Mop.GetComponent<Interactable>().enabled = false;
            Mop.layer = LayerMask.NameToLayer("Default");

            MopChild1.gameObject.SetActive(false);
            MopChild2.gameObject.SetActive(true);
        }

        boxUI = GameObject.Find("NullBox");

        if (currentScene.name == "Tutorial")
        {
            sceneInfo.storageMax = 1;
        }

  

    }

    private void TestingInputSystem_OnInteractAction(object sender, System.EventArgs e)
    {
        float interactDistance = 1.2f;

            Vector3 moveDir = transform.TransformDirection(Vector3.forward);

        Debug.DrawRay(RayZone.transform.position, moveDir, Color.green);
        if (Physics.Raycast(RayZone.transform.position, moveDir, out RaycastHit raycastHit, interactDistance, interactablesLayerMask))
        {
            if (raycastHit.transform.gameObject.CompareTag("Storage"))
            {
                return;
            }

            if (raycastHit.transform.TryGetComponent(out Interactable interactable) && Holding == false)
            {
                              
                    if (interactable.CompareTag("Storage"))
                    {
                        return;
                    }
                    if (interactable.CompareTag("NPC"))
                    {
                        HoldingNPC = true;
                    }
                    Holding = true;

                    ObjectHeld = interactable.gameObject;

                    interactable.Interact();
                    //pickupEffect.gameObject.transform.SetParent(ObjectHeld.transform);
                    //pickupEffect.gameObject.transform.position = ObjectHeld.transform.position;
                    pickupEffect.Play();
                    
                   if(pickupEffect.isStopped)
                   {
                    //pickupEffect.gameObject.transform.SetParent(this.transform);
                    
                   }
                    Grabbing.setParameterByNameWithLabel("Parameter 2", "Grab");
                    UpdateGrabSound();
                    //AudioManager.instance.PlayOneShot(FMODEvents.instance.grabSound, this.transform.position);
                    //setparameterbynamewithlabel("Parameter2, "grab");
            }




            /* else if (storage.atStorage && Holding == true)
             {
                 Debug.Log("storing1");
                 interactable.Store();
             }
             else if (storage2.atStorage && Holding == true)
             {
                 Debug.Log("storing2");
                 interactable.Store();
             }
             else if (storage3.atStorage && Holding == true)
             {
                 Debug.Log("storing3");
                 interactable.Store();
             }*/

            /*else if (storage.atStorage && Holding == false)
            {
                interactable.Interact();
            }
            else if (storage2.atStorage && Holding == false)
            {
                interactable.Interact();
            }
            else if (storage3.atStorage && Holding == false)
            {
                interactable.Interact();
            }*/
            else if (Holding)
            {
                interactable.Drop();
                //dropEffect.transform.SetParent(ObjectHeld.transform);
                dropEffect.Play();
                if(dropEffect.isStopped)
                {
                  //dropEffect.gameObject.transform.SetParent(this.transform);
                }
                Grabbing.setParameterByNameWithLabel("Parameter 2", "Drop");
                UpdateGrabSound();
                //AudioManager.instance.PlayOneShot(FMODEvents.instance.dropSound, this.transform.position);
                Holding = false;
                HoldingNPC = false;

            }


            
        }
    }
    public void LoadData(GameData data)
    {
      
        rb.position = data.playerPosition;

        if (currentScene.name == "Tutorial")
        {
            rb.position = data.tutorialPosition;
        }
        else
        {
            rb.position = data.playerPosition;
        }

        sceneInfo.money = data.playerAttributesData.money;
        //moneytracker.currentTime = data.playerAttributesData.currentTime;
       

        sceneInfo.playerSpeed = data.playerAttributesData.playerSpeed;
        sceneInfo.playerRotSpeed = data.playerAttributesData.playerRotSpeed;
        sceneInfo.playerAcceleration = data.playerAttributesData.playerAcceleration;
        sceneInfo.playerMaxThrowForce = data.playerAttributesData.playerMaxThrowForce;

        sceneInfo.TotalInjuryCounter = data.playerAttributesData.TotalInjuryCounter;
        sceneInfo.DashUnlocked = data.playerAttributesData.DashUnlocked;
        sceneInfo.instaStockUnlocked = data.playerAttributesData.instaStockUnlocked;
        sceneInfo.gumballMachineUnlocked = data.playerAttributesData.gumballMachineUnlocked;
        sceneInfo.roboMopUnlocked = data.playerAttributesData.roboMopUnlocked;
        sceneInfo.storageMax = data.playerAttributesData.storageMax;

        sceneInfo.ChaiLatteUnlocked = data.playerAttributesData.ChaiLatteUnlocked;
        sceneInfo.HotChocolateUnlocked = data.playerAttributesData.HotChocolateUnlocked;
        sceneInfo.IcedCoffeeUnlocked = data.playerAttributesData.IcedCoffeeUnlocked;
        sceneInfo.IcedLatteUnlocked = data.playerAttributesData.IcedLatteUnlocked;
        sceneInfo.MochaUnlocked = data.playerAttributesData.MochaUnlocked;
        sceneInfo.TeaUnlocked = data.playerAttributesData.TeaUnlocked;

        sceneInfo.SpillsCleaned = data.playerAttributesData.SpillsCleaned;
    }

    public void SaveData(GameData data)
    {

        data.playerPosition = rb.position;
        data.playerAttributesData.money = sceneInfo.money;
        //data.playerAttributesData.currentTime = moneytracker.currentTime;
       

        data.playerAttributesData.playerSpeed = sceneInfo.playerSpeed;
        data.playerAttributesData.playerRotSpeed = sceneInfo.playerRotSpeed;
        data.playerAttributesData.playerAcceleration = sceneInfo.playerAcceleration;
        data.playerAttributesData.playerMaxThrowForce = sceneInfo.playerMaxThrowForce;

        data.playerAttributesData.TotalInjuryCounter = sceneInfo.TotalInjuryCounter;
        data.playerAttributesData.DashUnlocked = sceneInfo.DashUnlocked;
        data.playerAttributesData.instaStockUnlocked = sceneInfo.instaStockUnlocked;
        data.playerAttributesData.gumballMachineUnlocked = sceneInfo.gumballMachineUnlocked;
        data.playerAttributesData.roboMopUnlocked = sceneInfo.roboMopUnlocked;
        data.playerAttributesData.storageMax = sceneInfo.storageMax;

        data.playerAttributesData.ChaiLatteUnlocked = sceneInfo.ChaiLatteUnlocked;
        data.playerAttributesData.HotChocolateUnlocked = sceneInfo.HotChocolateUnlocked;
        data.playerAttributesData.IcedCoffeeUnlocked = sceneInfo.IcedCoffeeUnlocked;
        data.playerAttributesData.IcedLatteUnlocked = sceneInfo.IcedLatteUnlocked;
        data.playerAttributesData.MochaUnlocked = sceneInfo.MochaUnlocked;
        data.playerAttributesData.TeaUnlocked = sceneInfo.TeaUnlocked;

        data.playerAttributesData.SpillsCleaned = sceneInfo.SpillsCleaned;

    }
    void Update()
    {
        InputSystem.onDeviceChange +=
    (device, change) =>
    {
        switch (change)
        {
            case InputDeviceChange.Added:
                Debug.Log($"Device {device} was added");
                sceneInfo.ControllerConnected = true;
                break;
            case InputDeviceChange.Removed:
                Debug.Log($"Device {device} was removed");
                sceneInfo.ControllerConnected = false;
                break;
        }
    };


        PlayerMovement.setParameterByName("PitchChange", Speed);
        if(!Dashing)
        {
            HandleMovement();
        }
        
        UpdateMovementSound();
        
        MaxSpeed = sceneInfo.playerSpeed;
        Acceleration = sceneInfo.playerAcceleration;
        RotSpeed = sceneInfo.playerRotSpeed;

        if (dashIsCooldown)
        {
            ApplyDashCooldown();
        }

        if (armIsCooldown)
        {
            ApplyArmCooldown();
        }

        Debug.DrawRay(RayZone.transform.position, transform.TransformDirection(Vector3.forward), Color.green);
        if (Physics.Raycast(RayZone.transform.position, transform.TransformDirection(Vector3.forward), out RaycastHit raycastHit, 1.2f, interactablesLayerMask))
        {
            if (raycastHit.transform.TryGetComponent(out StorageBox storageBox))
            {
                boxUI = storageBox.gameObject.transform.GetChild(0).gameObject;
                Highlighted = true;              
            }

            if(raycastHit.transform.TryGetComponent(out Interactable interactable) && !raycastHit.transform.TryGetComponent(out StorageBox storageBox1))
            {
                if(HighlightRing != null)
                {
                    HighlightRing.transform.parent = interactable.transform;
                    HighlightRing.transform.position = interactable.transform.localPosition;
                    HighlightRing.SetActive(true);
                    Highlighted = true;
                }

            }
        }
        else
        {
            Highlighted = false;
        }

        if (Highlighted && HighlightRing != null)
        {
            boxUI.SetActive(true);
            HighlightRing.SetActive(true);

        }
        else
        {
            if(HighlightRing != null)
            {
                boxUI.SetActive(false);
                HighlightRing.SetActive(false);
                HighlightRing.transform.parent = transform;
                HighlightRing.transform.position = transform.localPosition;
            }

        }



    }


    private void UpdateGrabSound()
    {

            PLAYBACK_STATE playbackState;
            Grabbing.getPlaybackState(out playbackState);
            if (playbackState.Equals(PLAYBACK_STATE.STOPPED))
            {
                Grabbing.start();
            }       
    }

    private void UpdateArmSound()
    {

        PLAYBACK_STATE playbackState;
        Grabbing.getPlaybackState(out playbackState);
        if (playbackState.Equals(PLAYBACK_STATE.STOPPED))
        {
            Arms.start();
        }
    }

    private void UpdateMovementSound()
    {
        if (isMoving)
        {
            PLAYBACK_STATE playbackState;
            PlayerMovement.getPlaybackState(out playbackState);
            if (playbackState.Equals(PLAYBACK_STATE.STOPPED))
            {
                PlayerMovement.start();

            }

        }
        else if (!isMoving)
        {
            PlayerMovement.stop(STOP_MODE.ALLOWFADEOUT);

        }
    }
    private void HandleMovement()
    {
        
        float Move = testingInputSystem.GetMovementFloat();
        float RotDirection = testingInputSystem.GetRotFloat();

        // rb.MovePosition(transform.position + (transform.forward * Move) * Speed * Time.fixedDeltaTime);
       
        if (Move != 0 && IsGrounded())
        {
            isMoving = true;
            rb.velocity = (transform.forward * Move) * Speed * Time.fixedDeltaTime;
        }
        else
        {
                isMoving = false;
        }


            if (isMoving)
            {


                Speed += Acceleration * Time.fixedDeltaTime;
                if (Speed >= MaxSpeed)
                {
                    Speed = MaxSpeed;

                }
            }
            else
            {
                Speed = OgSpeed;
            }
        

            var rotationVelocity = new Vector3(0, RotSpeed * RotDirection, 0);

        rb.MoveRotation(rb.rotation * Quaternion.Euler(rotationVelocity * Time.deltaTime));
    }

    public void Dash(InputAction.CallbackContext context)
    {
        if (sceneInfo.DashUnlocked)
        {


            if (dashIsCooldown)
            {
                return;
            }

           
            if (context.performed && IsGrounded())
            {
                Dashing = true;
                dashIsCooldown = true;
                dashCooldownTimer = dashCooldownTime;
                playerInput.enabled = false;
                rb.AddForce(transform.forward * DashForce, ForceMode.Impulse);
                AudioManager.instance.PlayOneShot(FMODEvents.instance.Flip, this.transform.position);
            }
        }
        else
        {
            return;
        }

    }

    public void SecretFlip(InputAction.CallbackContext context)
    {
        if(context.performed && IsGrounded())
        {
            rb.AddForce(Vector3.up * FlipForceRot, ForceMode.Impulse);
            rb.AddTorque(Vector3.forward * FlipForceRot, ForceMode.Impulse);
        }
    }

    public void ApplyDashCooldown()
    {
        dashCooldownTimer -= Time.deltaTime;
        if(dashCooldownTimer < 1f)
        {
            Dashing = false;
        }
         
        if(dashCooldownTimer < 0.0f)
        {
            playerInput.enabled = true;
            dashIsCooldown = false;
            //play sound to indicate cooldown ended
        }

    }

    public void ApplyArmCooldown()
    {
       armsCooldownTimer -= Time.deltaTime;

        if (armsCooldownTimer < 0.0f)
        {
            playerInput.enabled = true;
            armIsCooldown = false;
            //play sound to indicate cooldown ended
        }

    }

    public void Storing(InputAction.CallbackContext context)
    {
        if(GumballMachine != null)
        {
            if (context.performed && GumballMachine.GetComponent<GumballMachine>().AtMachine)
            {
                EPressed = true;
            }
        }



        if (Holding && storage.atStorage)
        {
            storage.gameObject.GetComponent<Interactable>().Store();
            return;
        }
        else if (Holding && storage2.atStorage)
        {
            storage2.gameObject.GetComponent<Interactable>().Store();
            return;
        }
        else if (Holding && storage3.atStorage)
        {
            storage3.gameObject.GetComponent<Interactable>().Store();
            return;
        }
    }

    public void Throw(InputAction.CallbackContext context)
    {
        if(!Holding)
        {
            return;
        }

        Vector3 ThrowDir = transform.TransformDirection(Vector3.forward);

        if (context.started && Holding)
        {
            StartCoroutine(HoldButtonRoutine());
        }

        if(context.canceled && Holding)
        {
            
            ObjectHeld.GetComponent<Interactable>().Drop();
            //pickupEffect.gameObject.transform.SetParent(this.transform);
           // dropEffect.gameObject.transform.SetParent(this.transform);

            ObjectHeld.GetComponent<Rigidbody>().AddForce(ThrowDir * ThrowForce, ForceMode.Impulse);
            ObjectHeld.GetComponent<Rigidbody>().AddForce(Vector3.up * (ThrowForce / 2), ForceMode.Impulse);
            ObjectHeld.GetComponent<Rigidbody>().AddTorque(transform.TransformDirection(Vector3.forward) * FlipForceRot, ForceMode.Impulse);

            ThrowForce = 0f;
            Holding = false;
            HoldingNPC = false;
            throwBar.SetThrow(ThrowForce);
            
        }
        IEnumerator HoldButtonRoutine()
        {
            yield return new WaitUntil(context.ReadValueAsButton);
            while (context.ReadValueAsButton())
            {
                ThrowForce += ThrowChargeSpeed * Time.fixedDeltaTime;
                throwBar.SetThrow(ThrowForce);

                if (ThrowForce >= MaxThrowForce)
                {
                    ThrowForce = MaxThrowForce;
                }

                yield return waitForEndOfFrame;
            }




        }
    }

    







    void ArmRising()
    {

        Arms.setParameterByName("Parameter 1", 0);
        UpdateArmSound();
        
        //armies.setparamaterbyname("parameter1, 0);
        /*when the animation is finished, THEN set the bool to true*/

        PlayerAnim.SetBool("ArmsRaised", true);


        PlayerAnim.SetBool("ArmsLowered", false);


    }

    void ArmLowering()
    {
        Arms.setParameterByName("Parameter 1", 1);
        UpdateArmSound();


        PlayerAnim.SetBool("ArmsLowered", true);
        PlayerAnim.SetBool("ArmsRaised", false);


    }


    public void ArmRaise(InputAction.CallbackContext context)
    {
        if(armIsCooldown)
        {
            return;
        }

        if (context.performed && !ArmsRaised)
        {
            ArmsRaised = true;
            armIsCooldown = true;
            armsCooldownTimer = armsCooldownTime;
            playerInput.enabled = false;
            ArmRising();
        }
        else if (context.performed && ArmsRaised)
        {
            ArmsRaised = false;
            armIsCooldown = true;
            armsCooldownTimer = armsCooldownTime;
            playerInput.enabled = false;
            ArmLowering();
        }

    }




    private bool IsGrounded()
    {
        
        float extraHeight = 0.1f;
        RaycastHit raycastHit;
        Ray ray = new Ray(boxCollider.bounds.center, Vector3.down);

        Physics.Raycast(boxCollider.bounds.center, Vector3.down, boxCollider.bounds.extents.y + extraHeight);

        Color rayColor;

        if (Physics.Raycast(ray, out raycastHit, boxCollider.bounds.extents.y + extraHeight))
        {
            Debug.DrawRay(boxCollider.bounds.center, Vector2.down * (boxCollider.bounds.extents.y + extraHeight));
            rayColor = Color.green;
            return true;
        }
        else
        {
            Debug.DrawRay(boxCollider.bounds.center, Vector2.down * (boxCollider.bounds.extents.y + extraHeight));
            rayColor = Color.red;
            return false;
        }

         
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawCube(transform.position -transform.up * maxDistance, boxSize);
    }

    public IEnumerator NoCredits()
    {
        noCreditsText.gameObject.SetActive(true);

        yield return new WaitForSecondsRealtime(2);

        noCreditsText.gameObject.SetActive(false);

    }


     public void UpgradeSpeed()
     {
        if(sceneInfo.money >= 10)
        {
            sceneInfo.money -= 10;
            MaxSpeed += 10;
            Acceleration += 10;
            sceneInfo.playerSpeed = MaxSpeed;
            sceneInfo.playerAcceleration = Acceleration;
            MoneyText.text = ": " + sceneInfo.money.ToString("0");
        }
        else
        {
            StartCoroutine(NoCredits());
        }
     }

    public void UpgradeHandling()
     {
        if (sceneInfo.money >= 10)
        {
            sceneInfo.money -= 10;
            RotSpeed += 10;
            sceneInfo.playerRotSpeed = RotSpeed;
            //sceneInfo.money = moneytracker.money;
            MoneyText.text = ": " + sceneInfo.money.ToString("0");
        }
        else
        {
            StartCoroutine(NoCredits());
        }

    }



    public void UpgradeMaxThrow()
    {
        if(sceneInfo.money >= 10)
        {
            sceneInfo.money -= 10;
            MaxThrowForce += 10;
            sceneInfo.playerMaxThrowForce = MaxThrowForce;
            //sceneInfo.money = moneytracker.money;
            MoneyText.text = ": " + sceneInfo.money.ToString("0");
        }
        else
        {
            StartCoroutine(NoCredits());
        }
    }

    public void UnlockDash()
    {
        if(sceneInfo.DashUnlocked)
        {
            return;
        }

        if (sceneInfo.money >= 70)
        {
            sceneInfo.money -= 70;
            sceneInfo.DashUnlocked = true;
            MoneyText.text = ": " + sceneInfo.money.ToString("0");
            pauseMenu.UpgradeUnlock2.interactable = false;
        }
        else
        {
            StartCoroutine(NoCredits());
        }


    }

    public void UnlockInstaStock()
    {

        if (sceneInfo.instaStockUnlocked)
        {
            return;
        }

        if (sceneInfo.instaStockUnlocked)
        {
            return;
        }
        if (sceneInfo.money >= 30)
        {
            sceneInfo.money -= 30;
            sceneInfo.instaStockUnlocked = true;
            MoneyText.text = ": " + sceneInfo.money.ToString("0");
            pauseMenu.UpgradeUnlock4.interactable = false;
        }
        else
        {
            StartCoroutine(NoCredits());
        }

    }

    public void UpgradeStorage()
    {

        if(sceneInfo.money >= 10)
        {
            sceneInfo.money -= 10;
            sceneInfo.storageMax++;
            MoneyText.text = ": " + sceneInfo.money.ToString("0");
        }
        else
        {
            StartCoroutine(NoCredits());
        }
    }
    public void UnlockGumballMachine()
    {
        if (sceneInfo.gumballMachineUnlocked)
        {
            return;
        }

        if (sceneInfo.money >= 70)
        {
            sceneInfo.money -= 70;
            sceneInfo.gumballMachineUnlocked = true;
            GumballMachine.gameObject.SetActive(true);
            MoneyText.text = ": " + sceneInfo.money.ToString("0");
            pauseMenu.UpgradeUnlock1.interactable = false;
        }
        else
        {
            StartCoroutine(NoCredits());
        }
    }

    public void UnlockRoboMop()
    {
        if (sceneInfo.roboMopUnlocked)
        {
            return;
        }

        if (sceneInfo.money >= 70)
        {
            sceneInfo.money -= 70;
            sceneInfo.roboMopUnlocked = true;
            MoneyText.text = ": " + sceneInfo.money.ToString("0");
            pauseMenu.UpgradeUnlock3.interactable = false;
        }
        else
        {
            StartCoroutine(NoCredits());
        }
    }









}
