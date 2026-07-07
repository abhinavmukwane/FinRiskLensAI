using FinRiskLensAI.Core.Interfaces;
using Autofac;
using FinRiskLensAI.ML.Features;
using FinRiskLensAI.ML.MachineLearning;
using FinRiskLensAI.ML.Services;
using FinRiskLensAI.ML.Storage;
using Microsoft.Extensions.Hosting;

namespace FinRiskLensAI.ML.DI
{
    public class MLModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // Feature extractors — stateless
            builder.RegisterType<UdyamFeatureExtractor>().AsSelf().SingleInstance();
            builder.RegisterType<ItrFeatureExtractor>().AsSelf().SingleInstance();
            builder.RegisterType<GstFeatureExtractor>().AsSelf().SingleInstance();
            builder.RegisterType<AaFeatureExtractor>().AsSelf().SingleInstance();
            builder.RegisterType<CashflowTrendAnalyzer>().AsSelf().SingleInstance();

            // Models train lazily on first use — keep them singletons so training happens once
            builder.RegisterType<SyntheticProfileGenerator>().AsSelf().SingleInstance();
            builder.RegisterType<IncomeAnomalyDetector>().AsSelf().SingleInstance();
            builder.RegisterType<ScoreCalibrationModel>().AsSelf().SingleInstance();

            // Blob storage — container client is created lazily on first use
            builder.RegisterType<AzureBlobDataStore>().As<IMsmeDataStore>().SingleInstance();

            // Startup warmup — trains the lazy ML models off the request path. Registered
            // explicitly as a singleton IHostedService and excluded from the convention
            // scan below (which would otherwise double-register it as IHostedService).
            builder.RegisterType<MlWarmupService>().As<IHostedService>().SingleInstance();

            // Services by convention (matches the ServicesModule pattern)
            builder.RegisterAssemblyTypes(ThisAssembly)
                   .Where(t => t.Name.EndsWith("Service"))
                   .Except<MlWarmupService>()
                   .AsImplementedInterfaces().InstancePerLifetimeScope();
        }
    }
}
