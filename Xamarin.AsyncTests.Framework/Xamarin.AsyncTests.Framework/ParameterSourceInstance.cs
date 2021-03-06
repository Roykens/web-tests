﻿//
// ParameterSourceInstance.cs
//
// Author:
//       Martin Baulig <martin.baulig@xamarin.com>
//
// Copyright (c) 2014 Xamarin Inc. (http://www.xamarin.com)
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
using System.Reflection;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Xamarin.AsyncTests.Framework
{
	class ParameterSourceInstance<T> : ParameterizedTestInstance
	{
		List<T> parameters;
		bool hasNext;
		T current;
		int index;

		new public ParameterSourceHost<T> Host {
			get { return (ParameterSourceHost<T>)base.Host; }
		}

		public ITestParameterSource<T> SourceInstance {
			get;
			private set;
		}

		public string Filter {
			get;
			private set;
		}

		public override object Current {
			get { return current; }
		}

		public ParameterSourceInstance (
			ParameterSourceHost<T> host, TestPath path, TestInstance parent,
			ITestParameterSource<T> sourceInstance, string filter)
			: base (host, path, parent)
		{
			SourceInstance = sourceInstance;
			Filter = filter;
		}

		public override void Initialize (TestContext ctx)
		{
			base.Initialize (ctx);

			if (Path.Parameter != null) {
				current = Clone (Host.Deserialize (Path.Parameter));
				hasNext = true;
				return;
			}

			parameters = new List<T> (SourceInstance.GetParameters (ctx, Filter));
			index = 0;
		}

		static T Clone (T value)
		{
			var cloneable = value as ICloneable;
			if (cloneable != null)
				value = (T)cloneable.Clone ();
			return value;
		}

		public override void Destroy (TestContext ctx)
		{
			parameters = null;
			current = default(T);
			index = -1;
			base.Destroy (ctx);
		}

		public override bool HasNext ()
		{
			return parameters != null ? index < parameters.Count : hasNext;
		}

		public override bool MoveNext (TestContext ctx)
		{
			if (!HasNext ())
				return false;

			if (parameters != null) {
				current = Clone (parameters [index]);
				index++;
				return true;
			} else {
				if (!hasNext)
					return false;
				hasNext = false;
				return true;
			}
		}
	}
}

