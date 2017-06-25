using System;

using NCrawler.Extensions;

namespace NCrawler
{
	public class CrawlStep : IEquatable<CrawlStep>, IComparable<CrawlStep>, IComparable
	{
		#region Constructors

		public CrawlStep(Uri uri, int depth)
		{
            this.Uri = uri;
            this.Depth = depth;
            this.IsAllowed = true;
            this.IsExternalUrl = false;
		}

		#endregion

		#region Instance Properties

		public int Depth { get; private set; }

		public bool IsAllowed { get; set; }

		public bool IsExternalUrl { get; set; }

		public Uri Uri { get; internal set; }

		#endregion

		#region Instance Methods

		public override bool Equals(object other)
		{
			if (other.IsNull())
			{
				return false;
			}

			if (ReferenceEquals(this, other))
			{
				return true;
			}

			if (other is CrawlStep)
			{
				return Equals((CrawlStep)other);
			}

			return false;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var result = this.Depth;
				result = (result*397) ^ this.IsAllowed.GetHashCode();
				result = (result*397) ^ this.IsExternalUrl.GetHashCode();
				result = (result*397) ^ (this.Uri != null ? this.Uri.GetHashCode() : 0);
				return result;
			}
		}

		public override string ToString()
		{
			return "Depth: {0}, IsAllowed: {1}, IsExternalUrl: {2}, Uri: {3}".FormatWith(this.Depth, this.IsAllowed, this.IsExternalUrl, this.Uri);
		}

		#endregion

		#region Operators

		public static bool operator ==(CrawlStep left, CrawlStep right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(CrawlStep left, CrawlStep right)
		{
			return !Equals(left, right);
		}

		#endregion

		#region IComparable Members

		public int CompareTo(object obj)
		{
			return CompareTo(obj as CrawlStep);
		}

		#endregion

		#region IComparable<CrawlStep> Members

		public int CompareTo(CrawlStep other)
		{
			return this.Uri.ToString().CompareTo(other.Uri.ToString());
		}

		#endregion

		#region IEquatable<CrawlStep> Members

		public bool Equals(CrawlStep other)
		{
			if (other.IsNull())
			{
				return false;
			}

			if (ReferenceEquals(this, other))
			{
				return true;
			}

			return Equals(other.Uri, this.Uri);
		}

		#endregion
	}
}