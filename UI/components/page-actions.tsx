"use client"

import {
  useFilter,
  Button,
  Grid,
  GridItem
} from "@chakra-ui/react"
import { useEffect } from "react"

function PageActionsComponent({canSave, canDelete, onSave, OnDelete}: any) {
    const { contains } = useFilter({ sensitivity: "base" });

    return (
        <div style={{ width: "100%", marginTop: "25px", textAlign: "center" }}>
            <hr style={{ width: "100%", marginBottom: "25px"}}/>
            <Button type="button" colorPalette="blue" onClick={() => onSave}>Save Record</Button>&nbsp;&nbsp;
            <Button type="button" colorPalette="red" onClick={() => OnDelete}>Delete Record</Button>
        </div>
        
    )
}



export default PageActionsComponent;