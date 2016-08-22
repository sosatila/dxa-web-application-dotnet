﻿using System;
using System.Collections.Generic;
using System.ServiceModel.Syndication;
using Sdl.Web.Common.Configuration;

namespace Sdl.Web.Common.Models
{
    /// <summary>
    /// Abstract base class for all (strongly typed) View Models
    /// </summary>
    public abstract class ViewModel
    {
        /// <summary>
        /// The internal/built-in Vocabulary ID used for semantic/CM mapping.
        /// </summary>
        public const string CoreVocabulary = "http://www.sdl.com/web/schemas/core";

        /// <summary>
        /// The Vocabulary ID for types defined by schema.org.
        /// </summary>
        public const string SchemaOrgVocabulary = "http://schema.org/"; 

        /// <summary>
        /// Gets or sets MVC data used to determine which View to use.
        /// </summary>
        [SemanticProperty(IgnoreMapping = true)]
        public MvcData MvcData
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets HTML CSS classes for use in View top level HTML element.
        /// </summary>
        [SemanticProperty(IgnoreMapping = true)]
        public string HtmlClasses
        {
            get;
            set;            
        }

        /// <summary>
        /// Gets or sets metadata used to render XPM markup
        /// </summary>
        [SemanticProperty(IgnoreMapping = true)]
        public IDictionary<string, object> XpmMetadata
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets extension data (additional properties which can be used by custom Model Builders, Controllers and/or Views)
        /// </summary>
        /// <value>
        /// The value is <c>null</c> if no extension data has been set.
        /// </value>
        [SemanticProperty(IgnoreMapping = true)]
        public IDictionary<string, object> ExtensionData
        {
            get;
            set;
        }

        /// <summary>
        ///  Sets an extension data key/value pair.
        /// </summary>
        /// <remarks>
        /// This convenience method ensures the <see cref="ExtensionData"/> dictionary is initialized before setting the key/value pair.
        /// </remarks>
        /// <param name="key">The key for the extension data.</param>
        /// <param name="value">The value.</param>
        public void SetExtensionData(string key, object value)
        {
            if (ExtensionData == null)
            {
                ExtensionData = new Dictionary<string, object>();
            }
            ExtensionData[key] = value;
        }

        /// <summary>
        /// Gets the rendered XPM markup
        /// </summary>
        /// <param name="localization">The context Localization.</param>
        /// <returns>The XPM markup.</returns>
        public abstract string GetXpmMarkup(Localization localization);

        #region Helper methods for syndication feed providers
        /// <summary>
        /// Concatenates all syndication feed items provided by a given set of feed item providers.
        /// </summary>
        /// <param name="feedItemProviders">The set of feed item providers.</param>
        /// <param name="localization">The context <see cref="Localization"/>.</param>
        /// <returns>The concatenated syndication feed items.</returns>
        protected IEnumerable<SyndicationItem> ConcatenateSyndicationFeedItems(IEnumerable<ISyndicationFeedItemProvider> feedItemProviders, Localization localization)
        {
            List<SyndicationItem> result = new List<SyndicationItem>();
            foreach (ISyndicationFeedItemProvider feedItemProvider in feedItemProviders)
            {
                result.AddRange(feedItemProvider.ExtractSyndicationFeedItems(localization));
            }
            return result;
        }

        /// <summary>
        /// Creates a syndication item link from a given <see cref="Link"/> instance.
        /// </summary>
        /// <param name="link">The <see cref="Link"/> instance.</param>
        /// <param name="localization">The context <see cref="Localization"/>.</param>
        /// <returns>The syndication item link or <c>null</c> if <paramref name="link"/> is <c>null</c> or an empty link.</returns>
        protected SyndicationLink CreateSyndicationLink(Link link, Localization localization)
        {
            if (link == null || string.IsNullOrEmpty(link.Url))
            {
                return null;
            }
            string absoluteUrl = SiteConfiguration.MakeFullUrl(link.Url, localization);
            return new SyndicationLink(new Uri(absoluteUrl));
        }

        /// <summary>
        /// Creates a syndication feed item based on essential data.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="content">The content. Can be a string or a <see cref="RichText"/> instance.</param>
        /// <param name="link">The link.</param>
        /// <param name="lastUpdated">The date/time this item was last updated. If <c>null</c>, the current date/time is used.</param>
        /// <param name="localization">The context <see cref="Localization"/>.</param>
        /// <returns>The syndication feed item.</returns>
        protected SyndicationItem CreateSyndicationItem(string title, object content, Link link, DateTime? lastUpdated, Localization localization)
        {
            SyndicationItem result = new SyndicationItem
            {
                Title = new TextSyndicationContent(title),
            };

            if (content != null)
            {
                TextSyndicationContentKind textKind = (content is RichText) ? TextSyndicationContentKind.Html : TextSyndicationContentKind.Plaintext;
                result.Content = new TextSyndicationContent(content.ToString(), textKind);
            }

            SyndicationLink syndicationLink = CreateSyndicationLink(link, localization);
            if (syndicationLink != null)
            {
                result.Links.Add(syndicationLink);
            }

            if (lastUpdated.HasValue)
            {
                result.LastUpdatedTime = lastUpdated.Value;
            }

            return result;
        }
        #endregion
    }
}
