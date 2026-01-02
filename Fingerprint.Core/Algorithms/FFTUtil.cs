using System;
using System.Numerics; // Требуется для работы с Complex

public static class FFTUtil
{
    public static Complex[] FFT(double[] input)
    {
        int N = input.Length;
        var complexArray = new Complex[N];
        
        for (var i = 0; i < N; i++) 
            complexArray[i] = new Complex(input[i], 0);
        
        return RecursiveFFT(complexArray);
    }
    
    private static Complex[] RecursiveFFT(Complex[] array)
    {
        int N = array.Length;

        if (N <= 1) return array;
        
        var even = new Complex[N / 2];
        var odd = new Complex[N / 2];

        // ИСПРАВЛЕНО: цикл идет до N/2
        for (var i = 0; i < N / 2; i++)
        {
            even[i] = array[i * 2];
            odd[i] = array[i * 2 + 1];
        }
        
        even = RecursiveFFT(even);
        odd = RecursiveFFT(odd);

        var result = new Complex[N];

        for (int k = 0; k < N / 2; k++)
        {
            // Вычисляем угол (формула Эйлера)
            double angle = -2.0 * Math.PI * k / N;
            Complex t = new Complex(Math.Cos(angle), Math.Sin(angle));  
            
            // "Бабочка" FFT
            result[k] = even[k] + t * odd[k]; 
            result[k + N / 2] = even[k] - t * odd[k]; 
        }
        
        return result;
    }
}