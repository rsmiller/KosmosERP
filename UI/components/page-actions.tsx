"use client"

import {
  Button,
  GridItem,
  Alert,
  CloseButton,
  Dialog,
  Portal,
  Spinner
} from "@chakra-ui/react"
import { useEffect, useState } from "react"

function PageActionsComponent({canSave=true, canDelete=true, onSave, onDelete, saveText="Save Record", successSaved, failedSaved, deleteText="Delete Record", hidden=false}: any) {
    const [isDeleteDialogOpen, setIsDeleteDialogOpen] = useState(false);
    const [isWorking, setIsWorking] = useState(false);

    useEffect(() => {
        setIsWorking(false);
    }, []);

    const doDelete = () => 
    {
        setIsWorking(true);
        onDelete()
    };

    return (
        <div hidden={hidden} style={{ width: "100%", marginTop: "25px", textAlign: "center" }}>
            <GridItem colSpan={5} hidden={!successSaved}>
                <Alert.Root status="success">
                    <Alert.Indicator />
                    <Alert.Content>
                        <Alert.Title>Record saved!</Alert.Title>
                    </Alert.Content>
                </Alert.Root>
            </GridItem>
            <GridItem colSpan={5} hidden={!failedSaved}>
                <Alert.Root status="error">
                    <Alert.Indicator />
                    <Alert.Content>
                        <Alert.Title>Record could not be saved!</Alert.Title>
                    </Alert.Content>
                </Alert.Root>
            </GridItem>
            <hr style={{ width: "100%", marginBottom: "25px"}}/>
            <Button type="button" colorPalette="blue" onClick={() => onSave()} disabled={canSave}>{saveText}</Button>
            {canDelete && (
               <Button type="button" colorPalette="red" onClick={() => setIsDeleteDialogOpen(true)} className={"page-action-dlt-btn"}>{deleteText}</Button>
            )}

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
                            Are you sure you want to delete this record?
                        </p>
                        </Dialog.Body>
                        <Dialog.Footer>
                        <Dialog.ActionTrigger asChild>
                            <Button variant="outline" disabled={isWorking}>Cancel</Button>
                        </Dialog.ActionTrigger>
                        <Button colorPalette="red" onClick={() => doDelete()} disabled={isWorking} ><Spinner hidden={!isWorking} /> Delete</Button>
                        </Dialog.Footer>
                        <Dialog.CloseTrigger asChild>
                        <CloseButton size="sm" />
                        </Dialog.CloseTrigger>
                    </Dialog.Content>
                    </Dialog.Positioner>
                </Portal>
            </Dialog.Root>
        </div>
        
    )
}



export default PageActionsComponent;