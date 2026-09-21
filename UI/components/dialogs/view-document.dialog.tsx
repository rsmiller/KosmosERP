
import { documentService } from "@/services/document-service";
import { Button, CloseButton, Dialog, Portal } from "@chakra-ui/react";
import { useAuth } from "@/lib/auth/auth-context";
import { forwardRef, useEffect, useState } from "react";

export class ViewDocumentDialogParams
{
    openDialog?: boolean;
    onClose?: () => void;
    document_revision_guid?: string;
}

export interface ViewDocumentDialogRef {
    isValid: () => boolean;
    clear: () => void;
}

const ViewDocumentDialog = forwardRef<ViewDocumentDialogRef, ViewDocumentDialogParams>(
    ({openDialog, onClose, document_revision_guid}, ref) => {
    const auth = useAuth();
    const [isValid, setIsValid] = useState(false);
    const [fileData, setFileData] = useState<Blob | null>(null);
    const [fileUrl, setFileUrl] = useState<string | null>(null);
    const [fileType, setFileType] = useState<string>('');
    const [isLoading, setIsLoading] = useState(false);

    useEffect(() => {
        if(auth.authenticated == false) return;

        if (openDialog && document_revision_guid) {
            setIsLoading(true);
            documentService.downloadFileByGuid(document_revision_guid, auth.token || "")
                .then((blob) => {
                    if (blob) {
                        setFileData(blob);
                        setFileType(blob.type);
                        const url = URL.createObjectURL(blob);
                        setFileUrl(url);
                        setIsValid(true);

                        //console.log(blob);
                        //console.log(blob.type);
                        //console.log(url);
                    } else {
                        setIsValid(false);
                    }
                })
                .catch((error) => {
                    console.error('Error loading document:', error);
                    setIsValid(false);
                })
                .finally(() => {
                    setIsLoading(false);
                });
        }

        // Cleanup function to revoke object URL when component unmounts or dialog closes
        return () => {
            if (fileUrl) {
                URL.revokeObjectURL(fileUrl);
                setFileUrl(null);
            }
        };
    }, [openDialog, document_revision_guid, auth.authenticated]);

    const handleClose = () => {
        // Clean up the file URL
        if (fileUrl) {
            URL.revokeObjectURL(fileUrl);
            setFileUrl(null);
        }
        
        // Reset state
        setFileData(null);
        setFileType('');
        setIsValid(false);
        setIsLoading(false);
        
        // Call the parent's onClose function
        if (onClose) {
            onClose();
        }
    };

    const renderFileContent = () => {
        if (isLoading) {
            return <div>Loading document...</div>;
        }

        if (!fileData || !fileUrl) {
            return <div>No document data available</div>;
        }

        // Render PDF
        if (fileType === 'application/pdf') {
            return (
                <iframe
                    src={fileUrl}
                    width="100%"
                    height="500px"
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
                    height="660px"
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

    return (
        <Dialog.Root size={'xl'} open={openDialog}>
            <Portal>
                <Dialog.Backdrop />
                <Dialog.Positioner>
                <Dialog.Content>
                    <Dialog.Header>
                    <Dialog.Title>Document Viewer</Dialog.Title>
                    </Dialog.Header>
                    <Dialog.Body style={{ overflow: 'visible', minHeight: '400px', textAlign: "center" }}>
                        {renderFileContent()}
                    </Dialog.Body>
                    <Dialog.Footer>
                        <Dialog.ActionTrigger asChild>
                            <Button variant="outline" onClick={handleClose}>Close</Button>
                        </Dialog.ActionTrigger>
                    </Dialog.Footer>
                    <Dialog.CloseTrigger asChild>
                    <CloseButton size="sm" onClick={handleClose}/>
                    </Dialog.CloseTrigger>
                </Dialog.Content>
                </Dialog.Positioner>
            </Portal>
        </Dialog.Root>
    );
});

export default ViewDocumentDialog;