using System;
using fooTitle.Extending;
using fooTitle.Tests;

namespace utest1 {

    public class MainClass : IExtension {
        public void Init() {
            HelperClass.foo();
            ExtensionLoaderTest.Instance.SetByTest = 123;
        }
    }
}
