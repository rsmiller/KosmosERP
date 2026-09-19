"use client"

import '../../../../styles/page.component.css';

import { useForm } from 'react-hook-form'
import {
  Grid,
  Stack,
  Button,
  GridItem,
  Field,
  Input,
  NumberInput,
  Portal,
  Dialog,
  CloseButton,
} from '@chakra-ui/react'

import { useEffect, useState, useRef } from "react";
import { AllCommunityModule, ColDef, ModuleRegistry } from 'ag-grid-community';
import { AgGridReact } from 'ag-grid-react';
import PageActionsComponent from '@/components/page-actions';
import { ShipmentHeaderEditCommand, ShipmentLineDto, ShipmentLineEditCommand, ShipmentHeaderDto, ShipmentHeaderDeleteCommand, ShipmentLineDeleteCommand } from '@/models/shipments-models';
import FreightCombobox, { FreightComboboxRef } from '@/components/freight-combobox';
import AddressViewBlock from '@/components/address-view-block';
import { AddressDto } from '@/models/address-models';
import { shipmentService } from '@/services/shipment-service';
import { permissionsService, ERPModules, ERPModulePermission } from '@/services/permissions-service';
import { useAuth } from '@/lib/auth/auth-context';
import { useParams, useRouter } from 'next/navigation';
import NumericWithBenefits from '@/components/ag-grid/numeric-wth-benefits';

ModuleRegistry.registerModules([AllCommunityModule]);

