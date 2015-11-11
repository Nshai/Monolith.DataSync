using System.Collections.Generic;
using System.Linq;
using System.Text;
using IntelliFlo.Platform;
using log4net;

namespace Microservice.Workflow.Engine
{
    /// <summary>
    /// Maintains a list and allows promotion of items through generations
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public class GenerationList<TKey>
    {
        private volatile object lockObj = new object();
        private readonly List<TKey>[] generations;
        private ILog log = LogManager.GetLogger(typeof(GenerationList<TKey>));

        public GenerationList(int generationCount = 3)
        {
            Check.IsTrue(generationCount > 0, "Generation count must be greater than zero");
            generations = new List<TKey>[generationCount];
            for (var generationIndex = 0; generationIndex < generationCount; generationIndex++)
            {
                generations[generationIndex] = new List<TKey>();
            }
        }

        /// <summary>
        /// Promotes the supplied items to relevant generation
        /// Any items already in the list which aren't supplied will be removed
        /// </summary>
        /// <param name="items"></param>
        public void Promote(IEnumerable<TKey> items)
        {
            lock (lockObj)
            {
                var promotedItems = new List<TKey>(items);
                for (var i = MaxGenerationIndex; i >= 0; i--)
                {
                    var currentGeneration = generations[i];
                    currentGeneration.RemoveAll(id => !promotedItems.Contains(id));
                    promotedItems.RemoveAll(id => currentGeneration.Contains(id));

                    if (i >= MaxGenerationIndex) continue;

                    var nextGeneration = generations[i + 1];
                    nextGeneration.AddRange(currentGeneration);
                    currentGeneration.Clear();

                    if (i == 0)
                        currentGeneration.AddRange(promotedItems);
                }
            }

            log.InfoFormat("GenerationPromoted State={0}", ToString());
        }

        /// <summary>
        /// Get the list in a specified generation
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public IEnumerable<TKey> GetGeneration(int index)
        {
            Check.IsTrue(index >= 0, "Index must be greater than or equal to zero");
            Check.IsTrue(index <= MaxGenerationIndex, "Index must be less than maximum number of generations");

            return generations[index];
        }
        
        /// <summary>
        /// Get the index of the last generation
        /// </summary>
        public int MaxGenerationIndex
        {
            get { return generations.Length - 1; }
        }

        /// <summary>
        /// Provide a representation of the current state of the list
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return generations.Aggregate(new StringBuilder(), (s, list) =>
            {
                if (s.Length > 0)
                    s.Append(",");
                s.AppendFormat("[{0}]", string.Join(",", list));
                return s;
            }, s => 
            {
                return s.ToString();
            });
        }
    }
}
