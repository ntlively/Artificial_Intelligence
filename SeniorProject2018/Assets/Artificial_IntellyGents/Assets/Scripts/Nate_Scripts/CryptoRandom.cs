using System;
using System.Security.Cryptography;
using UnityEngine;
namespace NeuralNet.NeuralNet
{
    public class CryptoRandom
    {
        public double RandomValue { get; set; }

        public CryptoRandom()
        {
                try
                {
                    RNGCryptoServiceProvider p = new RNGCryptoServiceProvider();
                    System.Random r = new System.Random(p.GetHashCode());
                    this.RandomValue = r.NextDouble();
                    
                }
                finally
                {
                    //Debug.Log("RNGCryptoServiceProvider in CryptoRandom.cs has failed.  Most likely because it is not System.IDisposable");
                }
        }

    }
}
