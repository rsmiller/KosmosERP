"use client"

import 'ag-grid-community/styles/ag-theme-quartz.css';
import '../../../styles/page.component.css'

import { Button, CloseButton, Dialog, Field, Grid, GridItem, Input, Portal, Spinner } from "@chakra-ui/react";
import { AllCommunityModule, ColDef, ModuleRegistry } from 'ag-grid-community';
import { useEffect, useRef, useState } from 'react';
import { KeyValueCreateCommand, KeyValueDeleteCommand, KeyValueDto, KeyValueEditCommand, KeyValueFindCommand, ModuleObjectDto } from '@/models/key-value-models';
import { keyValueService } from '@/services/keyvalue-service';
import AgGridCustomPagination from '@/components/ag-grid/pagination-control';
import { AgGridReact } from 'ag-grid-react';
import { ModuleNameRenderer } from '@/components/ag-grid/module-name-renderer';
import { useForm } from 'react-hook-form';
import ModuleListCombobox, { ModuleListComboboxRef } from '@/components/module-list-combobox';
import { useAuth } from '@/lib/auth/auth-context';

ModuleRegistry.registerModules([AllCommunityModule]);

function AdminListsPage() {
    const auth = useAuth();

    const [rowData, setRowData] = useState<KeyValueDto[]>([]);
    const [page, setPage] = useState<number>(1);
    const [pageSize, setPageSize] = useState<number>(50);
    const [totalCount, setTotalCount] = useState<number>(1);

    const [loading, setLoading] = useState(true);
    
    const [moduleData, setModuleData] = useState<ModuleObjectDto[]>([]);

    const [selectedLineId, setSelectedLineId] = useState<number>(0);
    const [isDeleteDialogOpen, setIsDeleteDialogOpen] = useState(false);
    const [isAddDialogOpen, setIsAddDialogOpen] = useState(false);
    const [isWorking, setIsWorking] = useState(false);
    const [isFormValid, setIsFormValid] = useState(false);

    const moduleComboboxRef = useRef<ModuleListComboboxRef>(null);
    const hasInitialized = useRef(false);

    const {
        register,
        handleSubmit,
        formState: { errors, isValid },
        setValue,
        watch,
        control,
    } = useForm<KeyValueCreateCommand>();

    useEffect(() => {
        if(auth.authenticated == false) return;

        if (hasInitialized.current)
            return;

        hasInitialized.current = true;

        keyValueService.getModuleAndKeyValueTypes(auth.token || "").then( (response) => 
        {
            //console.log(response)
            if(response.success && response.data)
            {
                setModuleData(response.data);
            }
        });
    }, [auth.authenticated]);

    useEffect(() => {
        if(auth.authenticated == false) return;

        const fetchData = async () => {
            let pageStart = (page * pageSize) - pageSize + 1;

            if(pageStart == 0) {
                pageStart = 1;    
            }

            setRowData([]);
            setLoading(true);

            let command = new KeyValueFindCommand();
            
            await keyValueService.find(command, auth.token || "", pageStart, pageSize).then( (response) => {
                //console.log(response);
                setLoading(false);

                if(response.success && response.data)
                {
                    setTotalCount(response.totalResultCount);

                    for(let i=0;i<response.data?.length; i++)
                    {
                        let record = response.data[i];

                        setRowData(prev => [...prev, { 
                            id: record.id, 
                            module_id: record.module_id, 
                            key: record.key, 
                            value: record.value, 
                            guid: record.guid 
                        }]);
                    }
                }
            });
        };
        
        fetchData();
    
    }, [page, pageSize, auth.authenticated]);

    const onPageEvent = async (page: any) =>
    {
        setPage(page);
    }
    
    const onPageSizeEvent = async (size: any) =>
    {
        setPageSize(size);
    }
    
    const cellEdited = (event: any) =>
    {
        //console.log(event);

        let command = new KeyValueEditCommand();
        command.id = event.data.id;
        command.value = event.value;

        keyValueService.update(command, auth.token || "").then( (response) => {
            //console.log(response);
        });
    }

    const handleDeleteClick = (id: any) => {
        //console.log(id);
        setSelectedLineId(id);
        setIsDeleteDialogOpen(true);
    }

    const doLineDelete = () =>
    {
        setIsWorking(true);

        let command = new KeyValueDeleteCommand();
        command.id = selectedLineId;

        keyValueService.delete(command, auth.token || "").then( (response) => {
            //console.log(response);

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

    const doLineAdd = () => 
    {
        setIsWorking(true);

        let command = new KeyValueCreateCommand();
        command.module_id = watch("module_id");
        command.key = watch("key");
        command.value = watch("value");

        keyValueService.create(command, auth.token || "").then( (response) => {
            //console.log(response);

            setIsWorking(false);
            setIsDeleteDialogOpen(false);
            setIsAddDialogOpen(false);

            if(response.success && response.data)
            {
                let new_record = response.data;
                setRowData(prev => [...prev, new_record]);
            }
            else
            {
                console.error(response);
            }
        });
    }

    const IsDirty = (formName: any) => {
        if(watch(formName) == undefined || watch(formName) == null || watch(formName) == '')
        {
            return true;
        }

        return false;
    }

    const handleModuleSelect = (dto: ModuleObjectDto) =>
    {
        if(dto != null)
        {
            setValue('module_id', dto.module_id);
        }
    }

    const CheckFormValidity = (valid: boolean) =>
    {
        //console.log(valid);
        const isVal = Boolean(watch('key') && watch('value')) && valid;

        setIsFormValid(isVal);
    }

    const [colDefs, setColDefs] = useState<ColDef<KeyValueDto>[]>([]);

    useEffect(() => {
        setColDefs([
            { field: "module_id", headerName: "Module", cellRenderer: ModuleNameRenderer, cellRendererParams: { moduleData: moduleData } },
            { field: "key", headerName: "Key", editable: false },
            { field: "value", headerName: "Value", editable: true },
            {
                field: "guid",
                headerName: "Actions",
                cellRenderer: (props: any) => {
                    return ( 
                        <div>
                            <Button type="button" colorPalette="red" onClick={() => handleDeleteClick(props.data.id)}>Delete</Button>
                        </div>
                    );
                }
            }]);
    }, [moduleData]);
    
    const defaultColDef: ColDef = {
        flex: 1,
        filter: true,
        sortable: true,
    };

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
                    <Button onClick={() => setIsAddDialogOpen(!isAddDialogOpen)} colorPalette="blue">Add New Entry</Button>
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
                <GridItem colSpan={3}></GridItem>
                <GridItem colSpan={3}>
                    <AgGridCustomPagination totalCount={totalCount} onPage={onPageEvent} onSizeChange={onPageSizeEvent}></AgGridCustomPagination>
                </GridItem>
            </Grid>
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
                            <Grid
                                templateColumns="repeat(4, 2fr)"
                                gap={6}
                                display="grid"
                                width="100%"
                                p="auto"
                                m="auto"
                                >
                                    <GridItem colSpan={2}>
                                        <ModuleListCombobox 
                                            ref={moduleComboboxRef}
                                            dbKey={watch('module_id') ? [watch('module_id') as string] : []}
                                            onChange={handleModuleSelect}
                                            control={control}
                                            name="module_id"
                                            error={errors.module_id}
                                            onValidationChange={CheckFormValidity}
                                            dataSet={moduleData}/>
                                    </GridItem>
                                    <GridItem colSpan={2}></GridItem>
                                    <GridItem colSpan={2}>
                                        <Field.Root invalid={IsDirty('key')} required={true}>
                                            <Field.Label><Field.RequiredIndicator /> Key</Field.Label>
                                            <Input 
                                                {...register('key')}
                                            />
                                            <Field.ErrorText>This field is required</Field.ErrorText>
                                        </Field.Root>
                                    </GridItem>
                                    <GridItem colSpan={2}>
                                        <Field.Root invalid={IsDirty('value')} required={true}>
                                            <Field.Label><Field.RequiredIndicator /> Value</Field.Label>
                                            <Input 
                                                {...register('value')}
                                            />
                                            <Field.ErrorText>This field is required</Field.ErrorText>
                                        </Field.Root>
                                    </GridItem>
                            </Grid>

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
        </div>
    );
}

export default AdminListsPage;
