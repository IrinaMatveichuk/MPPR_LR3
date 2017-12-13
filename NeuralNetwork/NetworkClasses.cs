using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace NeuralNetwork
{
    public static class Globals
    {
        public static Random rand = new Random();
    }
    public class Neuron
    {
        public double[,] weight;
        public int row = 28, column = 28;
        public int p = 50;

        public Neuron()
        {
            weight = new double[row, column];
            RandWeights();
        }
        
        public int Multiply(int[,] input)
        {
            double power = 0;
            for (int r = 0; r < row; r++)
            {
                for (int c = 0; c < column; c++)
                {
                    power += weight[r, c] * input[r, c];
                }
            }
            return power >= p ? 1 : 0;
        }
        
        void RandWeights()
        {
            for (int r = 0; r < row; r++)
                for (int c = 0; c < column; c++)
                    weight[r, c] = Globals.rand.NextDouble();
        }
        
        public void ChangeWeights(int[,] input, int d, double speed)
        {
            for (int r = 0; r < row; r++)
                for (int c = 0; c < column; c++)
                    weight[r, c] += d * input[r, c]*speed;
        }

        public void WriteTxt(string path)
        {
            double[,] w = Normalize(weight);
            using (StreamWriter sw = new StreamWriter(path, false, System.Text.Encoding.Default))
            {
                for (int i = 0; i < 28; i++)
                {
                    for (int j = 0; j < 28; j++)
                    {
                        sw.Write(Math.Round(w[i, j], 2) + " ");
                    }
                    sw.WriteLine();
                }
            }
        }

        public double[,] Normalize(double[,] input)
        {
            double[,] output = new double[row, row];
            double min = input[0, 0];
            double max = input[0, 0];
            for (int i = 0; i < row; i++)
                for (int j = 0; j < row; j++)
                {
                    if (input[i, j] > max) max = input[i, j];
                    if (input[i, j] < min) min = input[i, j];
                }
            for (int i = 0; i < row; i++)
                for (int j = 0; j < row; j++)
                {
                    output[i,j] = (input[i, j] - min) / (max - min);
                }
            return output;
        }
    }   

    public class NN
    {
        public Neuron[] neurons;
        public string[] files = new string[10];

        public NN()
        {
            neurons = new Neuron[10];

            for (int i = 0; i < neurons.Length; i++)
                neurons[i] = new Neuron();
        }
        
        int[] Count(int[,] input)
        {
            int[] output = new int[neurons.Length];
            for (int i = 0; i < output.Length; i++)
                output[i] = neurons[i].Multiply(input);

            return output;
        }
        
        public int GetAnswer(int[,] input)
        {
            int[] output = Count(input);
            int maxIndex = 0;
            for (int i = 1; i < output.Length; i++)
                if (output[i] > output[maxIndex])
                    maxIndex = i;

            return maxIndex;
        }
        
        public void study(int[,] input, int correctAnswer, double speed)
        {
            int[] correctOutput = new int[neurons.Length];
            correctOutput[correctAnswer] = 1;

            int[] output = Count(input);

            while (!CompareArrays(correctOutput, output))
            {
                for (int i = 0; i < neurons.Length; i++)
                {
                    int dif = correctOutput[i] - output[i];
                    neurons[i].ChangeWeights(input, dif, speed);
                }
                output = Count(input);
            }
        }
        
        bool CompareArrays(int[] a, int[] b)
        {
            if (a.Length != b.Length)
                return false;

            for (int i = 0; i < a.Length; i++)
                if (a[i] != b[i])
                    return false;

            return true;
        }
    }
}
