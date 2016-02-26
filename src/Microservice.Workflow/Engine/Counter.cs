using System.Threading;

namespace Microservice.Workflow.Engine
{
    /// <summary>
    /// Thread-safe counter for storing positive numbers
    /// </summary>
    public class Counter
    {
        private int count = 0;

        /// <summary>
        /// Create a new counter
        /// </summary>
        /// <param name="initialValue"></param>
        public Counter(int initialValue = 0)
        {
            count = initialValue;
        }

        /// <summary>
        /// Increment the counter
        /// </summary>
        /// <returns></returns>
        public int Increment()
        {
            return Interlocked.Increment(ref count);
        }

        /// <summary>
        /// Decrement the counter
        /// </summary>
        /// <returns></returns>
        public int Decrement()
        {
            int initialValue, newValue;
            do
            {
                initialValue = count;
                newValue = initialValue > 0 ? initialValue - 1 : 0;
            } while (initialValue != Interlocked.CompareExchange(ref count, newValue, initialValue));

            return newValue;
        }

        /// <summary>
        /// Retrieve the current value
        /// </summary>
        public int Value => count;
    }
}
