using System;
using fooTitle.Extending;
using fooTitle.Tests;

namespace utest3 {

    public class MainClass : IExtension {
        public void Init() {
            Console.WriteLine("Running second");
            ExtensionLoaderTest.Instance.SetByTest = 2;
        }
    }
}
