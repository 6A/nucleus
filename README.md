# Nucleus
My 'database' system, made to be as simple and fast as possible, whilst being small and standalone.

## Getting started
*Note: Nucleus.Tests contains some example uses.*

First, you have to create a ``Connection`` that extends ``BaseConnection``.
```csharp
public class Connection : CoreConnection
{
    private FileStream fs;
    
    protected override Stream RWStream { get { return fs; } }

    protected override T Deserialize<T>(byte[] bytes)
    {
        throw new NotImplementedException();
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

## Informations
- Data is saved to file everytime `BaseQuery.Save()` is called. By default, this method is called when `Disposed()` is called. This behaviour can be changed by passing `false` to `Connection.*Query(string, bool)` or by changing `BaseQuery.SaveOnDisposed` to `false`.

## Warning
I spent many hours making, improving, and optimizing Nucleus. However, this project is rather old, and may have some rare bugs.  
You're welcome to make sure it works for you.

Right now, it DOES pass many tests easily, and quickly.

## How does it work?
What follows is technical. You do not need to know how Nucleus works to use it.

The file is cut into small pieces named "Sectors." For each sector, there is a Query. Sectors are recognized based on their **name** and their **type**.  
Basically, a sector starts with four bytes that can be converted into an `int`. This `int` is the length of the sector. Once the length is parsed, the given length is read to bytes and given to a new `Sector`, that will parse the metadata, which is:
- first byte: [SectorType](Nucleus/Sectors/Sector.cs#L10)
- second to fifth byte: length of internal metadata
- all that follows: internal metadata and data

Once the Sector knows the length of the metadata, it will decode it to an UTF8 string, and read it as follows:
- the first characters, up to the first semicolon, are the name of the sector
- the characters that follow are split into pairs like this: `string.Split(';').Select(x => x.Split(':'))`. First character of the pair is the index of the value (it is essential to know it internally, but the order has no meaning), and the second is the length of the value

At this point, the Sector is fully parsed. Editing a sector is editing the metadata, and writing bytes to what follows.
