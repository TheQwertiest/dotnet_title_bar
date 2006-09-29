using fooTitle;
using fooTitle.Extending;
using fooTitle.Layers;
using System;
using System.IO;

namespace pokus {
    public class HelloWorld : IExtension {

        public void Init() {
            Console.WriteLine(">> Hello World <<");

            /*
            DecoratedWriter w = new DecoratedWriter();
            w.Write("Hello World");

            Console.WriteLine("i is {0}", fooTitle.MainClass.i);

            StreamWriter wr = File.CreateText("/home/xplasil/test/pokus.txt");
            wr.WriteLine("This is my file");
            wr.Close();
            */

        }
    }

    public class GoodBye : IExtension {

        public void Init() {
            Console.WriteLine("Good bye.");
        }
    }

    [LayerTypeAttribute("button")]
    public class Button : Layer {
        public Button(int x) :base(x) {
            Console.WriteLine("Button {0} created", x);

        }

    }
}
