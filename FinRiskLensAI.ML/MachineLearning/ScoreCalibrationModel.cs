using Microsoft.ML;
using Microsoft.ML.Trainers.LightGbm;

namespace FinRiskLensAI.ML.MachineLearning
{
    /// <summary>
    /// LightGBM regression model that maps the engineered feature vector to a 0-1000
    /// score. Trained once (lazily) on the synthetic population; its prediction is
    /// blended with the rules-engine score, and per-feature permutation importance
    /// feeds the explanation layer (the PFI step from Doc/03_SCORING_ENGINE.md).
    /// </summary>
    public class ScoreCalibrationModel
    {
        private readonly SyntheticProfileGenerator _generator;
        private readonly Lazy<(MLContext Ml, ITransformer Model)> _trained;

        public ScoreCalibrationModel(SyntheticProfileGenerator generator)
        {
            _generator = generator;
            _trained = new(Train);
        }

        private class Prediction { public float Score { get; set; } }

        /// <summary>Force the (lazy) LightGBM training now — call at startup so the first
        /// /analyze request doesn't pay the training cost on the request thread.</summary>
        public void Warmup() => _ = _trained.Value;

        public double PredictScore(ScoreFeatureVector input)
        {
            var (ml, model) = _trained.Value;
            var engine = ml.Model.CreatePredictionEngine<ScoreFeatureVector, Prediction>(model);
            return Math.Clamp(engine.Predict(input).Score, 0, 1000);
        }

        /// <summary>
        /// Permutation feature importance for this specific MSME: perturb one feature at a
        /// time across its plausible range and measure how much the predicted score moves.
        /// Returns (featureName, signedImpact) — positive means the feature's current value
        /// is helping the score, negative means it's dragging it down.
        /// </summary>
        public IReadOnlyList<(string Feature, double Impact)> ExplainPrediction(ScoreFeatureVector input)
        {
            var (ml, model) = _trained.Value;
            var engine = ml.Model.CreatePredictionEngine<ScoreFeatureVector, Prediction>(model);
            var baseline = engine.Predict(input).Score;

            var impacts = new List<(string, double)>();
            for (int i = 0; i < ScoreFeatureVector.Count; i++)
            {
                // Neutral reference value per feature (trends → 0, shares/ratios → 0.5)
                float neutral = ScoreFeatureVector.FeatureNames[i] is "GstTurnoverTrend" or "ItrIncomeTrend"
                    ? 0f : 0.5f;

                var perturbed = new ScoreFeatureVector { Features = (float[])input.Features.Clone() };
                perturbed.Features[i] = neutral;
                var withoutFeature = engine.Predict(perturbed).Score;

                // baseline - neutralized: how many points this feature's actual value contributes
                impacts.Add((ScoreFeatureVector.FeatureNames[i], baseline - withoutFeature));
            }
            return impacts;
        }

        private (MLContext, ITransformer) Train()
        {
            var ml = new MLContext(seed: 42);
            var data = ml.Data.LoadFromEnumerable(_generator.Generate(3000));

            // Cap training threads so the CPU-bound warmup leaves a core free for the
            // web server to service requests (blob I/O) — otherwise a cold-start warmup
            // saturates all cores and concurrent blob reads time out (TaskCanceledException).
            var options = new LightGbmRegressionTrainer.Options
            {
                LabelColumnName = nameof(ScoreFeatureVector.Label),
                FeatureColumnName = nameof(ScoreFeatureVector.Features),
                NumberOfLeaves = 31,
                NumberOfIterations = 150,
                MinimumExampleCountPerLeaf = 20,
                NumberOfThreads = Math.Max(1, Environment.ProcessorCount - 1)
            };
            var pipeline = ml.Regression.Trainers.LightGbm(options);

            return (ml, pipeline.Fit(data));
        }
    }
}
