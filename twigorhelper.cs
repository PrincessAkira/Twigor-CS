﻿using System;
using System.Collections.Generic;
using System.Management;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace Twigor;

public class twigorhelper
{
	public delegate void ShootyShootyBangBang(string response);

	// write to registry but check node first where to put it
	public static void WriteToReg(string hive, string key, string path, string name, string value, string type,
		string ignore, string folder, ShootyShootyBangBang callback)
	{
		try
		{
			// C# 8 switch expression
			var node = hive switch
			{
				"HKEY_CURRENT_USER" => Registry.CurrentUser,
				"HKEY_LOCAL_MACHINE" => Registry.LocalMachine,
				"HKEY_CURRENT_CONFIG" => Registry.CurrentConfig,
				"HKEY_CLASSES_ROOT" => Registry.ClassesRoot,
				"HKEY_USERS" => Registry.Users,
				_ => throw new Exception("Invalid hive")
			};
			Console.WriteLine("Writing to registry... in node: " + node);

			// check what reg type it is
			var regtype = type switch
			{
				"REG_DWORD" => RegistryValueKind.DWord,
				"REG_SZ" => RegistryValueKind.String,
				"REG_QWORD" => RegistryValueKind.QWord,
				"REG_BINARY" => RegistryValueKind.Binary,
				"REG_MULTI_SZ" => RegistryValueKind.MultiString,
				"REG_EXPAND_SZ" => RegistryValueKind.ExpandString,
				_ => throw new Exception("Invalid type")
			};
			Console.WriteLine("Writing to registry... with type: " + regtype);

			// convert value to type
			object valu = type switch
			{
				"REG_DWORD" => Convert.ToInt32(value),
				"REG_SZ" => value,
				"REG_QWORD" => Convert.ToInt64(value),
				"REG_BINARY" => Convert.FromBase64String(value),
				"REG_MULTI_SZ" => value.Split(','),
				"REG_EXPAND_SZ" => value,
				_ => throw new Exception("Invalid type")
			};
			Console.WriteLine("ICH BIN FUCKING COOL: " + valu);

			// write to registry
			// this works btw. I tested it.
			// https://i.imgur.com/lyxtUgM.png

			// get path from path
			var reg = node.OpenSubKey(path, true);
			// create key
			reg?.CreateSubKey(key);
			// open key
			var reg2 = node.OpenSubKey(path + "\\" + key, true);

			reg2?.SetValue(name, valu, regtype);
			// call success callback
			callback("Wrote to registry: " + key + " in " + hive + " with name " + node);
		} catch (Exception e)
		{
			// call error callback
			callback("Error: " + e.Message);
		}
	}

	public static List<RegTweaks>? ReadJson(string fileName)
	{
		using (var r = new StreamReader(fileName))
		{
			var tw = r.ReadToEnd();
			return JsonConvert.DeserializeObject<List<RegTweaks>>(tw);
		}
	}
	
	[DllImport("Srclient.dll")]
	public static extern int SRRemoveRestorePoint(int index);
	
	public static bool CreateRestorePoint(string RPName, int RPType, int EventType)
	{

		// create restore point in windows
		ManagementClass SRClass = new ManagementClass("//./root/default:SystemRestore");

		ManagementBaseObject SRArgs = SRClass.GetMethodParameters("CreateRestorePoint");
		SRArgs["Description"] = RPName;
		SRArgs["RestorePointType"] = RPType;
		SRArgs["EventType"] = EventType;

		try
		{
			ManagementBaseObject outParams = SRClass.InvokeMethod("CreateRestorePoint", SRArgs, new InvokeMethodOptions(null, System.TimeSpan.MaxValue));
			return true;
		}
		catch (ManagementException err)
		{
			Console.WriteLine(err);
			return false;
		}

	}
	
	
	// convert JSON to Indented String
	public static string JsonToIndentedString(string jsonpath)
	{
		if (string.IsNullOrEmpty(jsonpath)) return string.Empty;

		var obj = JsonConvert.DeserializeObject(jsonpath);
		return JsonConvert.SerializeObject(obj, Formatting.Indented);
	}
}