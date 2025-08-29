namespace FluffyVoid.Logging;

/// <summary>
///     LogManager pooling class used to pool the different logging classes
/// </summary>
internal class LogPoolManager
{
    /// <summary>
    ///     The default size for the object pools
    /// </summary>
    private readonly uint _defaultPoolSize;
    /// <summary>
    ///     Object pool for the LogInformation objects used to hold logging data
    /// </summary>
    private readonly Dictionary<Type, Queue<LogInformation>> _logInformationPool;
    /// <summary>
    ///     Object pool for the LogQueueData objects used to dispatch log messages
    /// </summary>
    private readonly Queue<LogQueueData> _logQueuePool;

    /// <summary>
    ///     Constructor used to initialize the manager and create the default object pools
    /// </summary>
    /// <param name="defaultPoolSize">Tge default size for the object pools</param>
    public LogPoolManager(uint defaultPoolSize)
    {
        _defaultPoolSize = defaultPoolSize;

        Type defaulPool = typeof(LogInformation);
        _logInformationPool = new Dictionary<Type, Queue<LogInformation>>();
        _logInformationPool[defaulPool] = new Queue<LogInformation>((int)defaultPoolSize);
        _logQueuePool = new Queue<LogQueueData>();

        for(uint i = 0; i < defaultPoolSize; ++i)
        {
            LogInformation entry = new LogInformation();
            _logInformationPool[defaulPool].Enqueue(entry);
            _logQueuePool.Enqueue(new LogQueueData());
        }
    }
    /// <summary>
    ///     Retrieves a LogInformation object from the object pool
    /// </summary>
    /// <typeparam name="TType">The type of LogInformation to retrieve</typeparam>
    /// <returns>A LogInformation object to use in storing logging data</returns>
    internal LogInformation GetLogInformation<TType>()
        where TType : LogInformation
    {
        Type poolType = typeof(TType);

        if(_logInformationPool.TryGetValue(poolType, out Queue<LogInformation> pool) && pool != null)
        {
            if(pool.Count > 0)
            {
                return pool.Dequeue();
            }

            return Activator.CreateInstance<TType>();
        }

        _logInformationPool[poolType] = new Queue<LogInformation>((int)_defaultPoolSize);

        for(uint i = 0; i < _defaultPoolSize; ++i)
        {
            _logInformationPool[poolType].Enqueue(Activator.CreateInstance<TType>());
        }

        return _logInformationPool[poolType].Dequeue();
    }

    /// <summary>
    ///     Retrieves a LogQueueData object from the object pool
    /// </summary>
    /// <returns>A LogQueueData object to use in storing log entry data</returns>
    internal LogQueueData GetQueueData()
    {
        return _logQueuePool.Count > 0
            ? _logQueuePool.Dequeue()
            : new LogQueueData();
    }
    /// <summary>
    ///     Returns the LogInformation object back into the pool
    /// </summary>
    /// <param name="information">The LogInformation to return to the pool</param>
    internal void ReturnLogInformation(LogInformation information)
    {
        Type poolType = information.GetType();

        if(_logInformationPool.TryGetValue(poolType, out Queue<LogInformation> pool))
        {
            pool.Enqueue(information);
        }
    }
    /// <summary>
    ///     Returns the LogQueueData object back into the pool
    /// </summary>
    /// <param name="data">The LogQueueData to return to the pool</param>
    internal void ReturnQueueData(LogQueueData data)
    {
        _logQueuePool.Enqueue(data);
    }
}
