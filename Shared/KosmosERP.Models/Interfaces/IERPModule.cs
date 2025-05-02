
namespace KosmosERP.Models.Interfaces;

public interface IERPModule<DBModel, DTOModel, ListDTOModel, CreateCommandModel, EditCommandModel, DeleteCommandModel, FindCommandModel>
{
    DBModel? Get(int object_id);
    Task<DBModel?> GetAsync(int object_id);
    Task<Response<DTOModel>> GetDto(int object_id);
    Task<Response<DTOModel>> Create(CreateCommandModel commandModel);
    Task<Response<DTOModel>> Edit(EditCommandModel commandModel);
    Task<Response<DTOModel>> Delete(DeleteCommandModel commandModel);
    Task<PagingResult<ListDTOModel>> Find(PagingSortingParameters parameters, FindCommandModel commandModel);
    Task<Response<List<ListDTOModel>>> GlobalSearch(PagingSortingParameters parameters, string wildcard);
    Task<ListDTOModel> MapToListDto(DBModel databaseModel);
    Task<DTOModel> MapToDto(DBModel databaseModel);
    DBModel MapToDatabaseModel(DTOModel dtoModel);
}
