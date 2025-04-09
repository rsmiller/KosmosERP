using Prometheus.Database;
using Prometheus.Database.Models;
using Prometheus.Models;
using Prometheus.Models.Interfaces;
using Prometheus.Module;
using Prometheus.BusinessLayer.Models.Module.DocumentUpload.Command.Create;
using Prometheus.BusinessLayer.Models.Module.DocumentUpload.Command.Delete;
using Prometheus.BusinessLayer.Models.Module.DocumentUpload.Command.Edit;
using Prometheus.BusinessLayer.Models.Module.DocumentUpload.Command.Find;
using Prometheus.BusinessLayer.Models.Module.DocumentUpload.Dto;
using Microsoft.AspNetCore.Http;

namespace Prometheus.BusinessLayer.Modules;

public interface IDocumentUploadModule : IERPModule<DocumentUpload, DocumentUploadDto, DocumentUploadListDto, DocumentUploadCreateCommand, DocumentUploadEditCommand, DocumentUploadDeleteCommand, DocumentUploadFindCommand>, IBaseERPModule
{
	Task<Response<DocumentUploadDto>> CreateOverride(IFormFile file, DocumentUploadCreateCommand commandModel);
    
}

public class DocumentUploadModule : BaseERPModule, IDocumentUploadModule
{
	public override Guid ModuleIdentifier => Guid.Parse("4b0ce064-9c4b-4e39-8812-79cc3f69e945");
	public override string ModuleName => "DocumentUpload";

	private IBaseERPContext _Context;
	private IFileStorageSettings? _StorageSettings;

    public DocumentUploadModule(IBaseERPContext context) : base(context)
    {
        _Context = context;
    }

    public DocumentUploadModule(IBaseERPContext context, IFileStorageSettings storageSettings) : base(context)
	{
		_Context = context;
        _StorageSettings = storageSettings;

    }

	public override void SeedPermissions()
	{
	}

    public async Task<Response<DocumentUploadDto>> CreateOverride(IFormFile file, DocumentUploadCreateCommand commandModel)
	{
        throw new NotImplementedException();
    }


    public async Task<Response<DocumentUploadDto>> Create(DocumentUploadCreateCommand commandModel)
	{
		throw new NotImplementedException();
	}

	public async Task<Response<DocumentUploadDto>> Delete(DocumentUploadDeleteCommand commandModel)
	{
		throw new NotImplementedException();
	}

	public async Task<Response<DocumentUploadDto>> Edit(DocumentUploadEditCommand commandModel)
	{
		throw new NotImplementedException();
	}

	public async Task<PagingResult<DocumentUploadListDto>> Find(PagingSortingParameters parameters, DocumentUploadFindCommand commandModel)
	{
		throw new NotImplementedException();
	}

	public DocumentUpload? Get(int object_id)
	{
		throw new NotImplementedException();
	}

	public async Task<DocumentUpload?> GetAsync(int object_id)
	{
		throw new NotImplementedException();
	}

	public async Task<Response<DocumentUploadDto>> GetDto(int object_id)
	{
		var s = _StorageSettings == null ? "" : "asdasdasd";

		return new Response<DocumentUploadDto>(s, ResultCode.AlreadyExists);
		//throw new NotImplementedException();
	}

	public async Task<Response<List<DocumentUploadListDto>>> GlobalSearch(PagingSortingParameters parameters, string wildcard)
	{
		throw new NotImplementedException();
	}

	public DocumentUpload MapToDatabaseModel(DocumentUploadDto dtoModel)
	{
		throw new NotImplementedException();
	}

	public async Task<DocumentUploadDto> MapToDto(DocumentUpload databaseModel)
	{
		throw new NotImplementedException();
	}

	public async Task<DocumentUploadListDto> MapToListDto(DocumentUpload databaseModel)
	{
		throw new NotImplementedException();
	}
}
