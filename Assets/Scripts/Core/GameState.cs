public abstract class GameState: BaseObserver
{
    protected GameMaster gameMaster;

    public GameState(GameMaster gameMaster){
        this.gameMaster = gameMaster;
    }

    public virtual void Enter(){
        Initialize();
    }

    public virtual void Exit(){
        Cleanup();
    }

    public virtual void Update(){}
    public virtual void HandleInput(){}
    
}