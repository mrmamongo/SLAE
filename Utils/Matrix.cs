namespace Utils;

public class SqrMatrix
{
    protected SqrMatrix(){}

    public SqrMatrix(int size)
    {
        Size = size;
        _data = new double[Size, Size];
    }
    protected double CachedDet = double.NaN;
    public int Size { get; }

    protected double[,] _data;
    
    public ref double[,] Data => ref _data;

    public double this[int x, int y]
    {
        get => _data[x, y];
        set
        {
            _data[x, y] = value;
            CachedDet = double.NaN;
        }
    }
    
    private static void GetMatrixWithoutRowAndCol(double[,] matrix, int size, int row, int col, ref double[,] newMatrix) {
        int offsetRow = 0;
        for(var i = 0; i < size-1; i++) {

            if(i == row) {
                offsetRow = 1;
            }

            var offsetCol = 0;
            for(var j = 0; j < size-1; j++) {

                if(j == col)
                {
                    offsetCol = 1;
                }

                newMatrix[i, j] = matrix[i + offsetRow, j + offsetCol];
            }
        }
    }
    public static double Determinant(double[, ] matrix, int size) {
        double det = 0;
        var degree = 1;
        switch (size)
        {
            case 1:
                return matrix[0, 0];
            case 2:
                return matrix[0, 0] * matrix[1, 1] - matrix[0, 1] * matrix[1, 0];
            default:
            {
                var newMatrix = new double[size - 1, size - 1];
                for(var j = 0; j < size; j++) {
                    GetMatrixWithoutRowAndCol(matrix, size, 0, j, ref newMatrix);
                    det += (degree * matrix[0, j] * Determinant(newMatrix, size-1));
                    degree = -degree;
                }
                break;
            }
        }
        return det;
    }
    
    public double[] GetColumn(int col)
    {
        return Enumerable.Range(0, _data.GetLength(0)).Select(x => _data[x, col]).ToArray();
    }

    public void SetColumn(int c, IList<double> col)
    {
        for (var i = 0; i < Size; ++i)
        {
            _data[i, c] = col[i];
        }
    }

    public void SetRow(int r, IList<double> row)
    {
        for (var i = 0; i < Size; ++i)
        {
            _data[r, i] = row[i];
        }
    }
}