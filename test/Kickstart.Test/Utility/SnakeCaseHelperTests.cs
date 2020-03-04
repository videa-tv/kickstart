using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kickstart.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using Npgsql.NameTranslation;

namespace Kickstart.Utility.Tests
{
    [TestClass()]
    public class SnakeCaseHelperTests
    {
        [TestMethod()]
        public void SnakeCaseTest_DoW()
        {
            var val = "DoW".ToSnakeCase();

            Assert.AreEqual("dow", val);
        }

        [TestMethod()]
        public void SnakeCaseTest_ARgApi()
        {
            var val = "ARgApi".ToSnakeCase();

            Assert.AreEqual("arg_api", val);
        }

        [TestMethod()]
        public void SnakeCaseTest_Already_Snaked_Value()
        {
            
            var testVal = "Already_Snaked_Value";
            var val = testVal.ToSnakeCase();

            Assert.AreEqual("already_snaked_value", val);
        }

        [TestMethod]
        public void SnakeCaseTest_Many()
        {
            var testVals = GetTestVals();

            foreach (var testVal in testVals)
            {
                var trans = new NpgsqlSnakeCaseNameTranslator();

                Assert.AreEqual(testVal.Item2, testVal.Item1.ToSnakeCase());
            }
        }

        [TestMethod]
        public void SnakeCaseTest_ManyWithNpgsql()
        {
            var testVals = GetTestVals();

            foreach (var testVal in testVals)
            {
                var trans = new NpgsqlSnakeCaseNameTranslator(false);

                Assert.AreEqual(testVal.Item2, trans.TranslateMemberName(testVal.Item1));
            }
        }

        private static List<Tuple<string, string>> GetTestVals()
        {
            return new List<Tuple<string,string>>()
            {
                new Tuple<string, string>( "Already_Snaked_Value","already_snaked_value"),
                
                new Tuple<string, string>( "ARgApi","a_rg_api" ),
                new Tuple<string, string>( "DOW","dow"),
                new Tuple<string, string>( "DoW","dow"),

            };
        }
    }
}