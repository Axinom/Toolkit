namespace Axinom.Toolkit
{
    using System;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Xml;

    public static partial class DotNetHelpers
    {
        /// <summary>
        /// Uses <see cref="DataContractSerializer"/> to serialize an object.
        /// </summary>
        /// <remarks>
        /// The encoding attribute in the XML declaration will have the value UTF-8. You must take care
        /// to also specify UTF-8 encoding when you write the resulting XML string to any output stream(s).
        /// If you write the string to a non-UTF-8 output, encoding errors may occur during deserialization.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="obj"/> is null.</exception>
        public static string Serialize(this HelpersContainerClasses.DataContract container, object obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            using (var stream = new MemoryStream())
            {
                var writer = XmlWriter.Create(stream, new XmlWriterSettings
                {
                    Encoding = Encoding.UTF8,
                    IndentChars = "\t",
                    Indent = true
                });

                new DataContractSerializer(obj.GetType()).WriteObject(writer, obj);

                writer.Flush();
                stream.Seek(0, SeekOrigin.Begin);

                var reader = new StreamReader(stream, Encoding.UTF8);
                return reader.ReadToEnd();
            }
        }

        /// <summary>
        /// Deserializes an object using <see cref="DataContractSerializer"/>.
        /// </summary>
        /// <remarks>
        /// The encoding attribute in the XML declaration will be ignored, if present.
        /// It is your responsibility to ensure that the XML is loaded into the string with the correct reader encoding.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="xml"/> is null.</exception>
        public static T Deserialize<T>(this HelpersContainerClasses.DataContract container, string xml)
        {
            return (T)Helpers.DataContract.Deserialize(xml, typeof(T));
        }

        /// <summary>
        /// Deserializes an object using <see cref="DataContractSerializer"/>.
        /// </summary>
        /// <remarks>
        /// The encoding attribute in the XML declaration will be ignored, if present.
        /// It is your responsibility to ensure that the XML is loaded into the string with the correct reader encoding.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="xml"/> is null.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="type"/> is null.</exception>
        public static object Deserialize(this HelpersContainerClasses.DataContract container, string xml, Type type)
        {
            if (xml == null)
                throw new ArgumentNullException("xml");

            if (type == null)
                throw new ArgumentNullException("type");

            return new DataContractSerializer(type).ReadObject(XmlReader.Create(new StringReader(xml)));
        }
    }
}