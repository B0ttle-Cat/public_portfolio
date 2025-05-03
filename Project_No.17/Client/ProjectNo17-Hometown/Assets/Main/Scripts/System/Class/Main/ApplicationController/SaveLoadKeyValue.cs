using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

using Sirenix.OdinInspector;

using UnityEngine;

namespace TFSystem
{
	public interface ISaveLoadKeyValue
	{
		SaveLoadKeyValue KeyValue { get; set; }

		void SetData<T>(string key, T value) => KeyValue.SetData<T>(key, value);
		bool HasData(string key) => KeyValue.HasData(key);
		bool DeleteData(string key) => KeyValue.DeleteData(key);
		T GetData<T>(string key, T defaultValue = default) => KeyValue.GetData(key, defaultValue);
		bool TryGetData<T>(string key, out T tValue) => KeyValue.TryGetData(key, out tValue);
		bool TryNotGetData<T>(string key, out T tValue) => KeyValue.TryNotGetData(key, out tValue);
		T PopData<T>(string key, T defaultValue = default) => KeyValue.PopData(key, defaultValue);
		bool TryPopData<T>(string key, out T tValue) => KeyValue.TryPopData(key, out tValue);
		bool TryNotPopData<T>(string key, out T tValue) => KeyValue.TryNotPopData(key, out tValue);

		void SetData<T>(T value) => SetData(typeof(T).FullName, value);
		bool HasData<T>() => HasData(typeof(T).FullName);
		bool DeleteData<T>() => DeleteData(typeof(T).FullName);
		T GetData<T>(T defaultValue = default) => GetData<T>(typeof(T).FullName, defaultValue);
		bool TryGetData<T>(out T tValue) => TryGetData(typeof(T).FullName, out tValue);
		bool TryNotGetData<T>(out T tValue) => TryNotGetData(typeof(T).FullName, out tValue);
		T PopData<T>(T defaultValue = default) => PopData<T>(typeof(T).FullName, defaultValue);
		bool TryPopData<T>(out T tValue) => TryPopData(typeof(T).FullName, out tValue);
		bool TryNotPopData<T>(out T tValue) => TryNotPopData(typeof(T).FullName, out tValue);

		void SetJsonData<T>(string key, T value, bool prettyPrint = false) => KeyValue.SetData(key, JsonUtility.ToJson(value, prettyPrint));
		void SetJsonData<T>(T value, bool prettyPrint = false) => SetData(typeof(T).FullName, JsonUtility.ToJson(value, prettyPrint));

		T GetJsonData<T>()
		{
			return TryGetJsonData<T>(out T tValue) ? tValue : default;
		}
		T GetJsonData<T>(string key)
		{
			return TryGetJsonData<T>(key, out T tValue) ? tValue : default;
		}
		bool TryGetJsonData<T>(out T tValue)
		{
			return TryGetJsonData<T>(typeof(T).FullName, out tValue);
		}
		bool TryGetJsonData<T>(string key, out T tValue)
		{
			if(TryGetData(key, out string tJson))
			{
				try
				{
					tValue = JsonUtility.FromJson<T>(tJson);
					return true;
				}
				catch(Exception ex)
				{
					Debug.LogError($"Key({key}) Type({typeof(T).FullName}) Json({tJson})");
					Debug.LogException(ex);
					tValue = default;
					return false;
				}
			}
			tValue = default;
			return false;
		}
		bool TryGetJsonOverwrite<T>(ref T tValue)
		{
			return TryGetJsonOverwrite(typeof(T).FullName, ref tValue);
		}
		bool TryGetJsonOverwrite<T>(string key, ref T tValue)
		{
			if(TryGetData(key, out string tJson))
			{
				try
				{
					JsonUtility.FromJsonOverwrite(tJson, tValue);
					return true;
				}
				catch(Exception ex)
				{
					Debug.LogError($"Key({key}) Type({typeof(T).FullName}) Json({tJson})");
					Debug.LogException(ex);
					tValue = default;
					return false;
				}
			}
			tValue = default;
			return false;
		}

