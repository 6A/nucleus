# Nucleus
My 'database' system, made to be as simple and fast as possible, whilst being small and standalone.

## Getting started
*Note: Nucleus.Tests contains some example uses.*

First, you have to create a ``Connection`` that extends ``BaseConnection``.
```csharp
public class Connection : CoreConnection
{
    FileStream fs;

    protected override T Deserialize<T>(byte[] bytes)
    {
        throw new NotImplementedException();
    }

    protected override Stream GetRWStream()
    {
        return fs;
    }

    protected override byte[] Serialize<T>(T obj)
    {
        throw new NotImplementedException();
    }

    public Connection(string uri)
    {
        fs = new FileStream(uri, FileMode.OpenOrCreate, FileAccess.ReadWrite);
        this.Initialize();
    }
}
```
Then, do whatever you want with it.

## Usage

### ``Connection.Query<T>()``
```csharp
using (Query<DateTime> dic = connection.Query<DateTime>("time"))
{
    // Query<T> fully implements IList<T>
}
```

### ``Connection.DictionaryQuery<T>()``
```csharp
using (DictionaryQuery<DateTime> dic = connection.DictionaryQuery<DateTime>("time"))
{
    // DictionaryQuery<T> fully implements IDictionary<string, T>
}
```

### ``Connection.DictionaryQuery()``
```csharp
using (DynamicDictionaryQuery dic = connection.DictionaryQuery("time"))
{
    // DynamicDictionaryQuery fully implements IEnumerable<DynamicDictionaryKeyValuePair>
    dic.Set<string>("hello", "world");
    dic.Set<DateTime>("hello", DateTime.Now);
    
    dic.Get<string>("hello"); // => "world"
    dic.Get<DateTime>("hello"); // => the current time
    
    dic.Contains<string>("hello"); // => true
    dic.Contains<int>("hello"): // => false
    
    foreach (var x in dic)
    {
        Console.WriteLine("[{0}] {1} = {2}", x.ValueType, x.Key, x.Value);
    }
    
    dic.Clear();
}
```

You have a query opened from two places at the same time? No problem, queries are sync everywhere. See [Tests](Nucleus.Tests) for more information.
