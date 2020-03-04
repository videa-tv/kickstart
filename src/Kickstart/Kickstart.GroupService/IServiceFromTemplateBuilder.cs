using Kickstart.GroupService.Model;

namespace Kickstart.GroupService
{
    public interface IServiceFromTemplateBuilder
    {
        void BuildService(BuilderOptions builderOptions);
    }
}