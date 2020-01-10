using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace ImageVerifier.MVAProxy
{
    public class ProxyCache<T, U> where U : IProxyKey<T>
    {
        private Data data = null;
        private Object lockObject = new Object();
        private TimeSpan refreshPeriod;

        private static readonly TimeSpan ZeroLengthPeriod = new TimeSpan(0, 0, 0);

        public delegate U[] DataLoaderDelegate();

        private DataLoaderDelegate loader;
        //private TimeTrace m_timeTrace = new TimeTrace();

        public ProxyCache(DataLoaderDelegate loader, TimeSpan refreshPeriod)
        {
            this.loader = loader;
            this.refreshPeriod = refreshPeriod;
        }

        public List<U> Get()
        {
            return GetData().DataList;
        }

        public U Get(T key)
        {
            U u = default(U);
            GetData().DataDictionary.TryGetValue(key, out u);
            return u;
        }
        /// <summary>
        /// Refreshes the Cache
        /// </summary>
        public void Refresh()
        {
            data = null;
        }
        private Data GetData()
        {
            // First off, get our own reference to the data. If we were to simply test and then
            // return the 'data' variable, we could get into the condition where the timer resets
            // 'data' to null between our test and us returning it. By getting our own copy of the
            // reference, we are not affected from this point by what the timer thread does.
            Data currentData = data;
            if (currentData == null)
            {
                lock (lockObject)
                {
                    // If data is not null, then some other thread beat us to going to the db - just
                    // fall through to where we set the return value. Otherwise, it is up to us to 
                    // go to the web service and get the data.
                    if (data == null)
                    {
                        U[] results;
                        try
                        {
                            //m_timeTrace.Start();
                            // Before creating a new 'data' object, we execute the loader. If this fails
                            // and throws an exception, then we still have 'data' as null, so the next 
                            // thread will get a chance to get the data.
                            results = loader();
                        }
                        finally
                        {
                            MethodInfo delegateMethodInfo = loader.Method;
                            string operation = string.Format(
                                "{0}:{1}()",
                                delegateMethodInfo.DeclaringType.FullName,
                                delegateMethodInfo.Name);

                            //m_timeTrace.Stop(operation);
                        }

                        Data newData = new Data();
                        foreach (U item in results)
                        {
                            newData.DataDictionary.Add(item.Id, item);
                            newData.DataList.Add(item);
                        }

                        // We successfully got the new data from the web service. Set the 'data' object.
                        data = newData;

                        // setup the timer to destroy object
                        Timer timer = new Timer(delegate(Object state) { lock (lockObject) { data = null; } },  // callback - anonymous
                                                        null,
                                                        refreshPeriod,
                                                        ZeroLengthPeriod);

                    }

                    // Set the return object
                    currentData = data;
                }
            }

            return currentData;
        }

        private class Data
        {
            private Dictionary<T, U> dictionary;
            private List<U> list;

            public Dictionary<T, U> DataDictionary
            {
                get { return dictionary; }
            }

            public List<U> DataList
            {
                get { return list; }
            }

            public Data()
            {
                dictionary = new Dictionary<T, U>();
                list = new List<U>();
            }
        }
    }
}
