using System;

using System.Windows.Forms;
using Tao.OpenGl;
using Tao.FreeGlut;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Numerics;
using System.Diagnostics;
using static SETS2.Form1;
using System.Drawing;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SETS2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            AnT.InitializeContexts();
        }

        private System.Windows.Forms.RichTextBox richTextBoxPoint;
        private System.Windows.Forms.RichTextBox richTextBoxEq;

        List<Point> new_vertex_m = new List<Point>();
        List<Point> vertex_B = new List<Point>();
        List<Point> vertex_A = new List<Point>();
        List<Factor> eq_B = new List<Factor>();
        List<Factor> eq_A = new List<Factor>();
        List<Vector> phi = new List<Vector>();

        string WorkDirEq, WorkDirPoint;
        bool positive_basis = true;
        bool inside = true;
        double alpha;

        private void Form1_Load(object sender, EventArgs e)
        {
            Glut.glutInit();
            Glut.glutInitDisplayMode(Glut.GLUT_RGBA | Glut.GLUT_DOUBLE);
            Gl.glClearColor(1.0f, 1.0f, 1.0f, 1.0f);
            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glLoadIdentity();
            Glu.gluOrtho2D(-15, 15, -15, 15);
            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            Gl.glLoadIdentity();

            richTextBoxEq = richTextBox3;
            richTextBoxPoint = richTextBox4;
            WorkDirEq = "C:\\Users\\kosty\\source\\repos\\SETS2\\Eq1.txt";
            WorkDirPoint = "C:\\Users\\kosty\\source\\repos\\SETS2\\Point1.txt";
        }

        void Intersect(ref List<Factor> eq_B, ref List<Point> vertex_B)
        {
            for (int i = 0; i < eq_B.Count; i++)
            {
                int j = (i + 1) % eq_B.Count;

                Rational det = eq_B[i].A * eq_B[j].B - eq_B[j].A * eq_B[i].B;

                Rational Zero = new Rational(0, 1);

                if (det != Zero)
                {
                    Point point = new Point()
                    {
                        X = -(eq_B[i].B * eq_B[j].C - eq_B[j].B * eq_B[i].C) / det,
                        Y = -(eq_B[j].A * eq_B[i].C - eq_B[i].A * eq_B[j].C) / det
                    };

                    vertex_B.Add(point);
                }
            }

            List<Point> newVertex = new List<Point>();

            for (int i = 0; i < vertex_B.Count; i++)
            {
                Rational x1 = vertex_B[i].X, y1 = vertex_B[i].Y;
                bool isInside = true;

                for (int k = 0; k < eq_B.Count; k++)
                {
                    if (eq_B[k].A * x1 + eq_B[k].B * y1 - eq_B[k].C > 0)
                    {
                        isInside = false;
                        break;
                    }
                }

                if (isInside)
                {
                    newVertex.Add(vertex_B[i]);
                }
            }

            vertex_B = newVertex;
        }

        void Multiplier(ref List<Factor> eq_A, ref List<Point> vertex_A)
        {
            for (int i = 0; i < vertex_A.Count - 1; i++)
            {
                for (int j = i + 1; j < vertex_A.Count; j++)
                {
                    Rational x1 = vertex_A[i].X, x2 = vertex_A[j].X;
                    Rational y1 = vertex_A[i].Y, y2 = vertex_A[j].Y;

                    Factor factor = new Factor()
                    {
                        A = y2 - y1,
                        B = x1 - x2,
                        C = x1 * (y2 - y1) - y1 * (x2 - x1)
                    };

                    bool allInside = true;
                    bool allOutside = true;

                    foreach (Point point in vertex_A)
                    {
                        if (factor.A * point.X + factor.B * point.Y - factor.C > 0)
                            allInside = false;
                        if (factor.A * point.X + factor.B * point.Y - factor.C < 0)
                            allOutside = false;
                    }

                    if (allInside)
                        eq_A.Add(factor);

                    if (allOutside)
                        eq_A.Add(-factor);
                }
            }
        }

        void Examination(ref List<Factor> eq_A, ref List<Point> vertex_B)
        {

            for (int i = 0; i < vertex_B.Count(); i++)
            {
                Rational x = vertex_B[i].X, y = vertex_B[i].Y;

                for (int k = 0; k < eq_A.Count; k++)
                {
                    if (eq_A[k].A * 0 + eq_A[k].B * 0 - eq_A[k].C > 0)
                    {
                        positive_basis = false;
                    }

                    if (eq_A[k].A * x + eq_A[k].B * y - eq_A[k].C > 0)
                    {
                        inside = false;
                        break;
                    }
                }

                if (!inside)
                {
                    break;
                }
            }
        }

        void Phi(ref List<Factor> eq_A, ref List<Vector> phi)
        {
            for (int i = 0; i < eq_A.Count; i++)
            {
                Vector v = new Vector(eq_A[i].A, eq_A[i].B);

                phi.Add(v.Normalize());
            }
        }

        void Minimum(ref List<Point> vertex_A, ref List<Point> vertex_B, ref List<Vector> phi)
        {
            List<double> min = new List<double>();

            for (int i = 0; i < phi.Count; i++)
            {
                double max_A = double.MinValue;
                double max_B = double.MinValue;

                for (int j = 0; j < vertex_A.Count; j++)
                {
                    Vector a = new Vector(vertex_A[j].X, vertex_A[j].Y);
                    max_A = Math.Max(max_A, Vector.DotProduct(a, phi[i]));
                }

                for (int j = 0; j < vertex_B.Count; j++)
                {
                    Vector b = new Vector(vertex_B[j].X, vertex_B[j].Y);
                    max_B = Math.Max(max_B, Vector.DotProduct(b, phi[i]));
                }

                min.Add(max_A - max_B);
            }

            alpha = min.Min();
        }

        void Circle(ref List<Factor> eq_A, ref List<Point> vertex_A)
        {
            bool foundValue = false;
            double x = 0, y = 0;

            for (int i = 0; i < vertex_B.Count; i++)
            {
                double radius = alpha;
                double angle = 2 * Math.PI / 360;
                int v = (i + 1) % vertex_B.Count;
                double[] circleVertices = new double[360 * 2 + 2];

                for (int j = 360; j > 0; j--)
                {
                    circleVertices[j * 2] = vertex_B[i].X + (radius * Math.Cos(j * angle));
                    circleVertices[j * 2 + 1] = vertex_B[i].Y + (radius * Math.Sin(j * angle));
                }

                Gl.glBegin(Gl.GL_LINE_STRIP);
                for (int k = 360; k > 0; k--)
                {
                    if (eq_B[i].A * circleVertices[k * 2] + eq_B[i].B * circleVertices[k * 2 + 1] - eq_B[i].C > 0 &&
                        eq_B[v].A * circleVertices[k * 2] + eq_B[v].B * circleVertices[k * 2 + 1] - eq_B[v].C > 0)
                    {
                        if (!foundValue)
                        { x = circleVertices[k * 2]; y = circleVertices[k * 2 + 1]; }
                        Gl.glVertex2d(circleVertices[k * 2], circleVertices[k * 2 + 1]);
                        foundValue = true;
                    }

                }
            }
            Gl.glVertex2d(x, y);
            Gl.glEnd();
        }

        void Draw(in List<Point> vertex)
        {
            for (int i = 0; i < vertex.Count - 1; i++)
            {
                Gl.glVertex2d(vertex[i].X, vertex[i].Y);
                Gl.glVertex2d(vertex[i + 1].X, vertex[i + 1].Y);
            }

            Gl.glVertex2d(vertex[vertex.Count - 1].X, vertex[vertex.Count - 1].Y);
            Gl.glVertex2d(vertex[0].X, vertex[0].Y);

            Gl.glEnd();

            Gl.glFlush();
        }

        void Calculate(object sender, EventArgs e)
        {
            Intersect(ref eq_B, ref vertex_B);

            Multiplier(ref eq_A, ref vertex_A);

            Examination(ref eq_A, ref vertex_B);

            Phi(ref eq_A, ref phi);

            Minimum(ref vertex_A, ref vertex_B, ref phi);

            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);

            Gl.glBegin(Gl.GL_LINES);
            Gl.glColor3d(0, 255, 255);
            Draw(vertex_A);

            Gl.glBegin(Gl.GL_LINES);
            if (inside) { Gl.glColor3d(0, 255, 0); textBox1.AppendText("Alpha = " + alpha); }
            if (!inside) { Gl.glColor3d(255, 0, 0); textBox1.AppendText("Ошибка, B ∉ A "); }
            if (!positive_basis) { textBox1.AppendText("Ошибка, положительный базис не образуется "); }
            Draw(vertex_B);

            Gl.glColor3d(1, 0, 0);
            Circle(ref eq_B, ref vertex_B);

            Gl.glBegin(Gl.GL_LINES);
            Gl.glColor3d(0, 0, 0);
            Gl.glVertex2d(0, -15);
            Gl.glVertex2d(0, 15);
            Gl.glVertex2d(-15, 0);
            Gl.glVertex2d(15, 0);
            Gl.glVertex2d(0, 0);
            Gl.glEnd();

            AnT.Invalidate();
        }

        void AddEq(object sender, EventArgs e)
        {
            using (StreamReader sr = new StreamReader(WorkDirEq))
            {
                string firstLine = sr.ReadLine();
                string[] parts = firstLine.Split(' ');
                int numRows = int.Parse(parts[0]);

                int aNumerator, bNumerator, cNumerator;
                int aDenominator, bDenominator, cDenominator;

                for (int i = 0; i < numRows; i++)
                {
                    string line = sr.ReadLine();
                    string[] matrixValues = line.Split(' ');

                    if (matrixValues[0].Contains("/"))
                    {
                        string[] aParts = matrixValues[0].Split('/');
                        aNumerator = int.Parse(aParts[0]);
                        aDenominator = int.Parse(aParts[1]);
                    }
                    else
                    {
                        aNumerator = int.Parse(matrixValues[0]);
                        aDenominator = 1;
                    }

                    if (matrixValues[1].Contains("/"))
                    {
                        string[] bParts = matrixValues[1].Split('/');
                        bNumerator = int.Parse(bParts[0]);
                        bDenominator = int.Parse(bParts[1]);
                    }
                    else
                    {
                        bNumerator = int.Parse(matrixValues[1]);
                        bDenominator = 1;
                    }

                    if (matrixValues[2].Contains("/"))
                    {
                        string[] cParts = matrixValues[2].Split('/');
                        cNumerator = int.Parse(cParts[0]);
                        cDenominator = int.Parse(cParts[1]);
                    }
                    else
                    {
                        cNumerator = int.Parse(matrixValues[2]);
                        cDenominator = 1;
                    }

                    Factor factor = new Factor()
                    {
                        A = new Rational(aNumerator, aDenominator),
                        B = new Rational(bNumerator, bDenominator),
                        C = new Rational(cNumerator, cDenominator)
                    };

                    eq_B.Add(factor);

                    if (factor.B < 0)
                    {
                        richTextBoxEq.AppendText(factor.A + "x" + " + (" + factor.B + ")y" + " <= " + factor.C + "\n");
                    }
                    else
                    {
                        richTextBoxEq.AppendText(factor.A + "x" + " + " + factor.B + "y" + " <= " + factor.C + "\n");
                    }
                }
            }
        }

        void AddPoint(object sender, EventArgs e)
        {
            using (StreamReader sr = new StreamReader(WorkDirPoint))
            {
                string firstLine = sr.ReadLine();
                string[] parts = firstLine.Split(' ');
                int numRows = int.Parse(parts[0]);

                int xNumerator, yNumerator;
                int xDenominator, yDenominator;

                for (int i = 0; i < numRows; i++)
                {
                    string line = sr.ReadLine();
                    string[] matrixValues = line.Split(' ');

                    if (matrixValues[0].Contains("/"))
                    {
                        string[] xParts = matrixValues[0].Split('/');
                        xNumerator = int.Parse(xParts[0]);
                        xDenominator = int.Parse(xParts[1]);
                    }
                    else
                    {
                        xNumerator = int.Parse(matrixValues[0]);
                        xDenominator = 1;
                    }

                    if (matrixValues[1].Contains("/"))
                    {
                        string[] yParts = matrixValues[1].Split('/');
                        yNumerator = int.Parse(yParts[0]);
                        yDenominator = int.Parse(yParts[1]);
                    }
                    else
                    {
                        yNumerator = int.Parse(matrixValues[1]);
                        yDenominator = 1;
                    }

                    Point point = new Point()
                    {
                        X = new Rational(xNumerator, xDenominator),
                        Y = new Rational(yNumerator, yDenominator),
                    };

                    vertex_A.Add(point);

                    richTextBoxPoint.AppendText("(" + point.X + "; " + point.Y + ")" + "\n");
                }
            }
        }

        void ClearEq(object sender, EventArgs e)
        {
            eq_B.Clear();
            vertex_A.Clear();

            richTextBoxEq.Clear();
        }

        void ClearPoint(object sender, EventArgs e)
        {
            eq_A.Clear();
            vertex_B.Clear();

            richTextBoxPoint.Clear();
        }
    }


    public class Rational
    {
        private BigInteger numerator;
        private BigInteger denominator;

        public Rational(BigInteger n)
        {
            numerator = n;
            denominator = 1;
        }

        public Rational(BigInteger n, BigInteger d)
        {
            if (d == BigInteger.Zero)
            {
                throw new DivideByZeroException("Denominator cannot be zero");
            }

            if (d < BigInteger.Zero)
            {
                n = -n;
                d = -d;
            }

            BigInteger gcd = BigInteger.GreatestCommonDivisor(BigInteger.Abs(n), BigInteger.Abs(d));
            numerator = n / gcd;
            denominator = d / gcd;
        }

        public Rational(int n, int d = 1) : this(new BigInteger(n), new BigInteger(d))
        {
        }

        public BigInteger Numerator
        {
            get { return numerator; }
        }

        public BigInteger Denominator
        {
            get { return denominator; }
        }

        public static Rational Abs(Rational r)
        {
            BigInteger n = BigInteger.Abs(r.Numerator);
            BigInteger d = BigInteger.Abs(r.Denominator);
            return new Rational(n, d);
        }

        public override string ToString()
        {
            if (denominator == 1)
            {
                return numerator.ToString();
            }
            else
            {
                return numerator.ToString() + "/" + denominator.ToString();
            }
        }

        public static Rational operator +(Rational r1, Rational r2)
        {
            BigInteger n = r1.numerator * r2.denominator + r2.numerator * r1.denominator;
            BigInteger d = r1.denominator * r2.denominator;
            return new Rational(n, d);
        }

        public static Rational operator -(Rational r1, Rational r2)
        {
            BigInteger n = r1.numerator * r2.denominator - r2.numerator * r1.denominator;
            BigInteger d = r1.denominator * r2.denominator;
            return new Rational(n, d);
        }

        public static Rational operator *(Rational r1, Rational r2)
        {
            BigInteger n = r1.numerator * r2.numerator;
            BigInteger d = r1.denominator * r2.denominator;
            return new Rational(n, d);
        }

        public static Rational operator /(Rational r1, Rational r2)
        {
            if (r2.denominator == BigInteger.Zero)
            {
                throw new DivideByZeroException("Denominator cannot be zero");
            }

            BigInteger n = r1.numerator * r2.denominator;
            BigInteger d = r1.denominator * r2.numerator;
            return new Rational(n, d);
        }

        public static Rational operator *(Rational a, int b)
        {
            return new Rational(a.numerator * b, a.denominator);
        }

        public static bool operator >(Rational left, Rational right)
        {
            BigInteger leftNumerator = left.Numerator * right.Denominator;
            BigInteger rightNumerator = right.Numerator * left.Denominator;

            return leftNumerator > rightNumerator;
        }

        public static bool operator <(Rational left, Rational right)
        {
            BigInteger leftNumerator = left.Numerator * right.Denominator;
            BigInteger rightNumerator = right.Numerator * left.Denominator;

            return leftNumerator < rightNumerator;
        }

        public static Rational operator -(Rational r)
        {
            return new Rational(-r.Numerator, r.Denominator);
        }

        public static explicit operator int(Rational r1)
        {
            return (int)(r1.numerator / r1.denominator);
        }

        public static implicit operator double(Rational r1)
        {
            return (double)r1.numerator / (double)r1.denominator;
        }

        public static implicit operator Rational(int r1)
        {
            return new Rational(r1, 1);
        }
    }

    public class Vector
    {
        public double X { get; set; }
        public double Y { get; set; }

        public Vector(double x, double y)
        {
            X = x;
            Y = y;
        }

        public static Vector operator *(Vector v, double scalar)
        {
            return new Vector(v.X * scalar, v.Y * scalar);
        }
        public static Vector operator -(Vector v1, Vector v2)
        {
            double x = v1.X - v2.X;
            double y = v1.Y - v2.Y;

            return new Vector(x, y);
        }

        public double Length() => Math.Sqrt(X * X + Y * Y);
        public Vector Normalize()
        {
            double length = Length();
            return new Vector(X / length, Y / length);
        }

        public static double DotProduct(Vector v1, Vector v2)
        {
            return v1.X * v2.X + v1.Y * v2.Y;
        }
    }

    public struct Point
    {
        public Rational X { get; set; }
        public Rational Y { get; set; }
    }

    public struct Factor
    {
        public Rational A { get; set; }
        public Rational B { get; set; }
        public Rational C { get; set; }

        public static Factor operator -(Factor f) => new Factor()
        { A = -f.A, B = -f.B, C = -f.C };
    }
}