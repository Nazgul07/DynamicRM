# DynamicRM
DynamicRM is a lightwieght Dynamic Object Relational Mapper for .Net written in C#.

It uses non-traditional Entity objects that inherit from DynamicObject. This enables setting column values on Enitty objects at will.

It's super easy to use. Just create a DynamicSqlConnection like this:

```C#
DynamicSqlConnection connection = new DynamicSqlConnection(new SqlConnection("<your connection string>"));
```

Insert a record into the database like this:
```C#
dynamic entity = new Entity("Customers");
entity.FirstName = "John";
connection.Insert(entity);
```

Update records in the database like this:
```C#
dynamic entity = connection.QuerySingle("SELECT * FROM CUSTOMERS");
entity.EntityTableName = "Customers";
entity.FirstName = "Doe";
connection.Update(entity);
```

Delete records in the database like this:
```C#
dynamic entity = connection.QuerySingle("SELECT * FROM CUSTOMERS");
entity.EntityTableName = "Customers";
connection.Delete(entity);
```

You are not required to Query for existing records to work with them, you can use disconnected Entity objects by setting their Primary Key:
```C#
dynamic entity = new Entity("Customers");
entity.Id = 10001;
entity.FirstName = "Doe";
connection.Update(entity);
```

DynamicSqlConnection also provides the basic Command methods that SqlConnection does, like ExecuteNonQuery/ExecuteReader/ExecuteScalar.

NOTE: There is a downside, in that intellisense will not work with the DynamicRM Entity objects. This is a trade off for speed of development.
