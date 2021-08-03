
public class ErrorGameState : IGameState
{
    public bool ReportError = true;
    public bool Execute(float deltaTime)
    {
        if (ReportError)
            LogManager.LogError("Error game state is running on the stack.");
        return false;
    }

    public void Enter(object o)
    {
        if (ReportError)
            LogManager.LogError("Error game state enetered.");
    }

    public void Exit()
    {
        if (ReportError)
            LogManager.LogError("Error game state exited.");
    }

    public void HandleInput()
    {
        if (ReportError)
            LogManager.LogError("Error game state handle input.");
    }

    public string GetName() { return "ErrorGameState"; }

    public ErrorGameState() { }
}