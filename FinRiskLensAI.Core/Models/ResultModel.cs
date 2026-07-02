using FinRiskLensAI.Core.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinRiskLensAI.Core.Models
{
    [DebuggerDisplay("Result = {Result.ToString()} Message : {Message}")]
    public class ResultModel<T>
    {
        public ResultModel()
        {
            Result = tflResultType.tflUnknown;
            Message = string.Empty;
            RowsAffected = -1;
            Data = default(T);

        }
        public tflResultType Result { get; set; }
        public string Message { get; set; }
        public int RowsAffected { get; set; }
        public T Data { get; set; }
    }
}
