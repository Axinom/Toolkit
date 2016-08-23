namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net;
	using System.Text;

	/// <summary>
	/// Represents the query string of a <see cref="Uri"/> and allows its constituent parameters to be easily modified.
	/// Axinom Toolkit provides <see cref="Uri"/> extension methods to enable simple co-operation of the two types.
	/// Note that query string parameter names are case-insensitive.
	/// 
	/// See <see cref="ExtensionsForUri"/>.
	/// </summary>
	/// <remarks>
	/// A null parameter value has the meaning of a valueless parameter ("?name").
	/// An empty parameter value has the meaning of an empty value ("?name=").
	/// Any other parameter value has the meaning of a real value ("?name=value").
	/// </remarks>
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
	///		return HttpContext.Current.Request.Url.WithQueryString(qs);
	///	}
	/// ]]></code></example>
	/// <seealso cref="ExtensionsForUri"/>
	public sealed class QueryString
	{
		public static QueryString FromUrl(string url)
		{
			if (url == null)
				throw new ArgumentNullException("url");

			int qsIndex = url.IndexOf("?");

			if (qsIndex == -1)
				return FromQueryString("");

			int fragmentIndex = url.IndexOf("#");

			if (fragmentIndex == -1)
				return FromQueryString(url.Remove(0, qsIndex + 1));
			else
				return FromQueryString(url.Remove(0, qsIndex + 1).Remove(fragmentIndex - qsIndex - 1));
		}

		public static QueryString FromQueryString(string qs)
		{
			if (qs == null)
				throw new ArgumentNullException("qs");

			return new QueryString(qs);
		}

		public static QueryString FromUrl(Uri url)
		{
			if (url == null)
				throw new ArgumentNullException("url");

			return FromUrl(url.ToString());
		}

		private readonly Dictionary<string, string> _args = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		/// <summary>
		/// Initializes an empty QueryString instance without any defined query string parameters.
		/// </summary>
		public QueryString()
		{
		}

		private QueryString(string qs)
		{
			if (qs.Length == 0)
				return; // Nothing to do.

			// Remove initial questionmark if it exists.
			if (qs[0] == '?')
				qs = qs.Remove(0, 1);

			// Turn <plus> back to <space>
			// See: http://www.w3.org/TR/REC-html40/interact/forms.html#h-17.13.4.1
			qs = qs.Replace("+", " ");

			string[] args = qs.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (string arg in args)
			{
				string[] pair = arg.Split(new[] { '=' }, 2);

				string name = WebUtility.UrlDecode(pair[0]);
				string val = pair.Length == 2 ? WebUtility.UrlDecode(pair[1]) : null;

				_args[name] = val;
			}
		}

		public string Get(string key)
		{
			if (key == null)
				throw new ArgumentNullException("key");

			if (_args.ContainsKey(key))
				return (string)_args[key];
			else
				throw new ArgumentException("QueryString does not contain entry with the specified key.", "key");
		}

		public string TryGet(string key, string defaultValue)
		{
			if (key == null)
				throw new ArgumentNullException("key");

			if (_args.ContainsKey(key))
				return (string)_args[key];
			else
				return defaultValue;
		}

		public bool Contains(string key)
		{
			if (key == null)
				throw new ArgumentNullException("key");

			return _args.ContainsKey(key);
		}

		public void Set(string key, string value)
		{
			if (key == null)
				throw new ArgumentNullException("key");

			_args[key] = value;
		}

		public void Remove(string key)
		{
			if (key == null)
				throw new ArgumentNullException("key");

			_args.Remove(key);
		}

		public string this[string key]
		{
			get
			{
				if (key == null)
					throw new ArgumentNullException("key");

				return Get(key);
			}
			set
			{
				if (key == null)
					throw new ArgumentNullException("key");

				Set(key, value);
			}
		}

		public override string ToString()
		{
			if (_args.Count == 0)
				return "?";

			StringBuilder builder = new StringBuilder("?");
			bool isFirst = true;

			foreach (KeyValuePair<string, string> entry in _args)
			{
				if (!isFirst)
					builder.Append("&");
				isFirst = false;

				if (entry.Value == null)
				{
					builder.Append(WebUtility.UrlEncode(entry.Key));
				}
				else
				{
					builder.AppendFormat("{0}={1}", WebUtility.UrlEncode(entry.Key), WebUtility.UrlEncode(entry.Value));
				}
			}

			return builder.ToString();
		}
	}
}