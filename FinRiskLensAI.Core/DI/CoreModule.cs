using Autofac;
using FinRiskLensAI.Core.Common;
using FinRiskLensAI.Core.Interfaces.ICommon;

namespace FinRiskLensAI.Core.DI
{
    public class CoreModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // Shared HTTP helper (wraps one app-lifetime HttpClient) — used by the
            // chatbot (Groq) and the AA data-pull services.
            builder.RegisterType<HttpClientHelper>()
                   .As<IHttpClientHelper>()
                   .SingleInstance();
        }
    }
}
