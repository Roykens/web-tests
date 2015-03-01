﻿//
// ParameterSerializer.cs
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
using System.Text;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml;
using System.Xml.Linq;

namespace Xamarin.AsyncTests.Remoting
{
	using Framework;

	static class Serializer
	{
		public static readonly IValueSerializer<string> String = new StringSerializer ();
		public static readonly IValueSerializer<XElement> Element = new ElementSerializer ();
		public static readonly IValueSerializer<TestLoggerBackend.LogEntry> LogEntry = new LogEntrySerializer ();
		public static readonly IValueSerializer<TestLoggerBackend.StatisticsEventArgs> StatisticsEventArgs = new StatisticsEventArgsSerializer ();

		public static XElement Write (object instance)
		{
			if (instance is string)
				return String.Write ((string)instance);
			else if (instance is XElement)
				return Element.Write ((XElement)instance);
			else if (instance is TestName)
				return TestSerializer.WriteTestName ((TestName)instance);
			else if (instance is TestResult)
				return TestSerializer.WriteTestResult ((TestResult)instance);
			else if (instance is TestLoggerBackend.LogEntry)
				return LogEntry.Write ((TestLoggerBackend.LogEntry)instance);
			else if (instance is TestLoggerBackend.StatisticsEventArgs)
				return StatisticsEventArgs.Write ((TestLoggerBackend.StatisticsEventArgs)instance);
			else if (instance is ObjectProxy)
				return RemoteObjectManager.WriteProxy ((ObjectProxy)instance);
			else
				throw new ServerErrorException ();
		}

		public static T Read<T> (XElement node)
		{
			if (typeof(T) == typeof(string))
				return ((IValueSerializer<T>)String).Read (node);
			else if (typeof(T) == typeof(XElement))
				return ((IValueSerializer<T>)Element).Read (node);
			else if (typeof(T) == typeof(TestName))
				return (T)(object)TestSerializer.ReadTestName (node);
			else if (typeof(T) == typeof(TestResult))
				return (T)(object)TestSerializer.ReadTestResult (node);
			else if (typeof(T) == typeof(TestLoggerBackend.LogEntry))
				return ((IValueSerializer<T>)LogEntry).Read (node);
			else if (typeof(T) == typeof(TestLoggerBackend.StatisticsEventArgs))
				return ((IValueSerializer<T>)StatisticsEventArgs).Read (node);
			else
				throw new ServerErrorException ();
		}

		class StringSerializer : IValueSerializer<string>
		{
			public string Read (XElement node)
			{
				if (!node.Name.LocalName.Equals ("Text"))
					throw new ServerErrorException ();

				return node.Attribute ("Value").Value;
			}

			public XElement Write (string instance)
			{
				var element = new XElement ("Text");
				element.SetAttributeValue ("Value", instance);
				return element;
			}
		}

		class ElementSerializer : IValueSerializer<XElement>
		{
			public XElement Read (XElement node)
			{
				if (!node.Name.LocalName.Equals ("Element"))
					throw new ServerErrorException ();

				return (XElement)node.FirstNode;
			}

			public XElement Write (XElement instance)
			{
				return new XElement ("Element", instance);
			}
		}

		class StatisticsEventArgsSerializer : IValueSerializer<TestLoggerBackend.StatisticsEventArgs>
		{
			public TestLoggerBackend.StatisticsEventArgs Read (XElement node)
			{
				if (!node.Name.LocalName.Equals ("TestStatisticsEventArgs"))
					throw new ServerErrorException ();

				var instance = new TestLoggerBackend.StatisticsEventArgs ();
				instance.Type = (TestLoggerBackend.StatisticsEventType)Enum.Parse (typeof(TestLoggerBackend.StatisticsEventType), node.Attribute ("Type").Value);
				instance.Status = (TestStatus)Enum.Parse (typeof(TestStatus), node.Attribute ("Status").Value);
				instance.IsRemote = true;

				var name = node.Element ("TestName");
				if (name != null)
					instance.Name = TestSerializer.ReadTestName (name);

				return instance;
			}

			public XElement Write (TestLoggerBackend.StatisticsEventArgs instance)
			{
				if (instance.IsRemote)
					throw new ServerErrorException ();

				var element = new XElement ("TestStatisticsEventArgs");
				element.SetAttributeValue ("Type", instance.Type.ToString ());
				element.SetAttributeValue ("Status", instance.Status.ToString ());

				if (instance.Name != null)
					element.Add (TestSerializer.WriteTestName (instance.Name));

				return element;
			}
		}

		class LogEntrySerializer : IValueSerializer<TestLoggerBackend.LogEntry>
		{
			public TestLoggerBackend.LogEntry Read (XElement node)
			{
				if (!node.Name.LocalName.Equals ("LogEntry"))
					throw new ServerErrorException ();

				TestLoggerBackend.EntryKind kind;
				if (!Enum.TryParse (node.Attribute ("Kind").Value, out kind))
					throw new ServerErrorException ();

				var text = node.Attribute ("Text").Value;
				var logLevel = int.Parse (node.Attribute ("LogLevel").Value);

				Exception exception = null;
				var error = node.Element ("Error");
				if (error != null) {
					var errorMessage = error.Attribute ("Text").Value;
					var stackTrace = error.Attribute ("StackTrace");
					exception = new SavedException (errorMessage, stackTrace != null ? stackTrace.Value : null);
				}

				return new TestLoggerBackend.LogEntry (kind, logLevel, text, exception);
			}

			public XElement Write (TestLoggerBackend.LogEntry instance)
			{
				var element = new XElement ("LogEntry");
				element.SetAttributeValue ("Kind", instance.Kind);
				element.SetAttributeValue ("LogLevel", instance.LogLevel);
				element.SetAttributeValue ("Text", instance.Text);

				if (instance.Error != null) {
					var error = new XElement ("Error");
					error.SetAttributeValue ("Text", instance.Error.Message);
					error.SetAttributeValue ("StackTrace", instance.Error.StackTrace);
					element.Add (error);
				}

				return element;
			}
		}
	}

	interface IValueSerializer<T>
	{
		T Read (XElement node);

		XElement Write (T instance);
	}
}

