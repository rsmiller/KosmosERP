using KosmosERP.Database.Models;
using KosmosERP.Database;
using KosmosERP.Models.Helpers;
using KosmosERP.Models.Interfaces;
using KosmosERP.Models;
using KosmosERP.Module;
using Microsoft.EntityFrameworkCore;
using KosmosERP.BusinessLayer.Models.Module.Opportunity.Dto;
using KosmosERP.BusinessLayer.Models.Module.Opportunity.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.Opportunity.Command.Edit;
using KosmosERP.BusinessLayer.Models.Module.Opportunity.Command.Delete;
using KosmosERP.BusinessLayer.Models.Module.Opportunity.Command.Find;
using KosmosERP.BusinessLayer.Helpers;

namespace KosmosERP.BusinessLayer.Modules;

public interface IOpportunityModule : IERPModule<
    Opportunity,
    OpportunityDto,
    OpportunityListDto,
    OpportunityCreateCommand,
    OpportunityEditCommand,
    OpportunityDeleteCommand,
    OpportunityFindCommand>, IBaseERPModule
{
    Task<OpportunityLineDto> MapToLineDto(OpportunityLine databaseModel);
    Task<Response<OpportunityLineDto>> CreateLine(OpportunityLineCreateCommand commandModel);
    Task<Response<OpportunityLineDto>> EditLine(OpportunityLineEditCommand commandModel);
    Task<Response<OpportunityLineDto>> DeleteLine(OpportunityLineDeleteCommand commandModel);
}

public class OpportunityModule : BaseERPModule, IOpportunityModule
{
    public override Guid ModuleIdentifier => Guid.Parse("0c3959c3-15dc-44ab-8e2c-9b9e2773e65f");
    public override string ModuleName => "Opportunities";

    private readonly IBaseERPContext _Context;
    private IMemoryCacheService<KeyValueStore> _KVMemoryService;

    public OpportunityModule(IBaseERPContext context, ILogProviderFactory logProviderFactory) : base(context, logProviderFactory)
    {
        _Context = context;
    }

    public OpportunityModule(IBaseERPContext context, IMemoryCacheService<KeyValueStore> kvMService, 
                                ILogProviderFactory logProviderFactory) : base(context, logProviderFactory)
    {
        _Context = context;
        _KVMemoryService = kvMService;
    }

