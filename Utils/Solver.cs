namespace Utils;

public interface IMethod
{
    public IEnumerable<double> Solve(SqrMatrix a, IList<double> y);

    public int Number { get; }
}

public class Kramer : IMethod
{
    public int Number => 1;

    public IEnumerable<double> Solve(SqrMatrix a, IList<double> y)
    {
        var det = SqrMatrix.Determinant(a.Data, (int) a.Size);
        double[] output = new double[a.Size],
                 temp;
        
        for (var i = 0; i < a.Size; ++i)
        {
            temp = a.GetColumn(i);
            a.SetColumn(i, y);

            output[i] = SqrMatrix.Determinant(a.Data, (int)a.Size) / det;
            a.SetColumn(i, temp);
        }

        return output;
    }
}

public class Gauss : IMethod
{
    public int Number => 2;
    public IEnumerable<double> Solve(SqrMatrix a, IList<double> y)
    {
        const double eps = 0.00001; // Accuracy
        var x = new double[a.Size];
        var k = 0;
        while (k < a.Size)
        {
            var max = Math.Abs(a[k, k]);
            var index = k;
            for (var i = k + 1; i < a.Size; ++i)
            {
                if (!(Math.Abs(a[i, k]) > max)) continue;
                max = Math.Abs(a[i, k]);
                index = i;
            }

            if (max < eps)
            {
                Console.WriteLine($"Unable to solve cause of null column {index} matrix A");
                return Array.Empty<double>();
            }

            for (var j = 0; j < a.Size; ++j)
            {
                (a[k, j], a[index, j]) = (a[index, j], a[k, j]);
            }

            (y[k], y[index]) = (y[index], y[k]);

            for (var i = k; i < a.Size; ++i)
            {
                var temp = a[i, k];
                if (Math.Abs(temp) < eps) continue;
                for (var j = 0; j < a.Size; ++j)
                {
                    a[i, j] /= temp;
                }

                y[i] /= temp;
                if (i == k) continue;
                for (var j = 0; j < a.Size; ++j)
                {
                    a[i, j] -= a[k, j];
                }

                y[i] -= y[k];
            }

            ++k;
        }

        for (k = (int) (a.Size - 1); k >= 0; --k)
        {
            x[k] = y[k];
            for (var i = 0; i < k; ++i)
            {
                y[i] -= a[i, k] * x[k];
            }
        }

        return x;
    }

}

public class Solver
{
    private IMethod _method;

    public IMethod Method { set => _method = value; }

    public Solver(IMethod method)
    {
        _method = method;
    }

    public IEnumerable<double> Solve(SqrMatrix a, IList<double> y)
    {
        return _method.Solve(a, y);
    }
    
    public static void PrintMtx(SqrMatrix a, IList<double> y)
    {
        for (var i = 0; i < a.Size; ++i)
        {
            for (var j = 0; j < a.Size; ++j)
            {
                Console.Write($"{a[i, j]} * x{j}");
                if (j < a.Size - 1)
                {
                    Console.Write(" + ");
                }
            }
            Console.WriteLine($" = {y[i]}");
        }
    }
}