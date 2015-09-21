namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.IO.IsolatedStorage;
	using System.Linq;
	using System.Text;
	using System.Xml;

	/// <summary>
	/// Log listener that creates or overwrites a Log.xml file in Isolated Storage where it puts its log entries.
	/// </summary>
	/// <remarks>
	/// If the log file is already in use (e.g. by another instance of the application), the listener
	/// appends an increasing number to the end of the filename until it finds an available file (e.g. Log2.xml).
	/// 
	/// Note that you will encounter serious logging performance issues if you run out of space, due to repeated write failures.
	/// </remarks>
	public sealed class IsolatedStorageXmlLogListener : ILogListener
	{
		private readonly object _lock = new object();

		private IsolatedStorageFile _store;
		private IsolatedStorageFileStream _file;
		private XmlWriter _writer;

		public void OnWrite(LogEntry entry)
		{
			lock (_lock)
			{
				EnsureOpen();

				_writer.WriteStartElement("LogEntry");
				_writer.WriteAttributeString("Level", entry.Severity.ToString());
				_writer.WriteAttributeString("Timestamp", entry.Timestamp.ToString("u"));

				if (entry.Source != null)
					_writer.WriteAttributeString("Source", entry.Source);

				_writer.WriteCData(entry.Message);
				_writer.WriteEndElement();
			}
		}

		#region Log file creation
		private void EnsureOpen()
		{
			if (_store == null)
				_store = IsolatedStorageFile.GetUserStoreForApplication();

			if (_file == null)
			{
				if (TryOpenFile())
					return;

				for (int i = 2; i < 100; i++)
				{
					if (TryOpenFile(i))
						return;
				}

				// WARNING: If this starts failing on every log entry, it will get hella slow!
				throw new EnvironmentException("Unable to create log file in Isolated Storage.");
			}
		}

		private bool TryOpenFile(int? suffix = null)
		{
			string filename = "Log.xml";
			if (suffix != null)
				filename = "Log" + suffix + ".xml";

			try
			{
				_file = _store.OpenFile(filename, FileMode.Create, FileAccess.Write, FileShare.None);

				try
				{
					var settings = new XmlWriterSettings
					{
						Encoding = Encoding.UTF8,
						Indent = true,
						IndentChars = "\t",
					};

					_writer = XmlWriter.Create(_file, settings);

					_writer.WriteStartDocument();
					_writer.WriteStartElement("LogEntries");
				}
				catch
				{
					// WTF?
					_file.Close();
					_file = null;
					return false;
				}

				return true;
			}
			catch
			{
				return false;
			}
		}
		#endregion

		#region Cleanup
		public void Dispose()
		{
			if (_writer != null)
			{
				_writer.WriteEndDocument();
				_writer.Flush();
				_writer.Close();
				_writer = null;
			}

			if (_file != null)
			{
				_file.Flush(true);
				_file.Close();
				_file = null;
			}

			if (_store != null)
			{
				_store.Dispose();
				_store = null;
			}
		}
		#endregion
	}
}