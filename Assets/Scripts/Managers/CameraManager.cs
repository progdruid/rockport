using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera), typeof(Rigidbody2D))]
public class CameraManager : MonoBehaviour
{
    [SerializeField] float boxSize;

    [SerializeField] float boxTopHalfSize;
    [SerializeField] float boxBottomHalfSize;
    [SerializeField] float boxRightHalfSize;
    [SerializeField] float boxLeftHalfSize;

    [SerializeField] CustomSoundEmitter soundEmitter;

    private new Camera camera;
    private Animator transition;
    private Transform target;

    public bool transiting { get; private set; } = true;

    private void OnEnable() => Registry.ins.cameraManager = this;
    void Awake()
    {
        camera = GetComponent<Camera>();

        transition = camera.transform.GetChild(0).GetComponent<Animator>();
    }
    private void Start()
    {
        Registry.ins.playerManager.PlayerSpawnEvent += HandlePlayerSpawn;
    }

    private void OnDestroy()
    {
        Registry.ins.playerManager.PlayerSpawnEvent -= HandlePlayerSpawn;
    }

    void Update()
    {
        if (transiting || target == null)
            return;
        
        float newPosX = Mathf.Clamp(transform.position.x, target.position.x - boxLeftHalfSize, target.position.x + boxRightHalfSize);
        float newPosY = Mathf.Clamp(transform.position.y, target.position.y - boxBottomHalfSize, target.position.y + boxTopHalfSize);

        transform.position = new Vector3(newPosX, newPosY, transform.position.z);
    }

    public void SetTarget (Transform target)
    {
        this.target = target;
    }

    public void SetAndMoveToTarget (Transform target)
    {
        this.target = target;
        transform.position = new Vector3(target.position.x, target.position.y, transform.position.z);
    }

    public IEnumerator TransiteIn ()
    {
        transiting = true;
        Registry.ins.inputSet.Active = false;
        Registry.ins.deathsBar.SetActive(false);

        soundEmitter.EmitSound("TransiteIn");
        transition.SetTrigger("Transition");

        yield return new WaitUntil(() => transition.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Full");
    }

    public IEnumerator TransiteOut ()
    {
        soundEmitter.EmitSound("TransiteOut");
        transition.SetTrigger("Transition");

        yield return new WaitUntil(() => transition.GetCurrentAnimatorClipInfo(0)[0].clip.name == "None");
        
        Registry.ins.inputSet.Active = true;
        Registry.ins.deathsBar.SetActive(true);
        transiting = false;
    }

    private void HandlePlayerSpawn(GameObject player) => SetAndMoveToTarget(player.transform);
}
