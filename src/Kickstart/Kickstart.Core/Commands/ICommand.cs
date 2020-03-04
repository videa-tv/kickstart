namespace Kickstart.Commands
{
    public interface ICommand<in TOptions>
    {
        CommandResult Run(TOptions options);
    }
}
