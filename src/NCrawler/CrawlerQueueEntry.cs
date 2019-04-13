using System;
using System.Collections.Generic;
using System.Linq;

using NCrawler.Extensions;

namespace NCrawler
{
	public class CrawlerQueueEntry : IEquatable<CrawlerQueueEntry>, IComparable<CrawlerQueueEntry>, IComparable
	{
		#region Instance Properties

		public CrawlStep CrawlStep { get; set; }

		public Dictionary<string, object> Properties { get; set; }

		public CrawlStep Referrer { get; set; }

		#endregion

		#region Instance Methods

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
			{
				return false;
			}

			if (ReferenceEquals(this, obj))
			{
				return true;
			}

			if (obj.GetType() != typeof (CrawlerQueueEntry))
			{
				return false;
			}

			return this.Equals((CrawlerQueueEntry) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var result = (this.CrawlStep != null ? this.CrawlStep.GetHashCode() : 0);
				result = (result*397) ^ (this.Properties != null ? this.Properties.GetHashCode() : 0);
				result = (result*397) ^ (this.Referrer != null ? this.Referrer.GetHashCode() : 0);
				return result;
			}
		}

		public override string ToString()
		{
			return "CrawlStep: {0}, Properties: {1}, Referrer: {2}".FormatWith(this.CrawlStep,
                this.Properties.Select(d => "{0}:{1}".FormatWith(d.Key, d.Value)).Join("; "), this.Referrer);
		}

		#endregion

		#region Operators

		public static bool operator ==(CrawlerQueueEntry left, CrawlerQueueEntry right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(CrawlerQueueEntry left, CrawlerQueueEntry right)
		{
			return !Equals(left, right);
		}

		#endregion

		#region IComparable Members

		public int CompareTo(object obj)
		{
			return this.CompareTo((CrawlerQueueEntry) obj);
		}

		#endregion

		#region IComparable<CrawlerQueueEntry> Members

		public int CompareTo(CrawlerQueueEntry other)
		{
			return this.CrawlStep.CompareTo(other.CrawlStep);
		}

		#endregion

		#region IEquatable<CrawlerQueueEntry> Members

		public bool Equals(CrawlerQueueEntry other)
		{
			if (ReferenceEquals(null, other))
			{
				return false;
			}

			if (ReferenceEquals(this, other))
			{
				return true;
			}

			return Equals(other.CrawlStep, this.CrawlStep) &&
				Equals(other.Referrer, this.Referrer) &&
                this.Properties.Select(d => d.Key).SequenceEqual(other.Properties.Select(d => d.Key)) &&
                this.Properties.Select(d => d.Value).SequenceEqual(other.Properties.Select(d => d.Value));
		}

		#endregion
	}
}