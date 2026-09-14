"use client"

import { AddressDto } from "@/models/address-models";
import {
    useFilter,
} from "@chakra-ui/react"
import { useEffect } from "react"
import { useForm } from "react-hook-form";

function AddressViewBlock({model, title, onChange}: any) {
    const { contains } = useFilter({ sensitivity: "base" })

    const {
        register,
        handleSubmit,
        formState: { errors },
    } = useForm<AddressDto>();

    useEffect(() => {
        /// FETCH DATA
        //console.log(model);
    }, []);


    return (
        <div>
            <strong>{title}</strong>
            <div>{model.street_address1}</div>
            <div>{model.street_address2}</div>
            <div>
                {model.city}, {model.state} {model.postal_code}
            </div>
            <div>{model.country}</div>
        </div>
    )
}

export default AddressViewBlock;