namespace Kickstart.Wizard.View
{
    public interface IGenerationView : IView
    {
        string ProgressStatus { set; }

        void IncrementProgress(int progressPercentage, string progressMessage);
    }
}