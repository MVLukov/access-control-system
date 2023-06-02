using ACS.Hubs;
using ACS.Models;
using ACS.Repository;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;

namespace ACS.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private IHubContext<TagHub> _hub;
        private readonly ACSDbContext _context;
        private readonly IHistoryRepository _historyRepo;
        private readonly IDeviceRepository _deviceRepo;
        private readonly IEmployeeRepository _employeeRepo;
        private readonly ITagRepository _tagRepo;
        private readonly IUserRepository _userRepo;

        public HomeController(ILogger<HomeController> logger, IHubContext<TagHub> hub, ACSDbContext context, IEmployeeRepository empRepo, ITagRepository tagRepo, IDeviceRepository devRepo, IHistoryRepository histRepo, IUserRepository userRepo)
        {
            _logger = logger;
            _hub = hub;
            _context = context;
            _historyRepo = histRepo;
            _tagRepo = tagRepo;
            _deviceRepo = devRepo;
            _employeeRepo = empRepo;
            _userRepo = userRepo;
        }

        [HttpGet]
        [Authorize]
        public IActionResult Index()
        {
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                ViewBag.isAuthenticated = true;
            }

            return View();
        }

        [HttpGet]
        [Authorize]

        public IActionResult GetHistory([FromQuery] DateTime date, [FromQuery] int page = 1)
        {
            if (page <= 0)
            {
                return BadRequest();
            }

            try
            {
                return Ok(_historyRepo.GetHistory(page, date).ToList());
            }
            catch (System.Exception)
            {
                return BadRequest();
            }

        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AuthorizeTag([FromQuery] string tagId, [FromQuery] string deviceId)
        {
            _logger.LogInformation($"tag: {tagId} | device: {deviceId}");

            if (String.IsNullOrEmpty(tagId) || String.IsNullOrEmpty(deviceId))
            {
                return BadRequest();
            }

            ResultStatus authorizeTag = _tagRepo.AuthorizeTag(tagId, deviceId);

            if (authorizeTag == ResultStatus.TagAuthorized)
            {
                return Ok();
            }
            else
            {
                return Unauthorized();
            }
        }

        [HttpGet]
        [Authorize]
        public IActionResult Devices()
        {
            return View("Devices");
        }

        [HttpGet]
        [Authorize]

        public IActionResult GetDevices([FromQuery] int page = 0)
        {
            if (page < 0)
            {
                return BadRequest();
            }

            try
            {
                IEnumerable<DeviceDTO> getAllDevices = _deviceRepo.GetAllDevices(page);
                return Ok(getAllDevices);
            }
            catch (System.Exception)
            {
                return BadRequest();
            }
        }

        [HttpGet]
        [Authorize]

        public IActionResult GetDevicesOfEmployee([FromQuery] int employeeId = 0)
        {
            if (employeeId <= 0)
            {
                return BadRequest();
            }

            try
            {
                IEnumerable<DeviceDTO> getAllDevices = _deviceRepo.GetAllDevicesOfEmployee(employeeId);
                return Ok(getAllDevices);
            }
            catch (System.Exception)
            {
                return BadRequest();
                // throw;
            }
        }

        [HttpPost]
        [Authorize]

        public IActionResult AddDevice([FromBody] DeviceDTO device)
        {
            device.DeviceId = device?.DeviceId.Trim();
            device.DeviceName = device?.DeviceName.Trim();

            if (String.IsNullOrEmpty(device.DeviceId) || String.IsNullOrEmpty(device.DeviceName))
            {
                return BadRequest();
            }

            try
            {
                ResultStatus res = _deviceRepo.AddDevice(device);

                if (res == ResultStatus.DeviceAdded)
                {
                    return Ok();
                }
                else if (res == ResultStatus.DeviceIdAlreadyExists)
                {
                    return Ok("DEV_ID_ERR");
                }
                else
                {
                    return Ok("DEV_NAME_ERR");
                }
            }
            catch (System.Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest();
            }
        }

        [HttpPost]
        [Authorize]

        public IActionResult UpdateDevice([FromBody] DeviceDTO device)
        {
            device.DeviceId = device?.DeviceId?.Trim();
            device.DeviceName = device?.DeviceName.Trim();

            if (String.IsNullOrEmpty(device.DeviceId) || String.IsNullOrEmpty(device.DeviceName))
            {
                return BadRequest();
            }

            try
            {
                ResultStatus res = _deviceRepo.UpdateDevice(device);

                if (res == ResultStatus.DeviceUpdated)
                {
                    return Ok();
                }
                else if (res == ResultStatus.DeviceNameAlreadyExists)
                {
                    return Ok("DEV_NAME_ERR");
                }
                else
                {
                    return BadRequest("DEV_ERR");
                }
            }
            catch (System.Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest("DB_ERR");
            }
        }

        [HttpGet]
        [Authorize]

        public IActionResult DeleteDevice([FromQuery] string deviceId)
        {
            if (String.IsNullOrEmpty(deviceId))
            {
                return BadRequest();
            }

            try
            {
                ResultStatus res = _deviceRepo.DeleteDevice(deviceId);
                if (res == ResultStatus.DeviceDeleted)
                {
                    return Ok();
                }
                else
                {
                    return BadRequest("DEV_ERR");
                }
            }
            catch (System.Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest("DB_ERR");
            }
        }

        [HttpGet]
        [Authorize]
        public IActionResult Employees()
        {
            return View("Employees");
        }

        [HttpGet]
        [Authorize]

        public IActionResult GetAllEmployees([FromQuery] int page)
        {
            if (page <= 0)
            {
                return BadRequest();
            }

            try
            {
                return Ok(_employeeRepo.GetAllEmployees(page).ToList());
            }
            catch (System.Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest();
            }
        }

        [HttpGet]
        [Authorize]

        public IActionResult SearchEmployee([FromQuery] string employeeName)
        {
            employeeName = employeeName?.Trim();
            if (String.IsNullOrEmpty(employeeName))
            {
                return GetAllEmployees(1);
            }

            try
            {
                return Ok(_employeeRepo.SearchEmployee(employeeName));
            }
            catch (System.Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest();
            }
        }

        [HttpPost]
        [Authorize]

        public IActionResult AddEmployee([FromBody] EmployeeDTO employee)
        {
            employee.EmployeeName = employee?.EmployeeName.Trim();
            if (String.IsNullOrEmpty(employee.EmployeeName))
            {
                return BadRequest();
            }

            try
            {
                ResultStatus res = _employeeRepo.AddEmployee(employee);

                if (res == ResultStatus.EmployeeAdded)
                {
                    return Ok();
                }
                else if (res == ResultStatus.TagAlreadyExists)
                {
                    return Ok("TAG_ERR");
                }
                else
                {
                    return Ok("EMP_ERR");
                }
            }
            catch (System.Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest();
            }
        }

        [HttpPost]
        [Authorize]

        public IActionResult UpdateEmployee([FromBody] EmployeeDTO employee)
        {
            employee.EmployeeName = employee?.EmployeeName.Trim();
            if (String.IsNullOrEmpty(employee.EmployeeName))
            {
                return BadRequest();
            }

            try
            {
                ResultStatus res = _employeeRepo.UpdateEmployee(employee);

                if (res == ResultStatus.EmployeeUpdated)
                {
                    return Ok();
                }
                else if (res == ResultStatus.TagAlreadyExists)
                {
                    return Ok("TAG_ERR");
                }
                else if (res == ResultStatus.EmployeeAlreadyExists)
                {
                    return Ok("EMP_ERR");
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (System.Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest();
            }
        }

        [HttpGet]
        [Authorize]

        public IActionResult DeleteEmployee([FromQuery] int employeeId = 0)
        {
            if (employeeId <= 0)
            {
                return BadRequest();
            }

            try
            {
                ResultStatus res = _employeeRepo.DeleteEmployee(employeeId);

                if (res == ResultStatus.EmployeeDeleted)
                {
                    return Ok();
                }

                return BadRequest();
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        [HttpGet]
        [Authorize]

        public IActionResult ToggleEmployeeAccess([FromQuery] int employeeId)
        {
            if (employeeId <= 0)
            {
                return BadRequest();
            }

            try
            {
                _employeeRepo.ToggleEmployeeAccess(employeeId);
                return Ok();
            }
            catch (System.Exception)
            {
                return BadRequest();
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
            {
                return View();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDTO credentials)
        {
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                return RedirectToAction(nameof(Index));
            }

            User getUser = _userRepo.Login(credentials);

            if (getUser != null)
            {
                var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, getUser.Id.ToString())
                    };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    ExpiresUtc = DateTime.Now.AddMinutes(30),
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);
                return RedirectToAction(nameof(Index));
            }


            return Ok(ResultStatus.InvalidCredentials.ToString());
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        [Authorize]
        public IActionResult MyProfile()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] MyProfileDTO credentials)
        {
            credentials.CurrentPassword = credentials?.CurrentPassword.Trim();
            credentials.NewPassword = credentials?.NewPassword.Trim();

            if (String.IsNullOrEmpty(credentials.CurrentPassword) || String.IsNullOrEmpty(credentials.NewPassword))
            {
                return BadRequest();
            }

            try
            {
                string getUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (!String.IsNullOrEmpty(getUserId))
                {
                    int userId = 0;

                    if (int.TryParse(getUserId, out userId))
                    {
                        ResultStatus res = _userRepo.ChangePassword(credentials, userId);

                        if (res == ResultStatus.PasswordChanged)
                        {
                            await HttpContext.SignOutAsync();
                            return RedirectToAction(nameof(Login));
                        }

                        if (res == ResultStatus.PasswordNotChanged)
                        {
                            return Ok("INVALID_CREDENTIALS");
                        }
                    }

                    return BadRequest();
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (System.Exception)
            {

                throw;
            }
        }

        [HttpGet]
        [Authorize]
        public IActionResult EmployeeHistory()
        {
            return View("EmployeeHistory");
        }

        [HttpGet]
        [Authorize]
        public IActionResult GetEmployeeHistory([FromQuery] int employeeId, [FromQuery] DateTime date, [FromQuery] int page = 1)
        {
            if (employeeId <= 0)
            {
                return BadRequest();
            }

            try
            {
                return Ok(_historyRepo.GetEmployeeHistory(employeeId, page, date));
            }
            catch (System.Exception)
            {
                return BadRequest();
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}