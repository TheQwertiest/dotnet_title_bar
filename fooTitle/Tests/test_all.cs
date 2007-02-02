using System;
using System.Collections.Generic;
using System.Text;
using fooTitle.Tests;

namespace fooTitle.Tests {
    class test_all : fooTitle.Tests.TestList{
        public test_all() {
            // create the test list
            this.AddTest(new test_Config());
        }

    }
}
