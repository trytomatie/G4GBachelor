using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class Interactable_Door : Interactable
{
    public NetworkVariable<bool> isOpen = new NetworkVariable<bool>(false);
    private const float lockOutTime = 1;
    private float lockOutTimer = 0;
    public UnityEvent onFirstInteraction;
    private bool firstInteraction = true;
    private Animator anim;

    private void Start()
    {
        anim = GetComponent<Animator>();
        isOpen.OnValueChanged += OnOpenValueChanged;
    }

    private void OnOpenValueChanged(bool previousValue, bool newValue)
    {
        anim.SetBool("isOpen", newValue);
    }

    public override void Interact(GameObject source)
    {
        ToggleDoorRPC();
    }

    [Rpc(SendTo.Server)]
    public void ToggleDoorRPC()
    {
        if(lockOutTimer < Time.time)
        {
            isOpen.Value = !isOpen.Value;
            lockOutTimer = Time.time + lockOutTime;
            if(firstInteraction)
            {
                FirstInteractionRpc();
                firstInteraction = false;
            }
        }
    }

    [Rpc(SendTo.Everyone)]
    public void FirstInteractionRpc()
    {
        onFirstInteraction.Invoke();
    }

}
