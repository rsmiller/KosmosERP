"use client"

import 'ag-grid-community/styles/ag-theme-quartz.css';
import '../../../styles/page.component.css'

import { Button, Checkbox, CloseButton, Dialog, Field, Grid, GridItem, Input, Portal, Spinner } from "@chakra-ui/react";
import { AllCommunityModule, ColDef, ModuleRegistry } from 'ag-grid-community';
import { useEffect, useState, useRef } from 'react';
import { DocumentUploadObjectCreateCommand, DocumentUploadObjectDeleteCommand, DocumentUploadObjectDto, DocumentUploadObjectEditCommand, DocumentUploadObjectTagCreateCommand, DocumentUploadObjectTagDeleteCommand, DocumentUploadObjectTagDto, DocumentUploadObjectTagEditCommand } from '@/models/document-models';
import { documentService } from '@/services/document-service';
import { AgGridReact } from 'ag-grid-react';
import { useForm } from 'react-hook-form';
import { useKeycloak } from '@react-keycloak/web';

ModuleRegistry.registerModules([AllCommunityModule]);

function AdminDocumentTypePage() {
    const { keycloak } = useKeycloak();


    const [rowData, setRowData] = useState<DocumentUploadObjectDto[]>([]);
    const [rowTagsData, setRowTagsData] = useState<DocumentUploadObjectTagDto[]>([]);
    const [loading, setLoading] = useState(true);

    const [selectedLineId, setSelectedLineId] = useState<number>(0);
    const [selectedTagId, setSelectedTagId] = useState<number>(0);

    const [isDeleteDialogOpen, setIsDeleteDialogOpen] = useState(false);
    const [isDeleteTagDialogOpen, setIsDeleteTagDialogOpen] = useState(false);
    const [isAddDialogOpen, setIsAddDialogOpen] = useState(false);
    const [isTagDialogOpen, setIsTagDialogOpen] = useState(false);
    const [isNewTagDialogOpen, setIsNewTagDialogOpen] = useState(false);

    const [isWorking, setIsWorking] = useState(false);
    const [isFormValid, setIsFormValid] = useState(false);

    const hasInitialized = useRef(false);

    // For new document object
    const {
            register,
            setValue,
            watch,
    } = useForm<DocumentUploadObjectCreateCommand>();

    const {
            register: registerCreate,
            setValue: setValueCreate,
            watch: watchCreate,
    } = useForm<DocumentUploadObjectTagCreateCommand>();

    const fetchData = async () => {
        setRowData([]);
        setLoading(true);

        await documentService.getUploadObjects(keycloak.token || "").then( (response) => {
            setLoading(false);

            if(response.success && response.data)
            {
                setRowData(response.data);
            }
        });
    };

    useEffect(() => {

        if(keycloak.authenticated == false) return;

        if (hasInitialized.current)
            return;

        hasInitialized.current = true;

        
        
        fetchData();

    }, [keycloak.authenticated]);

    const cellEdited = (event: any) =>
    {
        //console.log("Cell edited:", event);

        if(event.data)
        {
            let command = new DocumentUploadObjectEditCommand();
            command.id = event.data.id;
            command.friendly_name = event.data.friendly_name;

            documentService.editUploadObject(command, keycloak.token || "").then( (response) =>
            {
                if(response && response.success == false)
                {
                    console.error(response);
                }
            });
        }
    }

    const [colDefs, setColDefs] = useState<ColDef<DocumentUploadObjectDto>[]>([
        { field: "friendly_name", headerName: "Friendly Name", editable: true },
        { field: "internal_name", headerName: "Internal Name", editable: false },
        {
            field: "guid",
            headerName: "Actions",
            cellRenderer: (props: any) => {
                return ( 
                    <div>
                        <Button type="button" colorPalette="green" onClick={() => doTagDialogOpen(props.data.id)}>Edit Tags</Button>&nbsp;<Button type="button" colorPalette="red" onClick={() => handleDeleteClick(props.data.id)}>Delete</Button>
                    </div>
                );
            }
        }
    ]);

    const [colTagsDefs, setColTagDefs] = useState<ColDef<DocumentUploadObjectTagDto>[]>([
        { field: "name", headerName: "Friendly Name", editable: true },
        { field: "is_required", headerName: "Is Required?", editable: true },
        {
            field: "id",
            headerName: "Actions",
            cellRenderer: (props: any) => {
                return ( 
                    <div>
                        <Button type="button" colorPalette="red" onClick={() => handleTagDelete(props.data.id)}>Delete</Button>
                    </div>
                );
            }
        }
    ]);
    
    const defaultColDef: ColDef = {
        flex: 1,
        filter: false,
        sortable: true,
    };

    const IsDirty = (formName: any) => {
        if(watch(formName) == undefined || watch(formName) == null || watch(formName) == '')
        {
            return true;
        }

        return false;
    }

    const IsCreateDirty = (formName: any) => {
        if(watchCreate(formName) == undefined || watchCreate(formName) == null || watchCreate(formName) == '')
        {
            return true;
        }

        return false;
    }
    
    const CheckAddObjectFormValidity = () =>
    {
        //console.log(valid);
        const isVal = Boolean(watch('internal_name') && watch('friendly_name'));

        setIsFormValid(isVal);
    }

    const CheckAddTagFormValidity = () =>
    {
        //console.log(valid);
        const isVal = Boolean(watchCreate('name'));

        setIsFormValid(isVal);
    }

    const handleDeleteClick = (id: any) => {
        //console.log(id);
        setSelectedLineId(id);
        setIsDeleteDialogOpen(true);
    }

    const doLineAdd = () => {
        let command = new DocumentUploadObjectCreateCommand();
        command.friendly_name = watch('friendly_name');
        command.internal_name = watch('internal_name');

        documentService.createUploadObject(command, keycloak.token || "").then( (response) => 
        {
            if(response.success && response.data)
            {
                setIsAddDialogOpen(false);

                setRowData(prev => [...prev, {
                    id: Number(response.data?.id),
                    friendly_name: response.data?.friendly_name,
                    internal_name: response.data?.internal_name,
                    guid: response.data?.guid
                }]);
            }
        });
    }

    const doLineDelete = () => {
        let command = new DocumentUploadObjectDeleteCommand();
        command.id = selectedLineId;

        setIsWorking(true);

        documentService.deleteUploadObject(command, keycloak.token || "").then( (response) => {
            setIsWorking(false);
            setIsDeleteDialogOpen(false);

            if(response.success && response.data)
            {
                let lineId = response.data.id;
                setRowData(prev => prev.filter(line => line.id !== lineId));
            }
            else
            {
                console.error(response);
            }
        });
    }

    const doTagDialogOpen = (id: number) => {
        setIsTagDialogOpen(true);
        setIsWorking(true);
        setRowTagsData([]);
        setSelectedLineId(id);
        
        documentService.getUploadObjectTags(id, keycloak.token || "").then( (response) =>
        {
            setIsWorking(false);

            if(response.success && response.data)
            {
                setRowTagsData(response.data);
            }
        });
    }

    const doTagsSave = () => {
        setIsWorking(true);

        let command = new DocumentUploadObjectTagCreateCommand();
        command.is_required = Boolean(watchCreate("is_required"));
        command.name = watchCreate("name");
        command.document_object_id = Number(selectedLineId);

        documentService.createUploadObjectTag(command, keycloak.token || "").then( (response) => 
        {
            if(response.success)
            {
                setIsNewTagDialogOpen(false);

                // Refresh
                doTagDialogOpen(selectedLineId);
            }
            else
            {
                console.error(response);
            }
        })

    }

    const cellTagsEdited = (event: any) =>
    {
        //console.log(event);
        
        if(event && event.data)
        {
            let command = new DocumentUploadObjectTagEditCommand();
            command.id = event.data.id;
            command.is_required = Boolean(event.data.is_required);
            command.name = event.data.name;

            documentService.editUploadObjectTag(command, keycloak.token || "").then( (response) => 
            {
                if(!response.success)
                {
                    console.error(response);
                }
            });
        }
        
    }

    const handleTagDelete = (id: number) => {
        setSelectedTagId(id);
        setIsDeleteTagDialogOpen(true);
    }


    const doTagDelete = () => {
        let command = new DocumentUploadObjectTagDeleteCommand();
        command.id = Number(selectedTagId);

        setIsWorking(true);

        documentService.deleteUploadObjectTag(command, keycloak.token || "").then( (response) => 
        {
            if(response.success)
            {
                setIsWorking(false);
                setIsDeleteTagDialogOpen(false);

                // Refresh
                doTagDialogOpen(selectedLineId);
            }
        });
    }

    return (
        <div>
            <Grid
                templateColumns="repeat(6, 2fr)"
                gap={6}
                display="grid"
                width="100%"
                p="auto"
                m="auto"
                >
                <GridItem colSpan={6}>
                    <Button colorPalette="blue" onClick={() => setIsAddDialogOpen(!isAddDialogOpen)}>Add New Entry</Button>
                    <div style={{ width: "100%", height: "500px" }}>
                        <AgGridReact
                            loading={loading}
                            rowData={rowData}
                            columnDefs={colDefs}
                            defaultColDef={defaultColDef}
                            onCellEditingStopped={cellEdited}
                        />
                    </div>
                </GridItem>
            </Grid>

            <Dialog.Root open={isAddDialogOpen} onOpenChange={(details) => setIsAddDialogOpen(details.open)}>
                <Portal>
                    <Dialog.Backdrop />
                    <Dialog.Positioner>
                    <Dialog.Content>
                        <Dialog.Header>
                        <Dialog.Title>Add New Entry</Dialog.Title>
                        </Dialog.Header>
                        <Dialog.Body>
                            <form onChange={CheckAddObjectFormValidity}>
                                <Grid
                                    templateColumns="repeat(2, 2fr)"
                                    gap={6}
                                    display="grid"
                                    width="100%"
                                    p="auto"
                                    m="auto"
                                    >
                                    <GridItem colSpan={2}>
                                        <Field.Root invalid={IsDirty('internal_name')} required={true}>
                                            <Field.Label><Field.RequiredIndicator /> Internal Name</Field.Label>
                                            <Input 
                                                {...register('internal_name')}
                                                onChange={(e) => {
                                                    const value = e.target.value.replace(/\s+/g, '_');
                                                    setValue('internal_name', value);
                                                }}
                                            />
                                            <Field.ErrorText>This field is required</Field.ErrorText>
                                        </Field.Root>
                                    </GridItem>
                                    <GridItem colSpan={2}>
                                        <Field.Root invalid={IsDirty('friendly_name')} required={true}>
                                            <Field.Label><Field.RequiredIndicator /> Friendly Name</Field.Label>
                                            <Input 
                                                {...register('friendly_name')}
                                            />
                                            <Field.ErrorText>This field is required</Field.ErrorText>
                                        </Field.Root>
                                    </GridItem>
                                </Grid>
                            </form>
                        </Dialog.Body>
                        <Dialog.Footer>
                        <Dialog.ActionTrigger asChild>
                            <Button variant="outline">Cancel</Button>
                        </Dialog.ActionTrigger>
                        <Button  onClick={() => doLineAdd()} disabled={!isFormValid} ><Spinner hidden={!isWorking} /> Add Entry</Button>
                        </Dialog.Footer>
                        <Dialog.CloseTrigger asChild>
                            <CloseButton size="sm" />
                        </Dialog.CloseTrigger>
                    </Dialog.Content>
                    </Dialog.Positioner>
                </Portal>
            </Dialog.Root>

            <Dialog.Root open={isDeleteDialogOpen} onOpenChange={(details) => setIsDeleteDialogOpen(details.open)} role="alertdialog">
                <Portal>
                    <Dialog.Backdrop />
                    <Dialog.Positioner>
                    <Dialog.Content>
                        <Dialog.Header>
                        <Dialog.Title>Are you sure?</Dialog.Title>
                        </Dialog.Header>
                        <Dialog.Body>
                        <p>
                            Are you sure you want to delete this entry?
                        </p>
                        </Dialog.Body>
                        <Dialog.Footer>
                        <Dialog.ActionTrigger asChild>
                            <Button variant="outline">Cancel</Button>
                        </Dialog.ActionTrigger>
                        <Button colorPalette="red" onClick={() => doLineDelete()} ><Spinner hidden={!isWorking} /> Delete</Button>
                        </Dialog.Footer>
                        <Dialog.CloseTrigger asChild>
                            <CloseButton size="sm" />
                        </Dialog.CloseTrigger>
                    </Dialog.Content>
                    </Dialog.Positioner>
                </Portal>
            </Dialog.Root>


            <Dialog.Root open={isAddDialogOpen} onOpenChange={(details) => setIsAddDialogOpen(details.open)}>
                <Portal>
                    <Dialog.Backdrop />
                    <Dialog.Positioner>
                    <Dialog.Content>
                        <Dialog.Header>
                        <Dialog.Title>Add New Entry</Dialog.Title>
                        </Dialog.Header>
                        <Dialog.Body>
                            <form onChange={CheckAddObjectFormValidity}>
                                <Grid
                                    templateColumns="repeat(2, 2fr)"
                                    gap={6}
                                    display="grid"
                                    width="100%"
                                    p="auto"
                                    m="auto"
                                    >
                                    <GridItem colSpan={2}>
                                        <Field.Root invalid={IsDirty('internal_name')} required={true}>
                                            <Field.Label><Field.RequiredIndicator /> Internal Name</Field.Label>
                                            <Input 
                                                {...register('internal_name')}
                                                onChange={(e) => {
                                                    const value = e.target.value.replace(/\s+/g, '_');
                                                    setValue('internal_name', value);
                                                }}
                                            />
                                            <Field.ErrorText>This field is required</Field.ErrorText>
                                        </Field.Root>
                                    </GridItem>
                                    <GridItem colSpan={2}>
                                        <Field.Root invalid={IsDirty('friendly_name')} required={true}>
                                            <Field.Label><Field.RequiredIndicator /> Friendly Name</Field.Label>
                                            <Input 
                                                {...register('friendly_name')}
                                            />
                                            <Field.ErrorText>This field is required</Field.ErrorText>
                                        </Field.Root>
                                    </GridItem>
                                </Grid>
                            </form>
                        </Dialog.Body>
                        <Dialog.Footer>
                        <Dialog.ActionTrigger asChild>
                            <Button variant="outline">Cancel</Button>
                        </Dialog.ActionTrigger>
                        <Button  onClick={() => doLineAdd()} disabled={!isFormValid} ><Spinner hidden={!isWorking} /> Add Entry</Button>
                        </Dialog.Footer>
                        <Dialog.CloseTrigger asChild>
                            <CloseButton size="sm" />
                        </Dialog.CloseTrigger>
                    </Dialog.Content>
                    </Dialog.Positioner>
                </Portal>
            </Dialog.Root>

            <Dialog.Root size="lg" open={isTagDialogOpen} onOpenChange={(details) => setIsTagDialogOpen(details.open)}>
                <Portal>
                    <Dialog.Backdrop />
                    <Dialog.Positioner>
                    <Dialog.Content>
                        <Dialog.Header>
                        <Dialog.Title>Tags</Dialog.Title>
                        </Dialog.Header>
                        <Dialog.Body>
                            <Button colorPalette="blue" onClick={() => setIsNewTagDialogOpen(!isAddDialogOpen)}>Add New Tag</Button>
                            <div style={{ width: "100%", height: "275px" }}>
                                
                                <AgGridReact
                                    loading={loading}
                                    rowData={rowTagsData}
                                    columnDefs={colTagsDefs}
                                    defaultColDef={defaultColDef}
                                    onCellEditingStopped={cellTagsEdited}
                                />
                            </div>
                        </Dialog.Body>
                        <Dialog.Footer>
                        <Dialog.ActionTrigger asChild>
                            <Button variant="outline">Cancel</Button>
                        </Dialog.ActionTrigger>
                        <Button  onClick={() => doTagsSave()} disabled={!isFormValid} ><Spinner hidden={!isWorking} /> Save</Button>
                        </Dialog.Footer>
                        <Dialog.CloseTrigger asChild>
                            <CloseButton size="sm" />
                        </Dialog.CloseTrigger>
                    </Dialog.Content>
                    </Dialog.Positioner>
                </Portal>
            </Dialog.Root>

            <Dialog.Root size="lg" open={isNewTagDialogOpen} onOpenChange={(details) => setIsNewTagDialogOpen(details.open)}>
                <Portal>
                    <Dialog.Backdrop />
                    <Dialog.Positioner>
                    <Dialog.Content>
                        <Dialog.Header>
                        <Dialog.Title>Add New Tag</Dialog.Title>
                        </Dialog.Header>
                        <Dialog.Body>
                            <form onChange={CheckAddTagFormValidity}>
                                <Grid
                                    templateColumns="repeat(2, 2fr)"
                                    gap={6}
                                    display="grid"
                                    width="100%"
                                    p="auto"
                                    m="auto"
                                    >
                                    <GridItem colSpan={2}>
                                        <Field.Root invalid={IsCreateDirty('name')} required={true}>
                                            <Field.Label><Field.RequiredIndicator /> Name</Field.Label>
                                            <Input 
                                                {...registerCreate('name')}
                                            />
                                            <Field.ErrorText>This field is required</Field.ErrorText>
                                        </Field.Root>
                                    </GridItem>
                                    <GridItem colSpan={2}>
                                        <Checkbox.Root checked={watchCreate('is_required')}>
                                            <Checkbox.HiddenInput {...registerCreate('is_required')} />
                                            <Checkbox.Control /> Is Required
                                        </Checkbox.Root>
                                    </GridItem>
                                </Grid>
                            </form>
                        </Dialog.Body>
                        <Dialog.Footer>
                        <Dialog.ActionTrigger asChild>
                            <Button variant="outline">Cancel</Button>
                        </Dialog.ActionTrigger>
                        <Button  onClick={() => doTagsSave()} disabled={!isFormValid} ><Spinner hidden={!isWorking} /> Save</Button>
                        </Dialog.Footer>
                        <Dialog.CloseTrigger asChild>
                            <CloseButton size="sm" />
                        </Dialog.CloseTrigger>
                    </Dialog.Content>
                    </Dialog.Positioner>
                </Portal>
            </Dialog.Root>



            <Dialog.Root open={isDeleteTagDialogOpen} onOpenChange={(details) => setIsDeleteTagDialogOpen(details.open)} role="alertdialog">
                <Portal>
                    <Dialog.Backdrop />
                    <Dialog.Positioner>
                    <Dialog.Content>
                        <Dialog.Header>
                        <Dialog.Title>Are you sure?</Dialog.Title>
                        </Dialog.Header>
                        <Dialog.Body>
                        <p>
                            Are you sure you want to delete this entry?
                        </p>
                        </Dialog.Body>
                        <Dialog.Footer>
                        <Dialog.ActionTrigger asChild>
                            <Button variant="outline">Cancel</Button>
                        </Dialog.ActionTrigger>
                        <Button colorPalette="red" onClick={() => doTagDelete()} ><Spinner hidden={!isWorking} /> Delete</Button>
                        </Dialog.Footer>
                        <Dialog.CloseTrigger asChild>
                            <CloseButton size="sm" />
                        </Dialog.CloseTrigger>
                    </Dialog.Content>
                    </Dialog.Positioner>
                </Portal>
            </Dialog.Root>
        </div>
    );
}

export default AdminDocumentTypePage;
