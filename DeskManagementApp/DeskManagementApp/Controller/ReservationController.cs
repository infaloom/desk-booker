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
using DeskManagementApp.Models;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Authorization;
using static DeskManagementApp.Controller.DeskController;
using Humanizer;
using static reservationManagementApp.Controller.ReservationController;
using NuGet.Packaging;
using Microsoft.AspNetCore.SignalR;
using DeskManagementApp.Hubs;

namespace reservationManagementApp.Controller
{
    [Route("api/reservations")]
    [ApiController]
    public class ReservationController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly IHubContext<ReservationHub> _reservationHubContext;

        public ReservationController(UserManager<IdentityUser> userManager, IConfiguration configuration, ApplicationDbContext context, IHubContext<ReservationHub> reservationHubContext)
        {
            _userManager = userManager;
            _context = context;
            _emailSender = new EmailSender(configuration);
            _reservationHubContext = reservationHubContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllReservations()
        {
            DateTime startDate = DateTime.Today;
            DateTime endDate = startDate.AddDays(30);

            var reservations = await _context.Reservations
                .Where(r => r.Date >= startDate && r.Date <= endDate)
                .ToListAsync();

            //var filteredReservations = reservations
            //    .GroupBy(r => new { r.DeskId, r.Date })
            //    .Select(group => group.FirstOrDefault(r => r.Type == null) ?? group.First());

            ReservationDTO[] reservationDTOs = new ReservationDTO[0];

            foreach (var reservation in reservations)
            {
                var user = await _userManager.FindByIdAsync(reservation.UserId);
                var reservationDTO = new ReservationDTO();
                reservationDTO.Id = reservation.Id;
                reservationDTO.DeskId = reservation.DeskId;
                reservationDTO.Date = reservation.Date;
                reservationDTO.UserDTO = new UserDTO { UserId = reservation.UserId, UserName = user.UserName };
                reservationDTO.Type = reservation.Type;
                ReservationDTO[] reservationDTOs1 = reservationDTOs.Concat(new[] { reservationDTO }).ToArray();
                reservationDTOs = reservationDTOs1;
            }

            return Ok(reservationDTOs);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetReservationById(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(reservation.UserId);
            var reservationDTO = new ReservationDTO();
            reservationDTO.Id = reservation.Id;
            reservationDTO.DeskId = reservation.DeskId;
            reservationDTO.Date = reservation.Date;
            reservationDTO.Type = reservation.Type;
            reservationDTO.UserDTO = new UserDTO
            {
                UserId = reservation.UserId,
                UserName = user.UserName
            };
 

            return Ok(reservationDTO);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetReservationsByUser(string userId)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            bool isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");
            bool isUser = await _userManager.IsInRoleAsync(currentUser, "User");

            if (isUser && (!isAdmin || userId != currentUser.Id))
            {
                return Forbid();
            }

            DateTime startDate = DateTime.Today;
            DateTime endDate = startDate.AddDays(30);

            var reservations = await _context.Reservations
                .Where(r => r.UserId == userId && r.Date >= startDate && r.Date <= endDate)
                .ToListAsync();

            var reservationDTOs = reservations.Select(r => new ReservationDTO
            {
                Id = r.Id,
                DeskId = r.DeskId,
                Date = r.Date,
                Type = r.Type,
                UserDTO = new UserDTO{UserId = r.UserId,UserName = currentUser.UserName}
            }).ToList();

            return Ok(reservationDTOs);
        }

        [HttpPost]
        public async Task<IActionResult> CreateReservation(ReservationDTO r)
        {
            var reservation = new Reservation();
            var reservationDTO = new ReservationDTO();

            reservation.UserId = r.UserDTO.UserId;
            reservation.DeskId = r.DeskId;
            reservation.Date = r.Date;
            reservation.Type = r.Type;
            reservationDTO.UserDTO = r.UserDTO;
            reservationDTO.DeskId = r.DeskId;
            reservationDTO.Date = r.Date;
            reservationDTO.Type = r.Type;
            
            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();


            if (reservation.Id > 0)
            {
                reservationDTO.Id = reservation.Id;
                var message = new ActionDTO
                {
                    Action = "create",
                    ReservationDTO = reservationDTO
                };
                await _reservationHubContext.Clients.All.SendAsync("ReservationNotify", message);
                return Ok(reservationDTO);
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReservation(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            bool isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");
            bool isDeskAdmin = await _userManager.IsInRoleAsync(currentUser, "DeskAdmin");

            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }

            var reservationDTO = new ReservationDTO
            {
                Id = reservation.Id
            };

            var message = new ActionDTO
            {
                Action = "delete",
                ReservationDTO = reservationDTO
            };

            if (isAdmin || isDeskAdmin)
            {
                _context.Reservations.Remove(reservation);
                await _context.SaveChangesAsync();
                await _reservationHubContext.Clients.All.SendAsync("ReservationNotify", message);

                var reservationUser = await _userManager.FindByIdAsync(reservation.UserId);

                if (reservationUser.Id != currentUser.Id)
                {
                    await _emailSender.SendEmailAsync(reservationUser.Email, "Reservation Cancellation",
                            $"Your reservation for {reservation.Date.ToString("dd.MM.yyyy")} is cancelled! Contact App Admin for more info!");
                }

                return Ok(true);
            }
            if (reservation.UserId != currentUser.Id)
            {
                return Forbid();
            }
            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();
            await _reservationHubContext.Clients.All.SendAsync("ReservationNotify", message );
            return Ok(true);
        }

        public class ReservationDTO
        {
            public int? Id { get; set; }
            [Required]
            public int DeskId { get; set; }
            [Required]
            public UserDTO UserDTO { get; set; }
            [Required]
            public DateTime Date { get; set; }
            public string? Type { get; set; }
        }

        public class UserDTO
        {
            [Required] 
            public string UserId { get; set; }
            [Required]
            public string UserName { get; set; }
        }

        public class ActionDTO
        {
            [Required]
            public ReservationDTO ReservationDTO { get; set; }
            [Required]
            public string Action { get; set; }
        }

    }
}