    public override void SeedPermissions()
    {
        var role = _Context.Roles.Any(m => m.name == "CRM Administrators");

        if (role == false)
        {
            _Context.Roles.Add(CommonDataHelper<Role>.FillCommonFields(new Role()
            {
                name = "CRM Administrators",
            }, 1));

            _Context.SaveChanges();

            base.CreateFirstRunRolePermissions();
        }

        
        var prospecting_stage = _Context.KeyValueStores.Where(m => m.module_id == this.ModuleIdentifier.ToString()
                                    && m.key == "opporunity_stage_prospecting").SingleOrDefault();
        var qualifying_stage = _Context.KeyValueStores.Where(m => m.module_id == this.ModuleIdentifier.ToString()
                                    && m.key == "opporunity_stage_qualifying").SingleOrDefault();
        var analysis_stage = _Context.KeyValueStores.Where(m => m.module_id == this.ModuleIdentifier.ToString()
                                    && m.key == "opporunity_stage_analysis").SingleOrDefault();
        var proposition_stage = _Context.KeyValueStores.Where(m => m.module_id == this.ModuleIdentifier.ToString()
                                    && m.key == "opporunity_stage_proposition").SingleOrDefault();
        var proposal_stage = _Context.KeyValueStores.Where(m => m.module_id == this.ModuleIdentifier.ToString()
                                    && m.key == "opporunity_stage_proposal").SingleOrDefault();
        var negotiation_stage = _Context.KeyValueStores.Where(m => m.module_id == this.ModuleIdentifier.ToString()
                                    && m.key == "opporunity_stage_negotiation").SingleOrDefault();
        var closed_won_stage = _Context.KeyValueStores.Where(m => m.module_id == this.ModuleIdentifier.ToString()
                                    && m.key == "opporunity_stage_closed_won").SingleOrDefault();
        var closed_lost_stage = _Context.KeyValueStores.Where(m => m.module_id == this.ModuleIdentifier.ToString()
                                    && m.key == "opporunity_stage_closed_lost").SingleOrDefault();

        if (prospecting_stage == null)
        {
            _Context.KeyValueStores.Add(CommonDataHelper<KeyValueStore>.FillCommonFields(new KeyValueStore()
            {
                key = "opporunity_stage_prospecting",
                value = "Prospecting",
                module_id = this.ModuleIdentifier.ToString()
            }, 1));

            _Context.SaveChanges();
        }

        if (qualifying_stage == null)
        {
            _Context.KeyValueStores.Add(CommonDataHelper<KeyValueStore>.FillCommonFields(new KeyValueStore()
            {
                key = "opporunity_stage_qualifying",
                value = "Qualifying",
                module_id = this.ModuleIdentifier.ToString()
            }, 1));

            _Context.SaveChanges();
        }

        if (analysis_stage == null)
        {
            _Context.KeyValueStores.Add(CommonDataHelper<KeyValueStore>.FillCommonFields(new KeyValueStore()
            {
                key = "opporunity_stage_analysis",
                value = "Analysis",
                module_id = this.ModuleIdentifier.ToString()
            }, 1));

            _Context.SaveChanges();
        }

        if (proposition_stage == null)
        {
            _Context.KeyValueStores.Add(CommonDataHelper<KeyValueStore>.FillCommonFields(new KeyValueStore()
            {
                key = "opporunity_stage_proposition",
                value = "Proposition",
                module_id = this.ModuleIdentifier.ToString()
            }, 1));

            _Context.SaveChanges();
        }

        if (proposal_stage == null)
        {
            _Context.KeyValueStores.Add(CommonDataHelper<KeyValueStore>.FillCommonFields(new KeyValueStore()
            {
                key = "opporunity_stage_proposal",
                value = "Proposal",
                module_id = this.ModuleIdentifier.ToString()
            }, 1));

            _Context.SaveChanges();
        }

        if (closed_won_stage == null)
        {
            _Context.KeyValueStores.Add(CommonDataHelper<KeyValueStore>.FillCommonFields(new KeyValueStore()
            {
                key = "opporunity_stage_closed_won",
                value = "Closed Won",
                module_id = this.ModuleIdentifier.ToString()
            }, 1));

            _Context.SaveChanges();
        }

        if (closed_lost_stage == null)
        {
            _Context.KeyValueStores.Add(CommonDataHelper<KeyValueStore>.FillCommonFields(new KeyValueStore()
            {
                key = "opporunity_stage_closed_lost",
                value = "Closed Lost",
                module_id = this.ModuleIdentifier.ToString()
            }, 1));

            _Context.SaveChanges();
        }
    }

    public Opportunity? Get(int object_id)
    {
        return _Context.Opportunities
            .SingleOrDefault(m => m.id == object_id);
    }

    public async Task<Opportunity?> GetAsync(int object_id)
    {
        return await _Context.Opportunities.Include("customer").Include("contact")
            .SingleOrDefaultAsync(m => m.id == object_id);
    }

    public OpportunityLine? GetLine(int object_id)
    {
        return _Context.OpportunityLines.SingleOrDefault(m => m.id == object_id);
    }

    public async Task<OpportunityLine?> GetLineAsync(int object_id)
    {
        return await _Context.OpportunityLines.SingleOrDefaultAsync(m => m.id == object_id);
    }

    public async Task<Response<OpportunityLineDto>> GetLineDto(int object_id)
    {
        var entity = await GetLineAsync(object_id);
        if (entity == null)
            return new Response<OpportunityLineDto>("Opportunity Line not found", ResultCode.NotFound);

        var dto = await MapToLineDto(entity);
        return new Response<OpportunityLineDto>(dto);
    }

