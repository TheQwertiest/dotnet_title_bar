using System;
using System.IO;
using NUnit.Framework;
using fooTitle.Extending;
using fooTitle.Layers;
using fooTitle.Geometries;

namespace fooTitle.Tests {

    [TestFixture]
    public class ExtensionLoaderTest {
        public static ExtensionLoaderTest Instance;
        public int SetByTest;

        public ExtensionLoaderTest() {
            Instance = this;
        }

        [Test]
        public void LoadExtension() {
            ExtensionLoader el = new ExtensionLoader(new LayerFactory(), new GeometryFactory());
            SetByTest = 0;
            el.LoadExtension("utest1", "skins/utest1/", new String[]{"utest1.cs", "file2.cs"});
            Assert.AreEqual(this.SetByTest, 123);
        }

        [Test]
        [ExpectedException(typeof(System.IO.FileNotFoundException))]
        public void MissingFile()  {
            ExtensionLoader el = new ExtensionLoader(new LayerFactory(), new GeometryFactory());
            el.LoadExtension("not-existing", "not-existing", new String[]{"non-existant.cs"});

        }

        [Test]
        [ExpectedException(typeof(fooTitle.Extending.ExtensionCompileException))]
        public void CompileError() {
            ExtensionLoader el = new ExtensionLoader(new LayerFactory(), new GeometryFactory());
            el.LoadExtension("utest2", "skins/utest2/", new String[]{"error.cs"});
        }

        /*
        [Test]
        public void TestRebuildByDate() {
            ExtensionLoader el = new ExtensionLoader(new LayerFactory(), new GeometryFactory());
            SetByTest = 0;

            File.Delete("skins/utest3/current.cs");
            File.Delete("skins/utest3/utest3.dll");


            //File.Copy("skins/utest3/first.cs", "skins/utest3/current.cs", true);
            File.SetLastWriteTime("skins/utest3/first.cs", DateTime.Now);
            el.LoadExtension("utest3", "skins/utest3/", new String[]{"first.cs"});
            Assert.AreEqual(this.SetByTest, 1);


            el = new ExtensionLoader(new LayerFactory(), new GeometryFactory());
            //File.Copy("skins/utest3/second.cs", "skins/utest3/current.cs", true);
            File.SetLastWriteTime("skins/utest3/second.cs", DateTime.Now.AddDays(10));
            el.LoadExtension("utest3", "skins/utest3/", new String[]{"second.cs"});
            Assert.AreEqual(this.SetByTest, 2);
        }
        */

    }

}
