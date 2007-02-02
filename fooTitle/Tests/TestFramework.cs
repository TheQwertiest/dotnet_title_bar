using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace fooTitle.Tests {
    [AttributeUsage(AttributeTargets.Method)]
    public class TestMethodAttribute : Attribute {

    }

    [AttributeUsage(AttributeTargets.Method)]
    public class ExpectedExceptionAttribute : Attribute {
        public ExpectedExceptionAttribute(string typeName) {

        }
    }

    public class TestFramework {
        protected int failedCount = 0;
        protected int passedCount = 0;

        public delegate void TestDelegate();

        public class TestResult {
            public bool passed;
            public object expected;
            public object received;

            public string file;
            public string method;
            public int line;
        }
        
        protected List<TestResult> results;

        public TestFramework() {
            results = new List<TestResult>();
        }

        public virtual List<TestResult> Run() {
            // run methods marked with the test attribute
            foreach (MethodInfo method in this.GetType().GetMethods()) {
                if (method.GetCustomAttributes(typeof(TestMethodAttribute), false).Length != 0) {
                    method.Invoke(this, null);
                }
            }

            return results;
        }

        #region Creating results
        protected TestResult passed() {
            TestResult res = createResult();
            res.passed = true;
            passedCount++;
            return res;
        }

        protected TestResult failed() {
            TestResult res = createResult();
            res.passed = false;
            failedCount++;
            return res;
        }

        private TestResult createResult() {
            TestResult res = new TestResult();
            System.Diagnostics.StackFrame stackFrame = new System.Diagnostics.StackFrame(3, true);
            res.file = stackFrame.GetFileName();
            res.line = stackFrame.GetFileLineNumber();
            res.method = stackFrame.GetMethod().Name;
            results.Add(res);
            return res;
        }

        #endregion

        #region Assertions
        public void AssertEquals(object received, object expected) {
            TestResult res;

            if (received == null) {
                if (expected == null) {
                    res = passed();
                } else {
                    res = failed();
                }
            } else {
                if (!received.Equals(expected)) {
                    res = failed();
                } else {
                    res = passed();
                }
            }

            res.expected = expected;
            res.received = received;
        }

        public void AssertExceptionThrown<T>(TestDelegate method) where T: System.Exception {
            try {
                method.DynamicInvoke(null);
            } catch (System.Reflection.TargetInvocationException e) {
                if (e.InnerException.GetType().Equals(typeof(T))) {
                    TestResult res = passed();
                    res.expected = typeof(T).ToString();
                } else {
                    TestResult res = failed();
                    res.expected = typeof(T).ToString();
                    res.received = e.InnerException.ToString();
                }
                return;
            }

            TestResult res2 = failed();
            res2.expected = typeof(T).ToString();
            res2.received = "no exception";
        }

        #endregion

        public void ReportGUI() {
            TestReport report = new TestReport(results);
            report.Show();
        }
    }


    /// <summary>
    /// Used to create a list of subtests to run.
    /// </summary>
    public class TestList : TestFramework{
        protected List<TestFramework> subtests = new List<TestFramework>();

        public void AddTest(TestFramework test) {
            subtests.Add(test);
        }

        public override List<TestResult> Run() {
            foreach (TestFramework t in subtests) {
                this.results.AddRange(t.Run());
            }

            return this.results;
        }
    }
}
