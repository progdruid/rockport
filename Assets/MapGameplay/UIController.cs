using UnityEngine;
using UnityEngine.Assertions;

public class UIController : MonoBehaviour, IController
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [Header("Dependencies")]
    [SerializeField] private MapManager manager;
    [SerializeField] private PlayerManager playerManager;

    private bool _allowMove;

    private int _leftMotion = 0;
    private int _rightMotion = 0;
    
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

    public void HandleJumpSensorStart()
    {
        if (_allowMove)
            playerManager.Player.MakeRegularJump();
    }

    public void HandleJumpSensorEnd()
    {
        if (_allowMove)
            playerManager.Player.SuppressJump();
    }

    public void HandleLeftSensorStart()
    {
        if (!_allowMove) return;
        _leftMotion = 1;
        playerManager.Player.HorizontalOrderDirection = _rightMotion - _leftMotion;
    }

    public void HandleLeftSensorEnd()
    {
        if (!_allowMove) return;
        _leftMotion = 0;
        playerManager.Player.HorizontalOrderDirection = _rightMotion - _leftMotion;
    }
    
    public void HandleRightSensorStart()
    {
        if (!_allowMove) return;
        _rightMotion = 1;
        playerManager.Player.HorizontalOrderDirection = _rightMotion - _leftMotion;
    }

    public void HandleRightSensorEnd()
    {
        if (!_allowMove) return;
        _rightMotion = 0;
        playerManager.Player.HorizontalOrderDirection = _rightMotion - _leftMotion;
    }
    
    public void HandleHitchSensorStart()
    {
        if (_allowMove) 
            playerManager.Player.OrderedToHitch = true;
    }

    public void HandleHitchSensorEnd()
    {
        if (_allowMove) 
            playerManager.Player.OrderedToHitch = false;
    }
}