		public string ToJson() => KeyValue.ToJson();
		public void FromJson(string jsonData) => KeyValue.FromJson(jsonData);
	}

	[Serializable]
	public class SaveLoadKeyValue : ISerializable, IDisposable
	{
		private bool isDisposable = false;
		private string separateKey;
		private JsonSerializerSettings serializerSettings;
		public SaveLoadKeyValue(string separateKey)
		{
			isDisposable = false;
			this.separateKey = separateKey;
			data = new Dictionary<string, object>();
			serializerSettings = new JsonSerializerSettings {
				// Converters = new List<JsonConverter> { new UnityJsonConverter() },
				ContractResolver = new FieldsOnlyContractResolver(),
				Formatting = Formatting.Indented,
				NullValueHandling = NullValueHandling.Ignore,
				TypeNameHandling = TypeNameHandling.Auto,
			};
		}
		private Dictionary<string, object> data = new Dictionary<string, object>();
		[ShowInInspector, ReadOnly]
		public Dictionary<string, object> Data {
			get {
				if(data== null) data = new Dictionary<string, object>();
				return data;
			}
			private set {
				data = value;
			}
		}
		public void SetData<T>(string key, T value)
		{
			Data[key] = value;
		}
		public bool HasData(string key)
		{
			return Data.ContainsKey(key);
		}
		public bool DeleteData(string key)
		{
			return Data.Remove(key);
		}
		public void ClearData()
		{
			Data.Clear();
		}
		public T GetData<T>(string key)
		{
			return TryGetData<T>(key, out var value) ? value : default;
		}
		public T GetData<T>(string key, T defaultValue = default)
		{
			return TryGetData<T>(key, out var value) ? value : defaultValue;
		}
		public bool TryGetData<T>(string key, out T tValue)
		{
			if(Data.TryGetValue(key, out var value))
			{
				if(value is T dataValue)
				{
					tValue = dataValue;
					return true;
				}
				else if(value is JObject jObj)
				{
					tValue = JObject.FromObject(jObj).ToObject<T>();
					return true;
				}
				else if(value is JArray jArray)
				{
					tValue = jArray.ToObject<T>();
					return true;
				}
				else if(value is JValue jValue)
				{
					tValue = jValue.ToObject<T>();
					return true;
				}
			}
			tValue = default;
			return false;
		}
		public bool TryNotGetData<T>(string key, out T tValue)
		{
			return !TryGetData<T>(key, out tValue);
		}
		public T PopData<T>(string key, T defaultValue = default)
		{
			return TryPopData<T>(key, out var tValue) ? tValue : defaultValue;
		}
		public bool TryPopData<T>(string key, out T tValue)
		{
			if(Data.TryGetValue(key, out var value))
			{
				if(value is T dataValue)
				{
					Data.Remove(key);
					tValue = dataValue;
					return true;
				}
				else if(value is JObject jObj)
				{
					Data.Remove(key);
					tValue = jObj.ToObject<T>();
					return true;
				}
				else if(value is JArray jArray)
				{
					Data.Remove(key);
					tValue = jArray.ToObject<T>();
					return true;
				}
				else if(value is JValue jValue)
				{
					Data.Remove(key);
					tValue = jValue.ToObject<T>();
					return true;
				}
			}
			tValue = default;
			return false;
		}
		public bool TryNotPopData<T>(string key, out T tValue)
		{
			return !TryPopData<T>(key, out tValue);
		}
		public string ToJson()
		{
			try
			{
				string json = JsonConvert.SerializeObject(Data, serializerSettings);
				return json;
			}
			catch(Exception ex)
			{
				Debug.LogException(ex);
				return "";
			}
		}
		public void FromJson(string jsonData)
		{
			Data = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonData, serializerSettings);
		}