    public async Task<Response<OpportunityDto>> GetDto(int object_id)
    {
        var entity = await GetAsync(object_id);
        if (entity == null)
            return new Response<OpportunityDto>("Opportunity not found", ResultCode.NotFound);

        var dto = await MapToDto(entity);
        return new Response<OpportunityDto>(dto);
    }

    public async Task<Response<OpportunityDto>> GetDtoByGuid(string guid)
    {
        var entity = await _Context.Opportunities.Include("customer").Include("contact").FirstOrDefaultAsync(c => c.guid == guid && !c.is_deleted);
        if (entity == null)
            return new Response<OpportunityDto>("Opportunity not found", ResultCode.NotFound);

        var dto = await MapToDto(entity);
        return new Response<OpportunityDto>(dto);
    }

    public async Task<Response<OpportunityDto>> Create(OpportunityCreateCommand commandModel)
    {
        Response<OpportunityDto> response = new Response<OpportunityDto>();

        try
        {
            var validationResult = ModelValidationHelper.ValidateModel(commandModel);
            if (!validationResult.Success)
                return new Response<OpportunityDto>(validationResult.Exception, ResultCode.DataValidationError);

            var record = this.MapToDatabaseModel(commandModel);
            record.owner_id = commandModel.calling_user_id;

            _Context.Opportunities.Add(record);
            await _Context.SaveChangesAsync();


            foreach (var order_line in commandModel.opportunity_lines)
            {
                var db_line = MapToLineDatabaseModel(order_line, record.id, commandModel.calling_user_id);

                await _Context.OpportunityLines.AddAsync(db_line);
                await _Context.SaveChangesAsync();
            }

            response.Data = await MapToDto(record);
        }
        catch (Exception ex)
        {
            await LogError(50, this.GetType().Name, nameof(Find), ex);
            response.SetException(ex.Message, ResultCode.Error);
        }

        return response;
    }

    public async Task<Response<OpportunityDto>> Edit(OpportunityEditCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<OpportunityDto>(validationResult.Exception, ResultCode.DataValidationError);

        var existingEntity = await GetAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<OpportunityDto>("Opportunity not found", ResultCode.NotFound);

        try
        {
            if (!string.IsNullOrEmpty(commandModel.opportunity_name)
                && existingEntity.opportunity_name != commandModel.opportunity_name)
            {
                existingEntity.opportunity_name = commandModel.opportunity_name;
            }

            if (commandModel.customer_id.HasValue && existingEntity.customer_id != commandModel.customer_id)
                existingEntity.customer_id = commandModel.customer_id.Value;

            if (commandModel.contact_id.HasValue && existingEntity.contact_id != commandModel.contact_id)
                existingEntity.contact_id = commandModel.contact_id.Value;

            if (commandModel.amount.HasValue && existingEntity.amount != commandModel.amount)
                existingEntity.amount = commandModel.amount.Value;

            if (!string.IsNullOrEmpty(commandModel.stage)
                && existingEntity.stage != commandModel.stage)
            {
                existingEntity.stage = commandModel.stage;
            }

            if (commandModel.win_chance.HasValue && existingEntity.win_chance != commandModel.win_chance)
                existingEntity.win_chance = commandModel.win_chance.Value;

            if (commandModel.expected_close.HasValue && existingEntity.expected_close != commandModel.expected_close)
                existingEntity.expected_close = commandModel.expected_close.Value;

            if (existingEntity.owner_id != commandModel.owner_id)
                existingEntity.owner_id = commandModel.owner_id;

            existingEntity = CommonDataHelper<Opportunity>.FillUpdateFields(existingEntity, commandModel.calling_user_id);


            _Context.Opportunities.Update(existingEntity);
            await _Context.SaveChangesAsync();


            foreach (var opp_line in commandModel.opportunity_lines)
            {
                if (opp_line.id.HasValue)
                {
                    var existing_line = await _Context.OpportunityLines.Where(m => m.id == opp_line.id).SingleOrDefaultAsync();

                    if (existing_line != null)
                    {
                        if (opp_line.product_id.HasValue && existing_line.product_id != opp_line.product_id)
                            existing_line.product_id = opp_line.product_id.Value;

                        if (opp_line.line_number.HasValue && existing_line.line_number != opp_line.line_number)
                            existing_line.line_number = opp_line.line_number.Value;

                        if (opp_line.quantity.HasValue && existing_line.quantity != opp_line.quantity)
                            existing_line.quantity = opp_line.quantity.Value;

                        if (opp_line.unit_price.HasValue && existing_line.unit_price != opp_line.unit_price)
                            existing_line.unit_price = opp_line.unit_price.Value;

                        if (existing_line.description != opp_line.description)
                            existing_line.description = opp_line.description;


                        existing_line = CommonDataHelper<OpportunityLine>.FillUpdateFields(existing_line, commandModel.calling_user_id);

                        _Context.OpportunityLines.Update(existing_line);
                        await _Context.SaveChangesAsync();
                    }
                }
                else
                {
                    var db_line = MapToLineDatabaseModel(opp_line, existingEntity.id, commandModel.calling_user_id);

                    await _Context.OpportunityLines.AddAsync(db_line);
                    await _Context.SaveChangesAsync();
                }
            }

            var dto = await MapToDto(existingEntity);
            return new Response<OpportunityDto>(dto);
        }
        catch (Exception ex)
        {
            await LogError(50, this.GetType().Name, nameof(Find), ex);
            return new Response<OpportunityDto>(ex.Message, ResultCode.Error);
        }
        
    }

