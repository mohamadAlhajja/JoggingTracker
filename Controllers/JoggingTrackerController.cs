using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JoggingTrackerAPI.Controllers
{
    [ApiController]
    [Route("jogging-tracker")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class JoggingTrackerController
    {
    }
}
