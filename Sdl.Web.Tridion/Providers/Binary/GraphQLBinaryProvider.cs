﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Sdl.Tridion.Api.Client.ContentModel;
using Sdl.Tridion.Api.Client.Utils;
using Sdl.Tridion.Api.GraphQL.Client;
using Sdl.Tridion.Api.Http.Client.Request;
using Sdl.Web.Common;
using Sdl.Web.Common.Interfaces;
using Sdl.Web.Common.Logging;
using Sdl.Web.Tridion.PCAClient;

namespace Sdl.Web.Tridion.Providers.Binary
{
    /// <summary>
    /// Binary Provider
    /// </summary>
    public class GraphQLBinaryProvider : IBinaryProvider
    {
        protected static readonly string DateTimeFormat = "MM/dd/yyyy HH:mm:ss";

        public DateTime GetBinaryLastPublishedDate(ILocalization localization, string urlPath)
        {
            var client = PCAClientFactory.Instance.CreateClient();
            var binary = client.GetBinaryComponent(localization.Namespace(), localization.PublicationId(), urlPath, null, null);
            return binary == null ? DateTime.MinValue : DateTime.ParseExact(binary.InitialPublishDate, DateTimeFormat, null);
        }

        public async Task<DateTime> GetBinaryLastPublishedDateAsync(ILocalization localization, string urlPath, CancellationToken cancellationToken = default(CancellationToken))
        {
            var client = PCAClientFactory.Instance.CreateClient();
            var binary = await client.GetBinaryComponentAsync(localization.Namespace(), localization.PublicationId(), urlPath, null, null, cancellationToken);
            return binary == null ? DateTime.MinValue : DateTime.ParseExact(binary.InitialPublishDate, DateTimeFormat, null);
        }

        public DateTime GetBinaryLastPublishedDate(ILocalization localization, int binaryId)
        {
            var client = PCAClientFactory.Instance.CreateClient();
            var binary = client.GetBinaryComponent(localization.Namespace(), localization.PublicationId(), binaryId, null, null);
            return binary == null ? DateTime.MinValue : DateTime.ParseExact(binary.InitialPublishDate, DateTimeFormat, null);
        }

        public async Task<DateTime> GetBinaryLastPublishedDateAsync(ILocalization localization, int binaryId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var client = PCAClientFactory.Instance.CreateClient();
            var binary = await client.GetBinaryComponentAsync(localization.Namespace(), localization.PublicationId(), binaryId, null, null, cancellationToken);
            return binary == null ? DateTime.MinValue : DateTime.ParseExact(binary.InitialPublishDate, DateTimeFormat, null);
        }

        public Tuple<byte[],string> GetBinary(ILocalization localization, int binaryId)
        {            
            var client = PCAClientFactory.Instance.CreateClient();
            var binary = client.GetBinaryComponent(localization.Namespace(), localization.PublicationId(), binaryId,
                null, null);
            var data = GetBinaryData(client, binary);
            if (data == null) throw new DxaItemNotFoundException(binaryId.ToString(), localization.Id);
            return data;
        }

        public async Task<Tuple<byte[], string>> GetBinaryAsync(ILocalization localization, int binaryId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var client = PCAClientFactory.Instance.CreateClient();
            var binary = await client.GetBinaryComponentAsync(localization.Namespace(), localization.PublicationId(), binaryId, null, null, cancellationToken);
            var data = await GetBinaryDataAsync(client, binary, cancellationToken);
            if (data == null) throw new DxaItemNotFoundException(binaryId.ToString(), localization.Id);
            return data;
        }             

        public Tuple<byte[],string> GetBinary(ILocalization localization, string urlPath)
        {
            var client = PCAClientFactory.Instance.CreateClient();
            var binary = client.GetBinaryComponent(localization.Namespace(), localization.PublicationId(), urlPath, null, null);
            var data = GetBinaryData(client, binary);
            if(data == null) throw new DxaItemNotFoundException(urlPath, localization.Id);
            return data;
        }

        public async Task<Tuple<byte[], string>> GetBinaryAsync(ILocalization localization, string urlPath, CancellationToken cancellationToken = default(CancellationToken))
        {
            var client = PCAClientFactory.Instance.CreateClient();
            var binary = await client.GetBinaryComponentAsync(localization.Namespace(), localization.PublicationId(), urlPath, null, null, cancellationToken);
            var data = await GetBinaryDataAsync(client, binary, cancellationToken);
            if (data == null) throw new DxaItemNotFoundException(urlPath, localization.Id);
            return data;
        }

        protected virtual Tuple<byte[],string> GetBinaryData(IGraphQLClient client, BinaryComponent binaryComponent)
        {
            if (binaryComponent == null) return null;
          
            if (binaryComponent?.Variants == null)
            {
                Log.Error("Unable to get binary data for CmUri (Variants null): " + binaryComponent.CmUri());
                return null;
            }
            try
            {
                if (binaryComponent.Variants.Edges == null || binaryComponent.Variants.Edges.Count == 0)
                {
                    Log.Error("Empty variants returned by GraphQL query for binary component: " + binaryComponent.CmUri());
                    return null;
                }
                var variant = binaryComponent.Variants.Edges[0].Node;
                if (string.IsNullOrEmpty(variant.DownloadUrl))
                {
                    Log.Error("Binary variant download Url is missing for binary component: " + binaryComponent.CmUri());
                    return null;
                }
                Log.Debug("Attempting to get binary at : " + variant.DownloadUrl);
                return new Tuple<byte[], string>(client.HttpClient.Execute<byte[]>(new HttpClientRequest
                {
                    AbsoluteUri = variant.DownloadUrl
                }).ResponseData, variant.Path);
            }
            catch(Exception ex)
            {
                Log.Error(ex, "Unable to get binary data for CmUri: " + binaryComponent.CmUri());
                return null;
            }          
        }

        protected virtual async Task<Tuple<byte[], string>> GetBinaryDataAsync(IGraphQLClient client, BinaryComponent binaryComponent, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (binaryComponent == null) return null;

            if (binaryComponent?.Variants == null)
            {
                Log.Error("Unable to get binary data for CmUri: " + binaryComponent.CmUri());
                return null;
            }
            var variant = binaryComponent.Variants.Edges[0].Node;
            try
            {
                Log.Debug("Attempting to get binary at : " + variant.DownloadUrl);
                var data = await client.HttpClient.ExecuteAsync<byte[]>(new HttpClientRequest
                {
                    AbsoluteUri = variant.DownloadUrl
                }, cancellationToken);

                return new Tuple<byte[], string>(data.ResponseData, variant.Path);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unable to get binary data for CmUri: " + binaryComponent.CmUri());
                return null;
            }
        }       
    }
}
