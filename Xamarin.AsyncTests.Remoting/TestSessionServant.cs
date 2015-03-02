﻿//
// TestSessionServant.cs
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
using System.Xml.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Xamarin.AsyncTests.Remoting
{
	using Portable;
	using Framework;

	class TestSessionServant : ObjectServant, RemoteTestSession
	{
		public TestSession LocalSession {
			get;
			private set;
		}

		public override string Type {
			get { return "TestSession"; }
		}

		public TestContext RootContext {
			get;
			private set;
		}

		public TestSessionServant (ServerConnection connection)
			: base (connection)
		{
			RootContext = new TestContext (
				connection.App.PortableSupport, connection.EventSink.LoggerClient,
				connection.Framework.Configuration.AsReadOnly (),
				new TestName (connection.Framework.Name), null);

			LocalSession = TestSession.CreateLocal (connection.App, connection.Framework, RootContext);
		}

		TestSessionClient RemoteObject<TestSessionClient, TestSessionServant>.Client {
			get { throw new ServerErrorException (); }
		}

		TestSessionServant RemoteObject<TestSessionClient, TestSessionServant>.Servant {
			get { return this; }
		}

		public Task<TestCase> GetRootTestCase (CancellationToken cancellationToken)
		{
			return LocalSession.GetRootTestCase (cancellationToken);
		}

		public Task<TestCase> ResolveFromPath (XElement path, CancellationToken cancellationToken)
		{
			return LocalSession.ResolveFromPath (path, cancellationToken);
		}

		public XElement GetConfiguration ()
		{
			return TestSerializer.WriteConfiguration (LocalSession.ConfigurationProvider);
			
		}
	}
}

