using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace fooTitle.Tests {
    public partial class TestReport : Form {
        public TestReport(List<TestFramework.TestResult> results) {
            InitializeComponent();

            fillResults(results);
        }

        protected void fillResults(List<TestFramework.TestResult> results) {
            panel1.BackColor = Color.Green;

            foreach (TestFramework.TestResult res in results) {
                treeView.Nodes.Add(createSingleResult(res));
            }
        }

        protected TreeNode createSingleResult(TestFramework.TestResult res) {
            TreeNode node = new TreeNode();
            node.Text = String.Format("method {0} expected {1}", res.method, res.expected);
            node.Nodes.Add(String.Format("passed: {0}", res.passed));
            node.Nodes.Add(String.Format("expected: {0}", res.expected));
            node.Nodes.Add(String.Format("received: {0}", res.received));
            node.Nodes.Add(String.Format("method: {0}", res.method));
            node.Nodes.Add(String.Format("File: {0}:{1}", res.file, res.line));
            node.Checked = res.passed;

            if (!res.passed)
                panel1.BackColor = Color.Red;
            return node;
        }
    }
}