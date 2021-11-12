using System.Linq;

namespace Capstone.Common.Medicare.Validation
{
    public static class MedicareCommonValidations
    {
        public static bool MedicareValidityCheck(string medicareNumber)
        {
            if (!(medicareNumber?.Length == 10 ) || !medicareNumber.All(char.IsDigit))
                return false;
           
            var medArray = medicareNumber.Select(c => (int)char.GetNumericValue(c)).ToArray();
            if (medArray[9] == 0)
            {
                return false;
            }
            int checkSum = medArray.Zip(new[] { 1, 3, 7, 9, 1, 3, 7, 9 }, (m, d) => m * d).Sum() % 10;
            if (checkSum == medArray[8])
            {
                return true;
            }
            return false;
        }
        public static bool VeteranNumberValidityCheck(string veteranNumber)
        {
            if ((veteranNumber?.Length < 3) || (veteranNumber?.Length > 9))
            {
                return false;
            }
            else {  
               return true;
            }
        }
    }
}
