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
        if (Input.GetKeyDown(KeyCode.F1))
            GameSystems.Ins.GameplayUISystem.ToggleHUDBar();
        
        if (Input.GetKeyDown(KeyCode.F))
            GameSystems.Ins.GameplayCamera.SetModeFollow();
        else if (Input.GetKeyDown(KeyCode.RightShift))
            GameSystems.Ins.GameplayCamera.SetModeStatic();
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            GameSystems.Ins.GameplayCamera.SetModeMove(Vector2.up);
        else if (Input.GetKeyDown(KeyCode.RightArrow))
            GameSystems.Ins.GameplayCamera.SetModeMove(Vector2.right);
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            GameSystems.Ins.GameplayCamera.SetModeMove(Vector2.down);
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            GameSystems.Ins.GameplayCamera.SetModeMove(Vector2.left);
        
        
        for (var i = 0; i <= 9; i++)
        {
            if (!Input.GetKeyDown(KeyCode.Alpha0 + i)) continue;
            GameSystems.Ins.GameplayCamera.SetStaticSpeedMultiplier(i);
            break;
        }

        if (Input.GetKey(KeyCode.Minus))
            GameSystems.Ins.GameplayCamera.SetZoom(-1f);
        else if (Input.GetKey(KeyCode.Equals))
            GameSystems.Ins.GameplayCamera.SetZoom(1f);
        
        
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
