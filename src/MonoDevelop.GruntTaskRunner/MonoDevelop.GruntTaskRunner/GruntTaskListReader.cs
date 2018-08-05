//
// GruntTaskListReader.cs
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
using System.Linq;
using Newtonsoft.Json;
using MonoDevelop.Core;

namespace MonoDevelop.GruntTaskRunner
{
	/// <summary>
	/// Output format:
	/// 
	/// Running "md-grunt-task-list" task
	/// {"default":{"name":"default","info":"Custom task.","meta":....}}
	/// 
	///  Done, without errors.
	/// </summary>
	static class GruntTaskListReader
	{
		public static IEnumerable<GruntTaskInformation> ReadTasks (string output)
		{
			int start = output.IndexOf ('{');
			int end = output.LastIndexOf ('}');

			if ((start < 0) || (end < 0)) {
				return Enumerable.Empty<GruntTaskInformation> ();
			}

			try {
				string json = output.Substring (start, end - start + 1);
				return ReadTasksFromJson (json);
			} catch (Exception ex) {
				LoggingService.LogError (string.Format ("Unable to read grunt tasks {0}", output), ex);
				return Enumerable.Empty<GruntTaskInformation> ();
			}
		}

		/// <summary>
		/// {
		///   "uglify": {
		///     "name": "uglify",
		///     "info": "Minify files with UglifyJS.",
		///     "meta": {
		///       "info": "\"grunt-contrib-uglify\" local Npm module",
		///       "filepath": "~/Projects/Tests/grunttest1234/node_modules/grunt-contrib-uglify/tasks/uglify.js"
		///     },
		///     "multi": true
		///   },
		///   "default": {
		///     "name": "default",
		///     "info": "Custom task.",
		///     "meta": {
		///       "info": "\"../grunttest\"",
		///       "filepath": "../grunttest/Gruntfile.js"
		///     }
		///   },
		///   "md-grunt-task-list": {
		///     "name": "md-grunt-task-list",
		///     "info": "Lists tasks",
		///     "meta": {
		///       "info": "\"../grunttest\"",
		///       "filepath": "../grunttest/GruntTaskList.js"
		///     }
		///   }
		/// }
		/// </summary>
		static IEnumerable<GruntTaskInformation> ReadTasksFromJson (string json)
		{
			var dictionary = JsonConvert.DeserializeObject<Dictionary<string, GruntTaskInformation>> (json);
			return dictionary.Values.Where (taskInfo => taskInfo.Name != GruntTaskListScript.TaskName);
		}
	}
}
