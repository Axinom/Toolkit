namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	/// <summary>
	/// A generic interface for validatable objects. Used to avoid having to re-implement an IValidatable
	/// interface in every project or - even worse - in every component. There also exist convenient
	/// extensions methods for the IValidatable interface, provided by <see cref="ExtensionsForIValidatable"/>.
	/// </summary>
	/// <seealso cref="ExtensionsForIValidatable"/>
	public interface IValidatable
	{
		/// <summary>
		/// Validates the state of the object. Throws exception on failure.
		/// </summary>
		/// <exception cref="ValidationException">Thrown if the state of the object is not valid.</exception>
		void Validate();
	}
}