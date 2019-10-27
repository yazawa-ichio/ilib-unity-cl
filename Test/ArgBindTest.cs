using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Assert = UnityEngine.Assertions.Assert;
using ILib;
using ILib.CL;
using UnityEngine.TestTools;
using System;
using NUnit.Framework;
using System.Linq;

public class ArgBindTest
{
	public enum EnumTest
	{
		Test1,
		Test2,
		Test3,
		Test4,
	}

	public class TestConfig
	{
		[Arg('t', "test")]
		public string Test;
		[Arg('e', "enum")]
		public EnumTest EnumTest;
		[Arg("num")]
		public int Num;
		[Arg('v')]
		public float Value;
	}

	public class TestRequiredConfig
	{
		[Arg('t', "test", Required = true)]
		public string Test;
	}

	IEnumerable<string> GetArgs()
	{
		yield return "-t";
		yield return "testtest";
		//列挙型
		yield return "--enum";
		yield return EnumTest.Test3.ToString();
		yield return "--num";
		yield return "5555";

		yield return "dummy";

		//後の方が優先される
		yield return "--num";
		yield return "4444";
		yield return "--test";
		yield return "testbbbb";

		//利用できない引数
		yield return "--num";
		yield return "-v";
	}

	[Test]
	public void Test1()
	{
		CLParser parser = new CLParser();
		var config = parser.Run<TestConfig>(GetArgs().ToArray());
		Assert.AreEqual("testbbbb", config.Test);
		Assert.AreEqual(EnumTest.Test3, config.EnumTest);
		Assert.AreEqual(4444, config.Num);
		Assert.AreEqual(0, config.Value);
	}

	[Test]
	public void Test2()
	{
		CLParser parser = new CLParser();
		bool success = false;
		Exception error = null;
		try
		{
			var config = parser.Run<TestRequiredConfig>(GetArgs().ToArray());
			success = true;
			config = parser.Run<TestRequiredConfig>(new string[] { });
		}
		catch (Exception ex)
		{
			error = ex;
		}
		Assert.IsTrue(success);
		Assert.IsNotNull(error);
	}

}
