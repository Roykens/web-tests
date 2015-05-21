﻿using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Diagnostics;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

using Xamarin.AsyncTests;
using Xamarin.AsyncTests.Portable;

namespace Xamarin.WebTests.ConnectionFramework
{
	using Providers;
	using Portable;
	using Server;

	public class DotNetServer : DotNetConnection, IServer
	{
		public IServerCertificate Certificate {
			get { return Parameters.ServerCertificate; }
		}

		new public ServerParameters Parameters {
			get { return (ServerParameters)base.Parameters; }
		}

		readonly ISslStreamProvider sslStreamProvider;

		public DotNetServer (ConnectionProvider provider, ServerParameters parameters, ISslStreamProvider sslStreamProvider)
			: base (provider, parameters)
		{
			this.sslStreamProvider = sslStreamProvider;
		}

		protected override bool IsServer {
			get { return true; }
		}

		protected override async Task<ISslStream> Start (TestContext ctx, Socket socket, CancellationToken cancellationToken)
		{
			ctx.LogMessage ("Accepted connection from {0}.", socket.RemoteEndPoint);

			var stream = new NetworkStream (socket);
			var server = await sslStreamProvider.CreateServerStreamAsync (stream, Parameters, cancellationToken);

			ctx.LogMessage ("Successfully authenticated server.");

			return server;
		}
	}
}

