using System.Collections.Generic;
using System.Linq;

namespace PSpectrum
{
    public class Normalizer
    {
        private List<float[]> Cache;
        private int Levels;

        public Normalizer(int levels)
        {
            // initialize new cache + 1 for the current data
            this.Cache = new List<float[]>();
            this.Levels = levels;
        }

        public float[] PreventOverdraw(float[] data, float overdraw = 3)
        {
            var hasOverdraw = data.Any(e => e >= overdraw);

            if (hasOverdraw) return this.Cache.First();
            return data;
        }

        public float[] Next(float[] data)
        {
            // add current data to cache
            this.Cache.Insert(0, data);

            // check if the cache is to large (+1 because of the current data)
            if (this.Cache.Count > this.Levels + 1) this.Cache.RemoveAt(this.Cache.Count - 1);

            // prepare result
            float[] result = new float[data.Length];

            // check each cache entry and add it to the result
            for (int i = 0; i < this.Cache.Count; i++)
            {
                for (int j = 0; j < this.Cache[i].Length; j++)
                {
                    result[j] += this.Cache[i][j];
                }
            }

            // go thru every item in result and devide it by the amount of cache levels
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = result[i] / this.Cache.Count;
            }

            return result;
        }
    }
}