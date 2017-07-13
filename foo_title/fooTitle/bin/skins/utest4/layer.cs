using fooTitle;
using fooTitle.Extending;
using fooTitle.Layers;
using System;
using System.IO;

namespace utest4 {

    [LayerTypeAttribute("test-element-ext")]
    public class TestElementExt : TestElement {

        public TestElementExt(int pl) :base(10) { 
        }

    }
}
