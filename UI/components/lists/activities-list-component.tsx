"use client"

import 'ag-grid-community/styles/ag-theme-quartz.css';
import "../../app/styles/page.component.css"

import { AllCommunityModule, ColDef, ModuleRegistry, CsvExportModule } from "ag-grid-community";
import { AgGridReact } from "ag-grid-react";
import { useEffect, useRef, useState } from "react";
import { Button, Checkbox, CloseButton, Dialog, Field, Grid, GridItem, Input, Portal, Textarea } from '@chakra-ui/react'
import { useRouter } from 'next/navigation';
import SessionStorage from '@/components/session-storage';
import AgGridCustomPagination from '@/components/ag-grid/pagination-control';
import { MdEditDocument, MdOutlinePageview } from 'react-icons/md';
import { ActivityCreateCommand, ActivityFindCommand, ActivityListDto } from '@/models/activity-models';
import { activityService } from '@/services/activity-service';
import { useForm } from 'react-hook-form';
import DatePicker from 'react-datepicker';
import { format, parse } from 'date-fns';
import ActivityStatusCombobox, { ActivityStatusComboboxRef } from '../activity-status-combobox';
import PriorityCombobox, { PriorityComboboxRef } from '../priority-combobox';
import ActivityTypeCombobox, { ActivityTypeComboboxRef } from '../activity-type-combobox';
import { useKeycloak } from '@react-keycloak/web';

ModuleRegistry.registerModules([AllCommunityModule]);


export class ActivitiesListComponentParams
{
    entity_id: any;
    entity_type: any;
    onChange: any;
}

