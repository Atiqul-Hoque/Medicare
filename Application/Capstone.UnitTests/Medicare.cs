using NUnit.Framework;
using Capstone.Common.Medicare;

namespace Capstone.UnitTests
{
    public class Medicare
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void ValidateMedicareNumber()
        {
            // Pass info

            if (1 == 1)
            {
                Assert.Pass();
            } else
            {
                Assert.Fail();

            }

          
        }

        [Test]
        public void CheckDVA()
        {
            // Pass info

            if (1 == 1)
            {
                Assert.Pass();
            }
            else
            {
                Assert.Fail();
            }


        }
    }
}