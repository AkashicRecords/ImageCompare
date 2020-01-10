namespace ImageVerifier.MVAProxy
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public interface IProxyKey<T>
    {
        /// <summary>
        /// The Key for the instance
        /// </summary>
        T Id
        {
            get;
        }
    }
}