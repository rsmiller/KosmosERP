import { AddressCreateCommand } from "@/models/address-models";
import { Button, CloseButton, Dialog, Portal } from "@chakra-ui/react";
import { forwardRef, useRef, useState } from "react";
import { Control, FieldError } from "react-hook-form";
import NewAddressBlock, { NewAddressBlockRef, NewAddressBlockResponse } from "../new-address-block";

export class AddAddressDialogParams
{
    openDialog: any;
    onChange: any;
    control?: Control<any>;
    name?: string;
    customer_id?: number;
    address_type?: number;
    error?: FieldError;
}

export interface AddAddressDialogRef {
    isValid: () => boolean;
    getValue: () => any;
    clear: () => void;
}

const AddAddressDialog = forwardRef<AddAddressDialogRef, AddAddressDialogParams>(
    ({onChange, control, name, error, customer_id, address_type, openDialog}, ref) => {
    
    const [isValid, setIsValid] = useState(false);
    const [createCommand, setCreateCommand] = useState<AddressCreateCommand>();

    const addressBlockRef = useRef<NewAddressBlockRef>(null);
    
    const doCancel = () => {
        setIsValid(false);
        if(onChange) {
            onChange(null);
        }
    }

    const doAdd = () => {
        
        if(onChange) {
            onChange(createCommand);
        }
    }

    const addressBlockChange = (command: any) => 
    {
    }

    const addressBlockValidationChange = (response: NewAddressBlockResponse) =>
    {
        console.log("ADDRESS BLOCK: ", response);
        //setAddressResponse(response);
        let data = new AddressCreateCommand();
        data.city = response.editCommand?.city;
        data.state = response.editCommand?.state;
        data.street_address1 = response.editCommand?.street_address1;
        data.street_address2 = response.editCommand?.street_address2;
        data.postal_code = response.editCommand?.postal_code;
        data.country = response.editCommand?.country;
        setCreateCommand(data);

        setIsValid(response.isValid);
    }

    return (
        <Dialog.Root size={'lg'} open={openDialog}>
            <Portal>
                <Dialog.Backdrop />
                <Dialog.Positioner>
                <Dialog.Content>
                    <Dialog.Header>
                    <Dialog.Title>Add Address</Dialog.Title>
                    </Dialog.Header>
                    <Dialog.Body style={{ overflow: 'visible' }}>
                        <div style={{ position: 'relative', zIndex: 1 }}>
                            <NewAddressBlock
                                ref={addressBlockRef}
                                address_id={null}
                                control={control}
                                name="address_id"
                                onValidationChange={addressBlockValidationChange}
                                onChange={addressBlockChange}
                            />
                        </div>
                    </Dialog.Body>
                    <Dialog.Footer>
                        <Dialog.ActionTrigger asChild>
                            <Button variant="outline" onClick={doCancel}>Cancel</Button>
                        </Dialog.ActionTrigger>
                        <Button onClick={doAdd} disabled={!isValid}>Save</Button>
                    </Dialog.Footer>
                    <Dialog.CloseTrigger asChild>
                    <CloseButton size="sm" onClick={doCancel}/>
                    </Dialog.CloseTrigger>
                </Dialog.Content>
                </Dialog.Positioner>
            </Portal>
        </Dialog.Root>
    );
});

export default AddAddressDialog;