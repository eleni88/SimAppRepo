using System.Security.Claims;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SimulationProject.DTO.CredentialsDTOs;
using SimulationProject.DTO.UserDTOs;
using SimulationProject.Models;
using SimulationProject.Services;

namespace SimulationProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CloudCredentialsController : ControllerBase
    {
        private readonly IUsersService _usersService;
        private readonly CloudCredentialsService _cloudCredentialsService;
        public CloudCredentialsController(IUsersService usersService, CloudCredentialsService cloudCredentialsService)
        {
            _usersService = usersService;
            _cloudCredentialsService = cloudCredentialsService;
        }

        //POST /api/cloudcredentials/new
        [Authorize]
        [HttpPost("new")]
        public async Task<IActionResult> AddCloudCredentials([FromBody] AddCredentialsDTO credentials)
        {
            string newcredshash = "";
            //extract user from token
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr))
            {
                return BadRequest(new { message = "Invalid user." });
            }
            var userId = Int32.Parse(userIdStr);
            var user = await _usersService.GetUserByIdAsync(userId);

            if (user == null)
            {
                return BadRequest(new { message = "User not found." });
            }
            credentials.Userid = userId;
            var creds = credentials.Adapt<Cloudcredential>();
            newcredshash = await _cloudCredentialsService.HashCredentialsAsync(credentials.Accesskeyid, "");
            if (newcredshash != "")
            {
                creds.Accesskeyid = newcredshash;
            }
            newcredshash = await _cloudCredentialsService.HashCredentialsAsync(credentials.Secretaccesskey, "");
            if (newcredshash != "")
            {
                creds.Accesskeyid = newcredshash;
            }
            newcredshash = await _cloudCredentialsService.HashCredentialsAsync(credentials.Clientid, "");
            if (newcredshash != "")
            {
                creds.Secretaccesskey = newcredshash;
            }
            newcredshash = await _cloudCredentialsService.HashCredentialsAsync(credentials.Clientsecret, "");
            if (newcredshash != "")
            {
                creds.Clientsecret = newcredshash;
            }
            newcredshash = await _cloudCredentialsService.HashCredentialsAsync(credentials.Tenantid, "");
            if (newcredshash != "")
            {
                creds.Tenantid = newcredshash;
            }
            newcredshash = await _cloudCredentialsService.HashCredentialsAsync(credentials.Subscriptionid, "");
            if (newcredshash != "")
            {
                creds.Subscriptionid = newcredshash;
            }
            newcredshash = await _cloudCredentialsService.HashCredentialsAsync(credentials.Gcpprojectid, "");
            if (newcredshash != "")
            {
                creds.Gcpprojectid = newcredshash;
            }
            newcredshash = await _cloudCredentialsService.HashCredentialsAsync(credentials.Gcpservicekeyjson, "");
            if (newcredshash != "")
            {
                creds.Gcpservicekeyjson = newcredshash;
            }
            creds.Cloudid = credentials.Cloudid;

            await _cloudCredentialsService.AddCredentials(creds);
            return Ok(new { message = "Cloud credentials added successfully." });

        }


        //PUT /api/cloudcredentials/update
        [Authorize]
        [HttpPut("update")]
        public async Task<IActionResult> UpdateCloudCredentials([FromBody] UpdateCredentialsDTO credentials)
        {
            string newcredshash = "";
            //extract user from token
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr))
            {
                return BadRequest(new { message = "Invalid user." });
            }
            var userId = Int32.Parse(userIdStr);
            var user = await _usersService.GetUserByIdAsync(userId);

            if (user == null)
            {
                return BadRequest(new { message = "User not found." });
            }

            // find user's credentials
            var userCreds = await _cloudCredentialsService.GetCredsByCredIdAsync(credentials.Credid);
            newcredshash = await _cloudCredentialsService.HashCredentialsAsync(credentials.Accesskeyid, userCreds.Accesskeyid);
            if (newcredshash != "")
            {
                userCreds.Accesskeyid = newcredshash;
                await _cloudCredentialsService.UpdateCloudCreadentialsAsync();
            }

            newcredshash = await _cloudCredentialsService.HashCredentialsAsync(credentials.SecretAccesskey, userCreds.Secretaccesskey);
            if (newcredshash != "")
            {
                userCreds.Accesskeyid = newcredshash;
                await _cloudCredentialsService.UpdateCloudCreadentialsAsync();
            }
            newcredshash = await _cloudCredentialsService.HashCredentialsAsync(credentials.Clientid, userCreds.Clientid);
            if (newcredshash != "")
            {
                userCreds.Secretaccesskey = newcredshash;
                await _cloudCredentialsService.UpdateCloudCreadentialsAsync();
            }
            newcredshash = await _cloudCredentialsService.HashCredentialsAsync(credentials.Clientsecret, userCreds.Clientsecret);
            if (newcredshash != "")
            {
                userCreds.Clientsecret = newcredshash;
                await _cloudCredentialsService.UpdateCloudCreadentialsAsync();
            }
            newcredshash = await _cloudCredentialsService.HashCredentialsAsync(credentials.Tenantid, userCreds.Tenantid);
            if (newcredshash != "")
            {
                userCreds.Tenantid = newcredshash;
                await _cloudCredentialsService.UpdateCloudCreadentialsAsync();
            }
            newcredshash = await _cloudCredentialsService.HashCredentialsAsync(credentials.Subscriptionid, userCreds.Subscriptionid);
            if (newcredshash != "")
            {
                userCreds.Subscriptionid = newcredshash;
                await _cloudCredentialsService.UpdateCloudCreadentialsAsync();
            }
            newcredshash = await _cloudCredentialsService.HashCredentialsAsync(credentials.Gcpprojectid, userCreds.Gcpprojectid);
            if (newcredshash != "")
            {
                userCreds.Gcpprojectid = newcredshash;
                await _cloudCredentialsService.UpdateCloudCreadentialsAsync();
            }
            newcredshash = await _cloudCredentialsService.HashCredentialsAsync(credentials.Gcpservicekeyjson, userCreds.Gcpservicekeyjson);
            if (newcredshash != "")
            {
                userCreds.Gcpservicekeyjson = newcredshash;
                await _cloudCredentialsService.UpdateCloudCreadentialsAsync();
            }

            if (userCreds.Cloudid != credentials.Cloudid)
            {
                userCreds.Cloudid = credentials.Cloudid;
                await _cloudCredentialsService.UpdateCloudCreadentialsAsync();

            }

            return Ok(new { message = "Cloud credentials updated successfully." });
        }
    }
}
