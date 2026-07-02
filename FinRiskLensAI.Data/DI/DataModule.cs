using Autofac;
using FinRiskLensAI.Core.Interfaces;
using FinRiskLensAI.Data.Repositories;

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

            // Generic fallback so IRepository<T> resolves for entities without a dedicated repository
            builder.RegisterGeneric(typeof(RepositoryBase<>))
                   .As(typeof(IRepository<>)).InstancePerLifetimeScope();
        }
    }
}
