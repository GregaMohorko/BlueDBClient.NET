# BlueDBClient.NET
A client library in .NET for [BlueDB](https://github.com/GregaMohorko/BlueDB) library. Includes base classes for entities and supports JSON encoding/decoding of entities.

[![Release](https://img.shields.io/github/release/GregaMohorko/BlueDBClient.NET.svg?style=flat-square)](https://github.com/GregaMohorko/BlueDBClient.NET/releases/latest)

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

(De)serializing single entities to/from JSON:
```C#
string json = JSON.Encode(user);
User user = JSON.Decode<User>(json);
```

(De)serializing lists of entities to/from JSON:
```C#
string json = JSON.Encode(users);
List<User> users = JSON.DecodeList<User>(json);
```

JSON utility class simply uses the [JSON.NET](http://www.newtonsoft.com/json) library, so you can of course encode/decode entity objects on your own.

## Requirements
.NET Framework 4.6.1

## Author and License

Grega Mohorko ([www.mohorko.info](https://www.mohorko.info))

Copyright (c) 2017 Grega Mohorko

[Apache License 2.0](./LICENSE)
