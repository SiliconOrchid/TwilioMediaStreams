using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TwilioMediaStreams.Models;

namespace TwilioMediaStreams.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BasicController : ControllerBase
    {
        private readonly ProjectSettings _projectSettings;

        public BasicController (IOptions<ProjectSettings> projectSettings)
        {
            _projectSettings = projectSettings.Value;
        }

        [HttpGet]
        [Route("/handshake")]
        public IActionResult HandShake()
        {
            return Content($@"<Response><Start><Stream url=""{_projectSettings.TwilioMediaStreamWebhookUri}""/></Start><Say>Please record a message.</Say><Pause length=""60""/></Response>", "text/xml");
        }
    }
}