
public class ErrorGameState : IGameState
{
    public bool Execute(float deltaTime)
    {
        LogManager.LogError("Error game state is running on the stack.");
        return false;
    }

    public void Enter(object o)
    {
        LogManager.LogError("Error game state enetered.");
    }

    public void Exit()
    {
        LogManager.LogError("Error game state exited.");
    }

    public void HandleInput()
    {
        LogManager.LogError("Error game state handle input.");
    }

    public string GetName() { return "ErrorGameState"; }  // TODO change to int

    public ErrorGameState() { }
}