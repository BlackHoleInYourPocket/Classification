using weka.classifiers;
using weka.core;

namespace Classification.Bll
{
    public class ClassifierManager
    {
        public TrainModel Classifier { get; internal set; }
        public Instances Instance { get; internal set; }


        public ClassifierManager(weka.core.Instances insts)
        {
            this.Instance = insts;
        }

        public double FindAccurancy() //accurancy döndürcek
        {

            int numCorrect = 0;
            for (int i = Classifier.TrainSize; i < Instance.numInstances(); i++)
            {
                weka.core.Instance currentInst = Instance.instance(i);
                double predictedClass = Classifier.classifier.classifyInstance(currentInst);
                if (predictedClass == Instance.instance(i).classValue())
                    numCorrect++;
            }
            return (double)numCorrect / (double)Classifier.TestSize * 100.0;

        }

        public void Discreatization(weka.core.Instances instances)
        {
            weka.filters.unsupervised.attribute.Discretize discretized = new weka.filters.unsupervised.attribute.Discretize();
            discretized.setInputFormat(instances);
            //discretize.setFindNumBins(true);
            instances = weka.filters.Filter.useFilter(instances, discretized);
            this.Instance = instances;
        }

        public void EliminateTargetAttribute()
        {
            this.Instance.setClassIndex(this.Instance.numAttributes() - 1);
        }




        /// <summary>
        /// Normalize to numeric instance
        /// </summary>
        /// <param name="instances"></param>
        /// <returns></returns>
        public void Normalization(weka.core.Instances instances)
        {

            weka.filters.Filter normalized = new weka.filters.unsupervised.instance.Normalize();
            normalized.setInputFormat(instances);
            instances = weka.filters.Filter.useFilter(instances, normalized);
            this.Instance = instances;
        }

        /// <summary>
        /// Dummy attribute
        /// </summary>
        /// <param name="instances"></param>
        /// <returns></returns>
        public void NominalToBinary(weka.core.Instances instances)
        {
            weka.filters.Filter nominalToBinary = new weka.filters.unsupervised.attribute.NominalToBinary();
            nominalToBinary.setInputFormat(instances);
            instances = weka.filters.Filter.useFilter(instances, nominalToBinary);
            this.Instance = instances;
        }

        /// <summary>
        /// Randomize select random data from data set
        /// </summary>
        /// <param name="instances"></param>
        /// <returns></returns>
        public void Randomize(weka.core.Instances instances)
        {
            weka.filters.Filter randomize = new weka.filters.unsupervised.instance.Randomize();
            randomize.setInputFormat(this.Instance);
            instances = weka.filters.Filter.useFilter(instances, randomize);
            this.Instance = instances;

        }


        /// <summary>
        /// Train
        /// </summary>
        /// <param name="instances"></param>
        /// <returns></returns>
        public TrainModel Train(weka.core.Instances instances, Classifier classifier)
        {
            const int percentSplit = 66;
            int trainSize = instances.numInstances() * percentSplit / 100;
            int testSize = instances.numInstances() - trainSize;
            weka.core.Instances train = new weka.core.Instances(instances, 0, trainSize);

            classifier.buildClassifier(train);

            return this.Classifier = new TrainModel()
            {
                PercentSplit = percentSplit,
                classifier = classifier,
                TestSize = testSize,
                TrainSize = trainSize,
                Instance = instances
            };

        }
    }

}
