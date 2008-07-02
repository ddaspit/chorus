using System.Collections.Generic;

namespace Chorus.VcsDrivers.Mercurial
{
	public class RevisionDescriptor
	{
		public string UserId;
		public string _revision;
		public string _hash;
		public string Summary;
		public string _tag;
		public string DateString;

		public RevisionDescriptor()
		 {
		 }

		public RevisionDescriptor(string name, string revision, string hash, string comment)
		{
			UserId = name;
			_revision = revision;
			_hash = hash;
			Summary = comment;
			_tag = "";
		}

		public static List<RevisionDescriptor>  GetRevisionsFromQueryOutput(string result)
		{
			//Debug.WriteLine(result);
			string[] lines = result.Split('\n');
			List<Dictionary<string, string>> rawChangeSets = new List<Dictionary<string, string>>();
			Dictionary<string, string> rawChangeSet = null;
			foreach (string line in lines)
			{
				if (line.StartsWith("changeset:"))
				{
					rawChangeSet = new Dictionary<string, string>();
					rawChangeSets.Add(rawChangeSet);
				}
				string[] parts = line.Split(new char[] { ':' });
				if (parts.Length < 2)
					continue;
				//join all but the first back together
				string contents = string.Join(":", parts, 1, parts.Length-1);
				rawChangeSet[parts[0].Trim()] = contents.Trim();
			}

			List<RevisionDescriptor> revisions = new List<RevisionDescriptor>();
			foreach (Dictionary<string, string> d in rawChangeSets)
			{
				string[] revisionParts = d["changeset"].Split(':');
				RevisionDescriptor revision = new RevisionDescriptor(d["user"], revisionParts[0], /*revisionParts[1]*/"unknown", d["summary"]);
				if(d.ContainsKey("tag"))
				{
					revision._tag = d["tag"];
				}
				revisions.Add(revision);

			}
			return revisions;
		}

		public bool IsMatchingStub(RevisionDescriptor stub)
		{
			return stub.Summary.Contains(string.Format("({0} partial from", UserId));
		}
	}
}