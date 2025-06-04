using System.Collections.Generic;

/*
  Expected usecases:
  Moving armies,
  Moving units in battle
*/

public interface ICommand{
  bool CanExecute();
  string Execute();
  void Undo();
}

public class CommandManager
{
  private readonly Stack<ICommand> executedCommands = new();
  private readonly Stack<ICommand> undoneCommands = new();

  public bool ExecuteCommand(ICommand command)
  {
    if(command.CanExecute()){
      command.Execute();
      executedCommands.Push(command);
      undoneCommands.Clear();
      return true;
    }
    Logger.Warning($"Command {command.GetType().Name} failed to execute");
    return false;
  }

  public void Undo()
  {
    if(executedCommands.Count > 0)
    {
      var command = executedCommands.Pop();
      command.Undo();
      undoneCommands.Push(command);
    }
  }

  public void Redo()
  {
    if(undoneCommands.Count > 0)
    {
      var command = undoneCommands.Pop();
      if(command.CanExecute())
      {
        command.Execute();
        executedCommands.Push(command);
      }
    }
  }
}