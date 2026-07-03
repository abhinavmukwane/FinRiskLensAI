using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinRiskLensAI.Core.Common
{
    public static class CommonCode
    {
        public static TTarget MapToModelObject<TSource, TTarget>(this TSource source, TTarget target)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (target == null)
                throw new ArgumentNullException(nameof(target));

            var sourceProps = typeof(TSource).GetProperties();
            var targetProps = typeof(TTarget).GetProperties();

            foreach (var sProp in sourceProps)
            {
                var tProp = targetProps
                    .FirstOrDefault(p =>
                        string.Equals(p.Name, sProp.Name, StringComparison.OrdinalIgnoreCase));

                if (tProp != null && tProp.CanWrite)
                {
                    tProp.SetValue(target, sProp.GetValue(source));
                }
            }

            return target;
        }

        public static TTarget MapToModelObject<TSource, TTarget>(this TSource source, TTarget target, List<string> excludeProperties)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (target == null)
                throw new ArgumentNullException(nameof(target));

            excludeProperties = excludeProperties ?? new List<string>();

            var sourceProps = typeof(TSource).GetProperties();
            var targetProps = typeof(TTarget).GetProperties();

            foreach (var sProp in sourceProps)
            {
                // Skip excluded properties (case-insensitive)
                if (excludeProperties.Any(e =>
                    string.Equals(e, sProp.Name, StringComparison.OrdinalIgnoreCase)))
                    continue;

                var tProp = targetProps.FirstOrDefault(p =>
                    string.Equals(p.Name, sProp.Name, StringComparison.OrdinalIgnoreCase));

                if (tProp == null || !tProp.CanWrite)
                    continue;

                var value = sProp.GetValue(source);

                // Optional: skip null values
                // if (value == null) continue;

                tProp.SetValue(target, value);
            }

            return target;
        }

    }
}
