using System;

namespace ILib.CL
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Class , AllowMultiple = true,Inherited =true)]
	public class ArgAttribute : Attribute
	{
		public string LongName { get; private set; }

		public char ShortName { get; private set; }

		public bool Required { get; set; }

		public ArgAttribute(string longName)
		{
			LongName = longName;
		}

		public ArgAttribute(char shortName)
		{
			ShortName = shortName;
		}

		public ArgAttribute(char shortName, string longName)
		{
			LongName = longName;
			ShortName = shortName;
		}

	}

}