    public async Task<Response<OpportunityDto>> Delete(OpportunityDeleteCommand commandModel)
    {
        var existingEntity = await GetAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<OpportunityDto>("Opportunity not found", ResultCode.NotFound);


        // Do delete
        existingEntity = CommonDataHelper<Opportunity>.FillDeleteFields(existingEntity, commandModel.calling_user_id);


        _Context.Opportunities.Update(existingEntity);
        await _Context.SaveChangesAsync();

        var dto = await MapToDto(existingEntity);
        return new Response<OpportunityDto>(dto);
    }

    public async Task<PagingResult<OpportunityListDto>> Find(PagingSortingParameters parameters, OpportunityFindCommand commandModel)
    {
        var response = new PagingResult<OpportunityListDto>();
        response.Data = new List<OpportunityListDto>();

        try
        {

            var query = _Context.Opportunities
                .Where(m => m.is_deleted == false).Include("customer").Include("contact");

            // If wildcard is not empty, filter by string fields
            if (!string.IsNullOrEmpty(commandModel.wildcard))
            {
                var wild = commandModel.wildcard.ToLower();
                query = query.Where(m =>
                    (m.opportunity_name.ToLower().Contains(wild))
                    || (m.stage.ToLower().Contains(wild))
                    || (m.guid.ToLower().Contains(wild))
                );
            }

            // Sort and page
            var totalCount = await query.CountAsync();
            var pagedItems = await query.SortAndPageBy(parameters).ToListAsync();

            // Convert to DTO
            var dtos = new List<OpportunityListDto>();
            foreach (var item in pagedItems)
            {
                dtos.Add(await MapToListDto(item));
            }

            response.Data = dtos;
            response.TotalResultCount = totalCount;
        }
        catch (Exception ex)
        {
            await LogError(50, this.GetType().Name, nameof(Find), ex);
            response.SetException(ex.Message, ResultCode.Error);
            response.TotalResultCount = 0;
        }

        return response;
    }

