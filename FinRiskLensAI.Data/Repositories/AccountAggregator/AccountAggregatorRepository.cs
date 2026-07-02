using FinRiskLensAI.Core.Common;
using FinRiskLensAI.Core.Interfaces.IRepositories.AccountAggregator;
using FinRiskLensAI.Core.Models;
using FinRiskLensAI.Core.Models.AccountAggregator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinRiskLensAI.Data.Repositories.AccountAggregator
{
    public class AccountAggregatorRepository : IAccountAggregatorRepository
    {
        private readonly DbContextEDMX.ApplicationDbContext _context;

        public AccountAggregatorRepository(DbContextEDMX.ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ResultModel<AccAggreTokenModel>> AddAAToken(AccAggreTokenModel entity)
        {
           
            return null;
        }

        public async Task<ResultModel<AccAggreTokenModel>> UpdateAAToken(AccAggreTokenModel entity)
        {
           
            return null;
        }

        public AccAggreTokenModel GetAAToken()
        {
            return null;
        }

        public bool CheckAATokenValidity()
        {
            return false;
        }

        public async Task<ResultModel<AAConsentReqModel>> AddAAConsentRequest(AAConsentReqModel entity)
        {
             return null;
        }

        public async Task<AAConsentReqModel> GetAAConsentRequest(string transactionId)
        {
            return null;
        }

        //public AccountAggregatorRepository(DbContextEDMX.ApplicationDbContext context)
        //{
        //    _context = context;
        //}

        //public async Task<ResultModel<AccAggreTokenModel>> AddAAToken(AccAggreTokenModel entity)
        //{
        //    var result = new ResultModel<AccAggreTokenModel>();
        //    try
        //    {
        //        ADM_ACCAGGRETOKEN paObj = new ADM_ACCAGGRETOKEN();
        //        entity.MapToModelObject(paObj);
        //        _context.ADM_ACCAGGRETOKEN.Add(paObj);
        //        await _context.SaveChangesAsync().ConfigureAwait(false);
        //        result.Result = tflResultType.tflSuccess;
        //        result.Message = "Data Saved Successfully";
        //        result.Data = entity;
        //    }
        //    catch (Exception ex)
        //    {
        //        result.Result = tflResultType.tflError;
        //        result.Message = ex.Message;
        //        result.Data = null;
        //    }
        //    return result;
        //}

        //public async Task<ResultModel<AccAggreTokenModel>> UpdateAAToken(AccAggreTokenModel entity)
        //{
        //    var result = new ResultModel<AccAggreTokenModel>();
        //    try
        //    {
        //        if (entity?.TokenID == null)
        //        {
        //            result.Result = tflResultType.tflError;
        //            result.Message = "Invalid input: TokenID is required.";
        //            return result;
        //        }

        //        var data = _context.ADM_ACCAGGRETOKEN
        //                           .FirstOrDefault(a => a.TOKENID == entity.TokenID);
        //        if (data != null)
        //        {
        //            entity.MapToModelObject(data, new List<string> { "TOKENID" });
        //            await _context.SaveChangesAsync().ConfigureAwait(false);
        //            result.Result = tflResultType.tflSuccess;
        //            result.Message = "Data Updated Successfully";
        //        }
        //        else
        //        {
        //            result.Result = tflResultType.tflNoRecordFound;
        //            result.Message = "No Data Found for this entry";
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        result.Result = tflResultType.tflError;
        //        result.Message = ex.Message;
        //        result.Data = null;
        //    }
        //    return result;
        //}

        //public AccAggreTokenModel GetAAToken()
        //{
        //    AccAggreTokenModel aaMdl = new AccAggreTokenModel();
        //    var entity = _context.ADM_ACCAGGRETOKEN.FirstOrDefault();
        //    if (entity != null) entity.MapToModelObject(aaMdl);
        //    return aaMdl;
        //}

        //public bool CheckAATokenValidity()
        //{
        //    var latestAuthRecord = _context.ADM_ACCAGGRETOKEN.FirstOrDefault();
        //    if (latestAuthRecord != null)
        //    {
        //        var timeDifference = DateTime.Now - latestAuthRecord.UPDATEDON;
        //        var expiryInMinutes = 1380;
        //        return timeDifference.TotalMinutes < expiryInMinutes;
        //    }
        //    return false;
        //}

        //public async Task<ResultModel<AAConsentReqModel>> AddAAConsentRequest(AAConsentReqModel entity)
        //{
        //    var result = new ResultModel<AAConsentReqModel>();
        //    try
        //    {
        //        T_ACCAGGRECONSENTREQUEST paObj = new T_ACCAGGRECONSENTREQUEST();
        //        entity.MapToModelObject(paObj);
        //        _context.T_ACCAGGRECONSENTREQUEST.Add(paObj);
        //        await _context.SaveChangesAsync().ConfigureAwait(false);
        //        result.Result = tflResultType.tflSuccess;
        //        result.Message = "Data Saved Successfully";
        //        result.Data = entity;
        //    }
        //    catch (Exception ex)
        //    {
        //        result.Result = tflResultType.tflError;
        //        result.Message = ex.Message;
        //        result.Data = null;
        //    }
        //    return result;
        //}

        //public async Task<AAConsentReqModel> GetAAConsentRequest(string transactionId)
        //{
        //    AAConsentReqModel aaMdl = new AAConsentReqModel();
        //    var entity = await _context.T_ACCAGGRECONSENTREQUEST
        //                              .FirstOrDefaultAsync(a => a.TRANSACTIONID == transactionId)
        //                              .ConfigureAwait(false);
        //    if (entity != null)
        //        entity.MapToModelObject(aaMdl);
        //    return aaMdl;
        //}
    }
}
