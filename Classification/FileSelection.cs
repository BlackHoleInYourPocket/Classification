using Classification.Bll;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using weka.classifiers;

namespace Classification
{
    public partial class FileSelection : Form
    {

        private List<AlgorithmModel> SuccessfulAlgorithm;
        private List<Object> testValues;
        private List<string> labelNames;
        Classifier predictor;
        private string algoritmName;
        List<string> classes;
        weka.core.Instances staticInsts;
        int pointY = 0;
        int pointX = 0;
        public FileSelection()
        {
            InitializeComponent();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            panel1.Controls.Clear();
            panel2.Controls.Clear();
            testValues = new List<object>();
            classes = new List<string>();
            algoritmName = String.Empty;
            SuccessfulAlgorithm = new List<AlgorithmModel>();
            labelNames = new List<string>();
            DialogResult result = openFileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                txtPath.Text = openFileDialog.FileName;
            }


            staticInsts = new weka.core.Instances(new java.io.FileReader(txtPath.Text));

            for (int i = 0; i < staticInsts.attribute(staticInsts.numAttributes() - 1).numValues(); i++)
            {
                classes.Add(staticInsts.attribute(staticInsts.numAttributes() - 1).value(i));
            }

            AlgoritmAccurancy(staticInsts, new weka.classifiers.bayes.NaiveBayes(), "Naive Bayes", true);
            AlgoritmAccurancy(staticInsts, new weka.classifiers.lazy.IBk(3), "KNN with k = 3", false);
            AlgoritmAccurancy(staticInsts, new weka.classifiers.trees.RandomForest(), "Random Forest");
            AlgoritmAccurancy(staticInsts, new weka.classifiers.trees.RandomTree(), "Random Tree");
            AlgoritmAccurancy(staticInsts, new weka.classifiers.trees.J48(), "J48");

            pointY = 20;
            pointX = 20;

            for (int i = 0; i < staticInsts.numAttributes() - 1; i++)
            {

                if (staticInsts.attribute(i).numValues() == 0)
                {
                    pointX = 0;
                    string attName = staticInsts.attribute(i).name(); // Bunlar numeric textbox aç
                    Label attributeName = new Label();
                    attributeName.Size = new Size(70, 20);
                    attributeName.Text = attName + "\t :";
                    labelNames.Add(attributeName.Text);
                    attributeName.Location = new Point(pointX, pointY);
                    panel1.Controls.Add(attributeName);

                    pointX += 70;
                    TextBox txtValue = new TextBox();
                    txtValue.Location = new Point(pointX, pointY);
                    panel1.Controls.Add(txtValue);
                    panel1.Show();
                    pointY += 30;
                    testValues.Add(txtValue);
                }
                else
                {
                    pointX = 0;
                    string attName = staticInsts.attribute(i).name(); // Bunlar numeric textbox aç
                    Label attributeName = new Label();
                    attributeName.Size = new Size(70, 20);
                    attributeName.Text = attName + "\t :";
                    labelNames.Add(attributeName.Text);
                    attributeName.Location = new Point(pointX, pointY);
                    panel1.Controls.Add(attributeName);
                    pointX += 70;

                    ComboBox cb = new ComboBox();
                    cb.DropDownStyle = ComboBoxStyle.DropDownList;
                    cb.Location = new Point(pointX, pointY);
                    List<string> items = new List<string>();
                    for (int j = 0; j < staticInsts.attribute(i).numValues(); j++)
                    {
                        items.Add(staticInsts.attribute(i).value(j).ToString()); // Bu gelen valueları dropdowna koy
                    }
                    cb.Items.AddRange(items.ToArray());
                    cb.SelectedIndex = 0;
                    panel1.Controls.Add(cb);
                    panel1.Show();
                    pointY += 30;
                    testValues.Add(cb);
                }
            }

            double maxRatio = Double.MinValue;
            foreach (var item in SuccessfulAlgorithm)
            {
                if (item.SuccessRatio > maxRatio)
                {
                    maxRatio = item.SuccessRatio;
                    algoritmName = item.AlgorithName;
                    predictor = item.TrainModel.classifier;
                }
            }
            string _maxRatio = string.Format("{0:0.00}", maxRatio);
            lblSuccessulAlgorithm.Text = "The most Successful Algoritm is " + algoritmName + " and the ratio of accurancy is %" + _maxRatio;

            Button btn = new Button();
            btn.Click += Btn_Click;
            btn.Location = new Point(pointX, pointY);
            btn.Size = new Size(80, 20);
            btn.Text = "DISCOVER";
            btn.BackColor = Color.White;
            panel1.Controls.Add(btn);
            panel1.Show();
        }

        private void Btn_Click(object sender, EventArgs e)
        {
            bool isValid = true;
            for (int j = 0; j < staticInsts.instance(0).numValues() - 1; j++)
            {
                if (!isValid) break;
                for (int i = 0; i < testValues.Count; i++)
                {
                    if (testValues[i].GetType() == typeof(TextBox))
                    {
                        double res = 0;
                        if (!string.IsNullOrEmpty(((TextBox)testValues[i]).Text) &&
                             !double.TryParse(((TextBox)testValues[i]).Text, out res))
                        {
                            MessageBox.Show("Enter a valid value for " + labelNames[i]);
                            isValid = false;
                            break;
                        }
                        else if (string.IsNullOrEmpty(((TextBox)testValues[i]).Text))
                        {
                            MessageBox.Show("Enter a Value for " + labelNames[i]);
                            isValid = false;
                            break;
                        }
                        else
                        {
                            staticInsts.instance(0).setValue(i, Convert.ToDouble(((TextBox)testValues[i]).Text));
                            isValid = true;
                        }
                    }
                    else if (testValues[i].GetType() == typeof(ComboBox))
                    {
                        staticInsts.instance(0).setValue(i, ((ComboBox)testValues[i]).SelectedItem.ToString());
                    }
                }
            }
            if (isValid)
            {
                ClassifierManager manager = new ClassifierManager(staticInsts);
                if (algoritmName == "Naive Bayes")
                {
                    manager.Discreatization(manager.Instance);
                }
                else if (algoritmName == "KNN with k = 3")
                {
                    manager.NominalToBinary(manager.Instance);
                    manager.Normalization(manager.Instance);
                }
                double a = predictor.classifyInstance(manager.Instance.firstInstance());
                string result = classes[(int)a];

                MessageBox.Show("Result : " + result);
            }
        }

        private void AlgoritmAccurancy(weka.core.Instances insts, Classifier classifier, string algoritmName, bool? isNominal = null)
        {
            ClassifierManager manager = new ClassifierManager(insts);
            manager.EliminateTargetAttribute();
            if (isNominal == true)
            {
                manager.Discreatization(manager.Instance);

            }
            else if (isNominal == false)
            {
                manager.NominalToBinary(manager.Instance);
                manager.Normalization(manager.Instance);
            }

            manager.Randomize(manager.Instance);

            TrainModel model = manager.Train(manager.Instance, classifier);

            SuccessfulAlgorithm.Add(new AlgorithmModel()
            {
                SuccessRatio = manager.FindAccurancy(),
                AlgorithName = algoritmName,
                TrainModel = model
            }); ;
        }
    }

}
