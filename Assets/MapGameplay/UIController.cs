using BrutalUI;
using UnityEngine;
using UnityEngine.Assertions;

public class UIController : MonoBehaviour, IController
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [Header("Dependencies")]
    [SerializeField] private MapManager manager;
    [SerializeField] private PlayerManager playerManager;
    [Header("Sensors")]
    [SerializeField] private SensorPanel leftSensor;
    [SerializeField] private SensorPanel rightSensor;
    [SerializeField] private SensorPanel jumpSensor;
    [SerializeField] private SensorPanel hitchSensor;
    
    private bool _allowMove;
    
    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    private void Awake()
    {
        Assert.IsNotNull(manager);
        Assert.IsNotNull(playerManager);
        Assert.IsNotNull(leftSensor);
        Assert.IsNotNull(rightSensor);
        Assert.IsNotNull(jumpSensor);
        Assert.IsNotNull(hitchSensor);
        
        GameSystems.Ins.Controller = this;
    }


    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////
    public void SetAllowMove(bool value)
    {
        _allowMove = value;
        if (!_allowMove) 
            playerManager.Player.HorizontalOrderDirection = 0;
    }

    private void Update()
    {
        if (!_allowMove) return;

        if (jumpSensor.Pressed && !jumpSensor.PreviousPressed)
            playerManager.Player.MakeRegularJump();
        else if (!jumpSensor.Pressed && jumpSensor.PreviousPressed)
            playerManager.Player.SuppressJump();

        
        playerManager.Player.HorizontalOrderDirection = (leftSensor.Pressed ? -1 : 0) + (rightSensor.Pressed ? 1 : 0);
        playerManager.Player.OrderedToHitch = hitchSensor.Pressed;
    }
}