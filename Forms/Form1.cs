using System.Globalization;
using System.Linq.Expressions;
using System.Numerics;
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
        private string[] _solution;

        private readonly Regex _rg = new(@"(\+|-)?\d+x\d+"),
            _rgSingle = new (
                @"(?'tetra'(\+|-)?\d*x\^4)|(?'cube'(\+|-)?\d*x\^3)|(?'quad'(\+|-)?\d*x\^2)|(?'normal'(\+|-)?\d+x)|(?'single'(\+|-)?\d*)"),
            _rgReplace = new (@"x(\^\d*)?");
        public Form1()
        {
            InitializeComponent();
            _solver = new Solver(new Kramer(), new Square());
            radioButton1.Checked = true;
            flowLayoutPanel1.Enabled = false;
            flowLayoutPanel1.Visible = false;
            flowLayoutPanel1.Controls.Add(new TextBox());
            singleTextBox.Enabled = true;
            singleTextBox.Visible = true;
            singleTextBox.PlaceholderText = "1x^2 + x + 1 = 0";
            flowLayoutPanel1.Controls[^1].Width = 256;
            (flowLayoutPanel1.Controls[^1] as TextBox)!.PlaceholderText = "x1 + x2 + ... = 1";
            _cached = false;
            radioButton1.Enabled = false;
            radioButton2.Enabled = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            flowLayoutPanel1.Controls.Add(new TextBox());
            flowLayoutPanel1.Controls[^1].Width = 256;
            (flowLayoutPanel1.Controls[^1] as TextBox)!.PlaceholderText = "x1 + x2 + ... = 1";
            _cached = false;
            if (flowLayoutPanel1.Controls.Count < 2) return;
            flowLayoutPanel1.Enabled = true;
            flowLayoutPanel1.Visible = true;
            singleTextBox.Visible = false;
            singleTextBox.Enabled = false;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (flowLayoutPanel1.Controls.Count >= 2)
            {
                flowLayoutPanel1.Controls.RemoveAt(flowLayoutPanel1.Controls.Count - 1);
                _cached = false;
            }

            if (flowLayoutPanel1.Controls.Count != 1) return;

            flowLayoutPanel1.Enabled = false;
            flowLayoutPanel1.Visible = false;
            singleTextBox.Enabled = true;
            singleTextBox.Visible = true;
            singleTextBox.Clear();
            singleTextBox.PlaceholderText = "1x^2 + x + 1 = 0";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            while (!_cached)
            {
                if (singleTextBox.Enabled)
                {
                    try
                    {
                        var eq = ParseSingle();
                        _solution = _solver.Solve(eq).Select(x => x.Imaginary != 0 ? $"{x.Real}+i*({x.Imaginary})" : x.Real.ToString()).ToArray();
                        break;
                    }
                    catch (InvalidDataException)
                    {
                        return;
                    }
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
                _solution = _solver.Solve(_sqrMatrix, _y).Select(x => x.ToString(CultureInfo.CurrentCulture)).ToArray();
                _cached = true;
            }

            FillSolution();
        }

        private void FillSolution()
        {
            flowLayoutPanel2.Controls.Clear();
            if (_solution.All(x => x == _solution[0]))
            {
                CreateTextNode($"y = {_solution[0]}");
                return;
            }
            for (var i = 0; i < _solution.Length; ++i)
            {
                CreateTextNode($"y[{i}] = {_solution[i]}");
            }
        }

        private void CreateTextNode(string msg)
        {
            flowLayoutPanel2.Controls.Add(new TextBox());
            flowLayoutPanel2.Controls[^1].Text = msg;
            (flowLayoutPanel2.Controls[^1] as TextBox).ReadOnly = true;
            flowLayoutPanel2.Controls[^1].Width = 512;
        }

        private List<double> ParseSingle()
        {
            var output = new List<double>();
            var eq = singleTextBox.Text.Replace(" ", "").Split('=');
            if (eq.Length != 2)
            {
                MessageBox.Show($"Неправильно введено уравнение. Проверьте знак '='");
                throw new InvalidDataException();

            }

            var nums = _rgSingle.Matches(eq[0]);
            var groups = new Dictionary<string, double>
            {
                {"tetra", 0}, {"cube", 0}, {"quad", 0}, {"normal", 0}, {"single", 0}
            };

            foreach (Match m in nums)
            {
                foreach (Group op in m.Groups)
                {
                    if (!op.Success || !groups.ContainsKey(op.Name)) continue;


                    foreach (Capture c in op.Captures)
                    {
                        var val = _rgReplace.Replace(c.Value, "");
                        if (val.Length == 0 || !val.All(x => char.IsDigit(x) || x is '+' or '-')) continue;
                        if (double.TryParse(val, out var r))
                        {
                            groups[op.Name] += r;
                        }
                        else
                        {
                            MessageBox.Show($"Ошибка чтения операнда: {c.Value}");
                        }
                    }
                }
            }

            output.Add(groups["tetra"]);
            // MessageBox.Show($"Parsed cube: {groups["cube"]}");
            output.Add(groups["cube"]);
            // MessageBox.Show($"Parsed quad: {groups["quad"]}");
            output.Add(groups["quad"]);

            // MessageBox.Show($"Parsed normal: {groups["normal"]}");
            output.Add(groups["normal"]);
            // MessageBox.Show($"Parsed single: {groups["single"]}");
            output.Add(groups["single"]);
            return output;
        }

        private double[] ParseEq(int i)
        {
            var output = new double[flowLayoutPanel1.Controls.Count + 1];
            var eq = flowLayoutPanel1.Controls[i].Text.Split("=");
            if (eq.Length != 2)
            {
                MessageBox.Show($"Неправильно введено уравнение {i}. Проверьте знак '='");
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