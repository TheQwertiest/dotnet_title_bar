using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using fooTitle.Extending;
using fooTitle.Layers;
using fooTitle.Geometries;

namespace fooTitle.Tests {

    [ElementTypeAttribute("test-element")]
    public class TestElement : Element {
        public TestElement(int a) : base() {

        }

        public int Test() {
            return 123;
        }
    }


    /// Keeps track of layer available in this and extension assemblies
    public class TestElementFactory : ElementFactory {

        public TestElementFactory() {
            elementType = typeof(TestElement);
            elementTypeAttributeType = typeof(ElementTypeAttribute);
        }

        public TestElement CreateLayer(String type, int pl) {
            return (TestElement)CreateElement(type, new object[] { pl });
        }

    }


    [TestFixture]
    public class ElementFactoryTest {

        [Test]
        public void TestCreateBuiltinElement() {
            TestElementFactory lf = new TestElementFactory();
            lf.SearchAssembly(Assembly.GetAssembly(typeof(TestElement)));
            TestElement l = (TestElement)lf.CreateLayer("test-element", 0);
            Assert.AreEqual(l.Test(), 123);
        }

        [Test]
        public void TestLayerInExtension() {
            TestElementFactory lf = new TestElementFactory();
            lf.SearchAssembly(Assembly.GetCallingAssembly());

            ExtensionLoader sl = new ExtensionLoader(new ElementFactory[] { lf });
            sl.LoadExtension("utest4", "skins/utest4/", new String[]{"layer.cs"});

            TestElement l = lf.CreateLayer("test-element-ext", 10);
        }

    }
}
