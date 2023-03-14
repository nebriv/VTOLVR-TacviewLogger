using System.Collections.Generic;
using System.Reflection;

namespace ExtensionMethods
{
    public static class Extensions
    {
        public static List<Variance> DetailedCompare<T>(this T val1, T val2)
        {
            List<Variance> variances = new List<Variance>();

            PropertyInfo[] fi = val1.GetType().GetProperties();

            foreach (PropertyInfo f in fi)
            {
                Variance v = new Variance();
                v.Prop = f.Name;
                v.valA = f.GetValue(val1);
                v.valB = f.GetValue(val2);
                if ((v.valB != null) && (v.valA == null))
                {
                    variances.Add(v);
                }
                else if ((v.valA != null) && (v.valB == null))
                {
                    variances.Add(v);
                }
                else if ((v.valA != null) && (v.valB != null))
                {
                    if (!v.valA.Equals(v.valB))
                        variances.Add(v);
                }
            }
            return variances;
        }
    }

    public class Variance
    {
        public string Prop { get; set; }
        public object valA { get; set; }
        public object valB { get; set; }
    }
}
