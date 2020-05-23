using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

namespace ILib.CL
{

	public abstract class ArgBinder
	{
		static Dictionary<System.Type, System.Func<ArgBinder>> s_Dic = new Dictionary<System.Type, System.Func<ArgBinder>>
		{
			{ typeof(string), () => new StringArgBinder() },
			{ typeof(bool), () => new BoolArgBinder() },
			{ typeof(int), () => new IntArgBinder() },
			{ typeof(float), () => new FloatArgBinder() },
		};

		public static ArgBinder[] Get(object obj)
		{
			var entries = new List<ArgBinder>();
			foreach (var attr in obj.GetType().GetCustomAttributes(typeof(ArgAttribute)))
			{
				if (attr is ArgAttribute arg)
				{
					entries.Add(new ImportArgBinder
					{
						m_Arg = arg,
						m_Obj = obj
					});
				}
			}
			foreach (var field in obj.GetType().GetFields())
			{
				var arg = field.GetCustomAttribute<ArgAttribute>();
				if (arg == null) continue;
				ArgBinder entry = null;
				if (field.FieldType.IsEnum)
				{
					entry = new EnumArgBinder();
				}
				else
				{
					entry = s_Dic[field.FieldType]();
				}
				entry.m_Obj = obj;
				entry.m_Info = field;
				entry.m_Arg = arg;
				entries.Add(entry);
			}
			return entries.ToArray();
		}

		protected object m_Obj;
		protected FieldInfo m_Info;
		protected ArgAttribute m_Arg;

		public bool IsMatch(ArgEntry entry)
		{
			if (m_Arg.ShortName != 0 && m_Arg.ShortName == entry.ShortName)
			{
				return true;
			}
			if (!string.IsNullOrEmpty(m_Arg.LongName) && m_Arg.LongName == entry.LongName)
			{
				return true;
			}
			return false;
		}

		public void CheckRequired(ArgEntry[] args)
		{
			foreach (var arg in args)
			{
				if (IsMatch(arg))
				{
					return;
				}
			}
			if (m_Arg.Required)
			{
				throw new System.Exception("args not found LongName --" + m_Arg.LongName + " or -" + m_Arg.ShortName);
			}
		}

		public abstract void Set(ArgEntry arg);

	}

	internal class ImportArgBinder : ArgBinder
	{
		public override void Set(ArgEntry arg)
		{
			var json = System.IO.File.ReadAllText(arg.Value);
			JsonUtility.FromJsonOverwrite(json, m_Obj);
		}
	}


	internal class StringArgBinder : ArgBinder
	{
		public override void Set(ArgEntry arg)
		{
			m_Info.SetValue(m_Obj, arg.Value);
		}
	}

	internal class EnumArgBinder : ArgBinder
	{
		public override void Set(ArgEntry arg)
		{
			m_Info.SetValue(m_Obj, System.Enum.Parse(m_Info.FieldType, arg.Value));
		}
	}

	internal class IntArgBinder : ArgBinder
	{
		public override void Set(ArgEntry arg)
		{
			if (int.TryParse(arg.Value, out int ret))
			{
				m_Info.SetValue(m_Obj, ret);
			}
		}
	}

	internal class FloatArgBinder : ArgBinder
	{
		public override void Set(ArgEntry arg)
		{
			if (float.TryParse(arg.Value, out float ret))
			{
				m_Info.SetValue(m_Obj, ret);
			}
		}
	}

	internal class BoolArgBinder : ArgBinder
	{
		public override void Set(ArgEntry arg)
		{
			if (string.IsNullOrEmpty(arg.Value))
			{
				m_Info.SetValue(m_Obj, true);
			}
			else if (bool.TryParse(arg.Value, out bool ret))
			{
				m_Info.SetValue(m_Obj, ret);
			}
		}
	}

}