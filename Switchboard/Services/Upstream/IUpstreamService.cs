using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Switchboard.Services.Common.Contracts;

namespace Switchboard.Services.Upstream
{
    internal interface IUpstreamService
    {
        [Obsolete("This method is obsolete. Use FindFacesV2 instead.")]
        Task<FacePosition> FindFace(Stream image, CancellationToken cancellationToken);

        Task<FacePosition[]> FindFacesV2(Stream image, CancellationToken cancellationToken);

        Task<double[]> GetFaceFeatureVector(Stream image, CancellationToken cancellationToken);

        Task<IDictionary<string, FaceSearchResult[]>> SearchFacesByFeatureVectors(IDictionary<string, double[]> vectors,
            CancellationToken cancellationToken);
    }
}