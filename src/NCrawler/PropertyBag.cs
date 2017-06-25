using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;

using NCrawler.Extensions;

namespace NCrawler
{
	public class PropertyBag : IEquatable<PropertyBag>, IComparable<PropertyBag>, IComparable
	{
		#region Fields

		// A Hashtable to contain the properties in the bag
		private Dictionary<string, Property> m_ObjPropertyCollection = new Dictionary<string, Property>();

		#endregion

		#region Instance Indexers

		/// <summary>
		/// Indexer which retrieves a property from the PropertyBag based on 
		/// the property name
		/// </summary>
		public Property this[string name]
		{
			get
			{
				if (this.m_ObjPropertyCollection == null)
				{
                    this.m_ObjPropertyCollection = new Dictionary<string, Property>();
				}

				// An instance of the Property that will be returned
				Property objProperty;

				// If the PropertyBag already contains a property whose name matches
				// the property required, ...
				if (this.m_ObjPropertyCollection.ContainsKey(name))
				{
					// ... then return the pre-existing property
					objProperty = this.m_ObjPropertyCollection[name];
				}
				else
				{
					// ... otherwise, create a new Property with a matching name, and
					// a null Value, and add it to the PropertyBag
					objProperty = new Property(name, this);
                    this.m_ObjPropertyCollection.Add(name, objProperty);
				}

				return objProperty;
			}
		}

		#endregion

		#region Instance Properties

		public string CharacterSet { get; internal set; }

		public string ContentEncoding { get; internal set; }

		public string ContentType { get; internal set; }

        public DateTime StartTime { get; internal set; }

        public DateTime EndTime { get; internal set; }

        public TimeSpan DownloadTime { get; internal set; }

		public HttpResponseHeaders Headers { get; internal set; }

		public bool IsFromCache { get; internal set; }

		public bool IsMutuallyAuthenticated { get; internal set; }

		public DateTimeOffset? LastModified { get; internal set; }

		public string Method { get; internal set; }

		public Uri OriginalReferrerUrl { get; internal set; }

		public string OriginalUrl { get; internal set; }

		public Version ProtocolVersion { get; internal set; }

		public CrawlStep Referrer { get; internal set; }

		public Func<Stream> GetResponse { get; set; }

		public Uri ResponseUri { get; internal set; }

		public string Server { get; internal set; }

		public HttpStatusCode StatusCode { get; internal set; }

		public string StatusDescription { get; internal set; }

		public CrawlStep Step { get; internal set; }

		public string Text { get; set; }

		public string Title { get; set; }

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

			if (obj.GetType() != typeof (PropertyBag))
			{
				return false;
			}

			return Equals((PropertyBag) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var result = (this.m_ObjPropertyCollection != null ? this.m_ObjPropertyCollection.GetHashCode() : 0);
				result = (result*397) ^ (this.CharacterSet != null ? this.CharacterSet.GetHashCode() : 0);
				result = (result*397) ^ (this.ContentEncoding != null ? this.ContentEncoding.GetHashCode() : 0);
				result = (result*397) ^ (this.ContentType != null ? this.ContentType.GetHashCode() : 0);
				result = (result*397) ^ (this.Headers != null ? this.Headers.GetHashCode() : 0);
				result = (result*397) ^ this.IsFromCache.GetHashCode();
				result = (result*397) ^ this.IsMutuallyAuthenticated.GetHashCode();
				result = (result*397) ^ this.LastModified.GetHashCode();
				result = (result*397) ^ (this.Method != null ? this.Method.GetHashCode() : 0);
				result = (result*397) ^ (this.OriginalReferrerUrl != null ? this.OriginalReferrerUrl.GetHashCode() : 0);
				result = (result*397) ^ (this.OriginalUrl != null ? this.OriginalUrl.GetHashCode() : 0);
				result = (result*397) ^ (this.ProtocolVersion != null ? this.ProtocolVersion.GetHashCode() : 0);
				result = (result*397) ^ (this.Referrer != null ? this.Referrer.GetHashCode() : 0);
				result = (result*397) ^ (this.ResponseUri != null ? this.ResponseUri.GetHashCode() : 0);
				result = (result*397) ^ (this.Server != null ? this.Server.GetHashCode() : 0);
				result = (result*397) ^ this.StatusCode.GetHashCode();
				result = (result*397) ^ (this.StatusDescription != null ? this.StatusDescription.GetHashCode() : 0);
				result = (result*397) ^ (this.Step != null ? this.Step.GetHashCode() : 0);
				result = (result*397) ^ (this.Text != null ? this.Text.GetHashCode() : 0);
				result = (result*397) ^ (this.Title != null ? this.Title.GetHashCode() : 0);
				result = (result*397) ^ this.DownloadTime.GetHashCode();
				return result;
			}
		}

