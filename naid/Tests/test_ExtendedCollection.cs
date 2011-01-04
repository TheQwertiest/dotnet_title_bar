using System;
using System.Collections.Generic;
using System.Text;

using NUnit.Framework;
using naid;

namespace naid.Tests {

    [TestFixture]
    public class test_ExtendedCollection  {

        private ExtendedCollection<int> intCollection;
        private ExtendedCollection<string> stringCollection;

        [SetUp]
        public void SetUp() {
            intCollection = new ExtendedCollection<int>();
            intCollection.Add(1);
            intCollection.Add(2);
            intCollection.Add(3);
            intCollection.Add(5);

            stringCollection = new ExtendedCollection<string>();
            stringCollection.Add("a");
            stringCollection.Add("b");
            stringCollection.Add("c");
        }

        [Test]
        public void test_Find() {

            Assert.AreEqual(intCollection.Find(delegate(int i) {
                return i == 5;
            }), 5);
            Assert.AreEqual(intCollection.Find(delegate(int i) {
                return i == 1;
            }), 1);

            Assert.AreEqual(intCollection.Find(delegate(int i) {
                return i == 0;
            }), 0);

            Assert.AreEqual(stringCollection.Find(delegate(string i) {
                return i == "b";
            }), "b");

            Assert.AreEqual(stringCollection.Find(delegate(string i) {
                return i == "not existing";
            }), null);
        }

        [Test]
        [ExpectedException(typeof(NullReferenceException))]
        public void test_exc_on_find_null() {
            intCollection.Find(null);
        }

        [Test]
        public void test_FindAll_int() {
            List<int> res = intCollection.FindAll(delegate(int i) {
                return (i == 1) || (i == 2) || (i == 3) || (i == 1000);
            });

            Assert.AreEqual(res.Count, 3);
            Assert.AreEqual(res.Contains(1), true);
            Assert.AreEqual(res.Contains(3), true);
            Assert.AreEqual(res.Contains(2), true);
        }
    }
}
