using System.Collections.Generic;

public class MultiHashMap<TKey, TValue>
{
    #region Private Fields

    private readonly Dictionary<TKey, IList<TValue>> storage;

    #endregion Private Fields

    #region Public Constructors

    public MultiHashMap()
    {
        storage = new Dictionary<TKey, IList<TValue>>();
    }

    #endregion Public Constructors

    #region Public Properties

    public IEnumerable<TKey> Keys
    {
        get { return storage.Keys; }
    }

    #endregion Public Properties

    #region Public Indexers

    public IList<TValue> this[TKey key]
    {
        get
        {
            if (!storage.ContainsKey(key))
                throw new KeyNotFoundException(
                    string.Format(
                        "The given key {0} was not found in the collection.", key));
            return storage[key];
        }
    }

    #endregion Public Indexers

    #region Public Methods

    public void Add(TKey key, TValue value)
    {
        if (!storage.ContainsKey(key)) storage.Add(key, new List<TValue>());
        storage[key].Add(value);
    }

    public bool ContainsKey(TKey key)
    {
        return storage.ContainsKey(key);
    }

    public int Count(TKey key)
    {
        if (!storage.ContainsKey(key)) return 0;
        return this[key].Count;
    }

    #endregion Public Methods
}