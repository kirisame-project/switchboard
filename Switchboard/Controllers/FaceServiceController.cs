using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Switchboard.Controllers.ResponseContracts;
using Switchboard.Services.Lambda;
using Switchboard.Services.Upstream;

namespace Switchboard.Controllers
{
    [ApiController]
    [Route("api/v1/faces")]
    public class FaceServiceController : ControllerBase
    {
        private readonly HttpUpstreamService _upstreamService;

        public FaceServiceController(HttpUpstreamService upstreamService)
        {
            // TODO: use IUpstreamService instead HttpUpstreamService impl
            // TODO: implement ComponentImplAttribute
            _upstreamService = upstreamService;
        }

        [Consumes("image/jpeg")]
        [HttpPost]
        [ProducesErrorResponseType(typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route("full")]
        public async Task<IActionResult> DoFullRequest()
        {
            if (Request.Body == null)
                return new BadRequestObjectResult(new ErrorResponse(400, "Image payload cannot be empty"));

            var image = new MemoryStream();
            await Request.Body.CopyToAsync(image);

            var task = new LambdaTask(image);

            var token = CancellationToken.None; // TODO: use upstream timeout
            await task.UpdateFacePositions(_upstreamService, token);
            await task.UpdateFaceFeatureVectors(_upstreamService, token);
            await task.UpdateFaceSearchResults(_upstreamService, token);

            return new OkObjectResult(task);
        }
    }
}