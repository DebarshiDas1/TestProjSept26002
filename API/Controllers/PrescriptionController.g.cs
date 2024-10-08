using Microsoft.AspNetCore.Mvc;
using TestProjSept26002.Models;
using TestProjSept26002.Services;
using TestProjSept26002.Entities;
using TestProjSept26002.Filter;
using TestProjSept26002.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Task = System.Threading.Tasks.Task;
using TestProjSept26002.Authorization;

namespace TestProjSept26002.Controllers
{
    /// <summary>
    /// Controller responsible for managing prescription related operations.
    /// </summary>
    /// <remarks>
    /// This Controller provides endpoints for adding, retrieving, updating, and deleting prescription information.
    /// </remarks>
    [Route("api/prescription")]
    [Authorize]
    public class PrescriptionController : BaseApiController
    {
        private readonly IPrescriptionService _prescriptionService;

        /// <summary>
        /// Initializes a new instance of the PrescriptionController class with the specified context.
        /// </summary>
        /// <param name="iprescriptionservice">The iprescriptionservice to be used by the controller.</param>
        public PrescriptionController(IPrescriptionService iprescriptionservice)
        {
            _prescriptionService = iprescriptionservice;
        }

        /// <summary>Adds a new prescription</summary>
        /// <param name="model">The prescription data to be added</param>
        /// <returns>The result of the operation</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        [UserAuthorize("Prescription", Entitlements.Create)]
        public async Task<IActionResult> Post([FromBody] Prescription model)
        {
            model.TenantId = TenantId;
            model.CreatedOn = DateTime.UtcNow;
            model.CreatedBy = UserId;
            var id = await _prescriptionService.Create(model);
            return Ok(new { id });
        }

        /// <summary>Retrieves a list of prescriptions based on specified filters</summary>
        /// <param name="filters">The filter criteria in JSON format. Use the following format: [{"PropertyName": "PropertyName", "Operator": "Equal", "Value": "FilterValue"}] </param>
        /// <param name="searchTerm">To searching data.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="sortField">The entity's field name to sort.</param>
        /// <param name="sortOrder">The sort order asc or desc.</param>
        /// <returns>The filtered list of prescriptions</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Produces("application/json")]
        [UserAuthorize("Prescription", Entitlements.Read)]
        public async Task<IActionResult> Get([FromQuery] string filters, string searchTerm, int pageNumber = 1, int pageSize = 10, string sortField = null, string sortOrder = "asc")
        {
            List<FilterCriteria> filterCriteria = null;
            if (pageSize < 1)
            {
                return BadRequest("Page size invalid.");
            }

            if (pageNumber < 1)
            {
                return BadRequest("Page mumber invalid.");
            }

            if (!string.IsNullOrEmpty(filters))
            {
                filterCriteria = JsonHelper.Deserialize<List<FilterCriteria>>(filters);
            }

            var result = await _prescriptionService.Get(filterCriteria, searchTerm, pageNumber, pageSize, sortField, sortOrder);
            return Ok(result);
        }

        /// <summary>Retrieves a specific prescription by its primary key</summary>
        /// <param name="id">The primary key of the prescription</param>
        /// <param name="fields">The fields is fetch data of selected fields</param>
        /// <returns>The prescription data</returns>
        [HttpGet]
        [Route("{id:Guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        [UserAuthorize("Prescription", Entitlements.Read)]
        public async Task<IActionResult> GetById([FromRoute] Guid id, string fields = null)
        {
            var result = await _prescriptionService.GetById( id, fields);
            return Ok(result);
        }

        /// <summary>Deletes a specific prescription by its primary key</summary>
        /// <param name="id">The primary key of the prescription</param>
        /// <returns>The result of the operation</returns>
        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Produces("application/json")]
        [Route("{id:Guid}")]
        [UserAuthorize("Prescription", Entitlements.Delete)]
        public async Task<IActionResult> DeleteById([FromRoute] Guid id)
        {
            var status = await _prescriptionService.Delete(id);
            return Ok(new { status });
        }

        /// <summary>Updates a specific prescription by its primary key</summary>
        /// <param name="id">The primary key of the prescription</param>
        /// <param name="updatedEntity">The prescription data to be updated</param>
        /// <returns>The result of the operation</returns>
        [HttpPut]
        [Route("{id:Guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Produces("application/json")]
        [UserAuthorize("Prescription", Entitlements.Update)]
        public async Task<IActionResult> UpdateById(Guid id, [FromBody] Prescription updatedEntity)
        {
            if (id != updatedEntity.Id)
            {
                return BadRequest("Mismatched Id");
            }

            updatedEntity.TenantId = TenantId;
            updatedEntity.UpdatedOn = DateTime.UtcNow;
            updatedEntity.UpdatedBy = UserId;
            var status = await _prescriptionService.Update(id, updatedEntity);
            return Ok(new { status });
        }

        /// <summary>Updates a specific prescription by its primary key</summary>
        /// <param name="id">The primary key of the prescription</param>
        /// <param name="updatedEntity">The prescription data to be updated</param>
        /// <returns>The result of the operation</returns>
        [HttpPatch]
        [Route("{id:Guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Produces("application/json")]
        [UserAuthorize("Prescription", Entitlements.Update)]
        public async Task<IActionResult> UpdateById(Guid id, [FromBody] JsonPatchDocument<Prescription> updatedEntity)
        {
            if (updatedEntity == null)
                return BadRequest("Patch document is missing.");
            var status = await _prescriptionService.Patch(id, updatedEntity);
            return Ok(new { status });
        }
    }
}