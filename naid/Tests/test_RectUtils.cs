using System;
using System.Collections.Generic;
using System.Text;

using NUnit.Framework;
using naid;
using System.Drawing;

namespace naid.Tests {

    [TestFixture]
    public class test_RectUtils {

        [Test]
        public void test_basic() {
            Rectangle r = new Rectangle(0, 0, 10, 10);
            RectUtils.SetLeft(ref r, -10);
            Assert.AreEqual(-10, r.Left);
            Assert.AreEqual(10, r.Right);

            RectUtils.SetTop(ref r, -10);
            Assert.AreEqual(-10, r.Top);
            Assert.AreEqual(10, r.Bottom);

            RectUtils.SetRight(ref r, 20);
            Assert.AreEqual(20, r.Right);
            Assert.AreEqual(-10, r.Left);

            RectUtils.SetBottom(ref r, 30);
            Assert.AreEqual(30, r.Bottom);
            Assert.AreEqual(-10, r.Top);
        }

    }
}
