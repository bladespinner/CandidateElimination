using System;
using System.Collections.Generic;
using System.Linq;

namespace CandidateElimination
{
    public class Example
    {
        public Example(float x, float y, bool goal)
        {
            this.x = x;
            this.y = y;
            this.goal = goal;
        }

        private float x, y;
        private bool goal;

        public float X
        {
            get
            {
                return x;
            }
        }
        public float Y
        {
            get
            {
                return y;
            }
        }

        public bool Goal
        {
            get
            {
                return goal;
            }
        }

        public override string ToString()
        {
            return string.Format("<{0},{1}> = {2}", X, Y, Goal);
        }
    }

    public class Hypothesis
    {
        public Hypothesis(float left, float right, float bottom, float top)
        {
            a = left;
            b = right;
            c = bottom;
            d = top; 
        }
        float a, b, c, d;

        public float Left
        {
            get
            {
                return a;
            }
        }

        public float Right
        {
            get
            {
                return b;
            }
        }

        public float Bottom
        {
            get
            {
                return c;
            }
        }

        public float Top
        {
            get
            {
                return d;
            }
        }
        
        public bool IsConsistent(Example example)
        {
            bool inArea = Left <= example.X && example.X <= Right &&
                          Bottom <= example.X && example.Y <= Top;
            //inArea = !IsEmpty();
            return example.Goal ? inArea : !inArea;
        }

        public bool IsEmpty()
        {
            return (float.IsNaN(Left) && float.IsNaN(Right)) ||
                   (float.IsNaN(Top) && float.IsNaN(Bottom));
        }

        public bool Contains(Hypothesis h)
        {
            if (IsEmpty())
            {
                return false;
            }
            return h.IsEmpty() ||
                   (
                       (h.Left > Left || (float.IsInfinity(h.Left) && float.IsInfinity(Left))) && 
                       (h.Right < Right || (float.IsInfinity(h.Right) && float.IsInfinity(Right))) &&
                       (h.Bottom > Bottom || (float.IsInfinity(h.Bottom) && float.IsInfinity(Bottom))) && 
                       (h.Top < Top || (float.IsInfinity(h.Top) && float.IsInfinity(Top)))
                   );
        }

        public static Hypothesis operator + (Hypothesis h, Example e)
        {
            if(h.IsEmpty())
            {
                return new Hypothesis(e.X, e.X, e.Y, e.Y);
            }

            return new Hypothesis(Math.Min(h.Left, e.X),
                Math.Max(h.Right, e.X),
                Math.Min(h.Bottom, e.Y),
                Math.Max(h.Top, e.Y));
        }

        public override string ToString()
        {
            return string.Format("([{0},{1}],[{2},{3}])", Left, Right, Bottom, Top);
        }
    }
    class Program
    {
        public static List<Example> GetExamples()
        {
            return new List<Example>()
            {
                new Example(2, 6, false),
                new Example(1, 3, false),
                new Example(4, 4, true),
                new Example(5, 1, false),
                new Example(5, 3, true),
                new Example(5, 8, false),
                new Example(6, 5, true),
                new Example(9, 4, false)
            };
        }

        public static IEnumerable<Hypothesis> SmallestGeneralizers(IEnumerable<Hypothesis> generalBorder,
                                                                   Example example,
                                                                   Hypothesis h)
        {
            var generalized = h + example;
            yield return generalized;
        }

        public static IEnumerable<Hypothesis> SmallestSpecifiers(IEnumerable<Hypothesis> specificBorder,
                                                                 Example example,
                                                                 Hypothesis h)
        {
            if (h.Left <= example.X - 1) yield return new Hypothesis(h.Left, example.X - 1, h.Bottom, h.Top);
            if (example.X + 1 <= h.Right) yield return new Hypothesis(example.X + 1, h.Right, h.Bottom, h.Top);
            if (example.Y + 1 <= h.Top) yield return new Hypothesis(h.Left, h.Right, example.Y + 1, h.Top);
            if (h.Bottom <= example.Y - 1) yield return new Hypothesis(h.Left, h.Right, h.Bottom, example.Y - 1);
        }
        static void Main(string[] args)
        {
            List<Example> examples = GetExamples();
            List<Hypothesis> generalBorder = new List<Hypothesis>()
            {
                new Hypothesis(float.NegativeInfinity, float.PositiveInfinity,
                               float.NegativeInfinity, float.PositiveInfinity)
            };

            List<Hypothesis> specificBorder = new List<Hypothesis>()
            {
                new Hypothesis(float.NaN, float.NaN,
                               float.NaN, float.NaN)
            };
            int i = 0;
            foreach (var example in examples)
            {
                Console.WriteLine("G" + i);
                foreach(var h in generalBorder)
                {
                    Console.WriteLine(h);
                }
                Console.WriteLine("S" + i);
                foreach (var h in specificBorder)
                {
                    Console.WriteLine(h);
                }
                i++;
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("d" + i);
                Console.WriteLine(example);
                if (example.Goal)
                {
                    generalBorder = generalBorder.Where(h => h.IsConsistent(example)).ToList();

                    var newSpecifiers = specificBorder
                        .Select(h => h.IsConsistent(example) ? new List<Hypothesis>() { h } : SmallestGeneralizers(generalBorder, example, h))
                        .Aggregate(new List<Hypothesis>().AsEnumerable(), (a, b) => a.Concat(b))
                        .Where(h => generalBorder.Any(a => a.Contains(h)));
                    specificBorder = newSpecifiers.Where(a => !newSpecifiers.Any(s => a.Contains(s))).ToList();
                }
                else
                {
                    specificBorder = specificBorder.Where(h => h.IsConsistent(example)).ToList();

                    var newGeneralizers = generalBorder
                        .Select(h => h.IsConsistent(example) ? new List<Hypothesis>() { h } : SmallestSpecifiers(generalBorder, example, h))
                        .Aggregate(new List<Hypothesis>().AsEnumerable(), (a, b) => a.Concat(b))
                        .Where(h => specificBorder.Any(a => h.Contains(a)));
                    generalBorder = newGeneralizers.Where(a => !newGeneralizers.Any(s => s.Contains(a))).ToList();
                }
            }
            Console.WriteLine("G:");
            foreach(var g in generalBorder)
            {
                Console.WriteLine(g);
            }
            Console.WriteLine("S:");
            foreach(var s in specificBorder)
            {
                Console.WriteLine(s);
            }
        }
    }
}