function NewARFromCustomerPage() {
  const params = useParams();
  const router = useRouter();

  const auth = useAuth();
  const [hasAccess, setHasAccess] = useState(true);
  const [hasEditPermission, setHasEditPermission] = useState(false);
  const [hasDeletePermission, setHasDeletePermission] = useState(false);

  const [shipment, setShipment] = useState<ShipmentHeaderDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [saveable, canSave] = useState(false);
  const [successSaved, setSuccessSaved] = useState(false);
  const [failedSaved, setFailedSaved] = useState(false);
  const [completedOrDisabled, setCompletedOrDisabled] = useState(false);

  const [isReleaseDialogOpen, setIsReleaseDialogOpen] = useState(false);

  const [addressModel, setAddressModel] = useState<AddressDto>({} as AddressDto);
  const [rowData, setRowData] = useState<ShipmentLineDto[]>([]);

  const freightComboboxRef = useRef<FreightComboboxRef>(null);
  const hasInitialized = useRef(false);

  useEffect(() => {
    if(auth.authenticated == false) return;

    const realmRoles = auth.roles || [];
    const hasPermission = permissionsService.HasPermission(
      ERPModules.ShippingModule,
      ERPModulePermission.Read,
      realmRoles
    );
    if (!hasPermission) {
      setHasAccess(false);
      router.push('/erp');
      return;
    }

    // Check Edit permission
    const canEdit = permissionsService.HasPermission(
      ERPModules.ShippingModule,
      ERPModulePermission.Edit,
      realmRoles
    );
    setHasEditPermission(canEdit);

    // Check Delete permission
    const canDelete = permissionsService.HasPermission(
      ERPModules.ShippingModule,
      ERPModulePermission.Delete,
      realmRoles
    );
    setHasDeletePermission(canDelete);
  }, [auth.authenticated, router]);

  const {
    register,
    formState: { errors },
    setValue,
    watch,
    control,
  } = useForm<ShipmentHeaderEditCommand>();

  const loadShipment = async () => {
      try {
        setLoading(true);
        const shipmentId = String(params.id);
        
        if (shipmentId == "") {
          setError('Invalid shipment ID');
          return;
        }

        const response = await shipmentService.getByGuid(shipmentId, auth.token || "");

        //console.log(response)

        if (response.success && response.data) {
          setShipment(response.data);
          
          // Set form values with loaded data
          setValue('id', response.data.id as number);
          setValue('order_id', response.data.order_header_id || 0);
          setValue('address_id', response.data.address_id || 0);
          setValue('ship_via', response.data.ship_via || '');
          setValue('ship_attn', response.data.ship_attn || '');
          setValue('freight_carrier', response.data.freight_carrier || '');
          setValue('freight_charge_amount', response.data.freight_charge_amount || 0);
          setValue('tax', response.data.tax || 0);
          setValue('is_complete', response.data.is_complete || false);
          setValue('is_canceled', response.data.is_canceled || false);

          // Set address model if available
          if (response.data.address) {
            setAddressModel(response.data.address);
          }

          // Render shipment lines if available
          if (response.data.shipment_lines) {
            RenderLines(response.data.shipment_lines);
          }
          
          // If this is completed we can't edit this
          if (response.data.is_complete || response.data.is_released) {
            setCompletedOrDisabled(true);
            canSave(false);
          }
          else
          {
            CheckFormValidity();
          }

        } else {
          setError('Failed to load shipment');
        }
      } catch (err) {
        console.error('Error loading shipment:', err);
        setError('Error loading shipment');
      } finally {
        setLoading(false);
      }
  };

  useEffect(() => {
    if(auth.authenticated == false) return;

    if (hasInitialized.current) return;
    hasInitialized.current = true;
    

    loadShipment();
  }, [params.id, setValue, auth.authenticated]);

  const RenderLines = (shipment_lines: ShipmentLineDto[]) => {
    setRowData([]);

    for (let i = 0; i < shipment_lines.length; i++) {
      let line = shipment_lines[i];

      setRowData(prev => [...prev, {
        id: line.id,
        line_number: line.line_number,
        line_description: line.line_description,
        units_to_ship: line.units_to_ship,
        units_shipped: line.units_shipped,
        is_complete: line.is_complete,
        guid: line.guid,
        shipment_header_id: line.shipment_header_id,
        units_ordered: line.units_ordered
      }]);
    }
  };

  const CheckFormValidity = () => {
    const hasRequiredFields = Boolean(watch('freight_charge_amount') && watch("freight_carrier"));
    const freightValid = freightComboboxRef.current?.isValid() || Boolean(watch("freight_carrier"));

    const allValid = hasRequiredFields && freightValid;

    //console.log("freightComboboxRef.current: ", freightComboboxRef.current);
    //console.log("hasRequiredFields: ", hasRequiredFields);
    //console.log("freightValid: ", freightValid);
    //console.log("allValid: ", allValid);
    
    if (completedOrDisabled == false) {
      canSave(allValid);
    }
    else
    {
      canSave(false);
    }
  };

  const handleDeleteClick = async () => {
    let command = new ShipmentHeaderDeleteCommand();
    command.id = shipment?.id;

    try {
      await shipmentService.delete(command, auth.token || "").then((response) => {
        if (response.success) {
          router.push("/erp/shipments/");
        } else {
          setSuccessSaved(false);
          setFailedSaved(true);
        }
      });
    } catch (e) {
      setSuccessSaved(false);
      setFailedSaved(true);
    }
  };

  const handleSaveClick = async () => {
    setSuccessSaved(false);

    let command = new ShipmentHeaderEditCommand();
    command.id = shipment?.id;
    command.order_id = watch('order_id');
    command.address_id = watch('address_id');
    command.ship_via = watch('ship_via');
    command.ship_attn = watch('ship_attn');
    command.freight_carrier = watch('freight_carrier');
    command.freight_charge_amount = watch('freight_charge_amount');
    command.tax = watch('tax');
    command.is_complete = Boolean(watch('is_complete'));
    command.is_canceled = Boolean(watch('is_canceled'));

    // Convert rowData to ShipmentLineEditCommand array
    const shipmentLines: ShipmentLineEditCommand[] = rowData.map((line, index) => ({
      id: line.id,
      order_line_id: line.order_line_id,
      units_to_ship: line.units_to_ship,
      units_shipped: line.units_shipped,
      is_complete: line.is_complete || false,
      is_canceled: line.is_canceled || false,
    }));

    command.shipment_lines = shipmentLines;

    //console.log(command);
    //return;

    try {
      await shipmentService.update(command, auth.token || "").then((response) => {
        console.log(response);

        if (response.success) {
          setSuccessSaved(true);
          setFailedSaved(false);
          setIsReleaseDialogOpen(true);
        } else {
          setSuccessSaved(false);
          setFailedSaved(true);
        }
      });
    } catch (e) {
      console.error(e);
      setSuccessSaved(false);
      setFailedSaved(true);
    }
  };

  const handleShipLineClick = async (id: number) => {
    // Find the line to update
    const lineToUpdate = rowData.find(line => line.id === id);
    if (!lineToUpdate) return;

    // Update the shipped units to match units to ship
    const updatedLine: ShipmentLineEditCommand = {
      id: lineToUpdate.id,
      order_line_id: lineToUpdate.order_line_id,
      units_to_ship: lineToUpdate.units_to_ship,
      units_shipped: lineToUpdate.units_to_ship, // Set shipped to match to_ship
      is_complete: true,
      is_canceled: false,
    };

    try {
      await shipmentService.updateLine(updatedLine, auth.token || "").then(async (response) => {
        if (response.success) {
          // Reload the shipment to get updated data
          const shipmentResponse = await shipmentService.get(shipment?.id || 0, auth.token || "");
          if (shipmentResponse.success && shipmentResponse.data && shipmentResponse.data.shipment_lines) {
            RenderLines(shipmentResponse.data.shipment_lines);
          }
        }
      });
    } catch (e) {
      console.error(e);
      setSuccessSaved(false);
      setFailedSaved(true);
    }
  };

  const handleFrieghtSelect = (value: any) => {
    //console.log(value)

    if(value == null)
    {
      setValue('freight_carrier', null);
    }
    else
    {
      setValue('freight_carrier', value?.key || '');
    }

    CheckFormValidity();
  };

  const doRelease = async () => {
    let command = new ShipmentHeaderEditCommand();
    command.id = shipment?.id;
    command.is_released = true;

    try {
      await shipmentService.update(command, auth.token || "").then((response) => {
        //console.log(response);

        if (response.success) {
          setSuccessSaved(true);
          setFailedSaved(false);
        } else {
          setSuccessSaved(false);
          setFailedSaved(true);
        }

        setIsReleaseDialogOpen(false);
      });
    } catch (e) {
      console.error(e);
      setSuccessSaved(false);
      setFailedSaved(true);
    }
  };

  const [colDefs, setColDefs] = useState<ColDef<ShipmentLineDto>[]>([
    { field: "line_description", headerName: "Line Description" },
    { field: "units_ordered", headerName: "Ordered Qty"},
    { field: "units_shipped", headerName: "Shipped Already"},
    { field: "units_to_ship", headerName: "Ship Now", 
        editable: true,
        cellEditor: NumericWithBenefits
    },
    {
      field: "id",
      headerName: "Actions",
      cellRenderer: (props: any) => {
        const { completedOrDisabled } = props.context;
        return (
          <div>
            <Button 
              type="button" 
              colorPalette="red" 
              onClick={() => handleShipLineClick(props.value)}
              disabled={completedOrDisabled || props.data.is_complete || props.data.is_released}
            >
              Delete
            </Button>
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

  if (!hasAccess) {
    return <div>Redirecting...</div>;
  }

  if (loading) {
    return <div>Loading shipment...</div>;
  }

  if (error) {
    return <div>Error: {error}</div>;
  }

  if (!shipment) {
    return <div>Shipment not found</div>;
  }

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
        <GridItem colSpan={6}>
          <h1>Edit Shipment - {shipment.shipment_number}</h1>
        </GridItem>

        <Stack gap="1" align="flex-start" maxW="md">
          <Field.Root invalid={!!errors.freight_carrier} required={true}>
            <Field.Label><Field.RequiredIndicator /> Freight Carrier</Field.Label>
            <FreightCombobox 
              dbKey={watch('freight_carrier') || ''}
              control={control}
              name="freight_carrier"
              error={errors.freight_carrier}
              onChange={handleFrieghtSelect}
              disabled={completedOrDisabled}
              ref={freightComboboxRef}
            />
          </Field.Root>
        </Stack>

        <Stack gap="1" align="flex-start" maxW="md">
          <Field.Root invalid={!!errors.ship_via} disabled={true}>
            <Field.Label>Ship Via</Field.Label>
            <Input 
              {...register('ship_via')}
              disabled={true}
            />
          </Field.Root>
        </Stack>

        <Stack gap="1" align="flex-start" maxW="md">
          <Field.Root invalid={!!errors.ship_attn}>
            <Field.Label>Ship Attention</Field.Label>
            <Input 
              {...register('ship_attn')}
              disabled={completedOrDisabled}
            />
          </Field.Root>
        </Stack>

        <GridItem colSpan={3}></GridItem>

        <Stack gap="4" align="flex-start" maxW="md">
          <Field.Root invalid={!!errors.freight_charge_amount}>
            <Field.Label>Freight Charge</Field.Label>
            <NumberInput.Root 
              defaultValue={String(watch('freight_charge_amount') || 0)}
              disabled={completedOrDisabled}
              onValueChange={CheckFormValidity}
              min={0}
            >
              <NumberInput.Control/>
              <NumberInput.Input {...register('freight_charge_amount')} />
            </NumberInput.Root>
          </Field.Root>
        </Stack>

        <Stack gap="4" align="flex-start" maxW="md">
          <Field.Root invalid={!!errors.tax}>
            <Field.Label>Freight Tax</Field.Label>
            <NumberInput.Root 
              defaultValue={String(watch('tax') || 0)}
              disabled={completedOrDisabled}
              onValueChange={CheckFormValidity}
              min={0}
            >
              <NumberInput.Control />
              <NumberInput.Input {...register('tax')} />
            </NumberInput.Root>
          </Field.Root>
        </Stack>

        <GridItem colSpan={4}></GridItem>

        <AddressViewBlock colSpan={3} model={addressModel} title="Shipping Address" />

        <GridItem colSpan={4}></GridItem>

        <GridItem colSpan={6}>
          <div style={{ width: "100%", height: "500px" }}>
            <AgGridReact
              rowData={rowData}
              columnDefs={colDefs}
              defaultColDef={defaultColDef}
              context={{ completedOrDisabled }}
            />
          </div>
        </GridItem>

        <GridItem colSpan={5}>
          <PageActionsComponent 
            canSave={!saveable || !hasEditPermission} 
            canDelete={hasDeletePermission}
            onSave={handleSaveClick} 
            onDelete={handleDeleteClick}
            successSaved={successSaved}
            failedSaved={failedSaved}
          />
        </GridItem>
      </Grid>
      <Dialog.Root open={isReleaseDialogOpen} onOpenChange={(details) => setIsReleaseDialogOpen(details.open)} role="alertdialog">
        <Portal>
          <Dialog.Backdrop />
          <Dialog.Positioner>
            <Dialog.Content>
              <Dialog.Header>
                <Dialog.Title>Release Shipment?</Dialog.Title>
              </Dialog.Header>
              <Dialog.Body>
                <p>
                  Would you like to release this shippment record? It will be finalized and you will not be able to edit this record again.
                </p>
              </Dialog.Body>
              <Dialog.Footer>
                <Dialog.ActionTrigger asChild>
                  <Button variant="outline" onClick={() => setIsReleaseDialogOpen(false)}>No</Button>
                </Dialog.ActionTrigger>
                <Button colorPalette="green" onClick={() => doRelease()}>Release</Button>
              </Dialog.Footer>
              <Dialog.CloseTrigger asChild>
                <CloseButton size="sm" />
              </Dialog.CloseTrigger>
            </Dialog.Content>
          </Dialog.Positioner>
        </Portal>
      </Dialog.Root>
    </form>
  )
}

export default NewARFromCustomerPage;