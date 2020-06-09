using weka.classifiers;
using weka.core;
namespace Classification.Bll
{
    public class TrainModel
    {
        public double PercentSplit { get; set; }
        public int TrainSize { get; set; }

        public int TestSize { get; set; }
        public Classifier classifier { get; set; }

        public Instances Instance { get; set; }
    }
}
