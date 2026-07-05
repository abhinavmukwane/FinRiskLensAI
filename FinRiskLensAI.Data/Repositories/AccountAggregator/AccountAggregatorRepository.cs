using FinRiskLensAI.Core.Common;
using FinRiskLensAI.Core.Interfaces.IRepositories.AccountAggregator;
using FinRiskLensAI.Core.Models;
using FinRiskLensAI.Core.Models.AccountAggregator;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FinRiskLensAI.Data.Repositories.AccountAggregator
{
    public class AccountAggregatorRepository : IAccountAggregatorRepository
    {
        // Finvu login tokens last ~24h; refresh a little early.
        private const int TokenValidityMinutes = 1380;   // 23 hours

        private readonly DbContextEDMX.ApplicationDbContext _context;

        public AccountAggregatorRepository(DbContextEDMX.ApplicationDbContext context)
        {
            _context = context;
        }

        // ── Finvu token (single-row store) ─────────────────────────────

        public async Task<ResultModel<AccAggreTokenModel>> AddAAToken(AccAggreTokenModel entity)
        {
            var result = new ResultModel<AccAggreTokenModel>();
            try
            {
                _context.AccAggreTokens.Add(entity);
                await _context.SaveChangesAsync().ConfigureAwait(false);

                result.Result = tflResultType.tflSuccess;
                result.Message = "Token saved.";
                result.Data = entity;
            }
            catch (Exception ex)
            {
                result.Result = tflResultType.tflError;
                result.Message = ex.Message;
            }
            return result;
        }

        public async Task<ResultModel<AccAggreTokenModel>> UpdateAAToken(AccAggreTokenModel entity)
        {
            var result = new ResultModel<AccAggreTokenModel>();
            try
            {
                // Upsert: one token row for the whole app. Create it on first login.
                var existing = await _context.AccAggreTokens.FirstOrDefaultAsync().ConfigureAwait(false);
                if (existing == null)
                {
                    _context.AccAggreTokens.Add(entity);
                }
                else
                {
                    existing.rid = entity.rid;
                    existing.ts = entity.ts;
                    existing.token = entity.token;
                    existing.UpdatedOn = entity.UpdatedOn;
                }

                await _context.SaveChangesAsync().ConfigureAwait(false);

                result.Result = tflResultType.tflSuccess;
                result.Message = "Token updated.";
                result.Data = existing ?? entity;
            }
            catch (Exception ex)
            {
                result.Result = tflResultType.tflError;
                result.Message = ex.Message;
            }
            return result;
        }

        public AccAggreTokenModel GetAAToken()
        {
            // Never null — the service populates this and calls UpdateAAToken (which upserts),
            // so a first-time run with an empty table still seeds the token row.
            return _context.AccAggreTokens.AsNoTracking().FirstOrDefault()
                   ?? new AccAggreTokenModel();
        }

        public bool CheckAATokenValidity()
        {
            var token = _context.AccAggreTokens.AsNoTracking().FirstOrDefault();
            if (token == null || string.IsNullOrEmpty(token.token))
                return false;

            return (DateTime.Now - token.UpdatedOn).TotalMinutes < TokenValidityMinutes;
        }

        // ── AA consent requests ────────────────────────────────────────

        public async Task<ResultModel<AAConsentReqModel>> AddAAConsentRequest(AAConsentReqModel entity)
        {
            var result = new ResultModel<AAConsentReqModel>();
            try
            {
                _context.AAConsentRequests.Add(entity);
                await _context.SaveChangesAsync().ConfigureAwait(false);

                result.Result = tflResultType.tflSuccess;
                result.Message = "Consent request saved.";
                result.Data = entity;
            }
            catch (Exception ex)
            {
                result.Result = tflResultType.tflError;
                result.Message = ex.Message;
            }
            return result;
        }

        public async Task<AAConsentReqModel> GetAAConsentRequest(string transactionId)
        {
            return await _context.AAConsentRequests
                                 .AsNoTracking()
                                 .OrderByDescending(x => x.ConsentReqId)
                                 .FirstOrDefaultAsync(x => x.transactionId == transactionId)
                                 .ConfigureAwait(false);
        }
    }
}