    public async Task<Response<List<OpportunityListDto>>> GlobalSearch(GlobalSearchFindCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<List<OpportunityListDto>>(validationResult.Exception, ResultCode.DataValidationError);

        var response = new Response<List<OpportunityListDto>>();

        try
        {
            var query = _Context.Opportunities
                .Where(m => !m.is_deleted);

            if (!string.IsNullOrEmpty(commandModel.wildcard))
            {
                var lower = commandModel.wildcard.ToLower();
                query = query.Where(m =>
                    m.opportunity_name.ToLower().Contains(lower)
                    || m.stage.ToLower().Contains(lower)
                    || m.guid.ToLower().Contains(lower));
            }

            var pagedItems = await query.SortAndPageBy(commandModel.parameters).ToListAsync();
            var dtos = new List<OpportunityListDto>();
            foreach (var item in pagedItems)
            {
                dtos.Add(await MapToListDto(item));
            }

            response.Data = dtos;
        }
        catch (Exception ex)
        {
            await LogError(50, this.GetType().Name, nameof(GlobalSearch), ex);
            response.SetException(ex.Message, ResultCode.Error);
        }

        return response;
    }

    public async Task<Response<OpportunityLineDto>> CreateLine(OpportunityLineCreateCommand commandModel)
    {
        Response<OpportunityLineDto> response = new Response<OpportunityLineDto>();

        try
        {
            var validationResult = ModelValidationHelper.ValidateModel(commandModel);
            if (!validationResult.Success)
                return new Response<OpportunityLineDto>(validationResult.Exception, ResultCode.DataValidationError);


            var record = this.MapToLineDatabaseModel(commandModel, commandModel.opportunity_id, commandModel.calling_user_id);

            _Context.OpportunityLines.Add(record);
            await _Context.SaveChangesAsync();


            response.Data = await MapToLineDto(record);
        }
        catch (Exception ex)
        {
            await LogError(50, this.GetType().Name, nameof(Find), ex);
            response.SetException(ex.Message, ResultCode.Error);
        }

        return response;
    }

    public async Task<Response<OpportunityLineDto>> EditLine(OpportunityLineEditCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<OpportunityLineDto>(validationResult.Exception, ResultCode.DataValidationError);

        if(!commandModel.id.HasValue)
            return new Response<OpportunityLineDto>("Opportunity Line must contain an id", ResultCode.DataValidationError);

        var existingEntity = await GetLineAsync(commandModel.id.Value);
        if (existingEntity == null)
            return new Response<OpportunityLineDto>("Opportunity Line not found", ResultCode.NotFound);


        if (!string.IsNullOrEmpty(commandModel.description)
            && existingEntity.description != commandModel.description)
        {
            existingEntity.description = commandModel.description;
        }

        if (commandModel.line_number.HasValue && existingEntity.line_number != commandModel.line_number)
            existingEntity.line_number = commandModel.line_number.Value;

        if (commandModel.product_id.HasValue && existingEntity.product_id != commandModel.product_id)
            existingEntity.product_id = commandModel.product_id.Value;

        if (commandModel.quantity.HasValue && existingEntity.quantity != commandModel.quantity)
            existingEntity.quantity = commandModel.quantity.Value;

        if (commandModel.unit_price.HasValue && existingEntity.unit_price != commandModel.unit_price)
            existingEntity.unit_price = commandModel.unit_price.Value;


        existingEntity = CommonDataHelper<OpportunityLine>.FillUpdateFields(existingEntity, commandModel.calling_user_id);


        _Context.OpportunityLines.Update(existingEntity);
        await _Context.SaveChangesAsync();

        var dto = await MapToLineDto(existingEntity);
        return new Response<OpportunityLineDto>(dto);
    }

    public async Task<Response<OpportunityLineDto>> DeleteLine(OpportunityLineDeleteCommand commandModel)
    {
        var existingEntity = await GetLineAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<OpportunityLineDto>("Opportunity not found", ResultCode.NotFound);


        // Do delete
        existingEntity = CommonDataHelper<OpportunityLine>.FillDeleteFields(existingEntity, commandModel.calling_user_id);


        _Context.OpportunityLines.Update(existingEntity);
        await _Context.SaveChangesAsync();

        var dto = await MapToLineDto(existingEntity);
        return new Response<OpportunityLineDto>(dto);
    }


