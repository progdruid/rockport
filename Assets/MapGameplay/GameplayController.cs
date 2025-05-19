using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

public class GameplayController : MonoBehaviour
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [FormerlySerializedAs("loader")]
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
    }

    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////

    public bool AllowMove
    {
        private get => _allowMove;
        set
        {
            _allowMove = value;
            if (!_allowMove)
                playerManager.Player.HorizontalOrderDirection = 0;
        }
    }
    
    
    //game events///////////////////////////////////////////////////////////////////////////////////////////////////////
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Q))
            manager.QuitToMenu();
        else if (Input.GetKeyDown(KeyCode.R))
            manager.ReloadLevel();
        else if (allowKillKey && Input.GetKeyDown(KeyCode.K))
            playerManager.KillPlayer();
        else if (AllowMove && Input.GetKeyDown(KeyCode.Space))
            playerManager.Player.MakeRegularJump();
        else if (AllowMove && Input.GetKeyUp(KeyCode.Space))
            playerManager.Player.SuppressJump();

        
        if (!AllowMove) return;
        playerManager.Player.HorizontalOrderDirection = 
            (Input.GetKey(KeyCode.A) ? -1 : 0) + (Input.GetKey(KeyCode.D) ? 1 : 0);
        playerManager.Player.OrderedToCling = Input.GetKey(KeyCode.LeftShift);
    }
}
