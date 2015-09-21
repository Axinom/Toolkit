namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Text;
	using System.Xml;
	using System.Xml.Serialization;

	/// <summary>
	/// Flags that apply special behaviors during XML serialization.
	/// </summary>
	[Flags]
	public enum XmlSerializationFlags
	{
		/// <summary>
		/// Removes the potentially unnecessary standard xmlns:xsi and xmlns:xsd declarations from the root node.
		/// This may have undesirable side-effects with custom namespaces and other functionality,
		/// so be careful - make sure you really do not need these namespace declarations.
		/// </summary>
		ClearNamespaceDefinitions,
		None
	}

	public static partial class DotNetHelpers
	{
		/// <summary>
		/// Uses XML serialization to serialize an object.
		/// </summary>
		/// <remarks>
		/// The encoding attribute in the XML declaration will have the value UTF-8. You must take care
		/// to also specify UTF-8 encoding when you write the resulting XML string to any output stream(s).
		/// If you write the string to a non-UTF-8 output, encoding errors may occur during deserialization.
		/// </remarks>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="obj"/> is null.</exception>
		public static string XmlSerialize(this HelpersContainerClasses.XmlSerialization container, object obj)
		{
			return Helpers.XmlSerialization.XmlSerialize(obj, XmlSerializationFlags.None);
		}

		/// <summary>
		/// Uses XML serialization to serialize an object. Flags may be specified to apply special behaviors.
		/// </summary>
		/// <remarks>
		/// The encoding attribute in the XML declaration will have the value UTF-8. You must take care
		/// to also specify UTF-8 encoding when you write the resulting XML string to any output stream(s).
		/// If you write the string to a non-UTF-8 output, encoding errors may occur during deserialization.
		/// </remarks>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="obj"/> is null.</exception>
		public static string XmlSerialize(this HelpersContainerClasses.XmlSerialization container, object obj, XmlSerializationFlags flags)
		{
			if (obj == null)
				throw new ArgumentNullException("obj");

			var serializer = new XmlSerializer(obj.GetType());
			using (var stream = new MemoryStream())
			{
				var writer = XmlWriter.Create(stream, new XmlWriterSettings
				{
					Encoding = Encoding.UTF8,
					IndentChars = "\t",
					Indent = true
				});

				if ((flags & XmlSerializationFlags.ClearNamespaceDefinitions) == XmlSerializationFlags.ClearNamespaceDefinitions)
				{
					var namespaces = new XmlSerializerNamespaces();
					namespaces.Add("", "");

					serializer.Serialize(writer, obj, namespaces);
				}
				else
				{
					// Mono 3.10 compiler incorrectly identifies this as unreachable code.
#pragma warning disable 162
					serializer.Serialize(writer, obj);
#pragma warning restore 162
				}

				writer.Flush();
				stream.Seek(0, SeekOrigin.Begin);

				var reader = new StreamReader(stream, Encoding.UTF8);
				return reader.ReadToEnd();
			}
		}

		/// <summary>
		/// Uses XML serialization to deserialize an object.
		/// </summary>
		/// <remarks>
		/// The encoding attribute in the XML declaration will be ignored, if present.
		/// It is your responsibility to ensure that the XML is loaded into the string with the correct reader encoding.
		/// </remarks>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="xml"/> is null.</exception>
		public static TObject XmlDeserialize<TObject>(this HelpersContainerClasses.XmlSerialization container, string xml)
		{
			return (TObject)Helpers.XmlSerialization.XmlDeserialize(xml, typeof(TObject));
		}

		/// <summary>
		/// Uses XML serialization to deserialize an object.
		/// </summary>
		/// <remarks>
		/// The encoding attribute in the XML declaration will be ignored, if present.
		/// It is your responsibility to ensure that the XML is loaded into the string with the correct reader encoding.
		/// </remarks>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="xml"/> is null.</exception>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="type"/> is null.</exception>
		public static object XmlDeserialize(this HelpersContainerClasses.XmlSerialization container, string xml, Type type)
		{
			if (xml == null)
				throw new ArgumentNullException("xml");

			if (type == null)
				throw new ArgumentNullException("type");

			var serializer = new XmlSerializer(type);
			using (var reader = new StringReader(xml))
				return serializer.Deserialize(reader);
		}
	}
}