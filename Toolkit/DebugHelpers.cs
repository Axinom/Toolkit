namespace Axinom.Toolkit
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using System.Text;

	public static partial class NetStandardHelpers
	{
		/// <summary>
		/// Gets the Message properties of an exception and all its inner exceptions.
		/// 
		/// Useful if you have top-level exceptions with pointless generic messages but also want to see deeper
		/// without spamming out the entire call stack information for every exception.
		/// </summary>
		public static string GetAllExceptionMessages(this HelpersContainerClasses.Debug container, Exception ex)
		{
			var messages = new List<string>();

			while (ex != null)
			{
				messages.Add(ex.Message);
				ex = ex.InnerException;
			}

			return string.Join(Environment.NewLine, messages);
		}

		/// <summary>
		/// How deep to go in the object hierarchy when creating a debug string for an object.
		/// </summary>
		private const int MaxDebugStringDepth = 4;

		/// <summary>
		/// The text value of one indent level in a debug string.
		/// </summary>
		private const string IndentString = "\t";

		/// <summary>
		/// Max number of collection items to display (per collection).
		/// </summary>
		private const int MaxCollectionItemsToList = 64;

		/// <summary>
		/// Allows you to create a deep and detailed representation of an object tree, including properties of the objects.
		/// Useful for debugging and reporting scenarios where <see cref="Object.ToString"/> simply does not give enough information.
		/// </summary>
		/// <remarks>
		/// The maximum object tree depth is limited, to avoid outputting too deep (or potentially infinite) hierarchies.
		/// Repeated objects are referred to by object ID, not output multiple times, to make the output more concise.
		/// 
		/// <para>Not everything is displayed - there is some fitering and transformation done. The specifics are not listed here.
		/// Some types have special handing to make them more meaningful (e.g. System.String, IEnumerable).</para>
		/// 
		/// <para>Generally, the data returned varies per type and may change in the future.
		/// Do not rely on the output remaining same in future versions, even for the same object.</para>
		/// </remarks>
		public static string ToDebugString(this HelpersContainerClasses.Debug container, object o)
		{
			Helpers.Argument.ValidateIsNotNull(o, nameof(o));

			StringBuilder s = new StringBuilder();
			var visitedObjects = new List<object>();

			CreateDebugString(o, s, 0, visitedObjects);

			return s.ToString();
		}

		// Types that are just .ToString().
		private static readonly TypeInfo[] TrivialTypes = new[]
		{
			typeof(string).GetTypeInfo(),
			typeof(bool).GetTypeInfo(),
			typeof(long).GetTypeInfo(),
			typeof(ulong).GetTypeInfo(),
			typeof(int).GetTypeInfo(),
			typeof(uint).GetTypeInfo(),
			typeof(short).GetTypeInfo(),
			typeof(ushort).GetTypeInfo(),
			typeof(byte).GetTypeInfo(),
			typeof(sbyte).GetTypeInfo(),
			typeof(double).GetTypeInfo(),
			typeof(float).GetTypeInfo(),
			typeof(decimal).GetTypeInfo(),
			typeof(Uri).GetTypeInfo(),
			typeof(TimeSpan).GetTypeInfo(),
			typeof(StringBuilder).GetTypeInfo(),
			typeof(Guid).GetTypeInfo()
		};

		// Types that have a special ToString() style formatter and do not need to be expanded.
		private static readonly TypeInfo[] SemiTrivialTypes = new[]
		{
			typeof(DateTime).GetTypeInfo(),
			typeof(DateTimeOffset).GetTypeInfo(),
		};

		// Trivial types whose derived types are also treated as trivial types.
		private static readonly TypeInfo[] DerivableTrivialTypes = new[]
		{
			typeof(Type).GetTypeInfo(),
			typeof(Enum).GetTypeInfo()
		};

		private static void CreateDebugString(object o, StringBuilder s, int depth, IList<object> visitedObjects, int? firstLineDepth = null)
		{
			// For some calls, we explicitly want to allow the first line not to have indent, so it can just continue
			// off the previous line without a line break. To do that, set firstLineDepth = 0.
			if (firstLineDepth == null)
				firstLineDepth = depth;

			if (o == null)
			{
				s.AppendFormatWithIndent(IndentString, firstLineDepth.Value, "null");
				s.AppendLine();
				return;
			}

			Type t = o.GetType();
			var ti = t.GetTypeInfo();

			if (TrivialTypes.Contains(ti) || DerivableTrivialTypes.Any(dtt => dtt.IsAssignableFrom(ti)))
			{
				s.AppendFormatWithIndent(IndentString, firstLineDepth.Value, "{0}", o);
				s.AppendLine();
				return;
			}

			// This is "almost" a trivial type but it requires a format string so we can't directly make it trivial.
			if (o is DateTime)
			{
				s.AppendFormatWithIndent(IndentString, firstLineDepth.Value, "{0}", ((DateTime)o).ToString("s"));
				s.AppendLine();
				return;
			}

			// This is "almost" a trivial type but it requires a format string so we can't directly make it trivial.
			if (o is DateTimeOffset)
			{
				s.AppendFormatWithIndent(IndentString, firstLineDepth.Value, "{0}", ((DateTimeOffset)o).ToString("u"));
				s.AppendLine();
				return;
			}

			// First, name the type of reference an existing object.
			if (!ti.IsValueType)
			{
				if (visitedObjects.Contains(o))
				{
					s.AppendFormatWithIndent(IndentString, firstLineDepth.Value, "Ref #{0}", visitedObjects.IndexOf(o));
					s.AppendLine();
					return;
				}

				visitedObjects.Add(o);

				s.AppendFormatWithIndent(IndentString, firstLineDepth.Value, "{0} (#{1})", t.Name, visitedObjects.Count - 1);
				s.AppendLine();
			}
			else
			{
				// Value types cannot be referenced, so we don't have a reference number here.
				s.AppendFormatWithIndent(IndentString, firstLineDepth.Value, "{0}", t.Name);
				s.AppendLine();
			}

			if (depth >= MaxDebugStringDepth)
			{
				s.AppendFormatWithIndent(IndentString, depth, "Nesting level too deep. Skipping.");
				s.AppendLine();
				return;
			}

			// If Hashtable, do special handling.
			if (_hashtableTypeInfo.Value != null && _hashtableTypeInfo.Value.IsAssignableFrom(ti))
			{
				var collection = (ICollection)o;
				var entriesToDisplay = collection.Cast<DictionaryEntry>().OrderBy(e => e.Key as string).Take(MaxCollectionItemsToList);

				foreach (var entry in entriesToDisplay)
				{
					if (entry.Value == null || TrivialTypes.Contains(entry.Value.GetType().GetTypeInfo()))
					{
						s.AppendFormatWithIndent(IndentString, depth, "{0} = {1}", entry.Key, entry.Value);
						s.AppendLine();
					}
					else
					{
						s.AppendFormatWithIndent(IndentString, depth, "{0} = ", entry.Key);
						CreateDebugString(entry.Value, s, depth + 1, visitedObjects, 0);
					}
				}

				if (collection.Count > MaxCollectionItemsToList)
				{
					s.AppendFormatWithIndent(IndentString, depth, "Max item limit hit - truncating output.");
					s.AppendLine();
				}

				s.AppendFormatWithIndent(IndentString, depth, "{0} total items: {1}", t.Name, collection.Count);
				s.AppendLine();

				return;
			}

			// If NameValueCollection, do special handling.
			if (_nameValueCollectionTypeInfo.Value != null && _nameValueCollectionTypeInfo.Value.IsAssignableFrom(ti))
			{
				var nvcIndexer = _nameValueCollectionTypeInfo.Value.DeclaredProperties
					.Where(pi =>
					{
						if (pi.Name != "Item")
							return false;

						if (pi.PropertyType != typeof(string))
							return false;

						var parameters = pi.GetIndexParameters();
						if (parameters.Length != 1)
							return false;

						if (parameters[0].ParameterType != typeof(string))
							return false;

						return true;
					})
					.Single();

				var collection = (ICollection)o;
				var entriesToDisplay = collection.Cast<string>().OrderBy(item => item).Take(MaxCollectionItemsToList);

				foreach (var key in entriesToDisplay)
				{
					var value = nvcIndexer.GetValue(collection, new object[] { key });

					s.AppendFormatWithIndent(IndentString, depth, "{0} = {1}", key, value);
					s.AppendLine();
				}

				if (collection.Count > MaxCollectionItemsToList)
				{
					s.AppendFormatWithIndent(IndentString, depth, "Max item limit hit - truncating output.");
					s.AppendLine();
				}

				s.AppendFormatWithIndent(IndentString, depth, "{0} total items: {1}", t.Name, collection.Count);
				s.AppendLine();

				return;
			}

			// If enumerable, do special handling. Strings (IEnumerable<char>) are already taken care of above.
			// We will just assume they are all of the same type (or at least same kind of type - value/class/etc).
			if (typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(ti))
			{
				int i = 0;
				bool needsNewline = false;

				foreach (object item in (IEnumerable)o)
				{
					var itemTypeInfo = item == null ? typeof(object).GetTypeInfo() : item.GetType().GetTypeInfo();

					if (item is string || item is Uri)
					{
						// These are long so they each get their own line.
						s.AppendFormatWithIndent(IndentString, depth, "{0}", item);
						s.AppendLine();
					}
					else if (TrivialTypes.Contains(itemTypeInfo))
					{
						// These are short so they all go on the same line.
						needsNewline = true;

						if (i == 0)
							s.AppendFormatWithIndent(IndentString, depth, " ");
						else
							s.AppendFormat(", ");

						s.Append(item);
					}
					else
					{
						CreateDebugString(item, s, depth, visitedObjects);
					}

					i++;

					if (i > MaxCollectionItemsToList)
					{
						// End the line if we printed the stuff on a single line.
						if (needsNewline)
						{
							s.AppendLine();
							needsNewline = false;
						}

						s.AppendFormatWithIndent(IndentString, depth, "Max item limit hit - truncating output.");
						s.AppendLine();
						break;
					}
				}

				if (needsNewline)
					s.AppendLine();

				s.AppendFormatWithIndent(IndentString, depth, "{0} total items: {1}", t.Name, i);
				s.AppendLine();

				return;
			}

			// All properties. Static before instance. Public only.
			foreach (var property in t.GetRuntimeProperties()
				.Where(pp => pp.GetMethod != null && pp.GetMethod.IsPublic)
				.Where(pp => pp.GetMethod.GetParameters().Length == 0)
				.OrderBy(pp => pp.GetMethod.IsStatic ? 0 : 1)
				.ThenBy(pp => pp.Name)
				.Select(pp => new
				{
					property = pp,
					getter = pp.GetMethod
				}))
			{
				object val = null;
				string valAsString;

				try
				{
					val = property.getter.Invoke(o, null);
					valAsString = val == null ? "null" : val.ToString();
				}
				catch (Exception ex)
				{
					valAsString = ex.GetType().Name + ": " + ex.Message;
				}

				var expand = val != null && ShouldExpand(val.GetType().GetTypeInfo());

				// We also do not expand if we are dealing with a struct that has a static member to another
				// instance of the same struct (e.g. IntPtr.Zero), since that will recurse forever and not be very useful.
				var isStructRecursion = val != null && ti.IsValueType && t == property.property.PropertyType;

				if (expand && !isStructRecursion)
				{
					if (val is ICollection)
					{
						var collection = (ICollection)val;
						try
						{
							if (collection.Count == 0)
							{
								s.AppendFormatWithIndent(IndentString, depth, "P {0}{1} = (Empty collection)", property.getter.IsStatic ? "static " : "", property.property.Name);
								s.AppendLine();
								continue;
							}
						}
						catch (Exception ex)
						{
							s.AppendFormatWithIndent(IndentString, depth, "P {0}{1} = Exception: {2}", property.getter.IsStatic ? "static " : "", property.property.Name, ex.Message);
							s.AppendLine();
							continue;
						}
					}

					s.AppendFormatWithIndent(IndentString, depth, "P {0}{1} = ", property.getter.IsStatic ? "static " : "", property.property.Name);
					CreateDebugString(val, s, depth + 1, visitedObjects, 0);
				}
				else
				{
					s.AppendFormatWithIndent(IndentString, depth, "P {0}{1} = {2}", property.getter.IsStatic ? "static " : "", property.property.Name, valAsString);
					s.AppendLine();
				}
			}

			// All fields. Static before instance. Public only.
			foreach (var field in t.GetRuntimeFields()
				.Where(ff => ff.IsPublic)
				.OrderBy(ff => ff.IsStatic ? 0 : 1)
				.ThenBy(ff => ff.Name))
			{
				object val = null;
				string valAsString;

				try
				{
					val = field.GetValue(o);
					valAsString = val == null ? "null" : val.ToString();
				}
				catch (Exception ex)
				{
					valAsString = ex.GetType().Name + ": " + ex.Message;
				}

				var expand = val != null && ShouldExpand(val.GetType().GetTypeInfo());

				// We also do not expand if we are dealing with a struct that has a static member to another
				// instance of the same struct (e.g. IntPtr.Zero), since that will recurse forever and not be very useful.
				var isStructRecursion = val != null && ti.IsValueType && t == field.FieldType;

				if (expand && !isStructRecursion)
				{
					if (val is ICollection)
					{
						var collection = (ICollection)val;
						try
						{
							if (collection.Count == 0)
							{
								s.AppendFormatWithIndent(IndentString, depth, "F {0}{1} = (Empty collection)", field.IsStatic ? "static " : "", field.Name);
								s.AppendLine();
								continue;
							}
						}
						catch (Exception ex)
						{
							s.AppendFormatWithIndent(IndentString, depth, "F {0}{1} = Exception: {2}", field.IsStatic ? "static " : "", field.Name, ex.Message);
							s.AppendLine();
							continue;
						}
					}

					s.AppendFormatWithIndent(IndentString, depth, "F {0}{1} = ", field.IsStatic ? "static " : "", field.Name);
					CreateDebugString(val, s, depth + 1, visitedObjects, 0);
				}
				else
				{
					s.AppendFormatWithIndent(IndentString, depth, "F {0}{1} = {2}", field.IsStatic ? "static " : "", field.Name, valAsString);
					s.AppendLine();
				}
			}
		}

		/// <summary>
		/// Gets whether a type should be expanded in a debug string.
		/// </summary>
		private static bool ShouldExpand(TypeInfo typeInfo)
		{
			return !TrivialTypes.Contains(typeInfo)
				   && !SemiTrivialTypes.Contains(typeInfo)
				   && !DerivableTrivialTypes.Any(dtt => dtt.IsAssignableFrom(typeInfo));
		}

		private static StringBuilder AppendFormatWithIndent(this StringBuilder sb, string indentString, int indentLevel, string format, params object[] args)
		{
			for (int i = 0; i < indentLevel; i++)
				sb.Append(indentString);

			return sb.AppendFormat(format, args);
		}

		// This is only present on .NET.
		private static readonly Lazy<TypeInfo> _hashtableTypeInfo = new Lazy<TypeInfo>(() => TryLoadTypeInfoAndIgnoreExceptions("System.Collections.Hashtable"));
		// This is only present on .NET.
		private static readonly Lazy<TypeInfo> _nameValueCollectionTypeInfo = new Lazy<TypeInfo>(() => TryLoadTypeInfoAndIgnoreExceptions("System.Collections.Specialized.NameValueCollection, System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"));

		private static TypeInfo TryLoadTypeInfoAndIgnoreExceptions(string name)
		{
			try
			{
				// Even if you leave it as the default (null on failure) it can occasionally still throw
				// exceptions like "Could not load file or assembly 'System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089' or one of its dependencies. The located assembly's manifest definition does not match the assembly reference." which are no fun and ruin the day.
				return Type.GetType(name)?.GetTypeInfo();
			}
			catch
			{
				// Whatevaaa.
				return null;
			}
		}
	}
}