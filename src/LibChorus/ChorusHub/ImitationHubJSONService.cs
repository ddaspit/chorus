﻿using System;
using System.Collections.Generic;
using Chorus.VcsDrivers;

namespace Chorus.ChorusHub
{
	/// <summary>
	/// This class serializes repository name and id into a JSON string or
	/// deserializes such a string into a ChorusHubRepositoryInformation object.
	/// </summary>
	/// <example>{"name": "fooProject", "id": "123abc"}</example>
	public static class ImitationHubJSONService
	{
		private const string format1 = "{\"name\": \"";
		private const string format2 = "\", \"id\": \"";
		private const string format3 = "\"}";

		// Serialize
		public static string MakeJsonString(string name, string id)
		{
			return format1 + name + format2 + id + format3;
		}

		// Deserialize one
		public static RepositoryInformation DechipherJsonString(string jsonString)
		{
			// Probably not the best way to do this, but...
			// If the parse fails for any reason, will throw ArgumentException.
			if (string.IsNullOrEmpty(jsonString) || !jsonString.StartsWith(format1) ||
				!jsonString.EndsWith(format3))
			{
				throw new ArgumentException("JSON object begins or ends with wrong format.");
			}
			var strippedString = jsonString.Substring(10, jsonString.Length - 12); // strip off 'format1' and 'format3'
			var endOfNameIndex = strippedString.IndexOf(format2, StringComparison.CurrentCulture);
			var name = strippedString.Substring(0, endOfNameIndex);
			var begOfIdIndex = endOfNameIndex + 10;
			var id = strippedString.Substring(begOfIdIndex, strippedString.Length - begOfIdIndex);
			return new RepositoryInformation(name, id);
		}

		// Deserialize a bunch
		public static IEnumerable<RepositoryInformation> ParseJsonStringsToChorusHubRepoInfos(string jsonInput)
		{
			var jsonStrings = jsonInput.Split(new [] {'/'}, StringSplitOptions.RemoveEmptyEntries);
			var result = new List<RepositoryInformation>();
			foreach (var jsonString in jsonStrings)
			{
				try
				{
					result.Add(DechipherJsonString(jsonString));
				}
				catch (ArgumentException e)
				{
					continue;
				}
			}
			return result;
		}
	}
}
