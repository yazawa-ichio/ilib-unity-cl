using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ILib.CL
{

	public class CLParser
	{
		public static T Run<T>() where T : new()
		{
			var parser = new CLParser();
			return parser.Run<T>(Environment.GetCommandLineArgs());
		}

		List<ArgEntry> m_Default = new List<ArgEntry>();

		public void SetDefault(char shortName, string longName, string value)
		{
			m_Default.Add(new ArgEntry
			{
				ShortName = shortName,
				LongName = longName,
				Value = value
			});
		}

		public T Run<T>(string[] args) where T : new()
		{
			T ret = new T();
			Run(ret, args);
			return ret;
		}

		bool IsArgName(string arg)
		{
			if (arg == null) return false;
			return arg.StartsWith("-") || arg.StartsWith("--");
		}

		public void Run(object obj, string[] args)
		{
			var binders = ArgBinder.Get(obj);
			var entries = Enumerable.Concat(m_Default, GetArgEntries(args).Where(x => x != null)).ToArray();
			foreach (var binder in binders)
			{
				binder.CheckRequired(entries);
			}
			foreach (var entry in entries)
			{
				foreach (var binder in binders)
				{
					if (binder.IsMatch(entry))
					{
						binder.Set(entry);
					}
				}
			}
		}

		IEnumerable<ArgEntry> GetArgEntries(string[] args)
		{
			string op = null;
			foreach (var arg in args)
			{
				if (arg == null) continue;
				if (op == null)
				{
					if (IsArgName(arg))
					{
						op = arg;
					}
					continue;
				}
				if (IsArgName(arg))
				{
					yield return GetImpl(op, null);
					op = arg;
					continue;
				}
				else
				{
					yield return GetImpl(op, arg);
					op = null;
				}
			}
			if (op != null)
			{
				yield return GetImpl(op, null);
			}
		}

		ArgEntry GetImpl(string arg, string val)
		{
			if (!IsArgName(arg)) return null;
			if (arg.StartsWith("--"))
			{
				return new ArgEntry
				{
					LongName = arg.Substring(2),
					Value = val
				};
			}
			else if (arg[0] == '-' && arg.Length == 2)
			{
				return new ArgEntry
				{
					ShortName = arg[1],
					Value = val
				};
			}
			return null;
		}
	}

}
