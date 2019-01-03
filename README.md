# DynamicRM
DynamicRM is a lightwieght Dynamic Object Relational Mapper for .Net written in C#.

It uses non-traditional Entity objects that inherit from DynamicObject. This enables setting column values on Entity objects at will.

It's super easy to use. Just create a DynamicSqlConnection like this:

```C#
DynamicSqlConnection connection = new DynamicSqlConnection(new SqlConnection("<your connection string>"));
```

Query for a record:
```C#
dynamic entity = connection.QuerySingle("SELECT * FROM CUSTOMERS");
```

Query for multiple records:
```C#
IEnumerable<dynamic> entities = connection.Query("SELECT * FROM CUSTOMERS");
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

Alternatively, you can subclass Entity and define your properties to get intellisense:
```C#
public class Customer : Entity
	{
		public Customer() : base("Customers")
		{
		}

		public string FirstName
		{
			get
			{
				TryGetProperty(nameof(FirstName), out object result);
				return result as string;
			}
			set
			{
				SetProperty(nameof(FirstName), value);
			}
		}
	}
```

You can then use the Generic Query methods as well:
```C#
Customer entity = context.QuerySingle<Customer>("SELECT * FROM CUSTOMERS");
entity.FirstName = "Doe";
context.Update(entity);
```
