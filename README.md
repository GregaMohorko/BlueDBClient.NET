# BlueDBClient.NET
A client library in .NET for [BlueDB](https://github.com/GregaMohorko/BlueDB) library. Includes base classes for entities and supports JSON encoding/decoding of entities.

Latest release: [v1.0](https://github.com/GregaMohorko/BlueDBClient.NET/releases/latest)

## Documentation & Tutorials
You can read the documentation and tutorials under the [Wiki](https://github.com/GregaMohorko/BlueDBClient.NET/wiki).

## Short examples
Defining entity classes:
```C#
class User : BlueDBEntity
{
  // example string field
  public static readonly Field NameField = Field.Register(nameof(Name), typeof(User));
  public string Name { get; set; }
}
```

(De)Serializing entities to/from JSON:
```C#
string json = JsonConvert.SerializeObject(user);
```

## Requirements
.NET Framework 4.6.1

## License
[Apache License 2.0](./LICENSE)
