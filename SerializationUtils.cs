/*
 * Copyright (c) 2020 Kristopher Gay
 *
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files (the
 * "Software"), to deal in the Software without restriction, including
 * without limitation the rights to use, copy, modify, merge, publish,
 * distribute, sublicense, and/or sell copies of the Software, and to
 * permit persons to whom the Software is furnished to do so, subject to
 * the following conditions:
 *
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
 * IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
 * CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
 * TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
 * SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
using System.Collections.Generic;
using UnityEngine;

namespace KGTools.Serialization
{
	public static class SerializationUtils
	{

		#region Constants

		/// <summary>
		/// Serialization key for the X vector value.
		/// </summary>
		private const string VECTOR_X = "x";

		/// <summary>
		/// Serialization key for the y vector value.
		/// </summary>
		private const string VECTOR_Y = "y";

		/// <summary>
		/// Serialization key for the z vector value.
		/// </summary>
		private const string VECTOR_Z = "z";

		/// <summary>
		/// Serialization key for the w vector value.
		/// </summary>
		private const string VECTOR_W = "w";

		/// <summary>
		/// Formatter for use with the colour serialization utils.
		/// </summary>
		private const string COLOUR_SERIALIZATION_FORMATTER = "#{0}";

		#endregion

		#region Dictionary Utils

		/// <summary>
		/// Gets a simple value for a specified key with an optional default fallback value.
		/// </summary>
		/// <param name="dict">The dictionary to look for the serialized data in.</param>
		/// <param name="valueKey">The key for the value.</param>
		/// <param name="fallbackValue">Value to use if the original value cannot be found.</param>
		/// <typeparam name="T">The type of object to deserialize.</typeparam>
		/// <returns>The requested value or the fallback value if the requested value cannot be found.</returns>
		public static T GetValueForKey<T>(this Dictionary<string, object> dict, string valueKey, T fallbackValue = default(T))
		{
			T value = fallbackValue;

			if (dict.ContainsKey(valueKey))
			{
				value = SerializationUtils.DeserializeValue<T>(dict[valueKey]);
			}

			return value;
		}

		/// <summary>
		/// Gets an ISerialized object from a dictionary.
		/// </summary>
		/// <typeparam name="T">Type of object implementing ISerialized.</typeparam>
		/// <returns>The requested object or the fallback value if the object cannot be found.</returns>
		public static T GetObjectForKey<T>(this Dictionary<string, object> dict, string objectKey, T fallbackValue = default(T)) where T : ISerialized, new()
		{
			T value = fallbackValue;

			if (dict.ContainsKey(objectKey))
			{
				value = SerializationUtils.DeserializeObject<T>(dict.DeserializeDictionary<string, object>(objectKey));
			}

			return value;
		}

		/// <summary>
		/// Assert a simple value existing in serialized form in the specified dictionary.
		/// </summary>
		/// <param name="dict">The dictionary to look for the serialized data in.</param>
		/// <param name="valueKey">The key for the value.</param>
		/// <typeparam name="T">Simple type of the object to return.</typeparam>
		/// <returns>The requested value.</returns>
		public static T AssertValueForKey<T>(this Dictionary<string, object> dict, string valueKey)
		{
			if (!dict.ContainsKey(valueKey))
			{
				throw new System.ArgumentException(string.Format("Could not find value {0} in dictionary.", valueKey));
			}

			return SerializationUtils.DeserializeValue<T>(dict[valueKey]);
		}

		/// <summary>
		/// Gets an ISerialized object from a dictionary.
		/// </summary>
		/// <typeparam name="T">Type of object implementing ISerialized.</typeparam>
		/// <returns>The requested object or the fallback value if the object cannot be found.</returns>
		public static T AssertObjectForKey<T>(this Dictionary<string, object> dict, string objectKey) where T : ISerialized, new()
		{
			if (!dict.ContainsKey(objectKey))
			{
				throw new System.ArgumentException(string.Format("Could not find object {0} in dictionary.", objectKey));
			}

				return SerializationUtils.DeserializeObject<T>(dict.DeserializeDictionary<string, object>(objectKey));
		}

		/// <summary>
		/// Deserializes a list of simple objects.
		/// </summary>
		/// <param name="dict">The dictionary containing the list of serialized object.</param>
		/// <param name="listKey">The key of the list to deserialize.</param>
		/// <typeparam name="T">The simple type to deserialize the list elements as.</typeparam>
		/// <returns>The requested list, deserialized or an empty list if that value couldn't be found.</returns>
		public static List<T> DeserializeList<T>(this Dictionary<string, object> dict, string listKey)
		{
			List<T> deserializedList = null;
			if (dict[listKey] is List<T>)
			{
				deserializedList = dict[listKey] as List<T>;
			}
			else if (dict.ContainsKey(listKey))
			{
				List<object> objectList = dict.GetValueForKey<List<object>>(listKey);
				deserializedList = new List<T>(objectList.Count);
				if (objectList != null) {
					for (int i = 0; i < objectList.Count; i++)
					{
						deserializedList.Add(SerializationUtils.DeserializeValue<T>(objectList[i]));
					}
				}
			}
			else
			{
				Debug.LogErrorFormat("Deserialization of list with key {0} failed.", listKey);
				deserializedList = new List<T>();
			}

			return deserializedList;
		}

		/// <summary>
		/// Deserializes an object list from a specified dictionary.
		/// </summary>
		/// <param name="dict">The dictionary to deserialize the object list from.</param>
		/// <param name="listKey">The key to find the serialized list under.</param>
		/// <typeparam name="T">The ISerialized derived type to create.</typeparam>
		/// <returns>A list of deserialized ISerialized objects or an empty list if that one couldn't be found.</returns>
		public static List<T> DeserializedObjectList<T>(this Dictionary<string, object> dict, string listKey) where T : ISerialized, new()
		{
			if (dict.ContainsKey(listKey))
			{
				List<Dictionary<string, object>> serializedObjectList = dict.DeserializeList<Dictionary<string, object>>(listKey);
				List<T> objectList = new List<T>(serializedObjectList.Count);
				
				for (int i = 0; i < serializedObjectList.Count; ++i)
				{
					objectList.Add(SerializationUtils.DeserializeObject<T>(serializedObjectList[i]));
				}

				return objectList;
			}
			else
			{
				return new List<T>();
			}
		}

		/// <summary>
		/// Serializes an object list into a list of serialized objects.
		/// </summary>
		/// <param name="listToSerialize">The list of items to serialize.</param>
		/// <typeparam name="T">Object type implementing ISerialized.</typeparam>
		/// <returns>The serialized list of objects.</returns>
		public static List<Dictionary<string, object>> SerializeObjectList<T>(IList<T> listToSerialize) where T : ISerialized
		{
			List<Dictionary<string, object>> serializedList = new List<Dictionary<string, object>>(listToSerialize.Count);

			for (int i = 0; i < serializedList.Count; ++i)
			{
				serializedList.Add(listToSerialize[i].Serialize());
			}

			return serializedList;
		}

		/// <summary>
		/// Deserializes a dictionary of the given key and value types from the specified dictionary. 
		/// If the key is not in the dictionary it will return an empty dictionary.
		/// </summary>
		/// <returns>The dictionary of the given key and value types that was deserialized.</returns>
		/// <param name="dict">Dictionary to get the value from.</param>
		/// <param name="dictKey">Key of the value to deserialize.</param>
		/// <typeparam name="K">The type of the key of the desired dictionary.</typeparam>
		/// <typeparam name="V">The type of the value of the desired dictionary.</typeparam>
		public static Dictionary<K, V> DeserializeDictionary<K, V>(this Dictionary<string, object> dict, string dictKey)
		{
			Dictionary<string, object> objectDict = dict.GetValueForKey<Dictionary<string, object>>(dictKey, null);
			if (objectDict != null)
			{
				Dictionary<K, V> deserializedDict = new Dictionary<K, V>(objectDict.Count);
				foreach (KeyValuePair<string, object> kvp in objectDict)
				{
					K typedKey = SerializationUtils.DeserializeValue<K>(kvp.Key);
					V typedValue = SerializationUtils.DeserializeValue<V>(kvp.Value);
						
					deserializedDict[typedKey] = typedValue;
				}

				return deserializedDict;
			}
			else
			{
				Debug.LogErrorFormat("Deserialzation of dictionary with key {0} failed.", dictKey);
				return new Dictionary<K, V>();
			}
		}

		#endregion

		#region JSON Value Serialization

		/// <summary>
		/// Deserializes an ISerialized object.
		/// </summary>
		/// <param name="serializedObject">The serialized version of the object to create.</param>
		/// <typeparam name="T">Type of object to create.</typeparam>
		/// <returns>The created object with the serialized value deserialized into it.</returns>
		public static T DeserializeObject<T>(Dictionary<string, object> serializedObject) where T : ISerialized, new()
		{
			T createdObject = new T();
			createdObject.Deserialize(serializedObject);
			return createdObject;
		}

		/// <summary>
		/// Deserializes a given value into the specified type.
		/// </summary>
		/// <param name="value">The object to deserialize.</param>
		/// <typeparam name="T">The type to deserialize the value as.</typeparam>
		/// <returns>The value cast to the correct type.</returns>
		public static T DeserializeValue<T>(object value)
		{
			System.Type objectType = typeof(T);
			if (value is T)
			{
				return (T)value;
			}
			else
			{
				if (objectType.IsEnum)
				{
					return (T)System.Enum.Parse(objectType, value.ToString(), true);
				}
				else
				{
					return (T)System.Convert.ChangeType(value, objectType, System.Globalization.CultureInfo.InvariantCulture);
				}
			}
		}

		#endregion

		#region Vector

		/// <summary>
		/// Serialized a Vector4 into a dictionary.
		/// </summary>
		/// <param name="vec">The vector to serialize.</param>
		/// <returns>The serialized vector.</returns>
		public static Dictionary<string, object> Serialize(Vector4 vec)
		{
			Dictionary<string, object> serializedVector = new Dictionary<string, object>(4);

			serializedVector[VECTOR_X] = vec.x;
			serializedVector[VECTOR_Y] = vec.y;
			serializedVector[VECTOR_Z] = vec.z;
			serializedVector[VECTOR_W] = vec.w;

			return serializedVector;
		}

		/// <summary>
		/// Deserializes a Vector4 value.
		/// </summary>
		/// <param name="serializedVector">The serialized vector.</param>
		/// <returns>Deserializes vector4 value.</returns>
		public static Vector4 DeserializeVec4(Dictionary<string, object> serializedVector)
		{
			return new Vector4(
				serializedVector.AssertValueForKey<float>(VECTOR_X),
				serializedVector.AssertValueForKey<float>(VECTOR_Y),
				serializedVector.AssertValueForKey<float>(VECTOR_Z),
				serializedVector.AssertValueForKey<float>(VECTOR_W)
			);
		}

		/// <summary>
		/// Serialized a Vector3 into a dictionary.
		/// </summary>
		/// <param name="vec">The vector to serialize.</param>
		/// <returns>The serialized vector.</returns>
		public static Dictionary<string, object> Serialize(Vector3 vec)
		{
			Dictionary<string, object> serializedVector = new Dictionary<string, object>(3);
			
			serializedVector[VECTOR_X] = vec.x;
			serializedVector[VECTOR_Y] = vec.y;
			serializedVector[VECTOR_Z] = vec.z;

			return serializedVector;
		}

		/// <summary>
		/// Deserializes a Vector3 value.
		/// </summary>
		/// <param name="serializedVector">The serialized vector.</param>
		/// <returns>Deserializes vector3 value.</returns>
		public static Vector3 DeserializeVec3(Dictionary<string, object> serializedVector)
		{
			return new Vector3(
				serializedVector.AssertValueForKey<float>(VECTOR_X),
				serializedVector.AssertValueForKey<float>(VECTOR_Y),
				serializedVector.AssertValueForKey<float>(VECTOR_Z)
			);
		}

		/// <summary>
		/// Serialized a Vector3Int into a dictionary.
		/// </summary>
		/// <param name="vec">The vector to serialize.</param>
		/// <returns>The serialized vector.</returns>
		public static Dictionary<string, object> Serialize(Vector3Int vec)
		{
			Dictionary<string, object> serializedVector = new Dictionary<string, object>(3);
			
			serializedVector[VECTOR_X] = vec.x;
			serializedVector[VECTOR_Y] = vec.y;
			serializedVector[VECTOR_Z] = vec.z;

			return serializedVector;
		}

		/// <summary>
		/// Deserializes a Vector3Int value.
		/// </summary>
		/// <param name="serializedVector">The serialized vector.</param>
		/// <returns>Deserializes vector3Int value.</returns>
		public static Vector3Int DeserializeVec3Int(Dictionary<string, object> serializedVector)
		{
			return new Vector3Int(
				serializedVector.AssertValueForKey<int>(VECTOR_X),
				serializedVector.AssertValueForKey<int>(VECTOR_Y),
				serializedVector.AssertValueForKey<int>(VECTOR_Z)
			);
		}

		/// <summary>
		/// Serialized a Vector2 into a dictionary.
		/// </summary>
		/// <param name="vec">The vector to serialize.</param>
		/// <returns>The serialized vector.</returns>
		public static Dictionary<string, object> Serialize(Vector2 vec)
		{
			Dictionary<string, object> serializedVector = new Dictionary<string, object>(2);
			
			serializedVector[VECTOR_X] = vec.x;
			serializedVector[VECTOR_Y] = vec.y;

			return serializedVector;
		}

		/// <summary>
		/// Deserializes a Vector2Int value.
		/// </summary>
		/// <param name="serializedVector">The serialized vector.</param>
		/// <returns>Deserializes vector2Int value.</returns>
		public static Vector2 DeserializeVec2(Dictionary<string, object> serializedVector)
		{
			return new Vector2(
				serializedVector.AssertValueForKey<float>(VECTOR_X),
				serializedVector.AssertValueForKey<float>(VECTOR_Y)
			);
		}

		/// <summary>
		/// Serialized a Vector2Int into a dictionary.
		/// </summary>
		/// <param name="vec">The vector to serialize.</param>
		/// <returns>The serialized vector.</returns>
		public static Dictionary<string, object> Serialize(Vector2Int vec)
		{
			Dictionary<string, object> serializedVector = new Dictionary<string, object>(2);
			
			serializedVector[VECTOR_X] = vec.x;
			serializedVector[VECTOR_Y] = vec.y;

			return serializedVector;
		}

		/// <summary>
		/// Deserializes a Vector2Int value.
		/// </summary>
		/// <param name="serializedVector">The serialized vector.</param>
		/// <returns>Deserializes vector2Int value.</returns>
		public static Vector2Int DeserializeVec2Int(Dictionary<string, object> serializedVector)
		{
			return new Vector2Int(
				serializedVector.AssertValueForKey<int>(VECTOR_X),
				serializedVector.AssertValueForKey<int>(VECTOR_Y)
			);
		}

		/// <summary>
		/// Serializes a colour into a format as required by the ColourUtility.
		/// </summary>
		/// <param name="colourToSerialize">The colour to serialize into a string.</param>
		/// <returns>The colour serialized into a string that can be used by ColourUtility.</returns>
		public static string SerializeColour(Color colourToSerialize)
		{
			return string.Format(COLOUR_SERIALIZATION_FORMATTER, ColorUtility.ToHtmlStringRGBA(colourToSerialize));
		}

		/// <summary>
		/// Deserializes an RGBA colour string.
		/// </summary>
		/// <param name="rgbString">A string formatted as required by ColourUtility helper class.</param>
		/// <returns>The deserialized colour.</returns>
		public static Color DeserializeRGBColour(string rgbString)
		{
			Color deserializedColour = Color.white;

			if (!ColorUtility.TryParseHtmlString(rgbString, out deserializedColour))
			{
				Debug.LogError("Failed parsing colour string: " + rgbString);
			}

			return deserializedColour;
		}

		#endregion

	}
}
