using System;
using System.Collections.Generic;
using System.Dynamic;

namespace DynamicRM
{
    public class Entity : DynamicObject
    {
		public string EntityTableName { get;  set; }

		internal Dictionary<string, object> _originalProperties = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
		private Dictionary<string, object> _dynamicProperties = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase); 


		public Entity(string entityName)
		{
			EntityTableName = entityName;
		}

		public override bool TryGetMember(GetMemberBinder binder, out object result)
		{
			return TryGetProperty(binder.Name, out result);
		}

		public bool TryGetProperty(string name, out object result)
		{
			if (_dynamicProperties.ContainsKey(name))
			{
				result = _dynamicProperties[name];
			}
			else
			{
				result = null;
				return false;
			}
			return true;
		}

		public override bool TrySetMember(SetMemberBinder binder, object value)
		{
			SetProperty(binder.Name, value);
			return true;
		}

		public void SetProperty(string name, object value)
		{
			_dynamicProperties[name] = value;
		}

		public void RemoveProperty(string name)
		{
			if(_dynamicProperties.ContainsKey(name))
				_dynamicProperties.Remove(name);
		}

		public override IEnumerable<string> GetDynamicMemberNames()
		{
			return _dynamicProperties.Keys;
		}

		public object this[string property]
		{
			get
			{
				object result;
				TryGetProperty(property, out result);
				return result;
			}
			set
			{
				SetProperty(property, value);
			}
		}

		/// <summary>
		/// Sets all currently loaded properties on this Entity as "original" values. They will not be updated by calling DynamicSqlConnection.Update();
		/// </summary>
		public void SetOriginalValues()
		{
			_originalProperties = new Dictionary<string, object>(_dynamicProperties);
		}
	}
}
