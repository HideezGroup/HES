﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using web.HES.Data;

namespace web.HES.Pages.Settings.Positions
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public IList<Position> Positions { get; set; }

        [BindProperty]
        public Position Position { get; set; }

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task OnGetAsync()
        {
            Positions = await _context.Position.ToListAsync();
        }

        #region Position

        public IActionResult OnGetCreatePosition()
        {
            return Partial("_CreatePosition", this);
        }

        public async Task<IActionResult> OnPostCreatePositionAsync()
        {
            if (!ModelState.IsValid)
            {
                return RedirectToPage("./Index");
            }

            _context.Position.Add(Position);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnGetEditPositionAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Position = await _context.Position.FirstOrDefaultAsync(m => m.Id == id);

            if (Position == null)
            {
                return NotFound();
            }

            return Partial("_EditPosition", this);
        }

        public async Task<IActionResult> OnPostEditPositionAsync(string id)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToPage("./Index");
            }

            Position.Id = id;
            _context.Attach(Position).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PositionExists(Position.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool PositionExists(string id)
        {
            return _context.Position.Any(e => e.Id == id);
        }

        public async Task<IActionResult> OnGetDeletePositionAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Position = await _context.Position.FirstOrDefaultAsync(m => m.Id == id);

            if (Position == null)
            {
                return NotFound();
            }
            return Partial("_DeletePosition", this);
        }

        public async Task<IActionResult> OnPostDeletePositionAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Position = await _context.Position.FindAsync(id);

            if (Position != null)
            {
                _context.Position.Remove(Position);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }

        #endregion
    }
}