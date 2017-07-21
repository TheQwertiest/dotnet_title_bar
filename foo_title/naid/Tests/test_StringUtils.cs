using System;
using System.Collections.Generic;
using System.Text;

using NUnit.Framework;

namespace naid.Tests {
    
    [TestFixture]
    public class test_StringUtils {

        [Test]
        public void test_Join() {
            Assert.AreEqual("1,2,3", StringUtils.Join(",", new string[] { "1", "2", "3" }));

            ICollection<string> coll = new List<string>();
            coll.Add("x");
            coll.Add("x");
            coll.Add("yyy");
            coll.Add("yyy/yyy");
            Assert.AreEqual("x/x/yyy/yyy/yyy", StringUtils.Join("/", coll));

            Assert.AreEqual("x|a|x|a|yyy|a|yyy/yyy", StringUtils.Join("|a|", coll));
        }

    }
}
