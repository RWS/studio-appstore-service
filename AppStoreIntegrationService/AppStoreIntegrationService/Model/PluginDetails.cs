using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace AppStoreIntegrationService.Model
{
	public class PluginDetails
	{
		private DateTime? _createdDate;

		public int Id { get; set; }
		public string Name { get;
			set; }
		public string Description { get; set; }
		public IconDetails Icon { get; set; }
		public DateTime? ReleaseDate { get; set; }
		public int DownloadCount { get; set; }
		public int CommentCount { get; set; }
		public string SupportText { get; set; }
		public bool PaidFor { get; set; }
		public bool Inactive { get; set; }
		public string Pricing { get; set; }
		public RatingDetails RatingSummary { get; set; }
		public DeveloperDetails Developer { get; set; }
		public List<IconDetails> Media { get; set; }	
		public List<PluginVersion> Versions { get; set; }

		public List<CategoryDetails> Categories { get; set; }
		public string DownloadUrl { get; set; }

		public DateTime? CreatedDate
		{
			get
			{
				if (_createdDate != null)
				{
					return _createdDate;
				}

				_createdDate = Versions
					.Aggregate((curMin, x) => x.CreatedDate < curMin.CreatedDate ? x : curMin)
					.CreatedDate;
				return _createdDate;
			}
		}
	}
}
