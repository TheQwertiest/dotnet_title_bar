using System;
using System.Collections.Generic;
using System.Text;

using NUnit.Framework;

namespace naid.Tests {

    class ReflA { };

    class ReflB {
        ReflB(int x) { }
    };

    [TestFixture]
    public class test_ReflectionUtils {

        [Test]
        public void test_ConstructParameterless() {
            ReflA a = ReflectionUtils.ConstructParameterless<ReflA>(typeof(ReflA));
            Assert.IsNotNull(a);

            a = ReflectionUtils.ConstructParameterless<ReflA>();
            Assert.IsNotNull(a);
        }

        [Test]
        public void test_ConstructParameterless_exc() {
            Assert.That(() => ReflectionUtils.ConstructParameterless<ReflB>(typeof(ReflB)),
                Throws.TypeOf<InvalidOperationException>());
        }
    }
}
