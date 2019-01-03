using System.Data.SqlClient;
using System.Text;

namespace DynamicRM
{
	internal static class SqlCRUDStatements
	{
		internal static void BuildInsertStatement(SqlCommand cmd, Entity entity, EntityProperties props)
		{
			StringBuilder builder = new StringBuilder();
			builder.Append("INSERT INTO ");
			builder.Append(entity.EntityTableName);
			builder.Append(" (");
			bool first = true;
			foreach(string column in props.Columns)
			{
				if (ShouldIncludeColumnForInsert(entity, props, column))
				{
					if (!first)
					{
						builder.Append(",");
					}
					first = false;
					builder.Append(column);
				}
			}
			builder.Append(") VALUES(");
			first = true;
			foreach (string column in props.Columns)
			{
				if (ShouldIncludeColumnForInsert(entity, props, column))
				{
					if (!first)
					{
						builder.Append(",");
					}
					first = false;
					builder.Append("@");
					builder.Append(column);
					cmd.Parameters.AddWithValue("@" + column, entity[column]);
				}
			}
			builder.Append(");");
			cmd.CommandText = builder.ToString();
		}

		internal static void BuildUpdateStatement(SqlCommand cmd, Entity entity, EntityProperties props)
		{
			if(props.AutoIncrementingKeys.Count == 0)
			{
				throw new System.Exception("Cannot dynamically update without an auto-incremented primary key.");
			}
			StringBuilder builder = new StringBuilder();
			builder.Append("UPDATE ");
			builder.Append(entity.EntityTableName);
			builder.Append(" SET ");
			bool first = true;
			foreach (string column in props.Columns)
			{
				if (ShouldIncludeColumnForUpdate(entity, props, column))
				{
					if (!first)
					{
						builder.Append(",");
					}
					first = false;
					builder.Append(column);
					builder.Append(" = ");
					builder.Append("@");
					builder.Append(column);
					cmd.Parameters.AddWithValue("@" + column, entity[column]);
				}
			}
			builder.Append(" WHERE ");
			first = true;
			foreach (string column in props.AutoIncrementingKeys)
			{
				if (entity[column] != null)
				{
					if (!first)
					{
						builder.Append(" AND ");
					}
					first = false;
					builder.Append(column);
					builder.Append(" = ");
					builder.Append("@");
					builder.Append(column);
					if(!cmd.Parameters.Contains("@" + column))
						cmd.Parameters.AddWithValue("@" + column, entity[column]);
				}
			}

			builder.Append(";");
			cmd.CommandText = builder.ToString();
		}

		internal static void BuildDeleteStatement(SqlCommand cmd, Entity entity, EntityProperties props)
		{
			StringBuilder builder = new StringBuilder();
			builder.Append("DELETE FROM ");
			builder.Append(entity.EntityTableName);
			builder.Append(" WHERE ");
			bool first = true;
			foreach (string column in props.KeyColumns)
			{
				if (entity[column] != null)
				{
					if (!first)
					{
						builder.Append(" AND ");
					}
					first = false;
					builder.Append(column);
					builder.Append(" = ");
					builder.Append("@");
					builder.Append(column);
					cmd.Parameters.AddWithValue("@" + column, entity[column]);
				}
			}

			builder.Append(";");
			cmd.CommandText = builder.ToString();
		}

		private static dynamic ShouldIncludeColumnForUpdate(Entity entity, EntityProperties props, string column)
		{
			object result;
			object original;
			return entity.TryGetProperty(column, out result) && !props.AutoIncrementingKeys.Contains(column) 
				&& (!entity._originalProperties.TryGetValue(column, out original)  || result != original);

		}

		private static bool ShouldIncludeColumnForInsert(Entity entity, EntityProperties props, string column)
		{
			return entity[column] != null && (!props.AutoIncrementingKeys.Contains(column));
		}
	}
}
