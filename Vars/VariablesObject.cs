using System;
using System.Collections.Generic;

namespace VariableManagement
{
	/// <summary>
	/// Can contains other objects or fields.
	/// </summary>
	public class VariablesObject
	{
		/// <summary>
		/// Registers a new object to the list.
		/// </summary>
		/// <param name="objectName">The name of the object to create.</param>
		public VariablesObject AddObject(string objectName)
		{
			VariablesObject nextObject = new VariablesObject();
			_subObjects[objectName] = nextObject;
			return nextObject;
		}

		/// <summary>
		/// Registers a field to this object.
		/// </summary>
		/// <param name="fieldName">The name of the field to register.</param>
		/// <param name="fieldValue">The value of the field to register.</param>
		public void AddField(string fieldName, object fieldValue)
		{
			_fields[fieldName] = fieldValue;
		}
		
		/// <summary>
		/// Returns an object from the sub-objects list.
		/// </summary>
		/// <param name="objectName">The name of the object to retrieve.</param>
		/// <returns>The object, if it exists, otherwise it returns null.</returns>
		public VariablesObject GetObject(string objectName)
		{
			if (_subObjects.ContainsKey(objectName))
			{
				return _subObjects[objectName];
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Retrieves a field from the internal list.
		/// </summary>
		/// <param name="fieldName">The name of the field to retrieve.</param>
		/// <returns>The value of the field, if it exists, otherwise null will be returned.</returns>
		public object GetField(string fieldName)
		{
			if (_fields.ContainsKey(fieldName))
			{
				return _fields[fieldName];
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Returns every single sub object stored in this object.
		/// </summary>
		public IEnumerable<KeyValuePair<string, VariablesObject>> GetAllObjects()
		{
			var objectsEnumerator = _subObjects.GetEnumerator();
			
			while (objectsEnumerator.MoveNext())
			{
				yield return objectsEnumerator.Current;
			}
		}

		/// <summary>
		/// Returns every single field stored in this object.
		/// </summary>
		public IEnumerable<KeyValuePair<string, object>> GetAllFields()
		{
			var fieldsEnumerator = _fields.GetEnumerator();

			while (fieldsEnumerator.MoveNext())
			{
				yield return fieldsEnumerator.Current;
			}
		}

		/// <summary>
		/// Updates the value of a field through its name.
		/// </summary>
		/// <param name="fieldName">The name of the field to update.</param>
		/// <param name="newValue">The new value for the field, note that this MUST be the same type as the previous value.</param>
		public void SetFieldValue(string fieldName, object newValue)
		{
			if (!_fields.ContainsKey(fieldName))
			{
				throw new Exception("Key " + fieldName + " does not exist!");
			}

			if (_fields[fieldName].GetType() != newValue.GetType())
			{
				throw new Exception("Could not assign " + newValue.GetType() + " value of " +
									newValue.ToString() + " to field " + fieldName + " of type " +
									_fields[fieldName].GetType() + "!");
			}

			// Assign new value...
			_fields[fieldName] = newValue;
		}

		/* ------------ */
		/* --- Data --- */
		/* ------------ */

		private Dictionary<string, VariablesObject> _subObjects = new Dictionary<string, VariablesObject>();
		private Dictionary<string, object> _fields = new Dictionary<string, object>();
	}
}
