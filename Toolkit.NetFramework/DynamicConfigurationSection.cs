namespace Axinom.Toolkit
{
    using System;
    using System.Configuration;
    using System.Xml;
    using System.Xml.Serialization;
    using System.Xml.XPath;

    /// <summary>
    /// Implementation of a dynamic configuration section handler.
    /// The handler looks at the "type" attribute defined on the section element and uses
    /// XML serialization to deserialize an instance of this type from the XML.
    /// 
    /// If the configuration object implements the IValidatable interface, it is
    /// automatically validated when loaded. ConfigurationErrorsException is thrown if a failure occurs.
    /// </summary>
    /// <example>
    /// <code title="web.config" lang="xml"><![CDATA[
    /// <configSections>
    ///		<section name="MySection" type="Axinom.Toolkit.DynamicConfigurationSection, Axinom.Toolkit.NetFramework" />
    /// </configSections>
    /// 
    /// <MySection type="Something.Something.MyConfiguration">
    ///		<Stuff>things</Stuff>
    /// </MySection>
    /// ]]></code>
    /// <code title="Using the configuration section"><![CDATA[
    /// MyConfiguration configuration = ConfigurationManager.GetSection("MySection") as MyConfiguration;
    /// ]]></code>
    /// </example>
    public sealed class DynamicConfigurationSection : IConfigurationSectionHandler
    {
        /// <inheritdoc />
        public object Create(object parent, object configContext, XmlNode section)
        {
            string typeOfObject = "";

            try
            {
                XPathNavigator navigator = section.CreateNavigator();
                typeOfObject = (string)navigator.Evaluate("string(@type)");

                Type t = Helpers.Type.GetTypeFromAnyAssembly(typeOfObject);
                if (t == null)
                    throw new ConfigurationErrorsException("Configuration section is of a type that does not exist: " + typeOfObject);

                XmlSerializer ser = new XmlSerializer(t);

                object result;

                using (XmlNodeReader xNodeReader = new XmlNodeReader(section))
                    result = ser.Deserialize(xNodeReader);

                if (result is IValidatable)
                {
                    try
                    {
                        ((IValidatable)result).Validate();
                    }
                    catch (ValidationException ex)
                    {
                        throw new ConfigurationErrorsException(ex.Message, ex);
                    }
                    catch (Exception ex)
                    {
                        throw new ConfigurationErrorsException(string.Format("Unexpected error occurred during validation of dynamic configuration section {0} ({1}): {2}", section.Name, typeOfObject, ex.Message), ex);
                    }
                }

                return result;
            }
            catch (ConfigurationErrorsException ex)
            {
                Log.Default.Error(ex.ToString());
                throw;
            }
            catch (Exception ex)
            {
                Log.Default.Error(ex.ToString());
                throw new ConfigurationErrorsException(string.Format("Unable to deserialize dynamic configuration section {0} ({1}): {2}", section.Name, typeOfObject, ex.Message), ex);
            }
        }
    }
}