		public void SetData<T>(T value) => SetData(typeof(T).FullName, value);
		public bool HasData<T>() => HasData(typeof(T).FullName);
		public bool DeleteData<T>() => DeleteData(typeof(T).FullName);
		public T GetData<T>(T defaultValue) => GetData<T>(typeof(T).FullName, defaultValue);
		public T GetData<T>() => GetData<T>(typeof(T).FullName);
		public bool TryGetData<T>(out T tValue) => TryGetData(typeof(T).FullName, out tValue);
		public bool TryNotGetData<T>(out T tValue) => TryNotGetData(typeof(T).FullName, out tValue);
		public T PopData<T>(T defaultValue = default) => PopData<T>(typeof(T).FullName, defaultValue);
		public bool TryPopData<T>(out T tValue) => TryPopData(typeof(T).FullName, out tValue);
		public bool TryNotPopData<T>(out T tValue) => TryNotPopData(typeof(T).FullName, out tValue);

		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if(info == null)
				throw new ArgumentNullException(nameof(info));

			// 데이터 Dictionary를 JSON 문자열로 변환하여 SerializationInfo에 추가
			string json = ToJson();
			info.AddValue("Data", json);
		}
		protected SaveLoadKeyValue(SerializationInfo info, StreamingContext context)
		{
			if(info == null)
				throw new ArgumentNullException(nameof(info));

			// JSON 데이터를 복원
			string jsonData = info.GetString("Data");
			FromJson(jsonData);
		}

		public void Dispose()
		{
			if(isDisposable) return;
			isDisposable = true;
			separateKey = null;
			data?.Clear();
			data = null;
		}
	}

	public class UnityJsonConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(Vector2) ||
				   objectType == typeof(Vector3) ||
				   objectType == typeof(Vector4) ||
				   objectType == typeof(Quaternion) ||
				   objectType == typeof(Pose) ||
				   objectType == typeof(Color) ||
				   objectType == typeof(Color32) ||
				   objectType == typeof(Rect) ||
				   objectType == typeof(Bounds);
		}
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			writer.WriteStartObject();

			switch(value)
			{
				case Vector2 v2:
					writer.WritePropertyName("type"); writer.WriteValue("Vector2");
					writer.WritePropertyName("x"); writer.WriteValue(v2.x);
					writer.WritePropertyName("y"); writer.WriteValue(v2.y);
					break;
				case Vector3 v3:
					writer.WritePropertyName("type"); writer.WriteValue("Vector3");
					writer.WritePropertyName("x"); writer.WriteValue(v3.x);
					writer.WritePropertyName("y"); writer.WriteValue(v3.y);
					writer.WritePropertyName("z"); writer.WriteValue(v3.z);
					break;
				case Vector4 v4:
					writer.WritePropertyName("type"); writer.WriteValue("Vector4");
					writer.WritePropertyName("x"); writer.WriteValue(v4.x);
					writer.WritePropertyName("y"); writer.WriteValue(v4.y);
					writer.WritePropertyName("z"); writer.WriteValue(v4.z);
					writer.WritePropertyName("w"); writer.WriteValue(v4.w);
					break;
				case Quaternion q:
					writer.WritePropertyName("type"); writer.WriteValue("Quaternion");
					writer.WritePropertyName("x"); writer.WriteValue(q.x);
					writer.WritePropertyName("y"); writer.WriteValue(q.y);
					writer.WritePropertyName("z"); writer.WriteValue(q.z);
					writer.WritePropertyName("w"); writer.WriteValue(q.w);
					break;
				case Pose p:
					writer.WritePropertyName("type"); writer.WriteValue("Pose");
					writer.WritePropertyName("position"); serializer.Serialize(writer, p.position);
					writer.WritePropertyName("rotation"); serializer.Serialize(writer, p.rotation);
					break;
				case Color c:
					writer.WritePropertyName("type"); writer.WriteValue("Color");
					writer.WritePropertyName("r"); writer.WriteValue(c.r);
					writer.WritePropertyName("g"); writer.WriteValue(c.g);
					writer.WritePropertyName("b"); writer.WriteValue(c.b);
					writer.WritePropertyName("a"); writer.WriteValue(c.a);
					break;
				case Color32 c32:
					writer.WritePropertyName("type"); writer.WriteValue("Color32");
					writer.WritePropertyName("r"); writer.WriteValue(c32.r);
					writer.WritePropertyName("g"); writer.WriteValue(c32.g);
					writer.WritePropertyName("b"); writer.WriteValue(c32.b);
					writer.WritePropertyName("a"); writer.WriteValue(c32.a);
					break;
				case Rect rect:
					writer.WritePropertyName("type"); writer.WriteValue("Rect");
					writer.WritePropertyName("x"); writer.WriteValue(rect.x);
					writer.WritePropertyName("y"); writer.WriteValue(rect.y);
					writer.WritePropertyName("width"); writer.WriteValue(rect.width);
					writer.WritePropertyName("height"); writer.WriteValue(rect.height);
					break;
				case Bounds bounds:
					writer.WritePropertyName("type"); writer.WriteValue("Bounds");
					writer.WritePropertyName("center"); serializer.Serialize(writer, bounds.center);
					writer.WritePropertyName("size"); serializer.Serialize(writer, bounds.size);
					break;
			}

			writer.WriteEndObject();
		}
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			string type = null;
			var jo = new Newtonsoft.Json.Linq.JObject();

			while(reader.Read())
			{
				if(reader.TokenType == JsonToken.EndObject) break;

				var propName = reader.Value?.ToString();
				reader.Read();
				jo[propName] = JToken.FromObject(reader.Value);
				if(propName == "type") type = reader.Value?.ToString();
			}

			switch(type)
			{
				case "Vector2":
					return new Vector2((float)jo["x"], (float)jo["y"]);
				case "Vector3":
					return new Vector3((float)jo["x"], (float)jo["y"], (float)jo["z"]);
				case "Vector4":
					return new Vector4((float)jo["x"], (float)jo["y"], (float)jo["z"], (float)jo["w"]);
				case "Quaternion":
					return new Quaternion((float)jo["x"], (float)jo["y"], (float)jo["z"], (float)jo["w"]);
				case "Pose":
					return new Pose(jo["position"].ToObject<Vector3>(), jo["rotation"].ToObject<Quaternion>());
				case "Color":
					return new Color((float)jo["r"], (float)jo["g"], (float)jo["b"], (float)jo["a"]);
				case "Color32":
					return new Color32((byte)jo["r"], (byte)jo["g"], (byte)jo["b"], (byte)jo["a"]);
				case "Rect":
					return new Rect((float)jo["x"], (float)jo["y"], (float)jo["width"], (float)jo["height"]);
				case "Bounds":
					return new Bounds(jo["center"].ToObject<Vector3>(), jo["size"].ToObject<Vector3>());
				default:
					throw new JsonSerializationException($"Unknown Unity type: {type}");
			}
		}

	}
	public class FieldsOnlyContractResolver : DefaultContractResolver
	{
		protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
		{
			var props = new List<JsonProperty>();

			// 필드 처리 (Unity 기준)
			var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			foreach(var field in fields)
			{
				// public 이거나 SerializeField 를 가지고 있는 경우
				//if(field.IsPublic || field.GetCustomAttribute<SerializeField>() != null)
				{
					var prop = base.CreateProperty(field, memberSerialization);
					prop.Readable = true;
					prop.Writable = true;
					props.Add(prop);
				}
			}

			// 자동 프로퍼티 포함 여부 판단
			var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			foreach(var propInfo in properties)
			{
				if(!propInfo.CanRead || !propInfo.CanWrite) continue; // 읽기/쓰기 모두 가능해야 함
				if(propInfo.GetIndexParameters().Length > 0) continue; // 인덱서 제외

				// 자동 프로퍼티( {get;set;} 같은 거)인지 확인 (Backing field 존재 여부 확인)
				var backingField = type.GetField($"<{propInfo.Name}>k__BackingField",
				BindingFlags.Instance | BindingFlags.NonPublic);

				if(backingField != null &&
					backingField.GetCustomAttribute<NonSerializedAttribute>() == null)
				{
					var jsonProp = base.CreateProperty(propInfo, memberSerialization);
					jsonProp.Readable = true;
					jsonProp.Writable = true;
					props.Add(jsonProp);
				}
			}

			return props;
		}
	}

}
