using DeskManagementApp.Services;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using static DeskManagementApp.Controller.DeskController;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using Microsoft.EntityFrameworkCore;
using SendGrid.Helpers.Mail;
using static reservationManagementApp.Controller.ReservationController;
using DeskManagementApp.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace DeskManagementApp.Controller
{
    [Route("api/employees")]
    [ApiController]
    public class EmployeerController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IHubContext<EmployeeHub> _employeeHubContext;


        public EmployeerController(UserManager<IdentityUser> userManager, 
            IHubContext<EmployeeHub> employeeHubContext)
        {
            _userManager = userManager;
            _employeeHubContext = employeeHubContext;
        }

        [Authorize(Roles = "Admin, DeskAdmin")]
        [HttpGet]
        public async Task<IActionResult> GetAllEmployees()
        {
            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            admins = admins.Where(a => a.EmailConfirmed).ToList();
            var adminDTOs = admins.Select(a => new UserDTO
            {
                Name = a.UserName,
                Role = "Admin",
                Id = a.Id
            }).ToList();

            var deskAdmins = await _userManager.GetUsersInRoleAsync("DeskAdmin");
            deskAdmins = deskAdmins.Where(da => da.EmailConfirmed).ToList();
            var deskAdminDTOs = deskAdmins.Select(da => new UserDTO
            {
                Name = da.UserName,
                Role = "DeskAdmin",
                Id = da.Id
            }).ToList();

            var employees = await _userManager.GetUsersInRoleAsync("User");
            employees = employees.Where(e => e.EmailConfirmed).ToList();
            var employeeDTOs = employees.Select(e => new UserDTO
            {
                Name = e.UserName,
                Role = "User",
                Id = e.Id
            }).ToList();

            var list = adminDTOs.Concat(deskAdminDTOs).Concat(employeeDTOs).ToList();

            return Ok(list);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateEmployeeRole(string id, [FromBody] UpdateDTO role)
        {
            var employee = await _userManager.FindByIdAsync(id);
            var isDeskAdmin = await _userManager.IsInRoleAsync(employee, "DeskAdmin");
            if (employee == null) 
            {
                return NotFound();
            }

            var userDTO = new UserDTO
            {
                Id = employee.Id,
                Name = employee.UserName
            };

            if (role.role == "DeskAdmin")
            {
                if (!isDeskAdmin)
                {
                    await _userManager.RemoveFromRoleAsync(employee, "User");
                    await _userManager.AddToRoleAsync(employee, "DeskAdmin");
                    userDTO.Role = "DeskAdmin";
                }
                else
                {
                    return BadRequest("Greska");
                }
            }
            else { 
                if(isDeskAdmin)
                {
                    await _userManager.RemoveFromRoleAsync(employee, "DeskAdmin");
                    await _userManager.AddToRoleAsync(employee, "User");
                    userDTO.Role = "User";
                }
                else
                {
                    return BadRequest("Greska");
                }
            }

            var message = new ActionDTO
            {
                Action = "update",
                UserDTO = userDTO
            };
            await _employeeHubContext.Clients.All.SendAsync("EmployeeNotify", message);

            return Ok();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteEmployee(string id)
        {
            var employee = await _userManager.FindByIdAsync(id);
            if (employee == null)
            {
                return NotFound();
            }
   
            var userDTO = new UserDTO
            {
                Id = employee.Id,
                Name = employee.UserName
            };

            bool admin = await _userManager.IsInRoleAsync(employee, "Admin");
            bool deskAdmin = await _userManager.IsInRoleAsync(employee, "DeskAdmin");

            if (admin)
            {
                userDTO.Role = "Admin";
            }
            else if (deskAdmin)
            {
                userDTO.Role = "DeskAdmin";
            }
            userDTO.Role = "User";

            var result = await _userManager.DeleteAsync(employee);

            if (result.Succeeded)
            {
                var message = new ActionDTO
                {
                    Action = "delete",
                    UserDTO = userDTO
                };
                await _employeeHubContext.Clients.All.SendAsync("EmployeeNotify", message);
                return Ok(true);
            }
            else
            {
                return BadRequest(false);
            }
        }
    }

    public class UserDTO
    {
        [Required]
        public string Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Role { get; set; }
    }

    public class UpdateDTO
    {
        [Required]
        public string role { get; set; }
    }

}
