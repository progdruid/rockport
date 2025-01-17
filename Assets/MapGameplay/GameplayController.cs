using UnityEngine;
using UnityEngine.Assertions;

public class GameplayController : MonoBehaviour
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [Header("Dependencies")]
    [SerializeField] private MapLoader loader;
    [SerializeField] private PlayerManager playerManager;
    [Header("Settings")] 
    [SerializeField] private bool allowKillKey;

    private bool _allowMove;

    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    private void Awake()
    {
        Assert.IsNotNull(loader);
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
                playerManager.Player.HorizontalDirection = 0;
        }
    }
    
    
    //game events///////////////////////////////////////////////////////////////////////////////////////////////////////
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Q))
            loader.QuitToMenu();
        else if (Input.GetKeyDown(KeyCode.R))
            loader.ReloadLevel();
        else if (allowKillKey && Input.GetKeyDown(KeyCode.K))
            playerManager.KillPlayer();
        else if (AllowMove && Input.GetKeyDown(KeyCode.Space))
            playerManager.Player.MakeRegularJump();
        else if (AllowMove && Input.GetKeyUp(KeyCode.Space))
            playerManager.Player.SuppressJump();

        if (!AllowMove) return;
        playerManager.Player.HorizontalDirection = 
            (Input.GetKey(KeyCode.A) ? -1 : 0) + (Input.GetKey(KeyCode.D) ? 1 : 0);
    }
}
