using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Xml;

namespace DynamicRM
{
	public class DynamicSqlConnection : IDisposable
	{
		private SqlConnection _connection;
		private Dictionary<string, EntityProperties> _entities = new Dictionary<string, EntityProperties>(StringComparer.OrdinalIgnoreCase);
		private bool _disposed = false;

		public DynamicSqlConnection(SqlConnection connection)
		{
			_connection = connection;
			SqlCommand cmd = new SqlCommand();
			DataTable schemaTable;
			SqlDataReader reader;
			
			if(_connection.State != ConnectionState.Open)
				_connection.Open();
			
			cmd.Connection = _connection;
			cmd.CommandText = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.Tables ";
			reader = cmd.ExecuteReader();
			List<string> tables = new List<string>();
			while (reader.Read())
			{
				tables.Add(reader["TABLE_NAME"].ToString());
			}
			reader.Close();
			foreach (string table in tables)
			{
				cmd.CommandText = "SELECT * FROM " + table;
				reader = cmd.ExecuteReader(CommandBehavior.KeyInfo);
				schemaTable = reader.GetSchemaTable();
				EntityProperties props = new EntityProperties();
				foreach (DataRow myField in schemaTable.Rows)
				{
					string column = myField[schemaTable.Columns["ColumnName"]].ToString();
					props.AddColumn(column, (SqlDbType)(int)myField[schemaTable.Columns["ProviderType"]]);
					if((myField[schemaTable.Columns["IsKey"]] as bool?) == true)
					{
						bool auto = myField[schemaTable.Columns["IsAutoIncrement"]] as bool? == true;
						if (auto)
						{
							props.AddAutoIncrementingKey(column);
						}
						else
						{
							props.AddKey(column);
						}
					}
				}
				_entities.Add(table, props);
			}
			
			reader.Close();
		}

		public IEnumerable<dynamic> Query(string sqlQuery)
		{
			return Query<Entity>(sqlQuery);
		}

		public IEnumerable<T> Query<T>(string sqlQuery) where T : Entity, new()
		{
			SqlCommand cmd = new SqlCommand();
			cmd.Connection = _connection;
			cmd.CommandText = sqlQuery;
			using (SqlDataReader reader = cmd.ExecuteReader())
			{
				try
				{
					while (reader.Read())
					{
						dynamic row = new Entity("QueryResult");
						for (int i = 0; i < reader.FieldCount; i++)
						{
							string column = reader.GetName(i);
							row[column] = reader[column];
						}
						(row as Entity).SetOriginalValues();
						yield return row;
					}
				}
				finally
				{
					reader.Close();
				}
			}
		}

		public dynamic QuerySingle(string sqlQuery)
		{
			return QuerySingle<Entity>(sqlQuery);
		}

		public dynamic QuerySingle<T>(string sqlQuery) where T:  Entity, new()
		{
			SqlCommand cmd = new SqlCommand();
			cmd.Connection = _connection;
			cmd.CommandText = sqlQuery;
			dynamic result = new object();
			using (SqlDataReader reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					dynamic row = new T();
					for (int i = 0; i < reader.FieldCount; i++)
					{
						string column = reader.GetName(i);
						row[column] = reader[column];
					}
					(row as Entity).SetOriginalValues();
					result = row;
					break;
				}
				reader.Close();
			}
			return result;
		}
		

		public void Insert(params Entity[] entities)
		{
			foreach (Entity entity in entities)
			{
				VerifyTableName(entity);
				SqlCommand cmd = new SqlCommand();
				cmd.Connection = _connection;
				SqlCRUDStatements.BuildInsertStatement(cmd, entity, _entities[entity.EntityTableName]);
				cmd.ExecuteNonQuery();
			}
		}
		
		public void Update(params Entity[] entities)
		{
			foreach (Entity entity in entities)
			{
				VerifyTableName(entity);
				SqlCommand cmd = new SqlCommand();
				cmd.Connection = _connection;
				SqlCRUDStatements.BuildUpdateStatement(cmd, entity, _entities[entity.EntityTableName]);
				cmd.ExecuteNonQuery();
			}
		}

		public void Delete(params Entity[] entities)
		{
			foreach (Entity entity in entities)
			{
				VerifyTableName(entity);
				SqlCommand cmd = new SqlCommand();
				cmd.Connection = _connection;
				SqlCRUDStatements.BuildDeleteStatement(cmd, entity, _entities[(entity as Entity).EntityTableName]);
				cmd.ExecuteNonQuery();
			}
		}

		private void VerifyTableName(Entity entity)
		{
			if (!_entities.ContainsKey(entity.EntityTableName))
			{
				throw new Exception(string.Format("Table '{0}' does not exist. Please specify an EntityTableName that exists.", entity.EntityTableName));
			}
		}

		public int ExecuteNonQuery(string sql)
		{
			SqlCommand cmd = new SqlCommand();
			cmd.Connection = _connection;
			cmd.CommandText = sql;
			return cmd.ExecuteNonQuery();
		}

		public Task<int> ExecuteNonQueryAsync(string sql)
		{
			SqlCommand cmd = new SqlCommand();
			cmd.Connection = _connection;
			cmd.CommandText = sql;
			return cmd.ExecuteNonQueryAsync();
		}

		public object ExecuteScalar(string sql)
		{
			SqlCommand cmd = new SqlCommand();
			cmd.Connection = _connection;
			cmd.CommandText = sql;
			return cmd.ExecuteScalar();
		}

		public Task<object> ExecuteScalarAsync(string sql)
		{
			SqlCommand cmd = new SqlCommand();
			cmd.Connection = _connection;
			cmd.CommandText = sql;
			return cmd.ExecuteScalarAsync();
		}

		public IDataReader ExecuteReader(string sql)
		{
			SqlCommand cmd = new SqlCommand();
			cmd.Connection = _connection;
			cmd.CommandText = sql;
			return cmd.ExecuteReader();
		}

		public Task<SqlDataReader> ExecuteReaderAsync(string sql)
		{
			SqlCommand cmd = new SqlCommand();
			cmd.Connection = _connection;
			cmd.CommandText = sql;
			return cmd.ExecuteReaderAsync();
		}

		public XmlReader ExecuteXmlReader(string sql)
		{
			SqlCommand cmd = new SqlCommand();
			cmd.Connection = _connection;
			cmd.CommandText = sql;
			return cmd.ExecuteXmlReader();
		}

		public Task<XmlReader> ExecuteXmlReaderAsync(string sql)
		{
			SqlCommand cmd = new SqlCommand();
			cmd.Connection = _connection;
			cmd.CommandText = sql;
			return cmd.ExecuteXmlReaderAsync();
		}

		public IEnumerable<string> TableNames { get { return _entities.Keys; } }
		
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public void Dispose(bool disposing)
		{
			if (_disposed) return;

			if (disposing)
			{
				_connection?.Close();
				_connection?.Dispose();
			}
		}

		public IDbTransaction BeginTransaction()
		{
			return _connection.BeginTransaction();
		}

		public IDbTransaction BeginTransaction(IsolationLevel il)
		{
			return _connection.BeginTransaction(il);
		}
		
		public IDbCommand CreateCommand()
		{
			return _connection.CreateCommand();
		}
		
	}
}
