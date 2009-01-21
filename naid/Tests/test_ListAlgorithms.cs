using System;
using System.Collections.Generic;
using System.Text;

using NUnit.Framework;

namespace naid.Tests {
    [TestFixture]
    public class test_ListAlgorithms {

        [Test]
        public void test_BinarySearch() {
            List<int> x = new List<int>();
            x.Add(1);
            x.Add(3);
            x.Add(5);
            x.Add(7);
            x.Add(11);
            x.Add(13);
            x.Add(17);
            x.Add(19);

            Assert.AreEqual(3, ListAlgorithms<int>.BinarySearch(x, delegate(int item) {
                return item - 3;
            }));

            Assert.AreEqual(1, ListAlgorithms<int>.BinarySearch(x, delegate(int item) {
                return item - 1;
            }));

            Assert.AreEqual(19, ListAlgorithms<int>.BinarySearch(x, delegate(int item) {
                return item - 19;
            }));

            Assert.AreEqual(11, ListAlgorithms<int>.BinarySearch(x, delegate(int item) {
                return item - 11;
            }));

            Assert.AreEqual(7, ListAlgorithms<int>.BinarySearch(x, delegate(int item) {
                return item - 7;
            }));

            Assert.AreEqual(13, ListAlgorithms<int>.BinarySearch(x, delegate(int item) {
                return item - 13;
            }));

            Assert.AreEqual(0, ListAlgorithms<int>.BinarySearch(x, delegate(int item) {
                return item - 4; // searching for 4
            }));
        }

        [Test]
        public void test_BinarySearch_boundary() {
            Assert.AreEqual(0, ListAlgorithms<int>.BinarySearch(new List<int>(), delegate(int item) {
                return -1;
            }));

            Assert.AreEqual(0, ListAlgorithms<int>.BinarySearch(new List<int>(), delegate(int item) {
                return 1;
            }));

            ExtendedCollection<int> x = new ExtendedCollection<int>();
            x.Add(1);
            Assert.AreEqual(0, ListAlgorithms<int>.BinarySearch(x, delegate(int item) {
                return 1;
            }));
            Assert.AreEqual(1, ListAlgorithms<int>.BinarySearch(x, delegate(int item) {
                return item - 1;
            }));

        }

    }
}
