using Utils;

Console.WriteLine("Enter eq num: ");
var n = uint.Parse(Console.ReadLine() ?? string.Empty);

var a = new SqrMatrix(n);
var solver = new Solver(new Gauss());
double[] y = new double[n], x;

Console.WriteLine("Enter quotients (one per line):");
for (var i = 0; i < a.Size; i++) 
{
    for (var j = 0; j < a.Size; j++)
    {
        Console.Write($"a[{i}, {j}] = ");
        a[i, j] = int.Parse(Console.ReadLine() ?? string.Empty);
    }
}
for (var i = 0; i < a.Size; i++) 
{
    Console.Write($"y[{i}] = ");
    y[i] = int.Parse(Console.ReadLine() ?? string.Empty);
}

Solver.PrintMtx(a, y);

Console.WriteLine("Choose solution method:\n\t1 - Gauss\n\t2 - Kramer");
n = uint.Parse(Console.ReadLine() ?? string.Empty);

while (n != 1 && n != 2)
{
    Console.WriteLine($"You entered {n} Enter correct value\n");
    n = uint.Parse(Console.ReadLine() ?? string.Empty); 
}

if (n == 1)
{
    solver.Method = new Gauss();
}
else
{
    solver.Method = new Kramer();
}
x = solver.Solve(a, y).ToArray();

for (var i = 0; i < x.Length; ++i)
{
    Console.WriteLine($"x[{i}] = {x[i]:f4}");
}