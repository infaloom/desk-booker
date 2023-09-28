using Microsoft.AspNetCore.Mvc;
using DeskManagementApp.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Text;
using DeskManagementApp.Data;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using DeskManagementApp.Models;
using System.Runtime.InteropServices;
using DeskManagementApp.Hubs;
using Microsoft.AspNetCore.SignalR;
using static reservationManagementApp.Controller.ReservationController;
using static DeskManagementApp.Controller.DeskController;

namespace DeskManagementApp.Controller
{
    [Route("api/desks")]
    [ApiController]
    public class DeskController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<DeskHub> _deskHubContext;
        private readonly UserManager<IdentityUser> _userManager;
        public DeskController(ApplicationDbContext context,
            IHubContext<DeskHub> deskHubContext,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _deskHubContext = deskHubContext;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllDesks()
        {
            var desks = await _context.Desks.ToListAsync();
            var deskDTOs = desks.Select(d => new DeskDTO
            {
                Name = d.Name,
                Floor = d.Floor,
                Id = d.Id,
                UserId = d?.UserId,
                Username = d?.UserName
        }).ToList();
            return Ok(deskDTOs);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDeskById(int id)
        {
            var desk = await _context.Desks.FindAsync(id);
            if (desk == null)
            {
                return NotFound();
            }
            var deskDTO = new DeskDTO();
            deskDTO.Floor = desk.Floor;
            deskDTO.Name = desk.Name;
            deskDTO.Id = desk.Id;
            deskDTO.UserId = desk?.UserId;
            deskDTO.Username = desk?.User?.UserName;

            return Ok(deskDTO);
        }

        [HttpPost]
        [Authorize(Roles = "Admin, DeskAdmin")]
        public async Task<IActionResult> CreateDesk([FromBody] DeskDTO dto)
        {
            var deskDTO = new DeskDTO();
            var desk = new Desk();
            desk.Name = dto.Name;
            desk.Floor = dto.Floor;

            deskDTO.Name = desk.Name;
            deskDTO.Floor = desk.Floor;

            _context.Desks.Add(desk);
            await _context.SaveChangesAsync();

            if (desk.Id > 0) 
            {
                deskDTO.Id = desk.Id;
                var message = new ActionDTO
                {
                    Action = "create",
                    DeskDTO = deskDTO
                };
                await _deskHubContext.Clients.All.SendAsync("DeskNotify", message);
                return Ok(deskDTO);
            }
            else
            {
                return BadRequest();
            }
        }


        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin, DeskAdmin")]
        public async Task<IActionResult> DeleteDesk(int id)
        {
            var desk = await _context.Desks.FindAsync(id);
            if (desk == null)
            {
                return NotFound();
            }

            var deskDTO = new DeskDTO
            {
                Id = desk.Id
            };
            var message = new ActionDTO
            {
                Action = "delete",
                DeskDTO = deskDTO
            };

            _context.Desks.Remove(desk);
            await _context.SaveChangesAsync();
            await _deskHubContext.Clients.All.SendAsync("DeskNotify", message);
            return Ok(true);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin, DeskAdmin")]
        public async Task<IActionResult> UpdateDeskName(int id, [FromBody] DeskDTO dto)
        {
            var desk = await _context.Desks.FindAsync(id);
            if (desk == null)
            {
                return NotFound();
            }
            desk.Name = dto.Name;
            desk.Floor = dto.Floor;
            dto.Id = desk.Id;

            var message = new ActionDTO
            {
                Action = "update",
                DeskDTO = dto
            };

            _context.Entry(desk).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            await _deskHubContext.Clients.All.SendAsync("DeskNotify", message);
            return Ok(dto);
        }

        [HttpPatch("{id}")]
        [Authorize(Roles = "Admin, DeskAdmin")]
        public async Task<IActionResult> AssignDesk(int id, [FromBody] UserDTO dto)
        {
            var desk = await _context.Desks.FindAsync(id);
            if (desk == null)
            {
                return NotFound();
            }

            if (dto.UserId != "Not assigned")
            {
                var user = await _userManager.FindByIdAsync(dto.UserId);
                if (user == null)
                {
                    return NotFound();
                }
                desk.User = user;
                desk.UserId = dto.UserId;
                desk.UserName = user.UserName;
            }
            else
            {
                desk.UserId = null;
                desk.UserName = null;
            }

            var deskDTO = new DeskDTO
            {
                Name = desk.Name,
                Floor = desk.Floor,
                Id = desk.Id,
                UserId = desk.UserId,
                Username = desk.UserName
            };

            var message = new ActionDTO
            {
                Action = "update",
                DeskDTO = deskDTO
            };

            _context.Entry(desk).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            await _deskHubContext.Clients.All.SendAsync("DeskNotify", message);
            return Ok(true);
        }

        public class DeskDTO
        {
            [Required]
            public string Name { get; set; }
            [Required]
            public int Floor { get; set; }
            public int? Id { get; set; }
            public string? UserId { get; set; }
            public string? Username { get; set; }

        }

        public class UserDTO
        {
            [Required]
            public string UserId { get; set; }
        }

        public class ActionDTO
        {
            [Required]
            public DeskDTO DeskDTO { get; set; }
            [Required]
            public string Action { get; set; }
        }
    }
}
