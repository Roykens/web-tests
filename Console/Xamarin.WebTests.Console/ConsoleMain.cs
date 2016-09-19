﻿using System;
using Xamarin.AsyncTests;
using Xamarin.AsyncTests.Console;
using Xamarin.WebTests;
using Xamarin.WebTests.MonoTests;
using Xamarin.WebTests.MonoTestFramework;
using Xamarin.WebTests.TestFramework;
using Xamarin.WebTests.TestProvider;

[assembly: AsyncTestSuite (typeof (WebTestFeatures), true)]
[assembly: AsyncTestSuite (typeof (MonoWebTestFeatures), true)]

namespace Xamarin.WebTests.Console
{
	public class ConsoleMain
	{
		static void Main (string[] args)
		{
			DependencyInjector.RegisterAssembly (typeof(ConsoleMain).Assembly);
			DependencyInjector.RegisterAssembly (typeof(WebDependencyProvider).Assembly);
			DependencyInjector.RegisterAssembly (typeof(MonoTestFrameworkDependencyProvider).Assembly);
			DependencyInjector.RegisterDependency<ITestFrameworkSetup> (() => new ConsoleFrameworkSetup ());

			Program.Run (typeof (ConsoleMain).Assembly, args);
		}
	}
}

