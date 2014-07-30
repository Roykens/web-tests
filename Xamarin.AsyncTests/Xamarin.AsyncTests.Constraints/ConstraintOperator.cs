﻿//
// ConstraintExpression.cs
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

namespace Xamarin.AsyncTests.Constraints
{
	public abstract class ConstraintOperator
	{
		public abstract string Name {
			get;
		}

		public delegate bool OperatorFunc (object actual, out string message);

		public delegate bool OperatorDelegate (OperatorFunc func, object actual, out string message);

		public abstract bool Evaluate (OperatorFunc func, object actual, out string message);

		public Constraint False {
			get { return new ConstraintExpression (this, new FalseConstraint ()); }
		}

		public Constraint True {
			get { return new ConstraintExpression (this, new TrueConstraint ()); }
		}

		public Constraint Null {
			get { return new ConstraintExpression (this, new NullConstraint ()); }
		}

		public Constraint Empty {
			get { return new ConstraintExpression (this, new EmptyConstraint ()); }
		}

		public Constraint NullOrEmpty {
			get { return new ConstraintExpression (this, new NullOrEmptyConstraint ()); }
		}
	}
}

