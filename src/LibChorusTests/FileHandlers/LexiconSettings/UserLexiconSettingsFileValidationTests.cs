﻿using System;
using System.IO;
using System.Linq;
using Chorus.FileTypeHandlers;
using NUnit.Framework;
using SIL.IO;

namespace LibChorus.Tests.FileHandlers.LexiconSettings
{
	/// <summary>
	/// Make sure only a user lexicon settings xml file can be validated by the UserLexiconSettingsFileHandler class.
	/// </summary>
	[TestFixture]
	public class UserLexiconSettingsFileValidationTests
	{
		private IChorusFileTypeHandler _handler;
		private TempFile _goodXmlTempFile;
		private TempFile _illformedXmlTempFile;
		private TempFile _goodXmlButNotUserLexiconSettingsTempFile;
		private TempFile _nonXmlTempFile;

		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			_handler = (ChorusFileTypeHandlerCollection.CreateWithInstalledHandlers().Handlers.Where(
				handler => handler.GetType().Name == "UserLexiconSettingsFileHandler")).First();

			_goodXmlTempFile = TempFile.WithExtension(".ulsx");
#if MONO
			File.WriteAllText(_goodXmlTempFile.Path, "<?xml version='1.0' encoding='utf-8'?>" + Environment.NewLine + "<UserLexiconSettings>" + Environment.NewLine + "</UserLexiconSettings>");
#else
			File.WriteAllText(_goodXmlTempFile.Path, "<?xml version='1.0' encoding='utf-8'?>" + Environment.NewLine + "<UserLexiconSettings />");
#endif
			_illformedXmlTempFile = TempFile.WithExtension(".ulsx");
			File.WriteAllText(_illformedXmlTempFile.Path, "<?xml version='1.0' encoding='utf-8'?>" + Environment.NewLine + "<UserLexiconSettings>");

			_goodXmlButNotUserLexiconSettingsTempFile = TempFile.WithExtension(".ulsx");
			File.WriteAllText(_goodXmlButNotUserLexiconSettingsTempFile.Path, "<?xml version='1.0' encoding='utf-8'?>" + Environment.NewLine + "<nonUserLexiconSettingsstuff />");

			_nonXmlTempFile = TempFile.WithExtension(".txt");
			File.WriteAllText(_nonXmlTempFile.Path, "This is not a user lexicon settings file." + Environment.NewLine);
		}

		[TestFixtureTearDown]
		public void FixtureTearDown()
		{
			_handler = null;
			_goodXmlTempFile.Dispose();
			_goodXmlTempFile = null;

			_illformedXmlTempFile.Dispose();
			_illformedXmlTempFile = null;

			_goodXmlButNotUserLexiconSettingsTempFile.Dispose();
			_goodXmlButNotUserLexiconSettingsTempFile = null;

			_nonXmlTempFile.Dispose();
			_nonXmlTempFile = null;
		}

		[Test]
		public void Cannot_Validate_Nonexistant_File()
		{
			Assert.IsFalse(_handler.CanValidateFile("bogusPathname"));
		}

		[Test]
		public void Cannot_Validate_Null_File()
		{
			Assert.IsFalse(_handler.CanValidateFile(null));
		}

		[Test]
		public void Cannot_Validate_Empty_String_File()
		{
			Assert.IsFalse(_handler.CanValidateFile(String.Empty));
		}

		[Test]
		public void Cannot_Validate_Nonxml_File()
		{
			Assert.IsFalse(_handler.CanValidateFile(_nonXmlTempFile.Path));
		}

		[Test]
		public void Can_Validate_Fw_Xml_File()
		{
			Assert.IsTrue(_handler.CanValidateFile(_goodXmlTempFile.Path));
		}

		[Test]
		public void ValidateFile_Returns_Message_For_Empty_Pathname()
		{
			Assert.IsNotNull(_handler.ValidateFile("", null));
		}

		[Test]
		public void ValidateFile_Returns_Message_For_Null_Pathname()
		{
			Assert.IsNotNull(_handler.ValidateFile(null, null));
		}

		[Test]
		public void ValidateFile_Returns_Null_For_Good_File()
		{
			Assert.IsNull(_handler.ValidateFile(_goodXmlTempFile.Path, null));
		}

		[Test]
		public void ValidateFile_Returns_Message_For_Crummy_ProjectLexiconSettings_File()
		{
			Assert.IsNotNull(_handler.ValidateFile(_illformedXmlTempFile.Path, null));
		}

		[Test]
		public void ValidateFile_Returns_Message_For_Good_But_Not_ProjectLexiconSettings_File()
		{
			Assert.IsNotNull(_handler.ValidateFile(_goodXmlButNotUserLexiconSettingsTempFile.Path, null));
		}
	}
}
