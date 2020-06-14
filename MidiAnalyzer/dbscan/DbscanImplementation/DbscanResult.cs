using System.Collections.Generic;
using System.Linq;

namespace DbscanImplementation
{
    /// <summary>
    /// Result object of algorithm after clusters computed
    /// </summary>
    /// <typeparam name="TFeature">Feature data contribute into algorithm</typeparam>
    public class DbscanResult<TFeature>
    {
        private const int NoiseKey = 0;

        public DbscanResult(DbscanPoint<TFeature>[] allPoints)
        {
            var allClusters = allPoints
                .GroupBy(x => x.ClusterId)
                .ToDictionary(x => x.Key ?? NoiseKey, x => x.ToArray());

            var clusters = allClusters.Where(x => x.Key > NoiseKey);
            Clusters = new Dictionary<int, DbscanPoint<TFeature>[]>();
            foreach (var p in clusters)
            {
                Clusters.Add(p.Key, p.Value);
            }
            Noise = allClusters.ContainsKey(NoiseKey) ? allClusters[NoiseKey] : new DbscanPoint<TFeature>[NoiseKey];
        }

        public Dictionary<int, DbscanPoint<TFeature>[]> Clusters { get; }

        public DbscanPoint<TFeature>[] Noise { get; set; }
    }
}