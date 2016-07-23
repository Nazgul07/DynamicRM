using System;
using System.Collections.Generic;
using System.Data;

namespace DynamicRM
{
	internal class EntityProperties
	{
		internal List<string> Columns = new List<string>();
		internal List<string> KeyColumns = new List<string>();
		internal List<string> AutoIncrementingKeys = new List<string>();

		Dictionary<string, SqlDbType> _columnTypes = new Dictionary<string, SqlDbType>(StringComparer.OrdinalIgnoreCase);
		
		

		internal void AddKey(string column)
		{
			(KeyColumns as List<string>).Add(column);
		}

		internal void AddAutoIncrementingKey(string column)
		{
			(KeyColumns as List<string>).Add(column);
			(AutoIncrementingKeys as List<string>).Add(column);
		}

		internal void AddColumn(string column, SqlDbType type)
		{
			(Columns as List<string>).Add(column);
			_columnTypes.Add(column, type);
		}
	}
}
