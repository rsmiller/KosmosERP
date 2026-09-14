"use client"

import '../../styles/page.component.css'


import { AllCommunityModule, ColDef, ModuleRegistry } from "ag-grid-community";
import { Alert, Box, Button, createTreeCollection, DataList, Field, FileUpload, For, Grid, GridItem, Icon, Input, Spinner, Stack, Tabs, TreeView } from '@chakra-ui/react'
import { useRouter } from 'next/navigation';
import { LuFolder, LuUpload } from 'react-icons/lu';
import { AgGridReact } from 'ag-grid-react';
import { useEffect, useState, useRef } from 'react';
import { DocumentUploadCreateCommand, DocumentUploadDto, DocumentUploadFindCommand, DocumentUploadObjectTagTemplate, DocumentUploadRevisionDto, DocumentUploadRevisionTagCreateCommand } from '@/models/document-models';
import { DateTimeRender } from '@/components/ag-grid/date-time-renderer';
import { useForm } from 'react-hook-form';
import { documentService } from '@/services/document-service';
import { useKeycloak } from '@react-keycloak/web';
import { permissionsService, ERPModules, ERPModulePermission } from '@/services/permissions-service';

ModuleRegistry.registerModules([AllCommunityModule]);


function DocumentsPage() {

  const router = useRouter();
  const { keycloak } = useKeycloak();
  const [hasAccess, setHasAccess] = useState(true);

  const {
      register,
      formState: { errors, isValid },
      watch,
  } = useForm<any>();

  const [currentTab, setCurrentTab] = useState("tab-1");

  const [loading, setLoading] = useState(false);
  const [rowData, setRowData] = useState<DocumentUploadDto[]>([]);

  const [fileData, setFileData] = useState<Blob | null>(null);
  const [fileUrl, setFileUrl] = useState<string | null>(null);
  const [fileType, setFileType] = useState<string>('');

  const [categoryData, setCategoryData] = useState<Node>();

  const [revisionDto, setRevisionDto] = useState<DocumentUploadRevisionDto | undefined>();
  const [documentDto, setDocumentDto] = useState<DocumentUploadDto | undefined>();

  const [categoryNode, setCategoryNode] = useState<Node | undefined>();

  const [canUpload, setCanUpload] = useState<boolean>(false);

  const [isWorking, setIsWorking] = useState(false);
  const [hasFile, setHasFile] = useState(false);
  const [hasUpdated, setHasUpdated] = useState(false);
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const hasInitialized = useRef(false);
  
  const [displayGoodAlert, setDisplayGoodAlert] = useState(false);
  const [displayBadAlert, setDisplayBadAlert] = useState(false);

   useEffect(() => {
    if (keycloak.authenticated == false) return;

    // Check permission
    const realmRoles = keycloak?.tokenParsed?.realm_access?.roles || [];
    const hasPermission = permissionsService.HasPermission(
      ERPModules.DocumentModule,
      ERPModulePermission.Read,
      realmRoles
    );

    if (!hasPermission) {
      setHasAccess(false);
      router.push('/erp');
      return;
    }

    fetchCategoryData();
  }, [keycloak.authenticated]);

  const fetchCategoryData = async () => {
      documentService.getObjectCategories(keycloak.token || "").then( (response) =>
      {
        //console.log(response)
        if(response.success && response.data)
        {
          let root_node = {
            id: "ROOT",
            name: "",
            category_id: undefined,
            object_id: undefined,
            children: new Array<Node>(),
            attribute_tags: new Array<DocumentUploadObjectTagTemplate>()
          };

          for(let i=0; i<response.data.length; i++)
          {
            let inner_node = {
              id: response.data[i].internal_category_name ? response.data[i].internal_category_name : "",
              name: response.data[i].category_name ? response.data[i].category_name : "",
              category_id: response.data[i].id,
              object_id: undefined,
              children: new Array<Node>(),
              attribute_tags: new Array<DocumentUploadObjectTagTemplate>()
            } as Node;

            if(response.data[i].document_objects != undefined)
            {
              for(let y=0; y<response.data[i].document_objects.length; y++)
              {
                let obj = response.data[i].document_objects[y];

                let inner_inner_node = {
                  id: obj.internal_name ? obj.internal_name : "",
                  name: obj.friendly_name ? obj.friendly_name : "",
                  category_id: response.data[i].id,
                  object_id: response.data[i].document_objects[y].id,
                  children: new Array<Node>(),
                  attribute_tags: obj.tag_templates
                } as Node;

                inner_node.children?.push(inner_inner_node);
              }
            }

            root_node.children.push(inner_node);
          }

          setCategoryData(root_node);
        }
      });
  };

  useEffect(() => {
    if(keycloak.authenticated == false) return;

    if (hasInitialized.current) return;

    hasInitialized.current = true;

    
    fetchCategoryData();
    
  }, [keycloak.authenticated]);


  const [colDefs, setColDefs] = useState<ColDef<DocumentUploadDto>[]>([
      { headerName: "Document Name", cellRenderer: (p: any) =>{
          const revisions = p.data.document_revisions;
          if (!revisions || revisions.length === 0) return '';
          
          const most_recent = revisions
              .sort((a: any, b: any) => new Date(b.created_on).getTime() - new Date(a.created_on).getTime())[0];
          
          return most_recent?.document_name || '';
      }},
      { field: "created_on", headerName: "Uploaded On", cellRenderer: DateTimeRender  },
      {
      field: "guid",
      headerName: "Actions",
      cellRenderer: (props: any) => {
          return ( 
            <div>
              <Button type="button" colorPalette="black" variant="subtle" onClick={() => handleViewClick(props)}>View</Button>&nbsp;
            </div>
          );
      }
    }
  ]);

  const defaultColDef: ColDef = {
      flex: 1,
      filter: false,
      sortable: false,
  };

  const handleNewClick = () => {
    router.push("/erp/documents/new");
  };


  const handleViewClick = (thing: any) =>
  {
    //console.log(thing);
    const revisions = thing.data.document_revisions;
    
    if (!revisions || revisions.length === 0) return;
    
    setDocumentDto(thing.data);
    
    const most_recent = revisions.sort((a: any, b: any) => new Date(b.created_on).getTime() - new Date(a.created_on).getTime())[0] as DocumentUploadRevisionDto;

    if(most_recent && most_recent.guid)
    {
      setRevisionDto(most_recent);

      documentService.downloadFileByGuid(most_recent.guid, keycloak.token || "").then((blob) => {
        try
        {
          if (blob) {
              setFileData(blob);
              setFileType(blob.type);
              const url = URL.createObjectURL(blob);
              setFileUrl(url);

              setCurrentTab("tab-3")
          }
        }
        catch(e)
        {
            console.error('Error loading document:', e);
        }
      });
    }
  }


  interface Node {
    id: string
    name: string
    category_id: number | undefined
    object_id: number | undefined
    attribute_tags: DocumentUploadObjectTagTemplate[]
    children?: Node[]
  }

  const collection = createTreeCollection<Node>({
    nodeToValue: (node) => node.id,
    nodeToString: (node) => node.name,

    rootNode: categoryData? categoryData : {
      id: "ROOT",
      name: "",
      category_id: undefined,
      object_id: undefined,
      children: [],
      attribute_tags: []
    }
  })

  const renderFileContent = () => {
      if (!fileData || !fileUrl) {
          return <div>No document data available</div>;
      }

      // Render PDF
      if (fileType === 'application/pdf') {
          return (
              <iframe
                  src={fileUrl}
                  width="100%"
                  height="800px"
                  style={{ border: 'none' }}
                  title="PDF Document"
              />
          );
      }

      // Render images
      if (fileType.startsWith('image/')) {
          return (
              <img
                  src={fileUrl}
                  alt="Document"
                  style={{ 
                      maxWidth: '100%', 
                      maxHeight: '650px', 
                      objectFit: 'contain',
                      margin: 'auto'
                  }}
              />
          );
      }

      // Render text files
      if (fileType.startsWith('text/')) {
          return (
              <iframe
                  src={fileUrl}
                  width="100%"
                  height="800px"
                  style={{ border: 'none' }}
                  title="Text Document"
              />
          );
      }

      // For other file types, show download option
      return (
          <div style={{ textAlign: 'center', padding: '20px' }}>
              <p>This file type cannot be previewed in the browser.</p>
              <Button 
                  onClick={() => {
                      const link = document.createElement('a');
                      link.href = fileUrl;
                      link.download = 'document';
                      link.click();
                  }}
                  colorScheme="blue"
              >
                  Download File
              </Button>
          </div>
      );
  };
  
  const handleSearch = async () => 
  {
    //console.log(watch('search_text'));

    let command = new DocumentUploadFindCommand();
    command.wildcard = watch('search_text');

    setRowData([]);
    setLoading(true);

    try{
      await documentService.searchDocuments(command, keycloak.token || "").then( (response) => {
        //console.log(response)

        if(response.success && response.data)
        {
          setRowData(response.data);
          setLoading(false);
        }
        
      });
    }
    catch(e)
    {
      setLoading(false);
    }
  }

  const FormatDate = (value: any) =>
  {
    if(value == undefined || value == null || value == "")
    {
        return "";
    }

    const dateParts = value.split("-");
    const year = dateParts[0];
    const month = dateParts[1];
    const day = dateParts[2].split("T")[0];
    const time = dateParts[2].split("T")[1];
    
    const dateBuff = `${month}/${day}/${year} ${time.split(".")[0]}`;
    const timeBuff = new Date(dateBuff).toLocaleTimeString();

    return new Date(dateBuff).toLocaleDateString() + " " + timeBuff;
  }

  const treeItemClick = async (event: any) => {
    console.log(event);

    if(event.selectedNodes && event.selectedNodes.length > 0)
    {
      setCategoryNode(event.selectedNodes[0]);
    }
    else
    {
      setCategoryNode(undefined);
      return;
    }

    IsValidToUpload();

    let command = new DocumentUploadFindCommand();
    command.wildcard = watch('search_text');
    command.category_id = event.selectedNodes[0].category_id;

    if(event.selectedNodes[0].object_id != undefined)
    {
      command.object_id = event.selectedNodes[0].object_id;
    }

    setRowData([]);
    setLoading(true);

    try{
      await documentService.searchDocuments(command, keycloak.token || "").then( (response) => {
        //console.log(response)

        if(response.success && response.data)
        {
          setRowData(response.data);
          setLoading(false);
        }
        
      });
    }
    catch(e)
    {
      setLoading(false);
    }
  }

  const onFileUploadChanged = (event: any) => {
    //console.log(event);
    if(event.acceptedFiles.length > 0)
    {
        setHasFile(true);
        setSelectedFile(event.acceptedFiles[0]);
    }
    else
    {
        setHasFile(false);
        setSelectedFile(null);
    }
    setTimeout(() => {IsValidToUpload();}, 1000)
    
  } 

  const IsValidToUpload = () => 
  {
    let isValid = true;
    
    if(categoryNode)
    {
      for(let i=0;i<categoryNode.attribute_tags.length; i++)
      {
        let tag = categoryNode.attribute_tags[i];
        
        if(watch(tag.id.toString()) == undefined || watch(tag.id.toString()) == null || watch(tag.id.toString()) == "")
        {
          isValid = false;
        }
      }
    }

    const wrappup = hasFile && isValid

    setCanUpload(wrappup);
    
  }

  const uploadDocument = async () =>
  {
    setDisplayBadAlert(false);
    setDisplayGoodAlert(false);

    let document_tags = new Array<DocumentUploadRevisionTagCreateCommand>();

    if(categoryNode)
    {
      for(let i=0;i<categoryNode.attribute_tags.length; i++)
      {
        let tag = categoryNode.attribute_tags[i];
        
        let documentUploadRevision: DocumentUploadRevisionTagCreateCommand = 
        {
            document_upload_object_tag_id: tag.id,
            tag_name: tag.name,
            tag_value: watch(tag.id.toString())
        } as DocumentUploadRevisionTagCreateCommand;

        document_tags.push(documentUploadRevision);
      }
    }
    
    if(selectedFile)
    {
      let documentUploadCreate: DocumentUploadCreateCommand = {
        document_object_id: categoryNode?.object_id,
        revision_tags: document_tags,
        document_name: selectedFile.name 
          
      } as DocumentUploadCreateCommand;

      //console.log(documentUploadCreate)
      //return;

      setIsWorking(true);

      const docResponse = await documentService.create(keycloak.token || "", selectedFile, documentUploadCreate);

      if (!docResponse.success || !docResponse.data) { setIsWorking(false); setDisplayBadAlert(true); return; }
      
      setDisplayGoodAlert(true);
      setIsWorking(false);
    }
  }
  

  if (!hasAccess) {
    return <div>Redirecting...</div>;
  }

  return (
    <Grid
      templateColumns="repeat(5, 2fr)"
      gap={6}
      display="grid"
      width="100%"
      p="auto"
      m="auto"
      >
        <GridItem colSpan={1}>
          <h1>Documents</h1>
          <Stack gap="8">
            <TreeView.Root collection={collection} maxW="md" size="md" colorPalette="blue" onSelectionChange={treeItemClick}>
              <TreeView.Tree>
                <TreeView.Node
                  indentGuide={<TreeView.BranchIndentGuide />}
                  render={({ node, nodeState }) =>
                    nodeState.isBranch ? (
                      <TreeView.BranchControl>
                        <LuFolder />
                        <TreeView.BranchText>{node.name}</TreeView.BranchText>
                      </TreeView.BranchControl>
                    ) : (
                      <TreeView.Item>
                        <TreeView.ItemText>{node.name}</TreeView.ItemText>
                      </TreeView.Item>
                    )
                  }
                />
              </TreeView.Tree>
            </TreeView.Root>
          </Stack>
        </GridItem>
        <GridItem colSpan={4}>
          <Grid
            templateColumns="repeat(2, 2fr)"
            gap={6}
            display="grid"
            width="100%"
            p="auto"
            m="auto"
            >
              <GridItem colSpan={2}>
                <Tabs.Root defaultValue="tab-1" value={currentTab}>
                  <Tabs.List>
                    <Tabs.Trigger value="tab-0" onClick={() => setCurrentTab('tab-0')}>New Document</Tabs.Trigger>
                    <Tabs.Trigger value="tab-1" onClick={() => setCurrentTab('tab-1')}>Search</Tabs.Trigger>
                    <Tabs.Trigger value="tab-2" onClick={() => setCurrentTab('tab-2')}>Attributes</Tabs.Trigger>
                    <Tabs.Trigger value="tab-3" onClick={() => setCurrentTab('tab-3')}>Preview File</Tabs.Trigger>
                  </Tabs.List>
                  <Tabs.Content value="tab-0">
                    <Grid
                      templateColumns="repeat(2, 1fr)"
                      gap={6}
                      display="grid"
                      width="100%"
                      p="auto"
                      m="auto"
                      >
                      <Stack>
                        <FileUpload.Root onFileChange={onFileUploadChanged} maxFiles={1} alignItems="stretch" accept={["image/png","image/jpg", "application/pdf"]}>
                            <FileUpload.HiddenInput />
                            <FileUpload.Dropzone>
                                <Icon size="md" color="fg.muted">
                                <LuUpload />
                                </Icon>
                                <FileUpload.DropzoneContent>
                                <Box>Drag and drop files here</Box>
                                <Box color="fg.muted">.pdf, .png, .jpg up to 5MB</Box>
                                </FileUpload.DropzoneContent>
                            </FileUpload.Dropzone>
                            <FileUpload.List />
                        </FileUpload.Root>
                        <Button disabled={!canUpload} onClick={uploadDocument}><Spinner hidden={!isWorking}/> Upload</Button>
                        <Alert.Root status="success" style={{ marginTop: "10px"}} hidden={!displayGoodAlert}>
                          <Alert.Indicator />
                          <Alert.Content>
                              <Alert.Title>Record saved!</Alert.Title>
                          </Alert.Content>
                        </Alert.Root>
                        <Alert.Root status="error" style={{ marginTop: "10px"}} hidden={!displayBadAlert}>
                            <Alert.Indicator />
                            <Alert.Content>
                                <Alert.Title>Record could not be saved!</Alert.Title>
                            </Alert.Content>
                        </Alert.Root>
                      </Stack>
                      <Stack>
                        <h3>Attributes</h3>
                        <form onChange={IsValidToUpload}>
                          <For each={categoryNode?.attribute_tags}>
                              {(item) => (
                                <Field.Root w="300px" h="20" required={item.is_required} key={item.id.toString()}>
                                  <Field.Label><Field.RequiredIndicator hidden={!item.is_required} /> {item.name}</Field.Label>
                                  <Input {...register(item.id.toString())} />
                                </Field.Root>
                              )}
                            </For>
                          </form>
                      </Stack>
                    </Grid>
                  </Tabs.Content>
                  <Tabs.Content value="tab-1">
                    <Grid
                      templateColumns="repeat(5, 2fr)"
                      gap={6}
                      display="grid"
                      width="100%"
                      p="auto"
                      m="auto"
                      >
                        <GridItem colSpan={2}>
                          <Stack direction="row">
                            <Field.Root required={true}>
                                <Field.Label>Search Text</Field.Label>
                                <Input placeholder='Filename, description, attribute value...' {...register('search_text')}/>
                            </Field.Root>
                            <div style={{ marginTop: "25px" }}>
                              <Button type="submit" colorPalette="blue" onClick={handleSearch}>Search</Button>
                            </div>
                          </Stack>
                        </GridItem>
                        <GridItem colSpan={5}>
                          <div style={{ width: "100%", height: "500px" }}>
                            <AgGridReact
                                loading={loading}
                                rowData={rowData}
                                columnDefs={colDefs}
                                defaultColDef={defaultColDef}
                                onRowDoubleClicked={handleViewClick}
                            />
                          </div>
                        </GridItem>
                      </Grid>
                  </Tabs.Content>
                  <Tabs.Content value="tab-2">
                    <Grid
                      templateColumns="repeat(2, 1fr)"
                      gap={6}
                      display="grid"
                      width="100%"
                      p="auto"
                      m="auto"
                      >
                        <Stack>
                          <DataList.Root size="lg">
                            <h3>File Info</h3>
                            <DataList.Item>
                              <DataList.ItemLabel>File Name</DataList.ItemLabel>
                              <DataList.ItemValue>{revisionDto?.document_name}</DataList.ItemValue>
                            </DataList.Item>
                          </DataList.Root>
                          <DataList.Root size="lg">
                            <DataList.Item>
                              <DataList.ItemLabel>Revision #</DataList.ItemLabel>
                              <DataList.ItemValue>{documentDto?.rev_num}</DataList.ItemValue>
                            </DataList.Item>
                          </DataList.Root>
                          <DataList.Root size="lg">
                            <DataList.Item>
                              <DataList.ItemLabel>Uploaded</DataList.ItemLabel>
                              <DataList.ItemValue>{FormatDate(documentDto?.created_on)}</DataList.ItemValue>
                            </DataList.Item>
                          </DataList.Root>
                          <DataList.Root size="lg">
                            <DataList.Item>
                              <DataList.ItemLabel>Updated</DataList.ItemLabel>
                              <DataList.ItemValue>{FormatDate(revisionDto?.created_on)}</DataList.ItemValue>
                            </DataList.Item>
                          </DataList.Root>
                        </Stack>
                        <Stack>
                          <h3>Attributes</h3>
                          <For each={revisionDto?.revision_tags}>
                            {(item) => (
                              <DataList.Root size="lg">
                                <DataList.Item>
                                    <DataList.ItemLabel>{item.tag_name}</DataList.ItemLabel>
                                    <DataList.ItemValue>{item.tag_value}</DataList.ItemValue>
                                  </DataList.Item>
                              </DataList.Root>
                            )}
                          </For>
                        </Stack>
                      </Grid>
                    
                    
                  </Tabs.Content>
                  <Tabs.Content value="tab-3">
                    <div style={{ overflow: 'visible', minHeight: '400px', textAlign: "center" }}>
                        {renderFileContent()}
                    </div>
                  </Tabs.Content>
                </Tabs.Root>
                
              </GridItem>
            </Grid>
        </GridItem>
    </Grid>
  );
}


export default DocumentsPage;