    public async Task<OpportunityListDto> MapToListDto(Opportunity databaseModel)
    {
        var dto = new OpportunityListDto
        {
            id = databaseModel.id,
            is_deleted = databaseModel.is_deleted,
            created_on = databaseModel.created_on,
            created_by = databaseModel.created_by,
            updated_on = databaseModel.updated_on,
            updated_by = databaseModel.updated_by,
            deleted_on = databaseModel.deleted_on,
            deleted_by = databaseModel.deleted_by,
            created_on_string = databaseModel.created_on_string,
            created_on_timezone = databaseModel.created_on_timezone,
            updated_on_string = databaseModel.updated_on_string,
            updated_on_timezone = databaseModel.updated_on_timezone,
            deleted_on_string = databaseModel.deleted_on_string,
            deleted_on_timezone = databaseModel.deleted_on_timezone,
            opportunity_name = databaseModel.opportunity_name,
            customer_id = databaseModel.customer_id,
            contact_id = databaseModel.contact_id,
            amount = databaseModel.amount,
            stage = databaseModel.stage,
            win_chance = databaseModel.win_chance,
            expected_close = databaseModel.expected_close,
            owner_id = databaseModel.owner_id,
            guid = databaseModel.guid
        };

        dto.customer_name = databaseModel.customer.customer_name;
        dto.contact_name = databaseModel.contact.first_name + " " + databaseModel.contact.last_name;
        dto.owner_name = await _Context.Users.Where(m => m.external_id == databaseModel.owner_id).Select(m => m.first_name + " " + m.last_name).SingleOrDefaultAsync();

        var stage_val = await _KVMemoryService.GetKeyValue(databaseModel.stage);
        if (stage_val != null)
            dto.stage_name = stage_val.value;

        return dto;
    }

    public async Task<OpportunityDto> MapToDto(Opportunity databaseModel)
    {
        var dto = new OpportunityDto
        {
            id = databaseModel.id,
            is_deleted = databaseModel.is_deleted,
            created_on = databaseModel.created_on,
            created_by = databaseModel.created_by,
            updated_on = databaseModel.updated_on,
            updated_by = databaseModel.updated_by,
            deleted_on = databaseModel.deleted_on,
            deleted_by = databaseModel.deleted_by,
            created_on_string = databaseModel.created_on_string,
            created_on_timezone = databaseModel.created_on_timezone,
            updated_on_string = databaseModel.updated_on_string,
            updated_on_timezone = databaseModel.updated_on_timezone,
            deleted_on_string = databaseModel.deleted_on_string,
            deleted_on_timezone = databaseModel.deleted_on_timezone,
            opportunity_name = databaseModel.opportunity_name,
            customer_id = databaseModel.customer_id,
            contact_id = databaseModel.contact_id,
            amount = databaseModel.amount,
            stage = databaseModel.stage,
            win_chance = databaseModel.win_chance,
            expected_close = databaseModel.expected_close,
            owner_id = databaseModel.owner_id,
            guid = databaseModel.guid
        };

        var lines = await _Context.OpportunityLines.Where(m => m.opportunity_id == databaseModel.id && !m.is_deleted).ToListAsync();
        foreach (var line in lines)
        {
            dto.opportunity_lines.Add(await this.MapToLineDto(line));
        }


        if (databaseModel.customer != null && databaseModel.contact != null)
        {
            dto.customer_name = databaseModel.customer.customer_name;
            dto.contact_name = databaseModel.contact.first_name + " " + databaseModel.contact.last_name;
        }
        else
        {
            dto.customer_name = await _Context.Customers.Where(m => m.id == databaseModel.customer_id).Select(m => m.customer_name).SingleOrDefaultAsync();
            dto.contact_name = await _Context.Contacts.Where(m => m.id == databaseModel.contact_id).Select(m => m.first_name + " " + m.last_name).SingleOrDefaultAsync();
        }


        dto.owner_name = await _Context.Users.Where(m => m.external_id == databaseModel.owner_id).Select(m => m.first_name + " " + m.last_name).SingleOrDefaultAsync();

        var stage_val = await _KVMemoryService.GetKeyValue(databaseModel.stage);
        if (stage_val != null)
            dto.stage_name = stage_val.value;


        return dto;
    }

