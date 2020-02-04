﻿using NUnit.Framework;
using System.Linq;
using Unity.ProjectAuditor.Editor;

namespace UnityEditor.ProjectAuditor.EditorTests
{
	class ProjectReportTests
	{
		private ScriptResource m_ScriptResource;

		[OneTimeSetUp]
		public void SetUp()
		{
			m_ScriptResource = new ScriptResource("MyClass.cs", @"
using UnityEngine;
class MyClass
{
	void Dummy()
	{
		// Accessing Camera.main property is not recommended and will be reported as a possible performance problem.
		Debug.Log(Camera.main.name);
	}
}
");
		}

		[OneTimeTearDown]
		public void TearDown()
		{
			m_ScriptResource.Delete();
		}
		
		[Test]
		public void NewReportIsValid()
		{
			var projectReport = new ProjectReport();
			Assert.Zero( projectReport.NumTotalIssues);
			Assert.Zero( projectReport.GetNumIssues(IssueCategory.ApiCalls));
			Assert.Zero( projectReport.GetNumIssues(IssueCategory.ProjectSettings));
		}

		[Test]
		public void IssueIsAddedToReport()
		{
			var projectReport = new ProjectReport();
			
			projectReport.AddIssue(new ProjectIssue
			(
				new ProblemDescriptor{},
				"dummy issue",
				IssueCategory.ApiCalls								
			));
			
			Assert.AreEqual(1, projectReport.NumTotalIssues);
			Assert.AreEqual(1, projectReport.GetNumIssues(IssueCategory.ApiCalls));
			Assert.AreEqual(0, projectReport.GetNumIssues(IssueCategory.ProjectSettings));
		}

		[Test]
		public void ReportIsExportedAndFormatted()
		{
			var projectAuditor = new Unity.ProjectAuditor.Editor.ProjectAuditor();

			var projectReport = projectAuditor.Audit();
		
			const string path = "ProjectAuditor_Report.csv";
			projectReport.Export(path);
			Assert.True(System.IO.File.Exists(path));
			
			var scriptIssue = projectReport.GetIssues(IssueCategory.ApiCalls).Where(i => i.relativePath.Equals(m_ScriptResource.relativePath)).First();

			using (var file = new System.IO.StreamReader(path))
			{
				var line = file.ReadLine();
				Assert.True(line.Equals("Issue,Message,Area,Path"));

				var expectedLine =
					$"{scriptIssue.descriptor.description},{scriptIssue.description},{scriptIssue.descriptor.area},{scriptIssue.relativePath}:{scriptIssue.line}"; 
				line = file.ReadLine();
				Assert.True(line.Equals(expectedLine));
			}			
		}
	}	
}
