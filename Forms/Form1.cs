using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Utils;

namespace Forms
{
    public partial class Form1 : Form
    {
        private readonly Solver _solver;

        private bool _cached;
        private SqrMatrix _sqrMatrix;
        private double[] _y;
        private double[] _solution;

        private Regex _rg = new(@"(\+|\-)?\dx");

        public Form1()
        {
            InitializeComponent();
            _solver = new Solver(new Kramer());
            radioButton1.Checked = true;
            flowLayoutPanel1.Controls.Add(new TextBox());
            _cached = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            flowLayoutPanel1.Controls.Add(new TextBox());
            flowLayoutPanel1.Controls[^1].AutoSize = true;
            _cached = false;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (flowLayoutPanel1.Controls.Count == 0) return;
            flowLayoutPanel1.Controls.RemoveAt(flowLayoutPanel1.Controls.Count - 1);
            _cached = false;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (!_cached)
            {
                _sqrMatrix = new SqrMatrix(flowLayoutPanel1.Controls.Count);
                _y = new double[flowLayoutPanel1.Controls.Count];
                for (var i = 0; i < flowLayoutPanel1.Controls.Count; ++i)
                {
                    var eq = ParseEq(i);
                    _sqrMatrix.SetRow(i, eq[..(_sqrMatrix.Size)]);
                    _y[i] = eq[^1];
                }
                _solution = _solver.Solve(_sqrMatrix, _y).ToArray();
                _cached = true;
            }

            FillSolution();
        }

        private void FillSolution()
        {
            flowLayoutPanel2.Controls.Clear();
            for (var i = 0; i < _solution.Length; ++i)
            {
                flowLayoutPanel2.Controls.Add(new TextBox());
                flowLayoutPanel2.Controls[^1].Text = $"y[{i}] = {_solution[i]}";
                flowLayoutPanel2.Controls[^1].Enabled = false;
                flowLayoutPanel2.Controls[^1].AutoSize = true;
            }
        }

        private double[] ParseEq(int i) // x1, x2, x3, ... xn, y
        {
            var output = new double[_sqrMatrix.Size + 1];
            var eq = flowLayoutPanel1.Controls[i].Text.Split('=');
            var splitted = _rg.Matches(eq[0]);
            for (var j = 0; j < splitted.Count; ++j)
            {
                if (double.TryParse(splitted[j].Value[..(splitted[j].Length - 1)], out var r))
                {
                    output[j] = r;
                }   
            }

            if (double.TryParse(eq[1], out var res))
            {
                output[^1] = res;
            }

            return output;
        }

        private void radioButton1_Click(object sender, EventArgs e)
        {
            if (radioButton1.Checked) _solver.Method = new Kramer();
            else _solver.Method = new Gauss();
        }

        private void radioButton2_Click(object sender, EventArgs e)
        {
            if (radioButton1.Checked) _solver.Method = new Gauss();
            else _solver.Method = new Kramer();
        }
    }
}