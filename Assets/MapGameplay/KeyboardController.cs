using MapGameplay;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

public class KeyboardController : MonoBehaviour, IController
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [Header("Dependencies")]
    [SerializeField] private MapManager manager;
    [SerializeField] private PlayerManager playerManager;
    [Header("Settings")] 
    [SerializeField] private bool allowKillKey;

    private bool _allowMove;

    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    private void Awake()
    {
        Assert.IsNotNull(manager);
        Assert.IsNotNull(playerManager);
        
        GameSystems.Ins.Controller = this;
    }


    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////
    public void SetAllowMove(bool value)
    {
        _allowMove = value;
        if (!_allowMove)
            playerManager.Player.HorizontalOrderDirection = 0;
    }
    
    
    //game events///////////////////////////////////////////////////////////////////////////////////////////////////////
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Q))
            manager.QuitToScene("MainMenu");
        else if (Input.GetKeyDown(KeyCode.E))
            manager.QuitToScene("MapEditor");
        else if (Input.GetKeyDown(KeyCode.R))
            manager.ReloadMap();
        else if (allowKillKey && Input.GetKeyDown(KeyCode.K))
            playerManager.KillPlayer();
        else if (_allowMove && Input.GetKeyDown(KeyCode.Space))
            playerManager.Player.MakeRegularJump();
        else if (_allowMove && Input.GetKeyUp(KeyCode.Space))
            playerManager.Player.SuppressJump();

        
        if (!_allowMove) return;
        playerManager.Player.HorizontalOrderDirection = 
            (Input.GetKey(KeyCode.A) ? -1 : 0) + (Input.GetKey(KeyCode.D) ? 1 : 0);
        playerManager.Player.OrderedToHitch = Input.GetKey(KeyCode.LeftShift);
    }
}
