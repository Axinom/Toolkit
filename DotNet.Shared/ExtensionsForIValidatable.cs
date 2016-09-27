namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	/// <summary>
	/// Extensions for <see cref="IValidatable"/>. Enables easy validation of an entire object tree.
	/// </summary>
	/// <example><code><![CDATA[
	/// public class Homepage : Page
	/// {
	///		private MyConfiguration _configuration;
	/// 
	///		protected override OnLoad()
	///		{
	///			_configuration = MyConfiguration.LoadDefault();
	///			_configuration.Validate();
	///		}
	/// }
	/// 
	/// public class MyConfiguration : IValidatable
	/// {
	///		public string SystemEmail;
	///		public int Width;
	///		
	///		public void Validate()
	///		{
	///			if (Width <= 0)
	///				this.ThrowValidationFailureFor("Width", "Must be greater than zero.");
	///				
	///			if (string.IsNullOrEmpty(SystemEmail))
	///				this.ThrowValidationFailureFor("SystemEmail", "Must be specified");
	///		}
	/// }
	/// ]]></code></example>
	/// <seealso cref="IValidatable"/>
	public static class ExtensionsForIValidatable
	{
		/// <summary>
		/// Makes it somewhat easier to throw validation failures that have useful metadata on them (type and property name).
		/// </summary>
		public static void ThrowValidationFailureFor(this IValidatable instance, string memberName, string reason)
		{
			if (instance == null)
				throw new ArgumentNullException("instance");

			if (memberName == null)
				throw new ArgumentNullException("memberName");

			if (reason == null)
				throw new ArgumentNullException("reason");

			throw new ValidationException(string.Format("{0}.{1} - {2}", instance.GetType().FullName, memberName, reason));
		}

		/// <summary>
		/// Makes it somewhat easier to throw validation failures that have useful metadata on them (type).
		/// </summary>
		public static void ThrowValidationFailure(this IValidatable instance, string reason)
		{
			if (instance == null)
				throw new ArgumentNullException("instance");

			if (reason == null)
				throw new ArgumentNullException("reason");

			throw new ValidationException(string.Format("{0} - {1}", instance.GetType().FullName, reason));
		}
	}
}