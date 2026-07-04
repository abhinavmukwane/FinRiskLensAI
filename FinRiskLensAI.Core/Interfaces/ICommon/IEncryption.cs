using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinRiskLensAI.Core.Interfaces.ICommon
{
    public interface IEncryption
    {
        string EncryptString(string clearText);
        string DecryptString(string cipherText);

    }
}
