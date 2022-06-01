using System.Numerics;

namespace Utils;

public interface IMethodSingle
{
    public IEnumerable<Complex> Solve(IList<double> y);
}
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
        var det = a.Determinant();
        double[] output = new double[a.Size],
                 temp;
        
        for (var i = 0; i < a.Size; ++i)
        {
            temp = a.GetColumn(i);
            a.SetColumn(i, y);

            output[i] = a.Determinant() / det;
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

public class Poly : IMethodSingle
{
    public IEnumerable<Complex> Solve(IList<double> y)
    {
        return Array.Empty<Complex>();
    }
}

public class Square : IMethodSingle
{
    public Square(bool bi = false)
    {
        _isBi = bi;
    }

    private bool _isBi;

    public IEnumerable<Complex> Solve(IList<double> y)
    {
        if (_isBi)
        {
            _isBi = false;

            y[3] = y[2];
            y[2] = y[0];
            var sol = Solve(y).ToList();

            return new List<Complex>
            {
                Complex.Sqrt(sol[0]),
                -Complex.Sqrt(sol[0]),
                Complex.Sqrt(sol[1]),
                -Complex.Sqrt(sol[1])
            };
        }

        var det = y[3] * y[3] - 4 * y[2] * y[4];
        var absRoot = Math.Sqrt(Math.Abs(det));
        var root = det < 0 ? new Complex(0, absRoot) : new Complex(absRoot, 0);
        var q = -0.5 * (y[3] + Math.Sign(y[3]) * root);
        return new[]{q / y[2], y[4] / q};
    }
}

public class Cubic : IMethodSingle
{
    private static double XRoot(double a, double x)
    {
        double i = 1;
        if (a < 0)
            i = -1;
        return (i * Math.Exp(Math.Log(a * i) / x));
    }

    public IEnumerable<Complex> Solve(IList<double> y)
    {
        var output = new List<Complex>();
        double a1 = y[1], b = y[2], c = y[3], d = y[4];

        while (a1 != 0)
        {
            var a = b / a1;
            b = c / a1;
            c = d / a1;

            var p = -(a * a / 3.0) + b;
            var q = (2.0 / 27.0 * a * a * a) - (a * b / 3.0) + c;
            d = q * q / 4.0 + p * p * p / 27.0;
            if (Math.Abs(d) < Math.Pow(10.0, -11.0))
                d = 0;
            double v;
            double u;
            if (d > 1e-20)
            {
                u = XRoot(-q / 2.0 + Math.Sqrt(d), 3.0);
                v = XRoot(-q / 2.0 - Math.Sqrt(d), 3.0);
                output.Add(new Complex(u + v - a / 3.0, 0));
                output.Add(new Complex(-(u + v) / 2.0 - a / 3.0, Math.Sqrt(3.0) / 2.0 * (u - v)));
                output.Add(new Complex(output[1].Real, output[1].Imaginary));
                break;
            }

            if (Math.Abs(d) <= 1e-20)
            {
                u = XRoot(-q / 2.0, 3.0);
                v = XRoot(-q / 2.0, 3.0);
                output.Add(new Complex(u + v - a / 3.0, 0));
                output.Add(new Complex(-(u + v) / 2.0 - a / 3.0, 0));
                break;
            }

            if (d < -1e-20)
            {
                var r = Math.Sqrt(-p * p * p / 27.0);
                var alpha = Math.Atan(Math.Sqrt(-d) / q * 2.0);
                if (q > 0) // if q > 0 the angle becomes  PI + alpha
                    alpha = Math.PI + alpha;

                output.Add(new Complex(XRoot(r, 3.0) * (Math.Cos((6.0 * Math.PI - alpha) / 3.0) + Math.Cos(alpha / 3.0)) - a / 3.0, 0));
                output.Add(new Complex(XRoot(r, 3.0) *
                    (Math.Cos((2.0 * Math.PI + alpha) / 3.0) + Math.Cos((4.0 * Math.PI - alpha) / 3.0)) - a / 3.0, 0));
                output.Add(new Complex(XRoot(r, 3.0) *
                    (Math.Cos((4.0 * Math.PI + alpha) / 3.0) + Math.Cos((2.0 * Math.PI - alpha) / 3.0)) - a / 3.0, 0));
                break;
            }
        }

        return output;
    }
}

public class Solver
{
    private IMethod _method;
    private IMethodSingle _methodSingle;

    public IMethod Method { set => _method = value; }
    public IMethodSingle MethodSingle { set => _methodSingle = value; }

    public Solver(IMethod method, IMethodSingle methodSingle)
    {
        _method = method;
        _methodSingle = methodSingle;
    }

    public IEnumerable<double> Solve(SqrMatrix a, IList<double> y)
    {
        return _method.Solve(a, y);
    }

    public IEnumerable<Complex> Solve(IList<double> a)
    {
        if (a.Count != 5) return Array.Empty<Complex>();
        if (a[0] == 0)
        {
            if (a[1] == 0)
            {
                if (a[2] == 0)
                {
                    if (a[3] == 0)
                    {
                        return new[] { new Complex(a[4], 0) };
                    }

                    _methodSingle = new Poly();
                }
                else
                {
                    _methodSingle = new Square(false);
                }
            }
            else
            {
                _methodSingle = new Cubic();
            }
        }
        else
        {
            _methodSingle = new Square(true);
        }

        return _methodSingle.Solve(a);

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