		#endregion

		#region Operators

		public static bool operator ==(PropertyBag left, PropertyBag right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(PropertyBag left, PropertyBag right)
		{
			return !Equals(left, right);
		}

		#endregion

		#region IComparable Members

		public int CompareTo(object obj)
		{
			return CompareTo(obj as PropertyBag);
		}

		#endregion

		#region IComparable<PropertyBag> Members

		public int CompareTo(PropertyBag other)
		{
			return this.Step.CompareTo(other.Step);
		}

		#endregion

		#region IEquatable<PropertyBag> Members

		public bool Equals(PropertyBag other)
		{
			if (ReferenceEquals(null, other))
			{
				return false;
			}

			if (ReferenceEquals(this, other))
			{
				return true;
			}

			return Equals(other.m_ObjPropertyCollection, this.m_ObjPropertyCollection) &&
				Equals(other.CharacterSet, this.CharacterSet) &&
				Equals(other.ContentEncoding, this.ContentEncoding) &&
				Equals(other.ContentType, this.ContentType) &&
				Equals(other.Headers, this.Headers) &&
				other.IsFromCache.Equals(this.IsFromCache) &&
				other.IsMutuallyAuthenticated.Equals(this.IsMutuallyAuthenticated) &&
				other.LastModified.Equals(this.LastModified) &&
				Equals(other.Method, this.Method) &&
				Equals(other.OriginalReferrerUrl, this.OriginalReferrerUrl) &&
				Equals(other.OriginalUrl, this.OriginalUrl) &&
				Equals(other.ProtocolVersion, this.ProtocolVersion) &&
				Equals(other.Referrer, this.Referrer) &&
				Equals(other.ResponseUri, this.ResponseUri) &&
				Equals(other.Server, this.Server) &&
				Equals(other.StatusCode, this.StatusCode) &&
				Equals(other.StatusDescription, this.StatusDescription) &&
				Equals(other.Step, this.Step) &&
				Equals(other.Text, this.Text) &&
				Equals(other.Title, this.Title) &&
				other.DownloadTime.Equals(this.DownloadTime);
		}

		#endregion

		#region Nested type: Property

		public class Property
		{
			#region Fields

			/// Field to hold the name of the property 
			private object m_Value;

			#endregion

			#region Constructors

			/// Event fires immediately prior to value changes
			/// <summary>
			/// Constructor
			/// </summary>
			/// <param name="name">The name of the new property</param>
			/// <param name="owner">The owner i.e. parent of the PropertyBag</param>
			public Property(string name, object owner)
			{
                this.Name = name;
                this.Owner = owner;
                this.m_Value = null;
			}

			#endregion

			#region Instance Properties

			/// <summary>
			/// The name of the Property
			/// </summary>
			public string Name { get; private set; }

			/// <summary>
			/// A pointer to the ultimate client class of the Property / PropertyBag
			/// </summary>
			public object Owner { get; private set; }

			/// <summary>
			/// The property value
			/// </summary>
			public object Value
			{
				get
				{
					// The lock statement makes the class thread safe. Multiple threads 
					// can attempt to get the value of the Property at the same time
					lock (this)
					{
						return this.Owner.GetPropertyValue(this.Name, this.m_Value);
					}
				}
				set
				{
					// The lock statement makes the class thread safe. Multiple threads 
					// can attempt to set the value of the Property at the same time
					lock (this)
					{
                        this.m_Value = value;
                        this.Owner.SetPropertyValue(this.Name, value);
					}
				}
			}

			#endregion
		}

		#endregion
	}
}