//
// GruntTaskRunnerProvider.cs
//
// Author:
//       Matt Ward <matt.ward@microsoft.com>
//
// Copyright (c) 2018 Microsoft
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TaskRunnerExplorer;
using MonoDevelop.Core;

namespace MonoDevelop.GruntTaskRunner
{
	[TaskRunnerExport ("Gruntfile.js")]
	class GruntTaskRunnerProvider : ITaskRunner
	{
		public List<ITaskRunnerOption> Options { get; set; }

		public async Task<ITaskRunnerConfig> ParseConfig (ITaskRunnerCommandContext context, string configPath)
		{
			return await Task.Run (async () => {
				ITaskRunnerNode hierarchy = await TryLoadHierarchy (configPath);
				return new GruntTaskRunnerConfig (hierarchy);
			});
		}

		async Task<ITaskRunnerNode> TryLoadHierarchy (string configPath)
		{
			try {
				return await LoadHierarchy (configPath);
			} catch (Exception ex) {
				LogError (configPath, ex);
				return CreateErrorTaskRunnerNode (ex);
			}
		}

		async Task<ITaskRunnerNode> LoadHierarchy (string configPath)
		{
			string workingDirectory = Path.GetDirectoryName (configPath);

			var root = new TaskRunnerNode ("Grunt Task Runner");

			var tasksNode = new TaskRunnerNode (GettextCatalog.GetString ("Tasks", false));
			root.Children.Add (tasksNode);

			foreach (GruntTaskInformation task in await GruntCommandRunner.FindGruntTasks (workingDirectory)) {
				tasksNode.Children.Add (new TaskRunnerNode (task.Name, true) {
					Description = task.Info,
					Command = new TaskRunnerCommand (workingDirectory, "grunt", task.Name + " --no-color")
				});
			}

			return root;
		}

		static void LogError (string configPath, Exception ex)
		{
			LoggingService.LogError ("Load grunt tasks failed. ", ex);

			string logMessage = GettextCatalog.GetString ("Failed to load {0}{1}{2}", configPath, Environment.NewLine, ex.Message);
			TaskRunnerLogger.WriteLine (logMessage);
		}

		ITaskRunnerNode CreateErrorTaskRunnerNode (Exception ex)
		{
			var root = new TaskRunnerErrorNode ("Grunt Task Runner");

			string message = GettextCatalog.GetString ("Failed to load. See Task Runner Explorer Output for more details.");
			var tasksNode = new TaskRunnerErrorNode (message);
			root.Children.Add (tasksNode);

			return root;
		}
	}
}
