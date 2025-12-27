using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Billbyte_BE.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/sidebar")]
    public class SidebarController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetSidebarItems()
        {
            var items = new[]
                {
            new {
                label = "Dashboard",
                icon = "dashboard",
                route = "/dashboard"
            },
            new {
                label = "Menu Items",
                icon = "restaurant_menu",
                route = "/menu-items"
            },
new {
label = "Reports",
icon = "bar_chart",
route = "/reports"
},

            new {
                label = "Settings",
                icon = "settings",
                route = "/settings"
            }
        };

            return Ok(items);
        }
    }
}
