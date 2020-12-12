using Microsoft.AspNetCore.Mvc;
using Sample.Microservice.Device.App.Messages;
using Sample.Microservice.Device.App.Services;
using Sample.Microservice.Device.Infra.CrossCutting.Services;
using System.Threading.Tasks;

namespace Sample.Microservice.Device.Api.Controllers
{
    /// <summary>
    /// API with default endpoints to Sample Microservice Device - Manager.
    /// </summary>
    [Route("api/manager")]
    public class ManagerController : ControllerBase
    {
        private IManagerApplicationService ManagerApplicationService { get; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="managerApplicationService">Defaul application service.</param>
        public ManagerController(IManagerApplicationService managerApplicationService) => ManagerApplicationService = managerApplicationService;

        /// <summary>
        /// New device.
        /// </summary>
        /// <param name="request">Default request with parameter to input device.</param>
        /// <returns>Result of operation.</returns>
        /// <response code="204">Success and not content result.</response>
        /// <response code="400">The field is required.</response>
        /// <response code="401">Unauthorized because the Authorization-Token is invalid.</response>
        /// <response code="422">If validation to get user information.</response>
        /// <response code="500">Error on service layer.</response>
        [HttpPost, ProducesResponseType(204, Type = typeof(ResultResponseMessage))]
        public async Task<IActionResult> NewAsync([FromBody] DeviceRequestMessage request) => await ManagerApplicationService.NewAsync(request);

    }
}
