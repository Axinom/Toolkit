namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	public static partial class CoreHelpers
	{
		/// <summary>
		/// Implementation class to hold varibles for the helpers dealing with random data.
		/// </summary>
		private static class RandomData
		{
			public static readonly Lazy<string[]> Words = new Lazy<string[]>(() => BigText.Replace(".", "").Replace(Environment.NewLine, "").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));

			#region Big block of placeholder text
			private const string BigText = @"Lorem ipsum dolor sit amet, consectetur adipiscing elit. Morbi euismod varius mauris sit amet bibendum. Quisque sem tellus, venenatis non hendrerit in, mollis laoreet diam. Fusce eget eros est, vitae sodales libero. In neque nisi, volutpat ut laoreet placerat, laoreet sit amet metus. Mauris sit amet nibh sed libero imperdiet lobortis. Vivamus sed quam justo. Vestibulum malesuada viverra enim. Phasellus sodales ipsum non felis placerat faucibus tincidunt elit placerat. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Morbi molestie consequat sodales. 

Morbi vitae tellus eu libero sollicitudin fringilla et non sapien. Ut pellentesque rhoncus magna. Nunc molestie odio eu augue condimentum id hendrerit erat viverra. Aliquam pretium ullamcorper mauris faucibus fermentum. Vivamus id mollis felis. Donec nec convallis purus. Vestibulum eget eros purus, id rutrum sem. Donec et ante nunc, in molestie nisi. Aenean eget gravida libero. Duis vel enim a justo semper hendrerit. Fusce at mauris a nulla laoreet tempus et sed lectus. Pellentesque in vehicula nunc. Donec volutpat elit et purus ultricies cursus scelerisque risus aliquet. 

Vestibulum auctor felis nec metus tempus vel pulvinar orci pharetra. Cras mollis venenatis volutpat. Etiam bibendum aliquam erat, ut placerat neque viverra a. In lacinia fringilla iaculis. Cras imperdiet vehicula quam, vitae eleifend dui sollicitudin nec. Etiam sodales lacus id nisl accumsan molestie. Praesent suscipit pellentesque egestas. Curabitur mattis dolor at dui facilisis feugiat. Mauris mi mi, semper sed commodo a, sollicitudin eu est. Nam at ultricies odio. Nullam sed justo a lacus dignissim pulvinar. Nam consectetur, ligula non vehicula blandit, lectus augue lobortis odio, ac bibendum erat erat nec nisi. Duis dui tortor, vestibulum at porta ac, cursus et leo. Phasellus purus velit, congue quis condimentum in, tempor nec augue. Phasellus quis libero urna. Nunc feugiat quam eu odio semper ornare. Nullam vel nibh vitae eros congue congue. 

Sed pellentesque dolor sit amet velit vulputate id consectetur sapien vulputate. In odio sapien, lobortis scelerisque cursus non, pretium vitae leo. Suspendisse ut eros mi, eget luctus sem. Pellentesque pretium imperdiet tortor, ultrices sagittis sapien ornare ac. Integer a ante at tellus laoreet egestas. Donec suscipit, augue eu cursus lacinia, dolor massa aliquam dolor, et facilisis purus turpis quis arcu. Sed sollicitudin lacinia eros, non tincidunt turpis rhoncus eu. Suspendisse potenti. Cum sociis natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. Nulla vitae libero massa. Phasellus egestas feugiat ipsum, nec gravida mi sodales a. Aenean sit amet augue sit amet purus rhoncus venenatis ut vel dolor. Ut eget diam id leo interdum mattis. Praesent feugiat dolor quis leo volutpat tincidunt tincidunt metus ornare. 

Vestibulum molestie, risus bibendum consequat mattis, risus dui dictum augue, eget placerat ipsum arcu rutrum justo. Nulla facilisi. Vestibulum nisi nibh, scelerisque at laoreet ut, bibendum a nunc. Duis ut magna ante. Aenean eget lorem justo. Ut hendrerit, tortor mattis sodales mattis, felis dolor aliquet nisl, nec tempus tellus mi ac magna. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Phasellus feugiat vehicula fringilla. Cum sociis natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. Donec eget tellus a nisl rutrum mattis vel ut ligula. 

Aenean non justo vitae nisl lobortis dictum. Suspendisse mollis tellus a sem porta eget vestibulum ante hendrerit. Vestibulum tristique feugiat eros. Nam commodo viverra orci, at lacinia diam imperdiet ut. Quisque non pulvinar nisl. Aliquam cursus mauris vitae lorem molestie molestie. Integer lobortis consequat nulla, nec interdum arcu pretium in. Donec vel lacus quis neque lacinia vulputate. Mauris sed orci mauris, in vulputate diam. Quisque lacinia sapien id orci feugiat nec rutrum neque adipiscing. Quisque vel tellus metus. Donec nec elit vitae ipsum pulvinar ultrices. 

Mauris eget bibendum lorem. Cras lobortis eros in sapien dignissim hendrerit. Cum sociis natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. Fusce sed neque ut turpis ullamcorper dignissim in nec justo. Morbi dapibus ligula in mi vulputate ultricies. Nulla blandit, tortor vitae facilisis sollicitudin, leo sem pellentesque justo, a porttitor magna elit placerat nulla. Sed egestas, enim et vulputate mattis, metus neque dapibus elit, nec fringilla odio ipsum at orci. Morbi ante tortor, sagittis et sollicitudin mattis, hendrerit ut ipsum. Sed eget orci ipsum, at tristique dui. Phasellus non odio purus, sit amet egestas elit. Duis facilisis mattis viverra. Duis sit amet tristique elit. Duis commodo tincidunt purus. Praesent nisl augue, ornare vel fringilla sed, gravida tempus justo. Donec est arcu, imperdiet a vulputate non, aliquet molestie sapien. Fusce eu nibh justo. Nunc ornare leo id mauris condimentum id pharetra nisi dictum. 

Nulla diam nibh, faucibus in euismod ut, eleifend et nisl. Nunc sagittis molestie vehicula. Nulla nisi orci, tempor dignissim hendrerit at, convallis id enim. Etiam dictum nisi sed nisi tincidunt non rhoncus risus tempus. Vivamus congue magna et lorem feugiat vehicula. Curabitur quis lectus non tellus pharetra aliquet sit amet lacinia mi. Nullam magna sem, consequat vel tincidunt id, imperdiet a quam. Donec eu posuere quam. Vestibulum venenatis ligula eu odio fermentum placerat ullamcorper urna auctor. Donec dignissim massa et ipsum ultrices vestibulum. Curabitur mauris ipsum, hendrerit et lobortis vitae, mattis vitae massa. Proin nisl risus, auctor vitae porta id, congue nec mauris. Phasellus euismod scelerisque orci in interdum. Aliquam pellentesque libero quis mi venenatis mollis. Donec porttitor cursus est, quis tempor lacus dapibus in. Vestibulum libero libero, pharetra lacinia fringilla vitae, fermentum at justo. Praesent ullamcorper diam ut erat facilisis in pulvinar purus ornare. 

Vivamus elementum massa eu diam commodo iaculis. Ut lobortis odio non ante pretium cursus. Sed id nisi id est placerat consectetur pretium ac turpis. Aliquam eros purus, semper vitae gravida sed, rhoncus at eros. Etiam volutpat, mi et congue tincidunt, odio enim egestas est, ac aliquet elit lacus vitae elit. Ut porta sodales orci, eu ultrices nisl auctor nec. Mauris vitae dignissim odio. Praesent ac augue a odio interdum egestas. Nulla sollicitudin luctus luctus. Maecenas faucibus gravida accumsan. Aliquam eu massa at est egestas ullamcorper. Donec tristique iaculis tincidunt. 

Integer cursus purus et neque ultrices pretium id a quam. Aliquam consequat augue ac enim bibendum pretium. Nulla tortor dui, vestibulum eu cursus eu, commodo aliquet purus. Quisque eu nibh ac enim vestibulum egestas id vitae diam. Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia Curae; Nunc lacinia aliquam est at iaculis. Nullam dictum convallis pharetra. Donec vitae augue massa, eu dignissim risus. Phasellus varius sollicitudin luctus. Praesent turpis mauris, consectetur quis dictum id, luctus ut neque. 

Aenean sit amet leo sit amet nunc interdum sagittis quis nec neque. Integer ipsum justo, eleifend vel elementum sed, rhoncus eget nisi. Nunc in tortor in enim rhoncus rutrum ut nec enim. Aliquam erat volutpat. Ut aliquet imperdiet dui id lobortis. Mauris in nulla a arcu pharetra tincidunt quis id dolor. Duis nibh lacus, commodo vel varius ut, imperdiet eget purus. Phasellus accumsan porta tortor ac suscipit. Proin ornare mi id arcu convallis rutrum. Aliquam condimentum, turpis vel volutpat sollicitudin, lacus purus tristique urna, mollis vehicula ligula ipsum eget risus. Nam sollicitudin sodales turpis, ac fermentum metus bibendum eu. Fusce euismod venenatis libero, sit amet dignissim enim lacinia tempor. Aenean ut nunc lectus, eleifend porttitor augue. Mauris sit amet diam rutrum libero porttitor molestie. Integer eget tortor sed lacus placerat sodales. In sit amet pellentesque diam. Ut volutpat tortor a risus scelerisque in lacinia lectus hendrerit. Donec augue lorem, varius a rhoncus quis, ultrices rhoncus nulla. Vestibulum rhoncus, ligula sed pharetra pretium, erat lectus sollicitudin nisl, eget pulvinar dui turpis sit amet augue. 

Pellentesque fringilla fringilla erat sed consectetur. Quisque sagittis volutpat purus, a gravida nisi congue in. Phasellus interdum luctus quam rutrum aliquet. Donec ante sapien, vulputate sit amet lobortis eleifend, venenatis at magna. Nam nec magna enim, quis pulvinar elit. Curabitur nulla sapien, lobortis vitae bibendum sagittis, dictum ullamcorper diam. Nam malesuada lorem eu orci vulputate bibendum. Praesent quis mauris nec lectus gravida vulputate eget luctus sem. Morbi non purus quis justo hendrerit viverra vitae et turpis. Nam a consectetur velit. Sed massa libero, iaculis fringilla lacinia vel, sagittis vel tellus. Suspendisse faucibus fermentum mauris nec porta. Aliquam aliquam tellus massa, sed vestibulum leo. Proin cursus, sapien sit amet suscipit cursus, nunc massa imperdiet dui, nec placerat tortor nisi non magna. Aenean sit amet ante et magna porttitor lacinia vitae at nunc. 

Duis quis ligula in est ullamcorper venenatis quis vitae turpis. Sed porttitor bibendum venenatis. Vivamus eget erat libero, et facilisis arcu. Sed ornare magna quis massa ultrices at dapibus elit tincidunt. Nunc tempor tempor neque, feugiat pellentesque lacus laoreet a. Quisque gravida ultricies velit a pulvinar. Donec condimentum posuere justo, sed mattis massa iaculis in. Proin lectus magna, lobortis in varius et, molestie a enim. Aliquam pellentesque, metus mattis sodales aliquet, lectus velit gravida purus, et consectetur sapien elit vitae risus. Sed pellentesque volutpat nisl vitae tristique. Praesent at ipsum mauris, non congue erat. Duis aliquet molestie eros. Nullam ultricies mattis egestas. Sed varius eleifend mattis. 

Pellentesque laoreet nisl sed quam tempus gravida. Praesent dapibus enim sed odio suscipit luctus. Quisque varius suscipit felis ac varius. Ut est orci, suscipit et ultrices et, ullamcorper at diam. Donec ac malesuada velit. Nunc feugiat, risus vitae tincidunt blandit, nisl urna tempor sem, a ornare tortor augue et est. Nullam semper tortor ac nunc ullamcorper tempus. Duis eros magna, euismod in mattis id, lobortis eget est. Donec tincidunt ornare diam, congue tincidunt magna condimentum non. Fusce tincidunt risus et odio bibendum eget hendrerit dolor luctus. Pellentesque aliquet leo sit amet sapien pretium tempor. Sed elit arcu, tempor blandit lobortis quis, semper id ipsum. Praesent ut sem leo. Phasellus ullamcorper, ligula sit amet lobortis consectetur, orci nisi sodales mi, non pulvinar arcu ligula a nisl. Quisque sollicitudin cursus lectus. Curabitur id neque sem, sit amet varius diam. Nunc urna massa, faucibus quis ultricies sed, aliquam at elit. 

Vivamus nunc elit, vulputate a accumsan nec, imperdiet nec urna. Cras pretium massa eget mauris dapibus consequat. Fusce at lorem quam, in condimentum odio. Sed ac sodales diam. Morbi a placerat erat. Nunc euismod interdum libero sed semper. In viverra eros et risus molestie ac iaculis est sagittis. Praesent mollis dictum dolor, non tincidunt erat placerat eu. Suspendisse ut nulla et nibh rutrum porttitor. Maecenas non est arcu. Donec sed luctus orci. Pellentesque sollicitudin fermentum urna, a tristique quam pretium vitae. Fusce sit amet lectus a nunc egestas gravida. In interdum, diam vel pretium ullamcorper, turpis mi fringilla erat, sit amet consequat est orci quis velit. Nulla sit amet ultricies magna.";
			#endregion

			public static readonly Random Random = new Random();
			public static readonly object RandomLock = new object();
		}

		/// <summary>
		/// Gets a random bunch of text, size determined by number of words.
		/// No guarantees are made about the content of the words or any punctuation between the words,
		/// except that there will be at least a space between every word.
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="minWords"/> is negative.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="maxWords"/> is less than <paramref name="minWords"/>.</exception>
		public static string GetWords(this HelpersContainerClasses.Random container, int minWords, int maxWords)
		{
			if (minWords < 0)
				throw new ArgumentOutOfRangeException(nameof(minWords), "minWords cannot be negative");

			if (maxWords < minWords)
				throw new ArgumentOutOfRangeException(nameof(maxWords), "maxWords cannot be less than minWords");

			lock (RandomData.RandomLock)
			{
				int wordCount = RandomData.Random.Next(minWords, maxWords + 1);

				var sourceWords = RandomData.Words.Value;
				var words = new List<string>();

				for (int i = 0; i < wordCount; i++)
					words.Add(sourceWords[RandomData.Random.Next(sourceWords.Length)]);

				return string.Join(" ", words.ToArray());
			}
		}

		public static int GetInteger(this HelpersContainerClasses.Random container)
		{
			lock (RandomData.RandomLock)
				return RandomData.Random.Next();
		}

		public static int GetInteger(this HelpersContainerClasses.Random container, int maxValue)
		{
			lock (RandomData.RandomLock)
				return RandomData.Random.Next(maxValue);
		}

		public static int GetInteger(this HelpersContainerClasses.Random container, int minValue, int maxValue)
		{
			lock (RandomData.RandomLock)
				return RandomData.Random.Next(minValue, maxValue);
		}

		public static double GetDouble(this HelpersContainerClasses.Random container)
		{
			lock (RandomData.RandomLock)
				return RandomData.Random.NextDouble();
		}

		public static byte[] GetBytes(this HelpersContainerClasses.Random container, int count)
		{
			var buffer = new byte[count];

			lock (RandomData.RandomLock)
				RandomData.Random.NextBytes(buffer);

			return buffer;
		}

		public static T GetRandomItem<T>(this HelpersContainerClasses.Random container, IReadOnlyList<T> fromList)
		{
			Helpers.Argument.ValidateIsNotNull(fromList, nameof(fromList));

			if (fromList.Count == 0)
				throw new ArgumentException("Cannot get an item from an empty list.");

			var index = Helpers.Random.GetInteger(fromList.Count);

			return fromList[index];
		}

		public static TEnum GetEnum<TEnum>(this HelpersContainerClasses.Random container)
		{
			var values = Enum.GetValues(typeof(TEnum)).Cast<TEnum>().ToArray();

			return Helpers.Random.GetRandomItem(values);
		}
	}
}