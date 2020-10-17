using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RFI.MicroserviceFramework._Helpers
{
    public static class HExceptions
    {
        public static IEnumerable<Exception> GetInnerExceptions(this Exception ex)
        {
            if (ex == null)
            {
                throw new ArgumentNullException("ex");
            }

            var innerException = ex;
            do
            {
                yield return innerException;
                innerException = innerException.InnerException;
            }
            while (innerException != null);
        }
    }
}
