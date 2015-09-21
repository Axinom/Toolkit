namespace Axinom.Toolkit
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using System.Text;

	public static partial class CoreHelpers
	{
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
			if (o == null)
				throw new ArgumentNullException("o");

			StringBuilder s = new StringBuilder();
			var visitedObjects = new List<object>();

			CreateDebugString(o, s, 0, visitedObjects);

			return s.ToString();
		}

		// Types that are just .ToString().
		private static readonly Type[] TrivialTypes = new[]
		{
			typeof(string),
			typeof(bool),
			typeof(long),
			typeof(ulong),
			typeof(int),
			typeof(uint),
			typeof(short),
			typeof(ushort),
			typeof(byte),
			typeof(sbyte),
			typeof(double),
			typeof(float),
			typeof(decimal),
			typeof(Uri),
			typeof(TimeSpan),
			typeof(StringBuilder),
			typeof(Guid)
		};

		// Types that have a special ToString() style formatter and do not need to be expanded.
		private static readonly Type[] SemiTrivialTypes = new[]
		{
			typeof(DateTime),
			typeof(DateTimeOffset),
		};

		// Trivial types whose derived types are also treated as trivial types.
		private static readonly Type[] DerivableTrivialTypes = new[]
		{
			typeof(Type),
			typeof(Enum)
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

			if (TrivialTypes.Contains(t) || DerivableTrivialTypes.Any(dtt => dtt.IsAssignableFrom(t)))
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
			if (_hashtableType.Value != null && _hashtableType.Value.IsAssignableFrom(t))
			{
				var collection = (ICollection)o;
				var entriesToDisplay = collection.Cast<DictionaryEntry>().OrderBy(e => e.Key as string).Take(MaxCollectionItemsToList);

				foreach (var entry in entriesToDisplay)
				{
					if (entry.Value == null || TrivialTypes.Contains(entry.Value.GetType()))
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
			if (_nameValueCollectionType.Value != null && _nameValueCollectionType.Value.IsAssignableFrom(t))
			{
				var nvcIndexer = _nameValueCollectionType.Value.GetProperty("Item", typeof(string), new[] { typeof(string) });

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
			if (typeof(IEnumerable).IsAssignableFrom(t))
			{
				int i = 0;
				bool needsNewline = false;

				foreach (object item in (IEnumerable)o)
				{
					var itemType = item == null ? typeof(object) : item.GetType();

					if (item is string || item is Uri)
					{
						// These are long so they each get their own line.
						s.AppendFormatWithIndent(IndentString, depth, "{0}", item);
						s.AppendLine();
					}
					else if (TrivialTypes.Contains(itemType))
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
			foreach (var property in t.GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public)
				.Where(pp => pp.GetGetMethod(true) != null)
				.Where(pp => pp.GetGetMethod(true).GetParameters().Length == 0)
				.OrderBy(pp => pp.GetGetMethod(true).IsStatic ? 0 : 1)
				.ThenBy(pp => pp.Name)
				.Select(pp => new
				{
					property = pp,
					getter = pp.GetGetMethod(true)
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

				var expand = val != null && ShouldExpand(val.GetType());

				if (expand)
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
			foreach (var field in t.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public)
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

				var expand = val != null && ShouldExpand(val.GetType());

				if (expand)
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
		private static bool ShouldExpand(Type type)
		{
			return !TrivialTypes.Contains(type)
			       && !SemiTrivialTypes.Contains(type)
			       && !DerivableTrivialTypes.Any(dtt => dtt.IsAssignableFrom(type));
		}

		private static StringBuilder AppendFormatWithIndent(this StringBuilder sb, string indentString, int indentLevel, string format, params object[] args)
		{
			for (int i = 0; i < indentLevel; i++)
				sb.Append(indentString);

			return sb.AppendFormat(format, args);
		}

		// This is only present on .NET.
		private static readonly Lazy<Type> _hashtableType = new Lazy<Type>(() => TryLoadTypeAndIgnoreExceptions("System.Collections.Hashtable"));
		// This is only present on .NET.
		private static readonly Lazy<Type> _nameValueCollectionType = new Lazy<Type>(() => TryLoadTypeAndIgnoreExceptions("System.Collections.Specialized.NameValueCollection, System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"));

		private static Type TryLoadTypeAndIgnoreExceptions(string name)
		{
			try
			{
				// Even if you leave it as the default (null on failure) it can occasionally still throw
				// exceptions like "Could not load file or assembly 'System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089' or one of its dependencies. The located assembly's manifest definition does not match the assembly reference." which are no fun and ruin the day.
				return Type.GetType(name);
			}
			catch
			{
				// Whatevaaa.
				return null;
			}
		}
	}
}