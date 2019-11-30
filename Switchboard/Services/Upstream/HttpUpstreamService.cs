using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AtomicAkarin.Shirakami.Reflections;
using Microsoft.Extensions.DependencyInjection;
using Switchboard.Services.Common.Contracts;
using Switchboard.Services.Upstream.RemoteContracts;

namespace Switchboard.Services.Upstream
{
    [Component(ServiceLifetime.Singleton, Implements = typeof(IUpstreamService))]
    [RequireExternal(typeof(HttpClient))]
    internal class HttpUpstreamService : IUpstreamService
    {
        private readonly HttpClient _client;
        private readonly UpstreamServiceConfiguration _config;

        public HttpUpstreamService(UpstreamServiceConfiguration config, HttpClient client)
        {
            _config = config;
            _client = client;
        }

        public async Task<FacePosition> FindFace(Stream image, CancellationToken cancellationToken)
        {
            var response = await _client.PostStreamAsync<FaceDetectionResponse>(image, "image/jpeg",
                _config.Endpoints.Detection, CreateTimeoutCancellationToken(cancellationToken));

            if (response.Code != "200")
                throw new HttpRequestException("Response.Code != 200");

            var box = response.Box;
            return new FacePosition
            {
                X1 = box[0],
                Y1 = box[1],
                X2 = box[2],
                Y2 = box[3]
            };
        }

        public async Task<FacePosition[]> FindFacesV2(Stream image, CancellationToken cancellationToken)
        {
            var response = await _client.PostStreamAsync<FaceDetectionV2Response>(image, "image/jpeg",
                _config.Endpoints.DetectionV2, CreateTimeoutCancellationToken(cancellationToken));

            return response.Boxes.Select(box => new FacePosition
            {
                X1 = box[0],
                Y1 = box[1],
                X2 = box[2],
                Y2 = box[3]
            }).ToArray();
        }

        public async Task<double[]> GetFaceFeatureVector(Stream image, CancellationToken cancellationToken)
        {
            return (await _client.PostStreamAsync<double[][]>(image, "image/jpeg", _config.Endpoints.Recognition,
                CreateTimeoutCancellationToken(cancellationToken)))[0];
        }

        public async Task<IDictionary<string, FaceSearchResult[]>> SearchFacesByFeatureVectors(
            IDictionary<string, double[]> vectors, CancellationToken token)
        {
            var response = await _client.PostObjectAsync<FaceSearchResponse, FaceSearchRequest>(new FaceSearchRequest
            {
                Count = vectors.Count,
                CandidateCount = 3, // TODO: configurable candidate count
                Vectors = vectors.ToDictionary(pair => pair.Key, pair => pair.Value)
            }, _config.Endpoints.Search, CreateTimeoutCancellationToken(token));

            if (response.Code != 200)
                throw new HttpRequestException("Response.Code != 200");

            return response.ResultSet.ToDictionary(p => p.Key, p =>
            {
                var (_, result) = p;
                return result.TopDistances.Select((distance, index) => new FaceSearchResult
                {
                    Distance = distance, Label = result.TopLabels[index]
                }).ToArray();
            });
        }

        private CancellationToken CreateTimeoutCancellationToken(CancellationToken? peerCancellationToken)
        {
            var timeoutSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(_config.Timeout));
            return (peerCancellationToken == null
                ? timeoutSource
                : CancellationTokenSource.CreateLinkedTokenSource(
                    timeoutSource.Token, (CancellationToken) peerCancellationToken
                )).Token;
        }
    }
}