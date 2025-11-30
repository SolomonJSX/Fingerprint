using System.Numerics;

namespace Fingerprint.Alg.Algorithms;

public static class FFTUtil
{
    public static Complex[] FFT(double[] input)
    {
        var complexArray = new Complex[input.Length];
        
        for (var i =  0; i < input.Length; i++) 
            complexArray[i] = new Complex(input[i], 0);
        
        Complex[] ffrResult = new Complex[complexArray.Length];
        Array.Copy(complexArray, ffrResult, complexArray.Length);
        return RecursiveFFT(ffrResult);
    }
    
    private static Complex[] RecursiveFFT(Complex[] array)
    {
        int N = array.Length;

        if (array.Length <= 1) return array;
        
        var even = new Complex[N / 2];
        var odd = new Complex[N / 2];

        for (var i = 0; i < N; i++)
        {
            even[i] = array[i * 2];
            odd[i] = array[i * 2 + 1];
        }
        
        even = RecursiveFFT(even);
        odd = RecursiveFFT(odd);

        var result = new Complex[N];

        for (int k = 0; k < N / 2; k++)
        {
            double angle = -2.0 * Math.PI * k / N;
            Complex t = new Complex(Math.Cos(angle), Math.Sin(angle));  
            
            result[k] = even[k] + t * odd[k]; //Верхняя половина
            result[k + N / 2] = even[k] - t * odd[k]; //Нижняя половина
        }
        
        return result;
    }
}