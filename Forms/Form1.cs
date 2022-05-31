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

        private Regex _rg = new(@"(\+|-)?\d+x\d+"),
            _rgSingle = new(@"(\+|-)?(\d+x^\d+)|\d+");

        public Form1()
        {
            InitializeComponent();
            _solver = new Solver(new Kramer());
            radioButton1.Checked = true;
            flowLayoutPanel1.Controls.Add(new TextBox());
            flowLayoutPanel1.Controls[^1].Width = 256;
            _cached = false;
            radioButton1.Enabled = false;
            radioButton2.Enabled = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            flowLayoutPanel1.Controls.Add(new TextBox());
            flowLayoutPanel1.Controls[^1].Width = 256;
            _cached = false;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (flowLayoutPanel1.Controls.Count <= 1) return;
            flowLayoutPanel1.Controls.RemoveAt(flowLayoutPanel1.Controls.Count - 1);
            _cached = false;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            while (!_cached)
            {
                if (flowLayoutPanel1.Controls.Count == 1)
                {
                    var eq = ParseSingle();
                    _solution = eq;
                    break;
                }

                _sqrMatrix = new SqrMatrix(flowLayoutPanel1.Controls.Count);
                _y = new double[flowLayoutPanel1.Controls.Count];
                for (var i = 0; i < flowLayoutPanel1.Controls.Count; ++i)
                {
                    try
                    {
                        var eq = ParseEq(i);
                        _sqrMatrix.SetRow(i, eq[..(_sqrMatrix.Size)]);
                        _y[i] = eq[^1];
                    }
                    catch (InvalidDataException)
                    {
                        return;
                    }
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
                flowLayoutPanel2.Controls[^1].IsAccessible = false;
                flowLayoutPanel1.Controls[^1].Width = 128;
            }
        }

        private double[] ParseSingle()
        {
            var output = new List<double>();
            var eq = flowLayoutPanel1.Controls[0].Text.Split('=');
            if (eq.Length != 2)
            {
                MessageBox.Show($"Неправильно введено уравнение");
                throw new InvalidDataException();

            }

            var operands = _rg.Matches(eq[0]);
            foreach (var op in operands)
            {
                var nums = op.ToString()!.Split('x');
                if (nums.Length != 2)
                {
                    MessageBox.Show($"Неправильно введено уравнение");
                    throw new InvalidDataException();
                }

                if (int.TryParse(nums[1], out var n))
                {
                    if (double.TryParse(nums[0], out var v))
                    {
                        output.Add(v);
                    }
                    else
                    {
                        MessageBox.Show($"Неправильно введено уравнение");
                        throw new InvalidDataException();
                    }
                }
                else
                {
                    MessageBox.Show($"Неправильно введено уравнение");
                    throw new InvalidDataException();
                }
            }


            if (double.TryParse(eq[1], out var res))
            {
                output.Add(res);
            }
            else
            {
                MessageBox.Show($"Неправильно введено уравнение");
                throw new InvalidDataException();
            }

            return output.ToArray();
        }

        private double[] ParseEq(int i)
        {
            var output = new double[flowLayoutPanel1.Controls.Count + 1];
            var eq = flowLayoutPanel1.Controls[i].Text.Split("=");
            if (eq.Length != 2)
            {
                MessageBox.Show($"Неправильно введено уравнение {i}");
                throw new InvalidDataException();
            }
            var operands = _rg.Matches(eq[0]);
            for (var j = 0; j < operands.Count; ++j)
            {
                var nums = operands[j].ToString().Split('x');
                if (nums.Length != 2)
                {
                    MessageBox.Show($"Неправильно введено уравнение {i}. Проверьте операнд {j}");
                    throw new InvalidDataException();
                }

                if (int.TryParse(nums[1], out var n))
                {
                    if (double.TryParse(nums[0], out var v))
                    {
                        output[n - 1] = v;
                    }
                    else
                    {
                        MessageBox.Show($"Неправильно введено уравнение {i}. Проверьте операнд {j}");
                        throw new InvalidDataException();
                    }
                }
                else
                {
                    MessageBox.Show($"Неправильно введено уравнение {i}. Проверьте операнд {j}");
                    throw new InvalidDataException();
                }
            }


            if (double.TryParse(eq[1], out var res))
            {
                output[^1] = res;
            }
            else
            {
                MessageBox.Show($"Неправильно введено уравнение {i}");
                throw new InvalidDataException();
            }

            return output;
        }

        private void radioButton1_Click(object sender, EventArgs e)
        {
            if (radioButton1.Checked) _solver.Method = new Kramer();
            else _solver.Method = new Gauss();

            _cached = false;
        }

        private void radioButton2_Click(object sender, EventArgs e)
        {
            if (radioButton1.Checked) _solver.Method = new Gauss();
            else _solver.Method = new Kramer();

            _cached = false;
        }   

        private void flowLayoutPanel1_ControlRemoved(object sender, ControlEventArgs e)
        {
            if (flowLayoutPanel1.Controls.Count <= 1)
            {
                radioButton1.Enabled = false;
                radioButton2.Enabled = false;
            }
            else
            {
                radioButton1.Enabled = true;
                radioButton2.Enabled = true;
            }
        }

        private void flowLayoutPanel1_ControlAdded(object sender, ControlEventArgs e)
        {
            if (flowLayoutPanel1.Controls.Count <= 1)
            {
                radioButton1.Enabled = false;
                radioButton2.Enabled = false;
            }
            else
            {
                radioButton1.Enabled = true;
                radioButton2.Enabled = true;
            }
        }
        
    }
}