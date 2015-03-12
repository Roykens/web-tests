Xamarin.AsyncTests
==================

The new Mac GUI has two main modes of operation: it can either listen for incoming connections or
run tests locally by forking a test runner.

Test Suite and Dependencies
---------------------------

To ease testing on Mobile, a test suite typically lives in a PCL (though it does not have to
be a PCL).  Such PCLs usually require some platform-specific code, which can be used via the
`DependencyInjector.Get<T> ()` API.  You need to provide a platform-specific implementation
and register it via either one or more `DependencyInjector.RegisterDependency<T> (Func<T>)` or
`DependencyInjector.RegisterAssembly(Assembly)`.

These platform-specific implementations need to be loaded into the test runner process at
runtime.  To automate this, a platform-specific implementation assembly may use
`[assembly: AsyncTestSuite (typeof (provider), true)]` and you pass it to the command-line
tool instead of the actual test suite.  The framework then automatically registers all the
dependencies.

For an example, see Xamarin.WebTests.TestProvider.exe.  The main test suite is Xamarin.WebTests.dll,
which is a PCL, so it needs a platform-specific implementation.  Yon can either use

$ mono --debug Xamarin.AsyncTests.Console.exe --dependency=Xamarin.WebTests.TestProvider.exe Xamartin.WebTests.dll

or

$ mono --debug Xamarin.AsyncTests.Console.exe Xamartin.WebTests.Console.dll

Unfortunately, this technique does not work on Mobile, so a custom test app is required for
each test suite on each platform.

Listening for Connections
-------------------------

All you have to do is select Test Session / Listen from the main menu and it will listen
at 0.0.0.0:8888.  FIXME: this endpoint should be configurable.

Then connect from the Xamarin.AsyncTests.Console.exe command-line tool:

$ mono --debug Xamarin.AsyncTests.Console.exe --gui=127.0.0.1:8888 Xamarin.WebTests.TestProvider.exe

This tool understands some additional command-line options:

* --result=FILE
  Dump full test result in XML form into that file.

* --log-level=LEVEL
  Modify local log level.
  
* --optional-gui
  Fall back to running tests locally if GUI connection fails with SocketError.ConnectionRefused.
  
* --settings=FILE
  Load settings from that file.  The GUI will override those.

* --connect=ENDPOINT
  Connect to a remote server.  Cannot be used together with the GUI.  Used for Mobile.
  
* --gui=ENDPOINT
  Connect to the GUI and wait for commands.
  
* --dependency=ASSEMBLY
  Loads the specified assembly as a dependency.  Can be used multiple times.
  
This is the recommended mode of operation for debugging and fixing bugs.  You launch the
GUI from a terminal, then run Xamarin.AsyncTests.Console from within Xamarin Studio.  This
gives you the full Xamarin Studio debugging functionality while still being able to use the
GUI to view test results, select which tests to run, etc.

The external process is required because the TLS tests need to be run with a custom Mono
runtime which has the new TLS changes.  Without an external process, you would have to either
install this custom runtime as the default /Library/Frameworks/Mono.framework or build
Xamarin.Mac for your custom prefix.

The inter-process communication layer only uses Sockets and XML, so it won't interfer with
the testing.

Running from the GUI
--------------------

The tests can also be run directly from the GUI, in which case the GUI will launch the external
Xamarin.WebTests.TestProvider process.

Before you can do that for the first time, you need to open the settings dialog and configure
some values:

* "Mono Runtime"
  The Mono Runtime prefix, for instance `/Workspace/INSTALL`.

* "Launcher Path"
  Full path name of the `Xamarin.AsyncTests.Console.exe` assembly.
  
* "Test Suite"
  Full path name of the platform-specific test suite (for instance `Xamarin.WebTests.TestProvider.exe`).
  
* "Arguments"
  Optional arguments to be passed to the launcher.
  
Running on Mobile
-----------------

TODO


Last changed March 12th, 2015
Martin Baulig <martin.baulig@xamarin.com>

