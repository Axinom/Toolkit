
# Axinom Toolkit

This is a pile of fairly arbitrary helper functionality that has proven useful on many different projects and was thus made reusable. The most important and useful features are described here separately, with the rest being in the API documentation.

# Compatibility

Axinom Toolkit is supported on:

* .NET Framework 4.6
* Universal Windows Platform (minimum build 10240)

The set of functionality available on each may be slightly different, as determined by platform capabilities.

# Installation

Install the **Axinom.Toolkit** NuGet package from the Axinom cloud NuGet repository. This contains all the core functionality.

There also exists a separate **Axinom.Toolkit.NLog** NuGet package that contains the functionaltiy to integrate NLog with the Axinom Toolkit logging channels.

There also exists a separate **Axinom.Toolkit.Jose** NuGet package that contains JOSE protected messaging functionality. It is a separate package due to a dependency on a 3rd party library.

# Helper method usage

For boring technical reasons, you must access helper methods using the structure `Helpers.Argument.ValidateIsNotNull()`, always starting from the `Helpers` object. In code and documentation, these helpers are actually structured differently, for example as `CoreHelpers.ValidateIsNotNull()`.

This does not apply to stand-alone classes that are not merely helper methods.

# Logging channels

Axinom Toolkit uses and provides its own logging channels, which can be easily redirected into other standard logging systems such as the .NET Framework tracing channel or libraries such as `NLog`.

Here is a highly abbreviated usage reference:

```csharp
// Register a listener that writes log entries to the tracing channel.
Log.Default.RegisterListener(new TraceLogListener());

// Register a listener that writes log entries to file.
Log.Default.RegisterListener(new StreamWriterLogListener(new StreamWriter(File.Create("Log.log"))));

// Register a listener that writes all log entries to an NLog LogFactory.
// The Axinom Toolkit log entry source name is translated to the NLog logger name.
// Obviously, you also need to configure NLog to do something with the entries in real world code!
Log.Default.RegisterListener(new NLogListener(new LogFactory()))

// Write a log entry.
Log.Default.Info("Hello worolor.")

// Create a child log event source to categorize log entries.
var log = Log.Default.CreateChildSource("Example");
log.Warning("This is a log entry logged under the Example source.")

// Clean up and flush all listsners at application shutdown.
// Mandatory! Without this, some log entries near the end may go missing, depending on the listeners you use!
Log.Default.Dispose();
```

# Error reports

To easily log an exception and capture basic information about the operating environment, use the `ErrorReport` class.

```csharp
// Basic usage, logs the error to the Axinom Toolkit logging channels under the "ErrorReport" source.
ErrorReport.Log(new Exception("just an example"));

// You can also add any custom data you wish to error reports.
ErrorReport.Log(new Exception("another example"), new
{
	CustomData = "my name is",
	Something = new[] { 1, 2, 3, 4 }
});
```

# Web.config configuration section loading

You often want to have XML configuration files. The .NET Framework config file system is somewhat difficult to work with, so Axinom Toolkit provides some assistance for you, enabling you to load configuration sections using `XmlSerializer` and ensure they are always validated on load.

First, create a class to represent your configuration:

```csharp
[XmlRoot("CheckDashboard")]
public sealed class CheckDashboardConfiguration : IValidatable
{
	// Default values, if not provided in web.config.
	internal const int DefaultMaxConcurrentChecks = 100;

	[XmlElement]
	public string DashboardTitle { get; set; }

	[XmlElement]
	public int MaxConcurrentChecks { get; set; }

	#region Singleton
	internal static CheckDashboardConfiguration Current
	{
		get
		{
			var section = ConfigurationManager.GetSection("CheckDashboard") as CheckDashboardConfiguration;

			if (section == null)
				throw new ConfigurationErrorsException("CheckDashboard configuration section is missing or invalid.");

			return section;
		}
	}
	#endregion

	public CheckDashboardConfiguration()
	{
		// Initialize defaults here.
		MaxConcurrentChecks = DefaultMaxConcurrentChecks;
	}

	// Always called on load.
	void IValidatable.Validate()
	{
		if (string.IsNullOrWhiteSpace(DashboardTitle))
			this.ThrowValidationFailureFor(nameof(DashboardTitle), "No dashboard title has been set in the check dashboard configuration.");

		if (MaxConcurrentChecks <= 0)
			this.ThrowValidationFailureFor(nameof(MaxConcurrentChecks), "Must be positive.");
	}
}
```

Then write the equivalent in the web.config file:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
	<configSections>
		<section name="CheckDashboard" type="Axinom.Toolkit.DynamicConfigurationSection, Axinom.Toolkit.DotNet" />
	</configSections>

	<CheckDashboard type="Axinom.Monitoring.CheckDashboard.CheckDashboardConfiguration, Axinom.Monitoring.CheckDashboard">
		<DashboardTitle>Sample Dashboard</DashboardTitle>
		<MaxConcurrentChecks>3</MaxConcurrentChecks>
	</CheckDashboard>
</configuration>
```

Naturally, this also works for app.config, in addition to web.config.

# Object dumping

To transform any object to a human-readable string representation, use `Helpers.Debug.ToDebugString(x)`.

# Protected message exchange

Axinom Toolkit provides helper methods for wrapping data in encrypted and signed envelopes, either in XML or JOSE format. The envelope is encrypted for a specific recipient (identified by X.509 certificate) and signed by a specific author (also identified by X.509 certificate). You can use this for general-purpose secure data exchange.

Secure messaging is provided for .NET Framework only. The JOSE messaging support is in the Axinom.Toolkit.Jose NuGet package, as it depends on an external library for the JOSE serialization support.

XML messaging example code:

```csharp
XmlDocument document = LoadDocument();

// The document is modified in-place.
Helpers.ProtectedXml.EncryptAndSign(document, recipientCertificate, signerCertificateWithPrivateKey);

X509Certificate2 signedBy;

// The document is modified in-place.
Helpers.ProtectedXml.VerifyAndDecrypt(document, out signedBy, recipientCertificateWithPrivateKey);
```

JOSE messaging example code:

```csharp
byte[] data = LoadData();

var envelope = Helpers.Jose.EncryptAndSign(data, recipientCertificate, signerCertificateWithPrivateKey);

X509Certificate2 signedBy;
var decryptedData = Helpers.Jose.VerifyAndDecrypt(envelope, out signedBy, recipientCertificateWithPrivateKey);
```

The envelope format is custom, as no widely usable general purpose message format exists. The specific algorithms and cryptographic configuration are hardcoded but may change in future versions of Axinom Toolkit, though backward compatibility is guaranteed for read operations.

# API reference

Refer to the [.NET API reference](api/index.md).