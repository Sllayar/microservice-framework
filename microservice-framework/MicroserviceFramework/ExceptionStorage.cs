using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RFI
{
    public class ExceptionStorage
    {
        private static List<List<Tuple<string, DateTime>>> Exceptions { get; set; }
        public double TimeToReset { get; private set; }
        public double TimeToDrop { get; private set; }
        public int NumErrorToReload { get; private set; }

        /// <summary>
        /// Счетчик ошибок.
        /// </summary>
        /// <param name="timeToResetSec"> поподании в заданный интервал ошибок >= numErrorToReload выставит NeedReset в true </param>
        /// <param name="timeToDropSec"> Время через которое ошибка будет забыта </param>
        /// <param name="numErrorToReload"> Допустимое количество ошибок </param>
        public ExceptionStorage(double timeToResetSec, double timeToDropSec, int numErrorToReload)
        {
            Exceptions = new List<List<Tuple<string, DateTime>>>();

            TimeToReset = timeToResetSec;
            TimeToDrop = timeToDropSec;
            NumErrorToReload = numErrorToReload;
        }

        public void AddNewException(Exception newException)
        {
            var currExlist = Exceptions.FirstOrDefault(ex =>
                ex.Where(e => e.Item1.Equals(newException.Message)).Count() > 0);

            if (currExlist == null || currExlist.Count() == 0)
                Exceptions.Add(new List<Tuple<string, DateTime>>()
                    { new Tuple<string, DateTime>(newException.Message, DateTime.Now) });
            else
                currExlist.Add(new Tuple<string, DateTime>(newException.Message, DateTime.Now));
        }

        public void ClearOld()
        {
            foreach (var ex in Exceptions)
                if (ex.Count(e => (DateTime.Now - e.Item2).TotalSeconds > TimeToDrop) == ex.Count())
                    ex.Clear();
        }

        public bool NeedReset()
        {
            ClearOld();

            foreach (var ex in Exceptions)
                if (ex.Count >= NumErrorToReload && (ex.Max(e => e.Item2) - ex.Min(e => e.Item2)).TotalSeconds < TimeToReset)
                    return true;

            return false;
        }
    }
}
