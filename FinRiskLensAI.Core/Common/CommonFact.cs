using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinRiskLensAI.Core.Common
{
    public delegate void OnPropertyChange(object sender, object oldValue, object newValue);

    public enum tflResultType
    {
        tflUserAuthenticated = 0,
        tflUserNameOrPasswordBad = 1,
        tflUserIsInActive = 2,
        tflError = 3,
        tflWarning = 4,
        tflInformation = 5,
        tflSuccess = 6,
        tflUnknown = 7,
        tflEmptyModel = 8,
        tflNoRecordFound = 9,
        tflFileContentEmpty = 10,
        tflKeyIDValueIsZero = 11,
        tflUnauthorizedAccess = 12,
        tflAlreadyExists = 13
    }

    public abstract class CommonFacet
    {
        public DateTime CREATEDON { get; set; }
        //public int CreatedBy { get; set; }
        //public int? LastModifiedBy { get; set; }
        public DateTime? UPDATEDON { get; set; }
    }
}
