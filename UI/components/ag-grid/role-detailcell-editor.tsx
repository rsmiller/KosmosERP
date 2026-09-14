import { CustomCellEditorProps } from "ag-grid-react";
import { useEffect } from "react";


const RoleDetailEditor = (
    ({ data, value, onValueChange, eventKey, stopEditing }: CustomCellEditorProps) => {
    
    useEffect(() => {
        console.log(data);
        console.log(value);

    }, [data]);

    return (
        <div>
            
        </div>
    )
});

export default RoleDetailEditor;