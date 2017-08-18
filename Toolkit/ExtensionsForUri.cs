namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	/// <summary>
	/// Extends the <see cref="Uri"/> class. See <see cref="QueryString"/>.
	/// </summary>
	/// <example><code><![CDATA[
	/// public Uri GetNextItemUrl()
	/// {
	///		QueryString qs = HttpContext.Current.Request.Url.GetQueryString();
	///		
	///		qs["Advanced"] = "true";
	///		
	///		if (qs.Contains("ObjectID"))
	///			qs["ObjectID"] = (int.Parse(qs["ObjectID"]) + 1).ToString();
	///		else
	///			qs["ObjectID"] = 1;
	///		
	///		
	///		return HttpContext.Current.Request.Url.WithQueryString(qs);
	///	}
	/// ]]></code></example>
	/// <seealso cref="QueryString"/>
	public static class ExtensionsForUri
	{
		/// <summary>
		/// Adds a query string to a <see cref="Uri"/> or replaces the existing query string.
		/// </summary>
		public static Uri WithQueryString(this Uri instance, QueryString qs)
		{
			if (instance == null)
				throw new ArgumentNullException("instance");

			if (qs == null)
				throw new ArgumentNullException("qs");

			var fragment = instance.GetComponents(UriComponents.Fragment, UriFormat.UriEscaped);
			var beforeQs = instance.GetComponents(UriComponents.AbsoluteUri & ~(UriComponents.Fragment | UriComponents.Query), UriFormat.UriEscaped);

			return new Uri(beforeQs + qs + fragment, instance.IsAbsoluteUri ? UriKind.Absolute : UriKind.Relative);
		}

		/// <summary>
		/// Gets the query string of the <see cref="Uri"/> as a <see cref="QueryString"/> instance.
		/// </summary>
		public static QueryString GetQueryString(this Uri instance)
		{
			if (instance == null)
				throw new ArgumentNullException("instance");

			return QueryString.FromUrl(instance);
		}
	}
}