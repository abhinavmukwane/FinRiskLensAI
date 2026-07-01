using Autofac;

namespace FinRiskLensAI.Data.DI
{
    public class DataModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // ── Repositories 
            // Scans the assembly and registers all classes ending in "Repository"
            builder.RegisterAssemblyTypes(ThisAssembly)
                   .Where(t => t.Name.EndsWith("Repository"))
                   .AsImplementedInterfaces().InstancePerLifetimeScope();
        }
    }
}
