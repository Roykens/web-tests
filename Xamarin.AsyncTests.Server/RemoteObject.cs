﻿//
// RemoteObject.cs
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

namespace Xamarin.AsyncTests.Server
{
	abstract class RemoteObject<T,U>
	{
		public abstract class Proxy
		{
			public abstract Connection Connection {
				get;
			}

			public abstract long ObjectID {
				get;
			}
		}

		public sealed class ClientProxy : Proxy
		{
			readonly Connection connection;
			readonly long objectID;

			public override Connection Connection {
				get { return connection; }
			}

			public override long ObjectID {
				get { return objectID; }
			}

			public T Instance {
				get;
				private set;
			}

			public ClientProxy (Connection connection, RemoteObject<T,U> parent, long objectID)
			{
				this.connection = connection;
				this.objectID = objectID;

				Instance = parent.CreateClientProxy (this);
			}
		}

		public sealed class ServerProxy : Proxy
		{
			readonly Connection connection;
			readonly long objectID;

			public override Connection Connection {
				get { return connection; }
			}

			public override long ObjectID {
				get { return objectID; }
			}

			public U Instance {
				get;
				private set;
			}

			public ServerProxy (Connection connection, RemoteObject<T,U> parent)
			{
				this.connection = connection;
				objectID = connection.RegisterRemoteObject (this);
				Instance = parent.CreateServerProxy (connection);
			}
		}

		protected abstract T CreateClientProxy (ClientProxy proxy);

		protected abstract U CreateServerProxy (Connection proxy);
	}
}