    public async Task<OpportunityLineDto> MapToLineDto(OpportunityLine databaseModel)
    {
        var dto = new OpportunityLineDto
        {
            id = databaseModel.id,
            is_deleted = databaseModel.is_deleted,
            created_on = databaseModel.created_on,
            created_by = databaseModel.created_by,
            updated_on = databaseModel.updated_on,
            updated_by = databaseModel.updated_by,
            deleted_on = databaseModel.deleted_on,
            deleted_by = databaseModel.deleted_by,
            created_on_string = databaseModel.created_on_string,
            created_on_timezone = databaseModel.created_on_timezone,
            updated_on_string = databaseModel.updated_on_string,
            updated_on_timezone = databaseModel.updated_on_timezone,
            deleted_on_string = databaseModel.deleted_on_string,
            deleted_on_timezone = databaseModel.deleted_on_timezone,
            guid = databaseModel.guid,
            opportunity_id = databaseModel.opportunity_id,
            product_id = databaseModel.product_id,
            description = databaseModel.description,
            line_number = databaseModel.line_number,
            quantity = databaseModel.quantity,
            unit_price = databaseModel.unit_price,
        };

        if (databaseModel.product_id.HasValue)
        {
            var associated_product = await _Context.Products.Where(m => m.id == databaseModel.product_id).SingleOrDefaultAsync();

            if (associated_product != null)
            {
                dto.product_name = associated_product.product_name;
                dto.identifier1 = associated_product.identifier1;
            }
        }


        return dto;
    }

    public Opportunity MapToDatabaseModel(OpportunityDto dtoModel)
    {
        return new Opportunity
        {
            id = dtoModel.id,
            opportunity_name = dtoModel.opportunity_name,
            customer_id = dtoModel.customer_id,
            contact_id = dtoModel.contact_id,
            amount = dtoModel.amount,
            stage = dtoModel.stage,
            win_chance = dtoModel.win_chance,
            expected_close = dtoModel.expected_close,
            owner_id = dtoModel.owner_id,
        };
    }

    public Opportunity MapToDatabaseModel(OpportunityCreateCommand createCommand)
    {
        return CommonDataHelper<Opportunity>.FillCommonFields(new Opportunity
        {
            opportunity_name = createCommand.opportunity_name,
            customer_id = createCommand.customer_id,
            contact_id = createCommand.contact_id,
            amount = createCommand.amount,
            stage = createCommand.stage,
            win_chance = createCommand.win_chance,
            expected_close = createCommand.expected_close,
        }, createCommand.calling_user_id);

    }

    public OpportunityLine MapToLineDatabaseModel(OpportunityLineCreateCommand createCommand, int opportunity_id, string calling_user_id)
    {
        return CommonDataHelper<OpportunityLine>.FillCommonFields(new OpportunityLine
        {
            opportunity_id = opportunity_id,
            product_id = createCommand.product_id,
            description = createCommand.description,
            line_number = createCommand.line_number,
            quantity = createCommand.quantity,
            unit_price = createCommand.unit_price,
        }, calling_user_id);

    }
    
    public OpportunityLine MapToLineDatabaseModel(OpportunityLineEditCommand createCommand, int opportunity_id, string calling_user_id)
    {
        return CommonDataHelper<OpportunityLine>.FillCommonFields(new OpportunityLine
        {
            opportunity_id = opportunity_id,
            product_id = createCommand.product_id,
            description = createCommand.description,
            line_number = createCommand.line_number.Value,
            quantity = createCommand.quantity.Value,
            unit_price = createCommand.unit_price.Value,
        }, calling_user_id);
        
    }
}
