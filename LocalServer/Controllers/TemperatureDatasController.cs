using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LocalServer.Models;
using LocalServer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocalServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TemperatureDatasController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public TemperatureDatasController(ApplicationDBContext context)
        {
            _context = context;
        }

        // DELETE: api/TemperatureDatas/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<TemperatureData>> DeleteTemperatureData(int id)
        {
            var temperatureData = await _context.Data.FindAsync(id);
            if (temperatureData == null)
            {
                return NotFound();
            }

            _context.Data.Remove(temperatureData);
            await _context.SaveChangesAsync();

            return temperatureData;
        }

        // GET: api/TemperatureDatas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TemperatureData>>> GetData()
        {
            return await _context.Data.ToListAsync();
        }

        // GET: api/TemperatureDatas/last20
        [HttpGet]
        [Route("last20")]
        public async Task<ActionResult<IEnumerable<TemperatureData>>> GetLast20Data()
        {
            return await _context.Data.OrderByDescending(p => p.Time).Take(20).ToListAsync();
        }

        // GET: api/TemperatureDatas/last
        [HttpGet]
        [Route("last")]
        public async Task<ActionResult<IEnumerable<TemperatureData>>> GetLastData()
        {
            var keys = await _context.Data.GroupBy(x => x.MacAddress).Select(x => x.Key).ToListAsync();

            return keys.Select(key => _context.Data.Where(x => x.MacAddress == key).OrderByDescending(x => x.Time).First()).ToList();
        }

        // GET: api/TemperatureDatas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TemperatureData>> GetTemperatureData(int id)
        {
            var temperatureData = await _context.Data.FindAsync(id);

            if (temperatureData == null)
            {
                return NotFound();
            }

            return temperatureData;
        }

        // POST: api/TemperatureDatas
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<TemperatureData>> PostTemperatureData(TemperatureData temperatureData)
        {
            _context.Data.Add(temperatureData);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTemperatureData", new { id = temperatureData.Id }, temperatureData);
        }

        // PUT: api/TemperatureDatas/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTemperatureData(int id, TemperatureData temperatureData)
        {
            if (id != temperatureData.Id)
            {
                return BadRequest();
            }

            _context.Entry(temperatureData).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TemperatureDataExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        private bool TemperatureDataExists(int id)
        {
            return _context.Data.Any(e => e.Id == id);
        }
    }
}