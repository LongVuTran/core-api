using AutoMapper;
using CoreCodeCamp.Data;
using CoreCodeCamp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreCodeCamp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CampsController : ControllerBase
    {
        private readonly ICampRepository repository;
        private readonly IMapper mapper;
        private readonly LinkGenerator linkGenerator;

        public CampsController(ICampRepository repository, IMapper mapper, LinkGenerator linkGenerator)
        {
            this.repository = repository;
            this.mapper = mapper;
            this.linkGenerator = linkGenerator;
        }

        [HttpGet]
        public async Task<ActionResult<CampModel[]>> Get(bool includeTalks = false)
        {
            try
            {
                var results = await repository.GetAllCampsAsync(includeTalks);

                return mapper.Map<CampModel[]>(results);

            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        [HttpGet("{moniker}")]
        public async Task<ActionResult<CampModel>> Get(string moniker)
        {
            try
            {
                var result = await repository.GetCampAsync(moniker);

                if (result == null) return NotFound();
                return mapper.Map<CampModel>(result);
            }
            catch (Exception)
            {
                
                return this.StatusCode(StatusCodes.Status400BadRequest, "Database Failure");
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<CampModel[]>> SearchByDate(DateTime theDate, bool includeTalks = false)
        {
            try
            {
                var results = await repository.GetAllCampsByEventDate(theDate, includeTalks);
                if (!results.Any()) return NotFound();
                return mapper.Map<CampModel[]>(results);
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status400BadRequest, "Database Failure");
            }
        }

        public async Task<ActionResult<CampModel>> Post(CampModel model)
        {
            try
            {
                var location = linkGenerator.GetPathByAction("Get",
                    "Camp",
                    new { moniker = model.Moniker });

                if (string.IsNullOrWhiteSpace(location))
                {
                    return BadRequest("Could not use current moniker");
                }

                // create a new Camp
                var camp = mapper.Map<Camp>(model);
                repository.Add(camp);
                if (await repository.SaveChangesAsync())
                {
                    return Created($"/api/camps.{camp.Moniker}", mapper.Map<CampModel>(camp));
                }
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status400BadRequest, "Database Failure");
            }

            return BadRequest();
        }
    }
}
