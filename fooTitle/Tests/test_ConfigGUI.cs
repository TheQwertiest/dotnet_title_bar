using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;



using fooTitle.Tests;
using fooTitle.Config;



namespace fooTitle.Tests {

 
    public partial class test_ConfigGUI : Form {

    //    TextBoxWrapper wrapper, wrapper2;
     //   XTrackBarWrapper trwrapper;
//        XRadioGroupWrapper radioWrapper;
        AutoWrapperCreator autoWrapperCreator = new AutoWrapperCreator();
        public test_ConfigGUI() {
            InitializeComponent();
            /*
            wrapper = new TextBoxWrapper(textBox1, "test/paperCount");
            wrapper2 = new TextBoxWrapper(textBox2, "test/paperCount");
            trwrapper = new XTrackBarWrapper(trackBar1, "test/lunchType");
            radioWrapper = new XRadioGroupWrapper("test/lunchType");
            radioWrapper.AddRadioButton(radioButton1);
            radioWrapper.AddRadioButton(radioButton2);
            radioWrapper.AddRadioButton(radioButton3);
            */

            // load and set from the values manager
            // TODO - problem - co kdyz se nektere hodnoty vytvori az po vytvoreni tohohle dialogu ?
            //         + pri vytvoreni hodnoty asi hazet ValueChanged.. pripadne vytvorit dalsi event

            initForTesting();
            autoWrapperCreator.CreateWrappers(this);
        }
        TextBoxWrapper textBoxWrapper1, textBoxWrapper2;
        TrackBarWrapper trackBarWrapper1;
        RadioGroupWrapper radioGroupWrapper1;

        protected void initForTesting() {
            textBoxWrapper1 = new TextBoxWrapper(textBox1, "test/aCount");
            textBoxWrapper2 = new TextBoxWrapper(textBox2, "test/aCount");

            trackBarWrapper1 = new TrackBarWrapper(trackBar1, "test/bCount");

            radioGroupWrapper1 = new RadioGroupWrapper("test/cCount");
            radioGroupWrapper1.AddRadioButton(radioButton1);
            radioGroupWrapper1.AddRadioButton(radioButton2);
            radioGroupWrapper1.AddRadioButton(radioButton3);


        }



    }



     

    



    

   
}