function ActivitiesListComponent({entity_id, entity_type, onChange}: ActivitiesListComponentParams) {
  const router = useRouter();
  const userId = SessionStorage.getUserId();
  const sessionId = SessionStorage.getSession();
  const { keycloak } = useKeycloak();
  
  const [rowData, setRowData] = useState<ActivityListDto[]>([]);
  const [page, setPage] = useState<number>(1);
  const [pageSize, setPageSize] = useState<number>(50);
  const [totalCount, setTotalCount] = useState<number>(1);
  const [isDialogOpen, setDialogOpen] = useState<boolean>(false);

  const [loading, setLoading] = useState(true);
  const [saveable, canSave] = useState(false);
  const [completedOrDisabled, setCompletedOrDisabled] = useState(false);
  
  const activityTypeComboboxRef = useRef<ActivityTypeComboboxRef>(null);
  const activityStatusComboboxRef = useRef<ActivityStatusComboboxRef>(null);
  const priorityComboboxRef = useRef<PriorityComboboxRef>(null);


  const {
      register,
      handleSubmit,
      formState: { errors, isValid },
      setValue,
      watch,
      control,
  } = useForm<ActivityCreateCommand>();


  const handleViewClick = (guid: any) => {
    router.push("/erp/salesorders/view/" + guid);
  };

  const handleEditClick = (guid: any) => {
    router.push("/erp/salesorders/edit/" + guid);
  };

  const CheckFormValidity = () => 
  {
    //const typeValid = activityTypeComboboxRef.current?.isValid() || false;
    //const statusValid = activityStatusComboboxRef.current?.isValid() || false;

    const hasRequiredFields = Boolean(watch('subject') != '' && watch('description') != ''
                                      && watch('start_date') 
                                      && watch('priority') 
                                      && watch('activity_type')
                                      && watch('status'));
    //const allValid = typeValid && statusValid && hasRequiredFields;

    //console.log("typeValid: ", typeValid);
    //console.log("statusValid: ", statusValid);
    console.log("hasRequiredFields: ", hasRequiredFields);
    //console.log("allValid: ", allValid);

    if(completedOrDisabled == false)
    {
      canSave(hasRequiredFields);
    }
  }

  const saveActivity = () =>
  {
    setDialogOpen(false);
  }


  const handleActivityTypeSelect = (value: any) => {
    setValue('activity_type', value?.value || '');
    CheckFormValidity();
  };

  const handleActivityStatusSelect = (value: any) => {
    setValue('status', value?.value || '');
    CheckFormValidity();
  };

  const handlePrioritySelect = (value: any) => {
    setValue('priority', value?.value || '');
    CheckFormValidity();
  };

  const fetchData = async () => {
      let pageStart = (page * pageSize) - pageSize + 1;

      if(pageStart == 0) {
        pageStart = 1;    
      }

      setRowData([]);
      
      setLoading(true); // Show loading

      let command = new ActivityFindCommand();

      if(entity_id && entity_type)
      {
        if(entity_type == "lead")
        {
          command.lead_id = entity_id;
        }
        
        if(entity_type == "opportunity")
        {
          command.opportunity_id = entity_id;
        }

        if(entity_type == "contact")
        {
          command.contact_id = entity_id;
        }

        if(entity_type == "customer")
        {
          command.customer_id = entity_id;
        }
      }

      //console.log("pageStart: ", pageStart);
      //console.log("pageSize: ", pageSize);

      setTotalCount(1);

      
      let response = await activityService.find(command, keycloak?.token || "", pageStart, pageSize);
      //console.log(response)
      setLoading(false);

      if(response.success && response.data) {
        setTotalCount(response.totalResultCount);

        for(let i=0; i<response.data?.length; i++) {
          let record = response.data[i];
          setRowData(prev => [...prev, { 
            id: record.id, 
            subject: record.subject, 
            activity_type: record.activity_type, 
            status: record.status,
            owner_name: record.owner_name,
            start_date: record.start_date,
            guid: record.guid 
          }]);
        }
      }
  };

  useEffect(() => {
    /// FETCH DATA
    
    if(keycloak.authenticated == false) return;

    fetchData();
    
  }, [page, pageSize, keycloak.authenticated]);

  const onPageEvent = async (page: any) => {
    setPage(page);
  };

  const onPageSizeEvent = async (size: any) => {
    setPageSize(size);
  };

  const handleNewClick = () =>
  {
    setDialogOpen(true);
  }

  const getStartDate = () =>
  {
      if(watch('start_date') != undefined) {
          let start_date = watch('start_date') ? String(watch('start_date', "")) : "";
          if (start_date) {
              return parse(start_date, 'yyyy-MM-dd', new Date());
          }
      }

      return new Date();
  }
  
  const handleStartDateChange = (date: Date | null) => {
      if( date != null)
      {
          setValue('start_date', format(date, 'yyyy-MM-dd'));
      }
  }

  const handleEndDateChange = (date: Date | null) => {
      if( date != null)
      {
          setValue('end_date', format(date, 'yyyy-MM-dd'));
      }
  }


  const IsDirty = (formName: any) => {
      if(watch(formName) == undefined || watch(formName) == null || watch(formName) == '')
      {
          return true;
      }

      return false;
  }

  // Column Definitions: Defines & controls grid columns.
  const [colDefs, setColDefs] = useState<ColDef<ActivityListDto>[]>([
    { field: "subject", headerName: "Subject" },
    { field: "activity_type", headerName: "Activity Type"},
    { field: "status", headerName: "Status" },
    { field: "owner_name", headerName: "Owner" },
    { field: "start_date", headerName: "Start Date" },
    {
      field: "guid",
      headerName: "Actions",
      cellRenderer: (props: any) => {
          return ( 
            <div>
              <Button type="button" colorPalette="black" variant="subtle"onClick={() => handleViewClick(props.value)}><MdOutlinePageview /></Button>&nbsp;
              <Button type="button" colorPalette="green" onClick={() => handleEditClick(props.value)}><MdEditDocument /></Button>
            </div>
          );
      }
    }
  ]);

  const defaultColDef: ColDef = {
    flex: 1,
    filter: true,
    sortable: true,
  };

  return (
    <form onChange={CheckFormValidity}>
      <Grid
        templateColumns="repeat(5, 2fr)"
        gap={6}
        display="grid"
        width="100%"
        p="auto"
        m="auto"
      >
        <GridItem colSpan={6} hidden={entity_type == null || entity_type == ""}>
          <Button type="button" colorPalette="blue" onClick={() => handleNewClick()}>New Activity</Button>
        </GridItem>
        <GridItem colSpan={6}>
          <div style={{ width: "100%", height: "500px" }}>
            <AgGridReact
              loading={loading}
              rowData={rowData}
              columnDefs={colDefs}
              defaultColDef={defaultColDef}
              modules={[
                CsvExportModule
              ]}
            />
          </div>
        </GridItem>
        <GridItem colSpan={3}></GridItem>
        <GridItem colSpan={3}>
          <AgGridCustomPagination totalCount={totalCount} onPage={onPageEvent} onSizeChange={onPageSizeEvent}></AgGridCustomPagination>
        </GridItem>
      </Grid>

      <Dialog.Root size="lg" open={isDialogOpen} onOpenChange={(details) => setDialogOpen(details.open)} role="alertdialog">
          <Portal>
              <Dialog.Backdrop />
              <Dialog.Positioner>
                  <Dialog.Content>
                      <Dialog.Header>
                          <Dialog.Title>New Activity</Dialog.Title>
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
                            <Field.Root invalid={!!errors.activity_type}>
                              <ActivityTypeCombobox
                                ref={activityTypeComboboxRef}
                                dbKey={watch('activity_type') || ''}
                                title="Activity Type"
                                control={control}
                                name="activity_type"
                                error={errors.activity_type}
                                onChange={handleActivityTypeSelect} disabled={false}                                />
                            </Field.Root>
                          </GridItem>
                          <GridItem colSpan={2}>
                            <Field.Root invalid={!!errors.status}>
                              <ActivityStatusCombobox
                                ref={activityStatusComboboxRef}
                                dbKey={watch('status') || ''}
                                title="Activity Status"
                                control={control}
                                name="status"
                                error={errors.status}
                                onChange={handleActivityStatusSelect} disabled={false}                                />
                            </Field.Root>
                          </GridItem>
                          <GridItem colSpan={2}>
                            <Field.Root invalid={IsDirty('subject')} required={true}>
                                <Field.Label><Field.RequiredIndicator /> Subject</Field.Label>
                                <Input 
                                    {...register('subject')}
                                />
                                <Field.ErrorText>This field is required</Field.ErrorText>
                            </Field.Root>
                          </GridItem>
                          <GridItem colSpan={2}></GridItem>
                          <GridItem colSpan={4}>
                            <Field.Root invalid={IsDirty('description')} required={true}>
                                <Field.Label><Field.RequiredIndicator /> Description</Field.Label>
                                <Textarea 
                                    {...register('description')}
                                />
                                <Field.ErrorText>This field is required</Field.ErrorText>
                            </Field.Root>
                          </GridItem>
                          <GridItem colSpan={2}>
                            <Field.Root invalid={IsDirty('start_date')} required={true}>
                                <Field.Label><Field.RequiredIndicator /> Start Date</Field.Label>
                                <DatePicker 
                                    selected={getStartDate()}
                                    onChange={handleStartDateChange}
                                    dateFormat="MM/dd/yyyy"
                                    placeholderText="Select date"
                                />
                                <Field.ErrorText>This field is required</Field.ErrorText>
                            </Field.Root>
                          </GridItem>
                          <GridItem colSpan={2}>
                            <Field.Root>
                                <Field.Label>End Date</Field.Label>
                                <DatePicker 
                                    onChange={handleEndDateChange}
                                    dateFormat="MM/dd/yyyy"
                                    placeholderText="Select date"
                                />
                            </Field.Root>
                          </GridItem>
                          <GridItem colSpan={2}>
                            <Field.Root invalid={!!errors.priority}>
                              <PriorityCombobox
                                ref={priorityComboboxRef}
                                dbKey={watch('priority') || ''}
                                title="Priority"
                                control={control}
                                name="priority"
                                error={errors.priority}
                                onChange={handlePrioritySelect} disabled={false}                                />
                            </Field.Root>
                          </GridItem>
                          <GridItem colSpan={2}>
                            <Checkbox.Root checked={watch('is_all_day')}>
                                  <Checkbox.HiddenInput {...register('is_all_day')} />
                                  <Checkbox.Control /> All Day
                              </Checkbox.Root>
                          </GridItem>
                          <GridItem colSpan={2}>
                            <Field.Root>
                                <Field.Label>Location</Field.Label>
                                <Input 
                                    {...register('location')}
                                />
                            </Field.Root>
                          </GridItem>
                        </Grid>
                      </Dialog.Body>
                      <Dialog.Footer>
                          <Dialog.ActionTrigger asChild>
                              <Button disabled={!saveable} onClick={() => {saveActivity()}}>Save</Button>
                          </Dialog.ActionTrigger>
                          <Dialog.CloseTrigger asChild>
                              <CloseButton size="sm" />
                          </Dialog.CloseTrigger>
                      </Dialog.Footer>
                  </Dialog.Content>
              </Dialog.Positioner>
          </Portal>
      </Dialog.Root>
    </form>
  );
}

export default ActivitiesListComponent;