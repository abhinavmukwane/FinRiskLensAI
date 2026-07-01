using Autofac;

namespace FinRiskLensAI.Core.DI
{
    public class CoreModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // Example: register AutoMapper profiles, FluentValidation validators etc.
            // builder.RegisterType<ProductValidator>().As<IValidator<Product>>().InstancePerLifetimeScope();
        }
    }
}
