//
// GruntTaskRunnerConfig.cs
//
// Author:
//       Matt Ward <matt.ward@microsoft.com>
//
// Copyright (c) 2018 Microsoft
//
// Based on: https://github.com/madskristensen/TaskRunnerTemplate/blob/master/src/TaskRunnerExtension/TaskRunner/TaskRunnerConfig.cs
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
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TaskRunnerExplorer;
using MonoDevelop.Core;

namespace MonoDevelop.GruntTaskRunner
{
	class GruntTaskRunnerConfig : ITaskRunnerConfig
	{
		public IconId Icon => null;

		public ITaskRunnerNode TaskHierarchy { get; }

		public GruntTaskRunnerConfig (ITaskRunnerNode hierarchy)
		{
			TaskHierarchy = hierarchy;
		}

		public void Dispose ()
		{
		}

		public string LoadBindings (string configPath)
		{
			string bindingPath = configPath + ".bindings";

			if (File.Exists (bindingPath)) {
				foreach (string line in File.ReadAllLines (bindingPath)) {
					if (line.StartsWith ("///<binding", StringComparison.OrdinalIgnoreCase)) {
						return line.TrimStart ('/').Trim ();
					}
				}
			}

			return "<binding />";
		}

		public bool SaveBindings (string configPath, string bindingsXml)
		{
			string bindingPath = configPath + ".bindings";

			try {
				var sb = new StringBuilder ();

				if (File.Exists (bindingPath)) {
					string[] lines = File.ReadAllLines (bindingPath);

					foreach (string line in lines) {
						if (!line.TrimStart ().StartsWith ("///<binding", StringComparison.OrdinalIgnoreCase)) {
							sb.AppendLine (line);
						}
					}
				}

				if (bindingsXml != "<binding />") {
					sb.Insert (0, "///" + bindingsXml);
				}

				if (sb.Length == 0) {
					File.Delete (bindingPath);
				} else {
					File.WriteAllText (bindingPath, sb.ToString (), Encoding.UTF8);
				}

				return true;
			} catch (Exception ex) {
				LoggingService.LogError ("SavingBindings error.", ex);
				return false;
			}
		}
	}
}
