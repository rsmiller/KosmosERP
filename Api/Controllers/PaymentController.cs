using KosmosERP.Api.Authorization;
using KosmosERP.Api.Models;
using Microsoft.AspNetCore.Mvc;
using KosmosERP.BusinessLayer.Models.Module.Payment.Dto;
using KosmosERP.BusinessLayer.Models.Module.Payment.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.Payment.Command.Delete;
using KosmosERP.BusinessLayer.Models.Module.Payment.Command.Edit;
using KosmosERP.BusinessLayer.Models.Module.Payment.Command.Find;
using KosmosERP.BusinessLayer.Modules;
using KosmosERP.Models;
using KosmosERP.Module;
using KosmosERP.BusinessLayer.Models.Module.User.ListProfiles;
using KosmosERP.BusinessLayer.Models.Module.Payment;
using KosmosERP.BusinessLayer.PaymentProviders.Models;

namespace KosmosERP.Api.Controllers;

//[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class PaymentController : ERPApiController
{
    private IPaymentModule _Module;

    public PaymentController(IPaymentModule module) : base(module)
    {
        _Module = module;
    }

    
    [ERPAuthorize(new[] { ERPPermission.Read }, "payment_read")]
    [HttpGet("GetPayment", Name = "GetPayment")]
    [ProducesResponseType(typeof(Response<PaymentDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Get([FromQuery] int id)
    {
        var result = await _Module.GetDto(id);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Read }, "payment_read")]
    [HttpGet("GetPaymentByGuid", Name = "GetPaymentGuid")]
    [ProducesResponseType(typeof(Response<PaymentDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> GetByGuid([FromQuery] string guid)
    {
        var result = await _Module.GetDtoByGuid(guid);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Read }, "payment_read")]
    [HttpPost("FindPayment", Name = "FindPayment")]
    [ProducesResponseType(typeof(PagingResult<PaymentListDto>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> Find([FromQuery] GeneralListProfile listProfile, [FromBody] PaymentFindCommand command)
    {
        try
        {
            if (command != null)
            {
                command.calling_user_id = this.CurrentUserId;

                var sortingParams = new PagingSortingParameters(listProfile.Start, listProfile.ResultCount, listProfile.SortOrder);

                var result = await _Module.Find(sortingParams, command);

                return Ok(result);
            }
            else
            {
                return StatusCode(500, "Api body is null");
            }
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }

    [ERPAuthorize(new[] { ERPPermission.Write }, "payment_write")]
    [HttpPost("CreatePayment", Name = "CreatePayment")]
    [ProducesResponseType(typeof(Response<PaymentDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Create([FromBody] PaymentCreateCommand createCommand)
    {
        createCommand.calling_user_id = this.CurrentUserId;

        var result = await _Module.Create(createCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Edit }, "payment_edit")]
    [HttpPut("UpdatePayment", Name = "UpdatePayment")]
    [ProducesResponseType(typeof(Response<PaymentDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Edit([FromBody] PaymentEditCommand editCommand)
    {
        editCommand.calling_user_id = this.CurrentUserId;

        var result = await _Module.Edit(editCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Delete }, "payment_delete")]
    [HttpPost("DeletePayment", Name = "DeletePayment")]
    [ProducesResponseType(typeof(Response<PaymentDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Delete([FromBody] PaymentDeleteCommand deleteCommand)
    {
        deleteCommand.calling_user_id = this.CurrentUserId;

        var result = await _Module.Delete(deleteCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Write }, "payment_write")]
    [HttpPost("GetStripePaymentIntentFromARInvoce", Name = "GetStripePaymentIntentFromARInvoce")]
    [ProducesResponseType(typeof(string), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> GetStripePaymentIntentFromARInvoce([FromBody] CreateStripePaymentIntentCommand commandModel)
    {
        var result = await _Module.GetStripePaymentIntentFromARInvoce(commandModel);

        if (!result.Success)
            return BadRequest(result);
            
        return Ok(result);
    }


    [ERPAuthorize(new[] { ERPPermission.Read }, "payment_read")]
    [HttpPost("GetStripePaymentIntentStatus", Name = "GetStripePaymentIntentStatus")]
    [ProducesResponseType(typeof(string), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> GetStripePaymentIntentStatus([FromBody] StripePaymentIntentStatusCommand commandModel)
    {
        var result = await _Module.GetStripePaymentIntentStatus(commandModel);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Read }, "payment_read")]
    [HttpPost("GetSavedPaymentMethods", Name = "GetSavedPaymentMethods")]
    [ProducesResponseType(typeof(Response<SavedPaymentMethodsDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> GetSavedPaymentMethods([FromBody] GetSavedPaymentMethodsCommand commandModel)
    {
        var result = await _Module.GetSavedPaymentMethods(commandModel);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Write }, "payment_write")]
    [HttpPost("PayStripePaymentIntent", Name = "PayStripePaymentIntent")]
    [ProducesResponseType(typeof(Response<PaymentProviderTransactionGetDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> PayStripePaymentIntent([FromBody] StripePaymentIntentStatusCommand commandModel)
    {
        var result = await _Module.PayStripePaymentIntent(commandModel);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Write }, "payment_write")]
    [HttpPost("CreateStripeNewCard", Name = "CreateStripeNewCard")]
    [ProducesResponseType(typeof(Response<PaymentCardGetDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> CreateNewCardIntent([FromBody] CreateStripeNewCardIntentCommand commandModel)
    {
        var result = await _Module.CreateNewCardIntent(commandModel);

        return Ok(result);
    }

}
