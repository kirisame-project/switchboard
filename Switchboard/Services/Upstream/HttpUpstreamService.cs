using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Switchboard.Common;
using Switchboard.Services.Upstream.RemoteContracts;

namespace Switchboard.Services.Upstream
{
    [Component(ComponentLifestyle.Singleton, Implements = typeof(IUpstreamService))]
    public class HttpUpstreamService : IUpstreamService
    {
        private readonly UpstreamServiceConfiguration _config;
        private readonly HttpHelper _http;

        public HttpUpstreamService(UpstreamServiceConfiguration config, HttpHelper http)
        {
            _config = config;
            _http = http;
        }

        public async Task<FacePosition> FindFace(MemoryStream image, CancellationToken cancellationToken)
        {
            var response = await _http.PostStreamAsync<FaceDetectionResponse>(image, "image/jpeg",
                _config.Endpoints.Detection, cancellationToken);

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

        public async Task<FacePosition[]> FindFacesV2(MemoryStream image, CancellationToken cancellationToken)
        {
            var response = await _http.PostStreamAsync<FaceDetectionV2Response>(image, "image/jpeg",
                _config.Endpoints.DetectionV2, cancellationToken);

            return response.Boxes.Select(box => new FacePosition
            {
                X1 = box[0],
                Y1 = box[1],
                X2 = box[2],
                Y2 = box[3]
            }).ToArray();
        }

        public async Task<double[]> GetFaceFeatureVector(MemoryStream image, CancellationToken cancellationToken)
        {
            return (await _http.PostStreamAsync<double[][]>(image, "image/jpeg", _config.Endpoints.Recognition,
                cancellationToken))[0];
        }

        public async Task<IDictionary<string, FaceSearchResult>> SearchFacesByFeatureVectors(
            IDictionary<string, double[]> vectors, CancellationToken token)
        {
            var response = await _http.PostObjectAsync<FaceSearchResponse, FaceSearchRequest>(new FaceSearchRequest
            {
                Count = vectors.Count,
                CandidateCount = 3,
                Vectors = vectors.ToDictionary(pair => pair.Key, pair => pair.Value)
            }, _config.Endpoints.Search, token);

            if (response.Code != 200)
                throw new HttpRequestException("Response.Code != 200");

            return response.ResultSet.ToDictionary(p => p.Key, p => new FaceSearchResult
            {
                TopResults = p.Value.TopLabels.Select((label, i) => new FaceSearchResultRow
                {
                    Distance = p.Value.TopDistances[i],
                    Label = label
                }).ToArray()
            });
        }
    }
}