using FinRiskLensAI.Core.Common;
using FinRiskLensAI.Core.Interfaces.IRepositories.Common;
using FinRiskLensAI.Core.Models;
using FinRiskLensAI.Core.Models.Universal;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace FinRiskLensAI.Data.Repositories.Common
{
    public class StaticResponseRepository : IStaticResponseRepository
    {
        private readonly DbContextEDMX.ApplicationDbContext _context;

        public StaticResponseRepository(DbContextEDMX.ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ResultModel<StaticResponseModel>> FetchByUdyamNumber(string udyamNumber)
        {
            var result = new ResultModel<StaticResponseModel>();

            try
            {
                var entity = await _context.StaticResponseModel
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.UdyamNumber == udyamNumber);

                if (entity != null)
                {
                    result.Result = tflResultType.tflSuccess;
                    result.Message = "Data Fetched Successfully";
                    result.Data = entity;
                }
                else
                {
                    result.Result = tflResultType.tflNoRecordFound;
                    result.Message = "No Data Found";
                    result.Data = null;
                }
            }
            catch (Exception ex)
            {
                result.Result = tflResultType.tflError;
                result.Message = ex.Message;
                result.Data = null;
            }

            return result;
        }
    }
}
