﻿//
// IsEqualBlob.cs
//
// Author:
//       Martin Baulig <martin.baulig@xamarin.com>
//
// Copyright (c) 2015 Xamarin, Inc.
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

namespace Xamarin.AsyncTests.Constraints
{
	public class IsEqualBlob : Constraint
	{
		public byte[] Expected {
			get;
			private set;
		}

		public IsEqualBlob (byte[] expected)
		{
			Expected = expected;
		}
		
		public override bool Evaluate (object actual, out string message)
		{
			var buffer = actual as byte[];
			if (buffer != null)
				return CompareBuffer (buffer, out message);

			if (actual == null) {
				message = string.Format ("Expected blob, got null.");
				return false;
			}

			message = string.Format ("Expected blob, got instance of type `{0}'.", actual.GetType ());
			return false;
		}

		public override string Print ()
		{
			return string.Format ("IsEqualBlob({0} bytes)", Expected.Length);
		}

		bool CompareBuffer (byte[] actual, out string message)
		{
			if (Expected.Length != actual.Length) {
				message = string.Format (
					"Blobs differ in size: expected {0}, got {1}.", Expected.Length, actual.Length);
				return false;
			}

			for (int i = 0; i < Expected.Length; i++) {
				var e = Expected [i];
				var a = actual [i];
				if (e == a)
					continue;

				message = string.Format (
					"Blobs differ at element {0}: expected {1:2x}, got {2:2x}.", i, e, a);
				return false;
			}

			message = null;
			return true;
		}
	}
}

